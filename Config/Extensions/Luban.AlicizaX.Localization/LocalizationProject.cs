using System.Globalization;

namespace AlicizaX.LubanLocalization;

internal sealed record TextRow(string Key, string Source, bool IsConstant, string DataOut, string CodeOut,
    IReadOnlyDictionary<string, string?> Values, int ArgumentCount);

internal sealed class LocalizationProject
{
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    internal string[] Languages { get; }
    internal string DefaultLocale { get; }
    internal string RuntimeOptionsOut { get; }
    internal IReadOnlyList<TextRow> Rows { get; }
    internal HashSet<string> Keys { get; }
    internal IReadOnlyList<string> DataTemplates { get; }

    private LocalizationProject(string[] languages, string defaultLocale, string runtimeOptionsOut,
        List<TextRow> rows, List<string> dataTemplates)
    {
        Languages = languages;
        DefaultLocale = defaultLocale;
        RuntimeOptionsOut = runtimeOptionsOut;
        Rows = rows;
        Keys = rows.Select(row => row.Key).ToHashSet(StringComparer.Ordinal);
        DataTemplates = dataTemplates;
    }

    internal static LocalizationProject? Load(IniConfig ini)
    {
        if (!ini.HasSection("localization")) return null;
        string[] languages = ini.Get("localization", "languages", true)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (languages.Length == 0 || languages.Distinct(StringComparer.Ordinal).Count() != languages.Length)
            throw new FormatException("[localization] languages must contain unique names.");
        string defaultLocale = ini.Get("localization", "default_locale", true);
        if (!languages.Contains(defaultLocale, StringComparer.Ordinal))
            throw new FormatException("[localization] default_locale is not in languages.");
        string runtimeOptionsOut = ini.Resolve(ini.Get("localization", "runtime_options_out", true));
        if (!runtimeOptionsOut.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("[localization] runtime_options_out must name a .json file.");
        var excluded = ini.Get("localization", "exclude_constant_prefixes")
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);

        var candidates = new Dictionary<string, List<Rule>>(PathComparer);
        foreach (var entry in ini.Sections.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            string[] parts = entry.Key.Split('.', 3);
            if (parts.Length != 3 || parts[0] is not ("constant" or "ordinary") || parts[1] is not ("dir" or "file"))
                continue;
            string section = entry.Key;
            string path = ini.Resolve(ini.Get(section, "path", true));
            string dataOut = ini.Resolve(ini.Get(section, "data_out", true));
            if (!dataOut.Contains("{locale}", StringComparison.Ordinal) || !dataOut.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase))
                throw new FormatException($"[{section}] data_out must contain {{locale}} and end in .bytes.");
            string codeValue = ini.Get(section, "code_out");
            if (parts[0] == "ordinary" && codeValue.Length > 0)
                throw new FormatException($"[{section}] ordinary tables cannot have code_out.");
            string codeOut = codeValue.Length == 0 ? string.Empty : ini.Resolve(codeValue);
            if (codeOut.Length > 0 && !codeOut.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                throw new FormatException($"[{section}] code_out must name a .cs file.");
            string prefix = parts[1] == "dir" ? ini.Get(section, "prefix", true) : string.Empty;
            var rule = new Rule(section, parts[0] == "constant", parts[1] == "file", path, prefix, dataOut, codeOut);
            IEnumerable<string> files;
            if (rule.IsFile)
            {
                if (!File.Exists(path)) throw new FileNotFoundException($"[{section}] workbook not found.", path);
                files = new[] { path };
            }
            else
            {
                if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"[{section}] directory not found: {path}");
                files = Directory.GetFiles(path, "*.xlsx", SearchOption.TopDirectoryOnly)
                    .Where(file => Path.GetFileName(file).StartsWith(prefix, StringComparison.Ordinal)
                                   && !Path.GetFileName(file).StartsWith("~$", StringComparison.Ordinal));
            }
            foreach (string file in files)
            {
                if (!candidates.TryGetValue(file, out var matches)) candidates[file] = matches = new List<Rule>();
                matches.Add(rule);
            }
        }
        if (candidates.Count == 0) throw new InvalidOperationException("[localization] has no matching workbooks.");

