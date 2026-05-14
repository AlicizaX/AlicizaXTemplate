using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ConfigGenerate;

internal static partial class Program
{
    private static readonly string[] LanguageColumns = ["ChineseSimplified", "English", "Japanese"];
    private static readonly string[] ConstSheetNames = ["Hotfix", "Main"];
    private const string LocalizationOutputFormat = "tables_tblocalization_{0}.bytes";
    private const string LocalizationConstOutputFormat = "tables_tblocalizationconst_{0}.bytes";
    private const string EditorJsonName = "LocalizationConst.json";
    private const string DefaultLocalizationKeyCodeOut = "../Client/Assets/Scripts/Hotfix/GameLogic/LocalizationKey.cs";
    private const string DefaultLocalizationKeyCommentLanguage = "ChineseSimplified";

    private static readonly HashSet<string> CsKeywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
        "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
        "using", "virtual", "void", "volatile", "while",
    };

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);

        try
        {
            return await Run(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static async Task<int> Run(string[] args)
    {
        if (args.Length >= 1 && args[0] == "--export-localization-text")
        {
            if (args.Length != 4)
            {
                Console.Error.WriteLine("usage: ConfigGenerate.dll --export-localization-text <xlsx> <language> <output_dir>");
                return 1;
            }

            ExportLocalizationText(args[1], args[2], args[3]);
            return 0;
        }

        if (args.Length >= 1 && args[0] == "--export-localization-const")
        {
            if (args.Length == 5)
            {
                ExportLocalizationConst(args[1], args[2], args[3], args[4]);
                return 0;
            }

            if (args.Length == 6)
            {
                ExportLocalizationConst(args[1], args[2], args[3], args[5]);
                return 0;
            }

            Console.Error.WriteLine("usage: ConfigGenerate.dll --export-localization-const <xlsx> <language> <output_dir> <editor_config_dir>");
            return 1;
        }

        if (args.Length >= 1 && args[0] == "--export-localization-key")
        {
            if (args.Length is not (4 or 5))
            {
                Console.Error.WriteLine("usage: ConfigGenerate.dll --export-localization-key <xlsx> <output_file> <comment_language> [key_prefix_rules]");
                return 1;
            }

            ExportLocalizationKey(args[1], args[2], args[3], args.Length == 5 ? args[4] : null);
            return 0;
        }

        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: ConfigGenerate.dll <config.ini> <TemplateDir>");
            return 1;
        }

        return await RunAll(Path.GetFullPath(args[0]), args[1]);
    }

    private static void WriteU32(uint value, List<byte> buffer)
    {
        if (value < 0x80)
        {
            buffer.Add((byte)value);
        }
        else if (value < 0x4000)
        {
            buffer.Add((byte)((value >> 8) | 0x80));
            buffer.Add((byte)(value & 0xff));
        }
        else if (value < 0x200000)
        {
            buffer.Add((byte)((value >> 16) | 0xc0));
            buffer.Add((byte)((value >> 8) & 0xff));
            buffer.Add((byte)(value & 0xff));
        }
        else if (value < 0x10000000)
        {
            buffer.Add((byte)((value >> 24) | 0xe0));
            buffer.Add((byte)((value >> 16) & 0xff));
            buffer.Add((byte)((value >> 8) & 0xff));
            buffer.Add((byte)(value & 0xff));
        }
        else
        {
            buffer.Add(0xf0);
            buffer.Add((byte)((value >> 24) & 0xff));
            buffer.Add((byte)((value >> 16) & 0xff));
            buffer.Add((byte)((value >> 8) & 0xff));
            buffer.Add((byte)(value & 0xff));
        }
    }

    private static void WriteString(string? value, List<byte> buffer)
    {
        var raw = Encoding.UTF8.GetBytes(value ?? "");
        WriteU32((uint)raw.Length, buffer);
        buffer.AddRange(raw);
    }

    private static void WarnDuplicates(string label, IReadOnlyList<RowData> rows, bool checkId)
    {
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var seenIds = new HashSet<int>();

        foreach (var row in rows)
        {
            if (checkId)
            {
                if (!seenIds.Add(row.Id))
                {
                    Console.Error.WriteLine($"warning: {label}: duplicate id `{row.Id}`");
                }
            }

            if (!seenKeys.Add(row.Key))
            {
                Console.Error.WriteLine($"warning: {label}: duplicate key `{row.Key}`");
            }
        }
    }

    private static List<RowData> ParseSheet(SheetData sheet, bool requireId)
    {
        Dictionary<string, int> headers;
        int dataStartRow;
        if (sheet.Get(1, 1) == "##var")
        {
            (headers, dataStartRow) = BuildLubanHeaders(sheet, requireId);
        }
        else
        {
            (headers, dataStartRow) = BuildFlatHeaders(sheet, requireId);
        }

        var rows = new List<RowData>();
        for (var rowIndex = dataStartRow; rowIndex <= sheet.MaxRow; rowIndex++)
        {
            var rawKey = sheet.Get(rowIndex, headers["key"]);
            if (string.IsNullOrEmpty(rawKey))
            {
                continue;
            }

            int rowId;
            if (requireId)
            {
                var rawId = sheet.Get(rowIndex, headers["id"]);
                if (string.IsNullOrEmpty(rawId))
                {
                    continue;
                }

                rowId = ParseIntCell(rawId);
            }
            else
            {
                rowId = rows.Count + 1;
            }

            var texts = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var language in LanguageColumns)
            {
                texts[language] = sheet.Get(rowIndex, headers[language]) ?? "";
            }

            rows.Add(new RowData(rowId, rawKey, texts, sheet.Name, rowIndex, headers["key"]));
        }

        WarnDuplicates(sheet.Name, rows, requireId);
        return rows;
    }

    private static (Dictionary<string, int> Headers, int DataStartRow) BuildFlatHeaders(SheetData sheet, bool requireId)
    {
        var lookup = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var col = 1; col <= sheet.MaxCol; col++)
        {
            var text = CellText(sheet.Get(1, col));
            if (text.Length > 0)
            {
                lookup[text.ToLowerInvariant()] = col;
            }
        }

        var headers = new Dictionary<string, int>(StringComparer.Ordinal);
        if (requireId)
        {
            if (!lookup.TryGetValue("id", out var idCol))
            {
                throw new InvalidOperationException($"{sheet.Name}: missing column `ID`");
            }

            headers["id"] = idCol;
        }
        else if (lookup.TryGetValue("id", out var idCol))
        {
            headers["id"] = idCol;
        }

        if (!lookup.TryGetValue("key", out var keyCol))
        {
            throw new InvalidOperationException($"{sheet.Name}: missing column `key`");
        }

        headers["key"] = keyCol;

        foreach (var language in LanguageColumns)
        {
            if (!lookup.TryGetValue(language.ToLowerInvariant(), out var col))
            {
                throw new InvalidOperationException($"{sheet.Name}: missing column `{language}`");
            }

            headers[language] = col;
        }

        return (headers, 2);
    }

    private static (Dictionary<string, int> Headers, int DataStartRow) BuildLubanHeaders(SheetData sheet, bool requireId)
    {
        var dataOffset = sheet.Get(1, 2) == "id" ? 2 : 3;
        var headers = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var col = dataOffset; col <= sheet.MaxCol; col++)
        {
            var value = sheet.Get(1, col);
            if (!string.IsNullOrEmpty(value))
            {
                headers[value] = col;
            }
        }

        var requiredColumns = new List<string> { "key" };
        requiredColumns.AddRange(LanguageColumns);
        if (requireId)
        {
            requiredColumns.Insert(0, "id");
        }

        foreach (var column in requiredColumns)
        {
            if (!headers.ContainsKey(column))
            {
                throw new InvalidOperationException($"{sheet.Name}: missing column `{column}`");
            }
        }

        var dataStartRow = 5;
        for (var row = 1; row <= sheet.MaxRow; row++)
        {
            if (sheet.Get(row, 1) == "##")
            {
                dataStartRow = row + 1;
                break;
            }
        }

        return (headers, dataStartRow);
    }

    private static List<List<RowData>> LoadWorkbookRows(string xlsxPath, IReadOnlyList<string>? sheetNames = null, bool requireId = true)
    {
        using var workbook = XlsxWorkbook.Load(xlsxPath);
        var result = new List<List<RowData>>();
        var names = sheetNames ?? workbook.Sheets.Where(sheet => sheet.Visible).Select(sheet => sheet.Name).ToArray();

        foreach (var sheetName in names)
        {
            var sheetInfo = workbook.Sheets.FirstOrDefault(sheet => sheet.Name == sheetName);
            if (sheetInfo is null)
            {
                Console.Error.WriteLine($"warning: missing sheet `{sheetName}` in {xlsxPath}");
                continue;
            }

            result.Add(ParseSheet(workbook.ReadSheet(sheetInfo), requireId));
        }

        return result;
    }

    private static List<RowData> MergeRows(IEnumerable<List<RowData>> sheetRows, string label, bool checkId)
    {
        var merged = new List<RowData>();
        var seenIds = new HashSet<int>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var rows in sheetRows)
        {
            foreach (var row in rows)
            {
                if (checkId && !seenIds.Add(row.Id))
                {
                    throw new InvalidOperationException($"{label}: duplicate id in merged rows: {row.Id}");
                }

                if (!seenKeys.Add(row.Key))
                {
                    throw new InvalidOperationException($"{label}: duplicate key in merged rows: {row.Key}");
                }

                merged.Add(row);
            }
        }

        return merged;
    }

    private static List<string> ParsePrefixRules(string? value)
    {
        if (value is null)
        {
            return [];
        }

        var text = value.Trim();
        if (text.Length == 0 || text == "[]")
        {
            return [];
        }

        if (text.StartsWith('[') && text.EndsWith(']'))
        {
            text = text[1..^1];
        }

        return text
            .Split(',', StringSplitOptions.None)
            .Select(item => item.Trim().Trim('"', '\''))
            .Where(item => item.Length > 0)
            .ToList();
    }

    private static List<RowData> FilterRowsByKeyPrefix(List<RowData> rows, IReadOnlyList<string> keyPrefixRules)
    {
        if (keyPrefixRules.Count == 0)
        {
            return rows;
        }

        return rows.Where(row => keyPrefixRules.Any(prefix => row.Key.StartsWith(prefix, StringComparison.Ordinal))).ToList();
    }

    private static byte[] BuildLanguageBytes(IReadOnlyList<RowData> rows, string language)
    {
        var buffer = new List<byte>();
        WriteU32((uint)rows.Count, buffer);
        foreach (var row in rows)
        {
            WriteU32((uint)row.Id, buffer);
            WriteString(row.Key, buffer);
            WriteString(row.Texts.GetValueOrDefault(language, ""), buffer);
        }

        return buffer.ToArray();
    }

    private static void WriteBinary(string path, byte[] data)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(path, data);
    }

    private static void RemoveFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static void WriteEditorJson(string path, IReadOnlyList<RowData> rows)
    {
        var data = rows.Select(row => new EditorJsonRow
        {
            sheet = row.Sheet,
            id = row.Id,
            key = row.Key,
            ChineseSimplified = row.Texts.GetValueOrDefault("ChineseSimplified", ""),
            English = row.Texts.GetValueOrDefault("English", ""),
            Japanese = row.Texts.GetValueOrDefault("Japanese", ""),
        }).ToList();

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        json = NormalizeCellValue(json).Replace("\n", Environment.NewLine, StringComparison.Ordinal);
        File.WriteAllText(path, json, new UTF8Encoding(false));
    }

    private static string CsharpString(string value)
    {
        return "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
    }

    private static string XmlDocEscape(string value)
    {
        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
    }

    private static string ClassIdentifier(string value)
    {
        var cleaned = WhitespaceRegex().Replace(value, "").Replace(".", "_", StringComparison.Ordinal);
        if (cleaned.Length == 0)
        {
            return "";
        }

        if (InvalidIdentifierRegex().IsMatch(cleaned))
        {
            return "";
        }

        if (char.IsDigit(cleaned[0]))
        {
            return "";
        }

        if (CsKeywords.Contains(cleaned.ToLowerInvariant()))
        {
            return "";
        }

        return cleaned;
    }

    private static string MemberIdentifier(IReadOnlyList<string> segments)
    {
        var value = WhitespaceRegex().Replace(string.Join(".", segments), "").Replace(".", "_", StringComparison.Ordinal).ToUpperInvariant();
        if (value.Length == 0)
        {
            return "";
        }

        if (InvalidIdentifierRegex().IsMatch(value))
        {
            return "";
        }

        if (char.IsDigit(value[0]))
        {
            return "";
        }

        if (CsKeywords.Contains(value.ToLowerInvariant()))
        {
            return "";
        }

        return value;
    }

    private static void WarnInvalidKey(RowData row, string reason)
    {
        Console.Error.WriteLine($"warning: LocalizationConst {row.Sheet}!R{row.Row}C{row.KeyCol} key `{row.Key}` {reason}; row skipped");
    }

    private static string TryClassIdentifier(RowData row, string value)
    {
        if (InvalidClassSourceRegex().IsMatch(value))
        {
            var bad = new string(InvalidClassSourceRegex().Matches(value).Select(match => match.Value[0]).Distinct().OrderBy(ch => ch).ToArray());
            WarnInvalidKey(row, $"contains invalid C# class character(s) `{bad}` in the first key segment");
            return "";
        }

        var identifier = ClassIdentifier(value);
        if (identifier.Length == 0)
        {
            WarnInvalidKey(row, "cannot generate a valid C# class name from the first key segment");
        }

        return identifier;
    }

    private static string TryMemberIdentifier(RowData row, IReadOnlyList<string> segments)
    {
        var source = string.Join(".", segments);
        if (InvalidMemberSourceRegex().IsMatch(source))
        {
            var bad = new string(InvalidMemberSourceRegex().Matches(source).Select(match => match.Value[0]).Distinct().OrderBy(ch => ch).ToArray());
            WarnInvalidKey(row, $"contains invalid C# variable character(s) `{bad}`");
            return "";
        }

        var identifier = MemberIdentifier(segments);
        if (identifier.Length == 0)
        {
            WarnInvalidKey(row, "cannot generate a valid C# variable name");
        }

        return identifier;
    }

    private static int FormatArgCount(string? text)
    {
        var maxIndex = -1;
        foreach (Match match in FormatArgRegex().Matches(text ?? ""))
        {
            maxIndex = Math.Max(maxIndex, int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
        }

        return maxIndex + 1;
    }

    private static void AppendXmlSummary(List<string> lines, string indent, string comment)
    {
        if (string.IsNullOrEmpty(comment))
        {
            return;
        }

        lines.Add($"{indent}/// <summary>");
        foreach (var line in NormalizeCellValue(comment).Split('\n'))
        {
            lines.Add($"{indent}/// {XmlDocEscape(line)}");
        }

        lines.Add($"{indent}/// </summary>");
    }

    private static (string Code, int ExportedCount) GenerateLocalizationKeyCode(IReadOnlyList<RowData> rows, string commentLanguage)
    {
        var groups = new OrderedDictionary();
        foreach (var row in rows)
        {
            var parts = row.Key.Split('.');
            if (parts.Length < 2)
            {
                WarnInvalidKey(row, "must contain at least one dot for LocalizationKey grouping");
                continue;
            }

            var className = TryClassIdentifier(row, parts[0]);
            if (className.Length == 0)
            {
                continue;
            }

            if (!groups.Contains(className))
            {
                groups.Add(className, new List<RowData>());
            }

            ((List<RowData>)groups[className]!).Add(row);
        }

        var lines = new List<string>
        {
            "using AlicizaX;",
            "using AlicizaX.Localization.Runtime;",
            "",
            "/// <summary>",
            "/// AutoGenerate",
            "/// </summary>",
            "public static class LocalizationKey",
            "{",
            "    private static ILocalizationService _localizationService;",
            "",
            "    private static ILocalizationService LocalizationService",
            "    {",
            "        get",
            "        {",
            "            if (_localizationService == null)",
            "            {",
            "                _localizationService = AppServices.App.Require<ILocalizationService>();",
            "            }",
            "",
            "            return _localizationService;",
            "        }",
            "    }",
        };

        var exportedCount = 0;
        foreach (DictionaryEntry entry in groups)
        {
            var className = (string)entry.Key;
            var groupRows = (List<RowData>)entry.Value!;

            lines.Add("");
            lines.Add($"    public static class {className}");
            lines.Add("    {");

            var usedMembers = new HashSet<string>(StringComparer.Ordinal);
            var groupStartIndex = lines.Count;
            foreach (var row in groupRows)
            {
                var keyParts = row.Key.Split('.');
                var memberName = TryMemberIdentifier(row, keyParts.Skip(1).ToArray());
                if (memberName.Length == 0)
                {
                    continue;
                }

                if (!usedMembers.Add(memberName))
                {
                    throw new InvalidOperationException($"Duplicate LocalizationKey member `{className}.{memberName}` from key `{row.Key}`");
                }

                var comment = row.Texts.GetValueOrDefault(commentLanguage, "");
                AppendXmlSummary(lines, "        ", comment);

                var fallbackText = row.Texts.Values.FirstOrDefault(value => !string.IsNullOrEmpty(value)) ?? "";
                var argCount = FormatArgCount(string.IsNullOrEmpty(comment) ? fallbackText : comment);
                var keyLiteral = CsharpString(row.Key);
                if (argCount > 0)
                {
                    var args = Enumerable.Range(1, argCount).Select(index => $"arg{index}").ToArray();
                    lines.Add($"        public static string {memberName}({string.Join(", ", args.Select(arg => "string " + arg))})");
                    lines.Add("        {");
                    lines.Add($"            return LocalizationService.GetString({keyLiteral}, {string.Join(", ", args)});");
                    lines.Add("        }");
                }
                else
                {
                    lines.Add($"        public static string {memberName} => LocalizationService.GetString({keyLiteral});");
                }

                lines.Add($"        public static string {memberName}_Raw => {keyLiteral};");
                lines.Add("");
                exportedCount++;
            }

            if (lines.Count == groupStartIndex)
            {
                lines.RemoveRange(lines.Count - 3, 3);
                continue;
            }

            if (lines[^1] == "")
            {
                lines.RemoveAt(lines.Count - 1);
            }

            lines.Add("    }");
        }

        if (exportedCount <= 0)
        {
            return ("", 0);
        }

        lines.Add("}");
        lines.Add("");
        return (string.Join(Environment.NewLine, lines), exportedCount);
    }

    private static void ExportLocalizationKey(string xlsxPath, string outputFile, string commentLanguage, string? keyPrefixRules)
    {
        if (!LanguageColumns.Contains(commentLanguage, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"unsupported localization key comment language: {commentLanguage}");
        }

        var sheetRows = LoadWorkbookRows(Path.GetFullPath(xlsxPath), ConstSheetNames, true);
        var rows = MergeRows(sheetRows, "LocalizationConst", true);
        rows = FilterRowsByKeyPrefix(rows, ParsePrefixRules(keyPrefixRules));

        var (code, exportedCount) = GenerateLocalizationKeyCode(rows, commentLanguage);
        if (exportedCount <= 0)
        {
            Console.Error.WriteLine("warning: no valid LocalizationKey rows exported; output file was not changed");
            return;
        }

        var outputPath = Path.GetFullPath(outputFile);
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, code, new UTF8Encoding(true));
        Console.WriteLine($"generated localization key code: {outputPath}");
    }

    private static void ExportLocalizationText(string xlsxPath, string language, string outputDir)
    {
        if (!LanguageColumns.Contains(language, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"unsupported language: {language}");
        }

        var sheetRows = LoadWorkbookRows(Path.GetFullPath(xlsxPath), null, false);
        var rows = MergeRows(sheetRows, "Localization", false);
        var outputPath = Path.Combine(Path.GetFullPath(outputDir), string.Format(CultureInfo.InvariantCulture, LocalizationOutputFormat, language));
        WriteBinary(outputPath, BuildLanguageBytes(rows, language));
        Console.WriteLine($"[{language}] generated localization bytes: {outputPath}");
    }

    private static void ExportLocalizationConst(string xlsxPath, string language, string outputDir, string? editorConfigDir = null, bool writeEditor = true)
    {
        if (!LanguageColumns.Contains(language, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"unsupported language: {language}");
        }

        var sheetRows = LoadWorkbookRows(Path.GetFullPath(xlsxPath), ConstSheetNames, true);
        var rows = MergeRows(sheetRows, "LocalizationConst", true);
        var outputPath = Path.Combine(Path.GetFullPath(outputDir), string.Format(CultureInfo.InvariantCulture, LocalizationConstOutputFormat, language));

        if (rows.Count > 0)
        {
            WriteBinary(outputPath, BuildLanguageBytes(rows, language));
            Console.WriteLine($"[{language}] generated localization const bytes: {outputPath}");
        }
        else
        {
            RemoveFileIfExists(outputPath);
            Console.Error.WriteLine($"[{language}] warning: no LocalizationConst rows exported");
        }

        if (writeEditor && !string.IsNullOrEmpty(editorConfigDir))
        {
            var editorJsonPath = Path.Combine(Path.GetFullPath(editorConfigDir), EditorJsonName);
            if (rows.Count > 0)
            {
                WriteEditorJson(editorJsonPath, rows);
                Console.WriteLine($"[{language}] generated editor json: {editorJsonPath}");
            }
            else
            {
                RemoveFileIfExists(editorJsonPath);
            }
        }
    }

    private static void AddUtf8BomToCsFiles(string codeOut)
    {
        var directory = Path.GetFullPath(codeOut);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var csFile in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
        {
            var data = File.ReadAllBytes(csFile);
            var start = data.Length >= 3 && data[0] == 0xef && data[1] == 0xbb && data[2] == 0xbf ? 3 : 0;
            var output = new byte[data.Length - start + 3];
            output[0] = 0xef;
            output[1] = 0xbb;
            output[2] = 0xbf;
            Buffer.BlockCopy(data, start, output, 3, data.Length - start);
            File.WriteAllBytes(csFile, output);
        }
    }

    private static async Task<int> RunLuban(string scriptDir, string dataOut, string codeOut, string template)
    {
        var args = new[]
        {
            Path.Combine(scriptDir, "Tools", "Luban.dll"),
            "-t", "client",
            "-c", "cs-bin",
            "-d", "bin",
            "--conf", Path.Combine(scriptDir, "luban.conf"),
            "--customTemplateDir", ResolveTemplateDir(scriptDir, template),
            "-x", $"outputDataDir={dataOut}",
            "-x", $"outputCodeDir={codeOut}",
            "-x", "l10n.provider=default",
            "-x", "l10n.textFile.path=./Excels/Localization/Localization.xlsx",
            "-x", "l10n.textFile.keyFieldName=key",
            "-x", "l10n.textListFile=texts.txt",
        };

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = scriptDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("failed to start Luban");
        var stdoutTask = StreamProcessOutput(process.StandardOutput, Console.Out, "tables");
        var stderrTask = StreamProcessOutput(process.StandardError, Console.Error, "tables");
        await process.WaitForExitAsync();
        await Task.WhenAll(stdoutTask, stderrTask);
        Console.WriteLine($"[tables] exit code: {process.ExitCode}");
        return process.ExitCode;
    }

    private static async Task StreamProcessOutput(StreamReader reader, TextWriter writer, string prefix)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            await writer.WriteLineAsync($"[{prefix}] {line}");
        }
    }

    private static async Task<int> RunAll(string iniPath, string template)
    {
        var cfg = IniFile.Load(iniPath);
        var scriptDir = Path.GetDirectoryName(iniPath) ?? Directory.GetCurrentDirectory();
        var paths = cfg.GetSection("paths");
        var languages = cfg.GetSection("languages").Keys.ToList();

        var tableDataOut = paths.GetValueOrDefault("table_data_out") ?? paths.GetValueOrDefault("data_out") ?? "../Client/Assets/Bundles/Configs/bytes/";
        var l10nTextOut = paths.GetValueOrDefault("l10n_text_out") ?? paths.GetValueOrDefault("data_out") ?? tableDataOut;
        var l10nConstOut = paths.GetValueOrDefault("l10n_const_out") ?? paths.GetValueOrDefault("data_out") ?? tableDataOut;
        var codeOut = paths["code_out"] ?? throw new InvalidOperationException("paths.code_out is required");
        var l10nTextXlsx = paths.GetValueOrDefault("l10n_text_xlsx") ?? "./Excels/Localization/Localization.xlsx";
        var l10nConstXlsx = paths.GetValueOrDefault("l10n_const_xlsx") ?? paths.GetValueOrDefault("l10n_xlsx") ?? "./Excels/Localization/LocalizationConst.xlsx";
        var l10nEditor = paths.GetValueOrDefault("l10n_editor_out") ?? "../Client/Assets/Editor/Config";
        var l10nKeyCodeOut = paths.GetValueOrDefault("l10n_key_code_out") ?? DefaultLocalizationKeyCodeOut;
        var l10nKeyCommentLanguage = paths.GetValueOrDefault("l10n_key_comment_language") ?? paths.GetValueOrDefault("l10n_comment_language") ?? DefaultLocalizationKeyCommentLanguage;
        var l10nConstOutRule = paths.GetValueOrDefault("l10n_const_out_rule") ?? "";

        var lubanExitCode = await RunLuban(scriptDir, tableDataOut, codeOut, template);
        if (lubanExitCode != 0)
        {
            return 1;
        }

        AddUtf8BomToCsFiles(ResolvePath(scriptDir, codeOut));

        for (var index = 0; index < languages.Count; index++)
        {
            var language = languages[index];
            ExportLocalizationText(ResolvePath(scriptDir, l10nTextXlsx), language, ResolvePath(scriptDir, l10nTextOut));
            ExportLocalizationConst(
                ResolvePath(scriptDir, l10nConstXlsx),
                language,
                ResolvePath(scriptDir, l10nConstOut),
                ResolvePath(scriptDir, l10nEditor),
                index == 0);
        }

        ExportLocalizationKey(
            ResolvePath(scriptDir, l10nConstXlsx),
            ResolvePath(scriptDir, l10nKeyCodeOut),
            l10nKeyCommentLanguage,
            l10nConstOutRule);

        return 0;
    }

    private static string ResolvePath(string baseDir, string path)
    {
        return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(baseDir, path));
    }

    private static string ResolveTemplateDir(string scriptDir, string template)
    {
        var path = Path.IsPathRooted(template)
            ? template
            : Path.Combine(scriptDir, "CustomeTools", template);
        return Path.GetFullPath(path);
    }

    private static string CellText(string? value)
    {
        return value?.Trim() ?? "";
    }

    private static int ParseIntCell(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return checked((int)double.Parse(value, CultureInfo.InvariantCulture));
    }

    private static string NormalizeCellValue(string value)
    {
        return value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }

    private static int ColumnNameToIndex(string cellRef)
    {
        var result = 0;
        foreach (var ch in cellRef)
        {
            if (!char.IsLetter(ch))
            {
                break;
            }

            result = result * 26 + char.ToUpperInvariant(ch) - 'A' + 1;
        }

        return result;
    }

    private static int RowNameToIndex(string cellRef)
    {
        var digits = new string(cellRef.Where(char.IsDigit).ToArray());
        return int.Parse(digits, CultureInfo.InvariantCulture);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^0-9A-Za-z_]")]
    private static partial Regex InvalidIdentifierRegex();

    [GeneratedRegex(@"[^0-9A-Za-z_\s]")]
    private static partial Regex InvalidClassSourceRegex();

    [GeneratedRegex(@"[^0-9A-Za-z_.\s]")]
    private static partial Regex InvalidMemberSourceRegex();

    [GeneratedRegex(@"(?<!\{)\{(\d+)(?:[^{}]*)\}(?!\})")]
    private static partial Regex FormatArgRegex();

    private sealed record RowData(int Id, string Key, Dictionary<string, string> Texts, string Sheet, int Row, int KeyCol);

    private sealed class EditorJsonRow
    {
        public string sheet { get; init; } = "";
        public int id { get; init; }
        public string key { get; init; } = "";
        public string ChineseSimplified { get; init; } = "";
        public string English { get; init; } = "";
        public string Japanese { get; init; } = "";
    }

    private sealed class IniFile
    {
        private readonly Dictionary<string, Dictionary<string, string?>> _sections = new(StringComparer.Ordinal);

        public static IniFile Load(string path)
        {
            var ini = new IniFile();
            string? currentSection = null;
            foreach (var rawLine in File.ReadAllLines(path, Encoding.UTF8))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1];
                    ini._sections.TryAdd(currentSection, new Dictionary<string, string?>(StringComparer.Ordinal));
                    continue;
                }

                if (currentSection is null)
                {
                    continue;
                }

                var section = ini._sections[currentSection];
                var eqIndex = line.IndexOf('=');
                if (eqIndex < 0)
                {
                    section[line] = null;
                }
                else
                {
                    section[line[..eqIndex].Trim()] = line[(eqIndex + 1)..].Trim();
                }
            }

            return ini;
        }

        public Dictionary<string, string?> GetSection(string name)
        {
            return _sections.TryGetValue(name, out var section)
                ? section
                : throw new InvalidOperationException($"missing ini section `{name}`");
        }
    }

    private sealed class SheetInfo
    {
        public required string Name { get; init; }
        public required string Path { get; init; }
        public bool Visible { get; init; }
    }

    private sealed class SheetData
    {
        private readonly Dictionary<(int Row, int Col), string> _cells;

        public SheetData(string name, Dictionary<(int Row, int Col), string> cells, int maxRow, int maxCol)
        {
            Name = name;
            _cells = cells;
            MaxRow = maxRow;
            MaxCol = maxCol;
        }

        public string Name { get; }
        public int MaxRow { get; }
        public int MaxCol { get; }

        public string? Get(int row, int col)
        {
            return _cells.GetValueOrDefault((row, col));
        }
    }

    private sealed class XlsxWorkbook : IDisposable
    {
        private readonly ZipArchive _archive;
        private readonly List<string> _sharedStrings;

        private XlsxWorkbook(ZipArchive archive, List<SheetInfo> sheets, List<string> sharedStrings)
        {
            _archive = archive;
            Sheets = sheets;
            _sharedStrings = sharedStrings;
        }

        public List<SheetInfo> Sheets { get; }

        public static XlsxWorkbook Load(string path)
        {
            var archive = ZipFile.OpenRead(path);
            try
            {
                var sharedStrings = ReadSharedStrings(archive);
                var sheets = ReadWorkbookSheets(archive);
                return new XlsxWorkbook(archive, sheets, sharedStrings);
            }
            catch
            {
                archive.Dispose();
                throw;
            }
        }

        public SheetData ReadSheet(SheetInfo sheet)
        {
            var entry = _archive.GetEntry(sheet.Path) ?? throw new InvalidOperationException($"missing xlsx entry: {sheet.Path}");
            using var stream = entry.Open();
            var doc = XDocument.Load(stream, LoadOptions.None);
            var cells = new Dictionary<(int Row, int Col), string>();
            var maxRow = 0;
            var maxCol = 0;

            foreach (var c in doc.Descendants().Where(element => element.Name.LocalName == "c"))
            {
                var cellRef = c.Attribute("r")?.Value;
                if (string.IsNullOrEmpty(cellRef))
                {
                    continue;
                }

                var row = RowNameToIndex(cellRef);
                var col = ColumnNameToIndex(cellRef);
                var value = ReadCellValue(c);
                cells[(row, col)] = NormalizeCellValue(value);
                maxRow = Math.Max(maxRow, row);
                maxCol = Math.Max(maxCol, col);
            }

            return new SheetData(sheet.Name, cells, maxRow, maxCol);
        }

        private string ReadCellValue(XElement cell)
        {
            var type = cell.Attribute("t")?.Value;
            if (type == "inlineStr")
            {
                return string.Concat(cell.Descendants().Where(element => element.Name.LocalName == "t").Select(element => element.Value));
            }

            var raw = cell.Elements().FirstOrDefault(element => element.Name.LocalName == "v")?.Value ?? "";
            return type == "s" && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sharedStringIndex)
                ? _sharedStrings.ElementAtOrDefault(sharedStringIndex) ?? ""
                : raw;
        }

        public void Dispose()
        {
            _archive.Dispose();
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry is null)
            {
                return [];
            }

            using var stream = entry.Open();
            var doc = XDocument.Load(stream, LoadOptions.None);
            return doc
                .Descendants()
                .Where(element => element.Name.LocalName == "si")
                .Select(si => NormalizeCellValue(string.Concat(si.Descendants().Where(element => element.Name.LocalName == "t").Select(element => element.Value))))
                .ToList();
        }

        private static List<SheetInfo> ReadWorkbookSheets(ZipArchive archive)
        {
            var workbookEntry = archive.GetEntry("xl/workbook.xml") ?? throw new InvalidOperationException("missing xlsx entry: xl/workbook.xml");
            var relsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels") ?? throw new InvalidOperationException("missing xlsx entry: xl/_rels/workbook.xml.rels");

            using var relsStream = relsEntry.Open();
            var relsDoc = XDocument.Load(relsStream, LoadOptions.None);
            var relMap = relsDoc
                .Descendants()
                .Where(element => element.Name.LocalName == "Relationship")
                .Where(element => element.Attribute("Id") is not null && element.Attribute("Target") is not null)
                .ToDictionary(element => element.Attribute("Id")!.Value, element => element.Attribute("Target")!.Value, StringComparer.Ordinal);

            using var workbookStream = workbookEntry.Open();
            var workbookDoc = XDocument.Load(workbookStream, LoadOptions.None);
            XNamespace relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            var sheets = new List<SheetInfo>();
            foreach (var sheet in workbookDoc.Descendants().Where(element => element.Name.LocalName == "sheet"))
            {
                var name = sheet.Attribute("name")?.Value;
                var rid = sheet.Attribute(relNs + "id")?.Value;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(rid) || !relMap.TryGetValue(rid, out var target))
                {
                    continue;
                }

                var state = sheet.Attribute("state")?.Value;
                sheets.Add(new SheetInfo
                {
                    Name = name,
                    Path = NormalizeWorkbookTarget(target),
                    Visible = string.IsNullOrEmpty(state) || state == "visible",
                });
            }

            return sheets;
        }

        private static string NormalizeWorkbookTarget(string target)
        {
            var normalized = target.Replace('\\', '/').TrimStart('/');
            if (!normalized.StartsWith("xl/", StringComparison.Ordinal))
            {
                normalized = "xl/" + normalized;
            }

            var parts = new List<string>();
            foreach (var part in normalized.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (part == ".")
                {
                    continue;
                }

                if (part == "..")
                {
                    if (parts.Count > 0)
                    {
                        parts.RemoveAt(parts.Count - 1);
                    }

                    continue;
                }

                parts.Add(part);
            }

            return string.Join("/", parts);
        }
    }
}
