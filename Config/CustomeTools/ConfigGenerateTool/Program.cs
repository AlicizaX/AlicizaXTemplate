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
        Console.InputEncoding = new UTF8Encoding(false);
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
                Console.Error.WriteLine("用法：ConfigGenerate.dll --export-localization-text <xlsx文件> <语言> <输出目录>");
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

            Console.Error.WriteLine("用法：ConfigGenerate.dll --export-localization-const <xlsx文件> <语言> <输出目录> <编辑器配置目录>");
            return 1;
        }

        if (args.Length >= 1 && args[0] == "--export-localization-key")
        {
            if (args.Length is not (4 or 5))
            {
                Console.Error.WriteLine("用法：ConfigGenerate.dll --export-localization-key <xlsx文件> <输出文件> <注释语言> [Key前缀规则]");
                return 1;
            }

            ExportLocalizationKey(args[1], args[2], args[3], args.Length == 5 ? args[4] : null);
            return 0;
        }

        if (args.Length != 2)
        {
            Console.Error.WriteLine("用法：ConfigGenerate.dll <config.ini> <模板目录>");
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

    private static void WriteWarning(string message)
    {
        var previousColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Error.WriteLine(message);
        }
        finally
        {
            Console.ForegroundColor = previousColor;
        }
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
                    WriteWarning($"警告：{label} 存在重复 ID `{row.Id}`。");
                }
            }

            if (!seenKeys.Add(row.Key))
            {
                WriteWarning($"警告：{label} 存在重复 Key `{row.Key}`。");
            }
        }
    }

    private static List<RowData> ParseSheet(SheetData sheet, bool requireId, IReadOnlyList<string> languages)
    {
        Dictionary<string, int> headers;
        int dataStartRow;
        if (sheet.Get(1, 1) == "##var")
        {
            (headers, dataStartRow) = BuildLubanHeaders(sheet, requireId, languages);
        }
        else
        {
            (headers, dataStartRow) = BuildFlatHeaders(sheet, requireId, languages);
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
            foreach (var language in languages)
            {
                if (headers.TryGetValue(language, out var languageColumn))
                {
                    texts[language] = sheet.Get(rowIndex, languageColumn) ?? "";
                }
            }

            rows.Add(new RowData(rowId, rawKey, texts, sheet.Name, rowIndex, headers["key"]));
        }

        WarnDuplicates(sheet.Name, rows, requireId);
        return rows;
    }

    private static (Dictionary<string, int> Headers, int DataStartRow) BuildFlatHeaders(SheetData sheet, bool requireId, IReadOnlyList<string> languages)
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
                throw new InvalidOperationException($"{sheet.Name} 缺少列 `ID`。");
            }

            headers["id"] = idCol;
        }
        else if (lookup.TryGetValue("id", out var idCol))
        {
            headers["id"] = idCol;
        }

        if (!lookup.TryGetValue("key", out var keyCol))
        {
            throw new InvalidOperationException($"{sheet.Name} 缺少列 `key`。");
        }

        headers["key"] = keyCol;

        foreach (var language in languages)
        {
            if (!lookup.TryGetValue(language.ToLowerInvariant(), out var col))
            {
                WriteWarning($"警告：{sheet.Name} 缺少已配置的语言列 `{language}`，该语言内容将按空值处理。");
                continue;
            }

            headers[language] = col;
        }

        return (headers, 2);
    }

    private static (Dictionary<string, int> Headers, int DataStartRow) BuildLubanHeaders(SheetData sheet, bool requireId, IReadOnlyList<string> languages)
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
        if (requireId)
        {
            requiredColumns.Insert(0, "id");
        }

        foreach (var column in requiredColumns)
        {
            if (!headers.ContainsKey(column))
            {
                throw new InvalidOperationException($"{sheet.Name} 缺少列 `{column}`。");
            }
        }

        foreach (var language in languages)
        {
            if (!headers.ContainsKey(language))
            {
                WriteWarning($"警告：{sheet.Name} 缺少已配置的语言列 `{language}`，该语言内容将按空值处理。");
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

    private static List<List<RowData>> LoadWorkbookRows(string xlsxPath, IReadOnlyList<string>? sheetNames, bool requireId, IReadOnlyList<string> languages)
    {
        using var workbook = XlsxWorkbook.Load(xlsxPath);
        var result = new List<List<RowData>>();
        var names = sheetNames ?? workbook.Sheets.Where(sheet => sheet.Visible).Select(sheet => sheet.Name).ToArray();

        foreach (var sheetName in names)
        {
            var sheetInfo = workbook.Sheets.FirstOrDefault(sheet => sheet.Name == sheetName);
            if (sheetInfo is null)
            {
                WriteWarning($"警告：文件 `{xlsxPath}` 中缺少工作表 `{sheetName}`。");
                continue;
            }

            result.Add(ParseSheet(workbook.ReadSheet(sheetInfo), requireId, languages));
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
                    throw new InvalidOperationException($"{label} 合并后存在重复 ID：{row.Id}。");
                }

                if (!seenKeys.Add(row.Key))
                {
                    throw new InvalidOperationException($"{label} 合并后存在重复 Key：{row.Key}。");
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

    private static List<string> NormalizeLanguages(IEnumerable<string> languages)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var language in languages.Select(language => language.Trim()).Where(language => language.Length > 0))
        {
            if (seen.Add(language))
            {
                result.Add(language);
            }
            else
            {
                WriteWarning($"警告：重复配置的语言 `{language}` 已忽略。");
            }
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException("多语言配置至少需要包含一种语言。");
        }

        return result;
    }

    private static List<string> ReadConfiguredLanguages(IniFile cfg, Dictionary<string, string?> paths)
    {
        if (paths.TryGetValue("l10n_languages", out var configuredLanguages) && !string.IsNullOrWhiteSpace(configuredLanguages))
        {
            return NormalizeLanguages(ParsePrefixRules(configuredLanguages));
        }

        return NormalizeLanguages(cfg.GetSection("languages").Keys);
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

    private static bool HasLanguageData(IReadOnlyList<RowData> rows, string language)
    {
        return rows.Any(row => !string.IsNullOrWhiteSpace(row.Texts.GetValueOrDefault(language, "")));
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

    private static void RemoveDirectoryIfEmpty(string path)
    {
        if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path);
        }
    }

    private static void CopyDirectoryWithoutMeta(string sourceDir, string targetDir)
    {
        if (!Directory.Exists(sourceDir))
        {
            throw new DirectoryNotFoundException($"源目录不存在：{sourceDir}");
        }

        Directory.CreateDirectory(targetDir);

        foreach (var sourceFile in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(sourceFile).Equals(".meta", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(sourceDir, sourceFile);
            var targetFile = Path.Combine(targetDir, relativePath);
            var targetFileDir = Path.GetDirectoryName(targetFile);
            if (!string.IsNullOrEmpty(targetFileDir))
            {
                Directory.CreateDirectory(targetFileDir);
            }

            File.Copy(sourceFile, targetFile, true);
        }
    }

    private static void WriteEditorJson(string path, IReadOnlyList<RowData> rows, IReadOnlyList<string> languages)
    {
        var data = rows.Select(row =>
        {
            var item = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["sheet"] = row.Sheet,
                ["id"] = row.Id,
                ["key"] = row.Key,
            };

            foreach (var language in languages)
            {
                item[language] = row.Texts.GetValueOrDefault(language, "");
            }

            return item;
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
        WriteWarning($"警告：LocalizationConst {row.Sheet}!R{row.Row}C{row.KeyCol} 的 Key `{row.Key}` {reason}，已跳过该行。");
    }

    private static string TryClassIdentifier(RowData row, string value)
    {
        if (InvalidClassSourceRegex().IsMatch(value))
        {
            var bad = new string(InvalidClassSourceRegex().Matches(value).Select(match => match.Value[0]).Distinct().OrderBy(ch => ch).ToArray());
            WarnInvalidKey(row, $"首段包含非法 C# 类名字符 `{bad}`");
            return "";
        }

        var identifier = ClassIdentifier(value);
        if (identifier.Length == 0)
        {
            WarnInvalidKey(row, "无法根据首段生成有效的 C# 类名");
        }

        return identifier;
    }

    private static string TryMemberIdentifier(RowData row, IReadOnlyList<string> segments)
    {
        var source = string.Join(".", segments);
        if (InvalidMemberSourceRegex().IsMatch(source))
        {
            var bad = new string(InvalidMemberSourceRegex().Matches(source).Select(match => match.Value[0]).Distinct().OrderBy(ch => ch).ToArray());
            WarnInvalidKey(row, $"包含非法 C# 变量名字符 `{bad}`");
            return "";
        }

        var identifier = MemberIdentifier(segments);
        if (identifier.Length == 0)
        {
            WarnInvalidKey(row, "无法生成有效的 C# 变量名");
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
                    throw new InvalidOperationException($"LocalizationKey 成员 `{className}.{memberName}` 重复，来源 Key：`{row.Key}`。");
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
        ExportLocalizationKey(xlsxPath, outputFile, commentLanguage, keyPrefixRules, [commentLanguage]);
    }

    private static void ExportLocalizationKey(string xlsxPath, string outputFile, string commentLanguage, string? keyPrefixRules, IReadOnlyList<string> languages)
    {
        if (!languages.Contains(commentLanguage, StringComparer.Ordinal))
        {
            WriteWarning($"警告：LocalizationKey 注释语言 `{commentLanguage}` 不在已配置语言列表中，生成的注释可能为空。");
        }

        var sheetRows = LoadWorkbookRows(Path.GetFullPath(xlsxPath), ConstSheetNames, true, languages);
        var rows = MergeRows(sheetRows, "LocalizationConst", true);
        rows = FilterRowsByKeyPrefix(rows, ParsePrefixRules(keyPrefixRules));

        var (code, exportedCount) = GenerateLocalizationKeyCode(rows, commentLanguage);
        if (exportedCount <= 0)
        {
            WriteWarning("警告：没有可导出的 LocalizationKey 有效行，输出文件未修改。");
            return;
        }

        var outputPath = Path.GetFullPath(outputFile);
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, code, new UTF8Encoding(true));
        Console.WriteLine($"成功：已生成 LocalizationKey 代码：{outputPath}");
    }

    private static void ExportLocalizationText(string xlsxPath, string language, string outputDir)
    {
        ExportLocalizationText([Path.GetFullPath(xlsxPath)], language, outputDir, [language]);
    }

    private static void ExportLocalizationText(IReadOnlyList<string> xlsxPaths, string language, string outputDir, IReadOnlyList<string> languages)
    {
        ExportLocalizationTextRows(LoadLocalizationTextRows(xlsxPaths, languages), language, outputDir);
    }

    private static bool ExportLocalizationTextRows(IReadOnlyList<RowData> rows, string language, string outputDir)
    {
        var outputPath = Path.Combine(Path.GetFullPath(outputDir), string.Format(CultureInfo.InvariantCulture, LocalizationOutputFormat, language));
        if (!HasLanguageData(rows, language))
        {
            RemoveFileIfExists(outputPath);
            WriteWarning($"警告：[{language}] 未找到多语言文本数据，已跳过：{outputPath}");
            return false;
        }

        WriteBinary(outputPath, BuildLanguageBytes(rows, language));
        Console.WriteLine($"成功：[{language}] 已生成多语言文本 bytes：{outputPath}");
        return true;
    }

    private static List<RowData> LoadLocalizationTextRows(IReadOnlyList<string> xlsxPaths, IReadOnlyList<string> languages)
    {
        var sheetRows = new List<List<RowData>>();
        foreach (var xlsxPath in xlsxPaths)
        {
            sheetRows.AddRange(LoadWorkbookRows(Path.GetFullPath(xlsxPath), null, false, languages));
        }

        return MergeRows(sheetRows, "Localization", false);
    }

    private static void ExportLocalizationConst(string xlsxPath, string language, string outputDir, string? editorConfigDir = null, bool writeEditor = true)
    {
        ExportLocalizationConst(xlsxPath, language, outputDir, editorConfigDir, writeEditor, [language]);
    }

    private static bool ExportLocalizationConst(string xlsxPath, string language, string outputDir, string? editorConfigDir, bool writeEditor, IReadOnlyList<string> languages)
    {
        var sheetRows = LoadWorkbookRows(Path.GetFullPath(xlsxPath), ConstSheetNames, true, languages);
        var rows = MergeRows(sheetRows, "LocalizationConst", true);
        var exported = ExportLocalizationConstRows(rows, language, outputDir);

        if (!exported)
        {
            return false;
        }

        if (writeEditor && !string.IsNullOrEmpty(editorConfigDir))
        {
            var editorJsonPath = Path.Combine(Path.GetFullPath(editorConfigDir), EditorJsonName);
            WriteEditorJson(editorJsonPath, rows, [language]);
            Console.WriteLine($"成功：[{language}] 已生成编辑器 JSON：{editorJsonPath}");
        }

        return true;
    }

    private static bool ExportLocalizationConstRows(IReadOnlyList<RowData> rows, string language, string outputDir)
    {
        var outputPath = Path.Combine(Path.GetFullPath(outputDir), string.Format(CultureInfo.InvariantCulture, LocalizationConstOutputFormat, language));

        if (rows.Count > 0 && HasLanguageData(rows, language))
        {
            WriteBinary(outputPath, BuildLanguageBytes(rows, language));
            Console.WriteLine($"成功：[{language}] 已生成多语言常量 bytes：{outputPath}");
        }
        else
        {
            RemoveFileIfExists(outputPath);
            WriteWarning($"警告：[{language}] 未找到多语言常量数据，已跳过：{outputPath}");
            return false;
        }

        return true;
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

    private static List<string> ResolveLocalizationTextFiles(string scriptDir, string sourcePath, string filePattern)
    {
        var fullPath = ResolvePath(scriptDir, sourcePath);
        if (File.Exists(fullPath))
        {
            return [fullPath];
        }

        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"多语言文本目录不存在：{fullPath}");
        }

        var regex = new Regex(filePattern, RegexOptions.CultureInvariant);
        var files = Directory
            .EnumerateFiles(fullPath, "*.xlsx", SearchOption.TopDirectoryOnly)
            .Where(path => !Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal))
            .Where(path => regex.IsMatch(Path.GetFileNameWithoutExtension(path)))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

        if (files.Count == 0)
        {
            throw new InvalidOperationException($"目录 `{fullPath}` 中没有匹配规则 `{filePattern}` 的多语言文本 xlsx 文件。");
        }

        Console.WriteLine($"信息：多语言文本文件：{string.Join(", ", files.Select(Path.GetFileName))}");
        return files;
    }

    private static string ToLubanInputPath(string scriptDir, string path)
    {
        var fullScriptDir = Path.GetFullPath(scriptDir);
        var fullPath = Path.GetFullPath(path);
        var relativePath = Path.GetRelativePath(fullScriptDir, fullPath);
        return relativePath.Replace(Path.DirectorySeparatorChar.ToString(), "/", StringComparison.Ordinal)
            .Replace(Path.AltDirectorySeparatorChar.ToString(), "/", StringComparison.Ordinal);
    }

    private static string WriteLubanLocalizationTextFile(string scriptDir, IReadOnlyList<RowData> rows)
    {
        var path = Path.Combine(scriptDir, "Temp", "l10n_text_provider.xlsx");
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        WriteKeyOnlyXlsx(path, rows.Select(row => row.Key));
        return path;
    }

    private static void WriteKeyOnlyXlsx(string path, IEnumerable<string> keys)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        WriteZipEntry(archive, "[Content_Types].xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
              <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
            </Types>
            """);
        WriteZipEntry(archive, "_rels/.rels", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
            </Relationships>
            """);
        WriteZipEntry(archive, "xl/workbook.xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <sheets>
                <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
              </sheets>
            </workbook>
            """);
        WriteZipEntry(archive, "xl/_rels/workbook.xml.rels", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
            </Relationships>
            """);

        var sheet = new StringBuilder();
        sheet.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
        sheet.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
        sheet.AppendLine("""  <sheetData>""");
        sheet.AppendLine("""    <row r="1"><c r="A1" t="inlineStr"><is><t>##var</t></is></c><c r="B1" t="inlineStr"><is><t>key</t></is></c></row>""");
        sheet.AppendLine("""    <row r="2"><c r="A2" t="inlineStr"><is><t>##type</t></is></c><c r="B2" t="inlineStr"><is><t>string</t></is></c></row>""");
        sheet.AppendLine("""    <row r="3"><c r="A3" t="inlineStr"><is><t>##group</t></is></c><c r="B3" t="inlineStr"><is><t>c</t></is></c></row>""");
        sheet.AppendLine("""    <row r="4"><c r="A4" t="inlineStr"><is><t>##</t></is></c></row>""");

        var rowIndex = 5;
        foreach (var key in keys)
        {
            sheet.Append("    <row r=\"").Append(rowIndex.ToString(CultureInfo.InvariantCulture)).AppendLine("\">");
            sheet.Append("      <c r=\"B").Append(rowIndex.ToString(CultureInfo.InvariantCulture)).AppendLine("\" t=\"inlineStr\"><is><t>");
            sheet.Append(SecurityElementEscape(key));
            sheet.AppendLine("""</t></is></c>""");
            sheet.AppendLine("""    </row>""");
            rowIndex++;
        }

        sheet.AppendLine("""  </sheetData>""");
        sheet.AppendLine("""</worksheet>""");
        WriteZipEntry(archive, "xl/worksheets/sheet1.xml", sheet.ToString());
    }

    private static void WriteZipEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string SecurityElementEscape(string value)
    {
        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);
    }

    private static async Task<int> RunLuban(string scriptDir, string dataOut, string codeOut, string template, string? l10nTextFile)
    {
        var args = new List<string>
        {
            Path.Combine(scriptDir, "Tools", "Luban.dll"),
            "-t", "client",
            "-c", "cs-bin",
            "-d", "bin",
            "--conf", Path.Combine(scriptDir, "luban.conf"),
            "--customTemplateDir", ResolveTemplateDir(scriptDir, template),
            "-x", $"outputDataDir={dataOut}",
            "-x", $"outputCodeDir={codeOut}",
        };

        if (!string.IsNullOrEmpty(l10nTextFile))
        {
            args.AddRange([
                "-x", "l10n.provider=default",
                "-x", $"l10n.textFile.path={ToLubanInputPath(scriptDir, l10nTextFile)}",
                "-x", "l10n.textFile.keyFieldName=key",
                "-x", "l10n.textListFile=texts.txt",
            ]);
        }

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

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("启动 Luban 失败。");
        var stdoutTask = StreamProcessOutput(process.StandardOutput, Console.Out, "配置表");
        var stderrTask = StreamProcessOutput(process.StandardError, Console.Error, "配置表");
        await process.WaitForExitAsync();
        await Task.WhenAll(stdoutTask, stderrTask);
        Console.WriteLine($"信息：配置表生成进程退出码：{process.ExitCode}");
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

        var tableDataOut = paths.GetValueOrDefault("table_data_out") ?? paths.GetValueOrDefault("data_out") ?? "../Client/Assets/Bundles/Configs/bytes/";
        var codeOut = paths["code_out"] ?? throw new InvalidOperationException("缺少必需配置：paths.code_out。");
        var l10nEnabled = ParseBoolOption(paths.GetValueOrDefault("l10n_enabled"), true);
        var l10nTextRows = new List<RowData>();

        if (l10nEnabled)
        {
            var l10nProviderLanguages = ReadConfiguredLanguages(cfg, paths);
            var l10nTextXlsx = paths.GetValueOrDefault("l10n_text_xlsx") ?? "./Excels/Localization";
            var l10nTextFilePattern = paths.GetValueOrDefault("l10n_text_file_pattern") ?? "^Localization(?!Const$).*$";
            var l10nTextFiles = ResolveLocalizationTextFiles(scriptDir, l10nTextXlsx, l10nTextFilePattern);
            l10nTextRows = LoadLocalizationTextRows(l10nTextFiles, l10nProviderLanguages);
        }

        var lubanL10nTextFile = l10nEnabled
            ? WriteLubanLocalizationTextFile(scriptDir, l10nTextRows)
            : null;

        int lubanExitCode;
        try
        {
            lubanExitCode = await RunLuban(scriptDir, tableDataOut, codeOut, template, lubanL10nTextFile);
        }
        finally
        {
            if (!string.IsNullOrEmpty(lubanL10nTextFile))
            {
                RemoveFileIfExists(lubanL10nTextFile);
                RemoveDirectoryIfEmpty(Path.GetDirectoryName(lubanL10nTextFile) ?? "");
            }
        }

        if (lubanExitCode != 0)
        {
            return 1;
        }

        AddUtf8BomToCsFiles(ResolvePath(scriptDir, codeOut));

        if (!l10nEnabled)
        {
            Console.WriteLine("信息：paths.l10n_enabled 已关闭，多语言导出已跳过。");
            return 0;
        }

        var languages = ReadConfiguredLanguages(cfg, paths);
        var l10nTextOut = paths.GetValueOrDefault("l10n_text_out") ?? paths.GetValueOrDefault("data_out") ?? tableDataOut;
        var l10nConstOut = paths.GetValueOrDefault("l10n_const_out") ?? paths.GetValueOrDefault("data_out") ?? tableDataOut;
        var l10nConstXlsx = paths.GetValueOrDefault("l10n_const_xlsx") ?? paths.GetValueOrDefault("l10n_xlsx") ?? "./Excels/Localization/LocalizationConst.xlsx";
        var l10nEditor = paths.TryGetValue("l10n_editor_out", out var configuredL10nEditor)
            ? configuredL10nEditor
            : "../Client/Assets/Editor/Config";
        var l10nKeyCodeOut = paths.GetValueOrDefault("l10n_key_code_out") ?? DefaultLocalizationKeyCodeOut;
        var l10nKeyCommentLanguage = paths.GetValueOrDefault("l10n_key_comment_language") ?? paths.GetValueOrDefault("l10n_comment_language") ?? DefaultLocalizationKeyCommentLanguage;
        var l10nConstOutRule = paths.GetValueOrDefault("l10n_const_out_rule") ?? "";

        var localizationConstSource = Path.Combine(scriptDir, "CustomeTools", "LocalizationConst");
        var localizationConstTarget = Path.Combine(Path.GetDirectoryName(ResolvePath(scriptDir, codeOut)) ?? scriptDir, "LocalizationConst");
        CopyDirectoryWithoutMeta(localizationConstSource, localizationConstTarget);
        Console.WriteLine($"成功：已复制多语言常量代码：{localizationConstTarget}");

        var constSheetRows = LoadWorkbookRows(ResolvePath(scriptDir, l10nConstXlsx), ConstSheetNames, true, languages);
        var constRows = MergeRows(constSheetRows, "LocalizationConst", true);
        var exportedLanguages = new List<string>();
        for (var index = 0; index < languages.Count; index++)
        {
            var language = languages[index];
            var exportedText = ExportLocalizationTextRows(l10nTextRows, language, ResolvePath(scriptDir, l10nTextOut));
            var exportedConst = ExportLocalizationConstRows(constRows, language, ResolvePath(scriptDir, l10nConstOut));
            if (exportedText || exportedConst)
            {
                exportedLanguages.Add(language);
            }
        }

        if (string.IsNullOrWhiteSpace(l10nEditor))
        {
            WriteWarning("警告：l10n_editor_out 为空，已跳过多语言常量编辑器 JSON 导出。");
        }
        else
        {
            var editorJsonPath = Path.Combine(ResolvePath(scriptDir, l10nEditor), EditorJsonName);
            if (constRows.Count > 0 && exportedLanguages.Count > 0)
            {
                WriteEditorJson(editorJsonPath, constRows, exportedLanguages);
                Console.WriteLine($"成功：已生成编辑器 JSON：{editorJsonPath}");
            }
            else
            {
                RemoveFileIfExists(editorJsonPath);
                WriteWarning("警告：所有已配置语言都没有多语言常量数据，未导出编辑器 JSON。");
            }
        }

        ExportLocalizationKey(
            ResolvePath(scriptDir, l10nConstXlsx),
            ResolvePath(scriptDir, l10nKeyCodeOut),
            l10nKeyCommentLanguage,
            l10nConstOutRule,
            languages);

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

    private static bool ParseBoolOption(string? value, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "1" or "true" or "yes" or "on" or "enable" or "enabled" => true,
            "0" or "false" or "no" or "off" or "disable" or "disabled" => false,
            _ => throw new InvalidOperationException($"布尔配置值无效：`{value}`。"),
        };
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
                : throw new InvalidOperationException($"缺少 ini 配置段 `{name}`。");
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
            var entry = _archive.GetEntry(sheet.Path) ?? throw new InvalidOperationException($"xlsx 文件缺少条目：{sheet.Path}");
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
            var workbookEntry = archive.GetEntry("xl/workbook.xml") ?? throw new InvalidOperationException("xlsx 文件缺少条目：xl/workbook.xml");
            var relsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels") ?? throw new InvalidOperationException("xlsx 文件缺少条目：xl/_rels/workbook.xml.rels");

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