        var rows = new List<TextRow>();
        var seen = new Dictionary<string, string>(StringComparer.Ordinal);
        var dataTemplates = new HashSet<string>(PathComparer);
        foreach (var candidate in candidates.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            Rule[] fileRules = candidate.Value.Where(rule => rule.IsFile).ToArray();
            Rule[] available = fileRules.Length > 0 ? fileRules : candidate.Value.ToArray();
            if (available.Length != 1)
                throw new InvalidOperationException($"Workbook matches multiple { (fileRules.Length > 0 ? "file" : "directory") } rules: {candidate.Key} ({string.Join(", ", available.Select(rule => rule.Section))}).");
            Rule rule = available[0];
            dataTemplates.Add(rule.DataOut);
            foreach (TextRow row in ReadRows(candidate.Key, rule, languages, excluded))
            {
                if (seen.TryGetValue(row.Key, out string? previous))
                    throw new InvalidOperationException($"Duplicate localization Key '{row.Key}': {previous} and {row.Source}.");
                seen.Add(row.Key, row.Source);
                rows.Add(row);
            }
        }
        if (rows.Count == 0) throw new InvalidOperationException("[localization] workbooks contain no text rows.");
        return new LocalizationProject(languages, defaultLocale, runtimeOptionsOut,
            rows, dataTemplates.Order(StringComparer.Ordinal).ToList());
    }

    private static IEnumerable<TextRow> ReadRows(string path, Rule rule, string[] languages, HashSet<string> excluded)
    {
        List<Dictionary<int, string?>> sheet = Workbook.ReadTexts(path);
        if (sheet.Count < 2 || Cell(sheet[0], 0) != "##var" || Cell(sheet[1], 0) != "##type")
            throw new FormatException($"Missing Texts ##var or ##type header: {path}");
        int keyColumn = Column(sheet[0], "key", path);
        if (sheet[0].Values.Contains("layer")) throw new FormatException($"Remove the obsolete layer column: {path}");
        var columns = languages.ToDictionary(language => language, language => Column(sheet[0], language, path), StringComparer.Ordinal);
        for (int i = 2; i < sheet.Count; i++)
        {
            var row = sheet[i];
            if (Cell(row, 0)?.StartsWith("##", StringComparison.Ordinal) == true) continue;
            string key = Cell(row, keyColumn)?.Trim() ?? string.Empty;
            if (key.Length == 0) continue;
            var values = new Dictionary<string, string?>(StringComparer.Ordinal);
            int? argumentCount = null;
            string? firstSignature = null;
            foreach ((string locale, int column) in columns)
            {
                string? value = Cell(row, column);
                if (value == "\"\"") value = string.Empty;
                values.Add(locale, value);
                if (value == null) continue;
                int count = FormatArgumentCount(value, $"{path}:{i + 1} ({key}, {locale})", out string signature);
                if (argumentCount is not null && argumentCount != count)
                    throw new FormatException($"Format arguments differ across languages for '{key}' at {path}:{i + 1}.");
                if (firstSignature != null && firstSignature != signature)
                    throw new FormatException($"Format arguments differ across languages for '{key}' at {path}:{i + 1}.");
                argumentCount = count;
                firstSignature ??= signature;
            }
            bool isConstant = rule.IsConstant && !excluded.Contains(key.Split('.', 2)[0]);
            yield return new TextRow(key, $"{path}:{i + 1}", isConstant, rule.DataOut, rule.CodeOut, values, argumentCount ?? 0);
        }
    }

    private static int FormatArgumentCount(string text, string source, out string signature)
    {
        var used = new HashSet<int>();
        object[] args = Enumerable.Range(0, 257).Select(index => (object)new FormatMarker(index, used)).ToArray();
        try { _ = string.Format(CultureInfo.InvariantCulture, text, args); }
        catch (FormatException error) { throw new FormatException($"Invalid format string at {source}: {error.Message}", error); }
        int[] indexes = used.Order().ToArray();
        signature = string.Join(',', indexes);
        return indexes.Length == 0 ? 0 : indexes[^1] + 1;
    }

    private sealed class FormatMarker(int index, HashSet<int> used) : IFormattable
    {
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            used.Add(index);
            return "x";
        }
    }

    private static int Column(Dictionary<int, string?> header, string name, string path)
    {
        foreach ((int index, string? value) in header) if (value == name) return index;
        throw new FormatException($"Missing {name} column in {path}.");
    }

    private static string? Cell(Dictionary<int, string?> row, int column) => row.TryGetValue(column, out string? value) ? value : null;

    private sealed record Rule(string Section, bool IsConstant, bool IsFile, string Path, string Prefix, string DataOut, string CodeOut);
}
