namespace AlicizaX.LubanLocalization;

internal sealed record TextRow(string Key, IReadOnlyDictionary<string, string?> Values);

internal sealed class LocalizationProject
{
    internal required string[] Languages { get; init; }
    internal required string OutputDirectory { get; init; }
    internal required List<TextRow> Rows { get; init; }
    internal required HashSet<string> Keys { get; init; }

    internal static LocalizationProject? Load(IniConfig ini)
    {
        if (!ini.HasSection("localization")) return null;
        string[] languages = ini.Get("localization", "languages", true).Split(',', StringSplitOptions.TrimEntries);
        if (languages.Any(string.IsNullOrEmpty) || languages.Distinct(StringComparer.Ordinal).Count() != languages.Length)
            throw new FormatException("Localization languages must be nonempty and unique.");
        string directory = ini.Resolve(ini.Get("localization", "path", true));
        string ordinary = ini.Resolve(ini.Get("paths", "table_data_out", true));
        var rows = new List<TextRow>();
        var keys = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string path in Directory.GetFiles(directory, "*.xlsx").Where(p => !Path.GetFileName(p).StartsWith("~$", StringComparison.Ordinal)).Order(StringComparer.Ordinal))
        {
            var sheet = Workbook.ReadTexts(path);
            var header = sheet.FirstOrDefault(row => row.GetValueOrDefault(0) == "##var")
                ?? throw new FormatException("Missing ##var row: " + path);
            int Column(string name) => header.FirstOrDefault(p => p.Value == name, new KeyValuePair<int, string?>(-1, null)).Key is int column && column >= 0
                ? column : throw new FormatException($"Missing {name} column: {path}");
            int keyColumn = Column("key");
            var columns = languages.ToDictionary(locale => locale, Column, StringComparer.Ordinal);
            for (int i = 0; i < sheet.Count; i++)
            {
                var row = sheet[i];
                if (row.GetValueOrDefault(0)?.StartsWith("##", StringComparison.Ordinal) == true) continue;
                string key = row.GetValueOrDefault(keyColumn)?.Trim() ?? string.Empty;
                if (key.Length == 0) continue;
                string source = $"{path}:{i + 1}";
                if (!keys.TryAdd(key, source)) throw new FormatException($"Duplicate key '{key}': {keys[key]} / {source}");
                var values = new Dictionary<string, string?>(StringComparer.Ordinal);
                foreach (var pair in columns)
                {
                    string? value = row.GetValueOrDefault(pair.Value);
                    values.Add(pair.Key, value == "\"\"" ? string.Empty : value);
                }
                rows.Add(new TextRow(key, values));
            }
        }
        return new LocalizationProject
        {
            Languages = languages,
            OutputDirectory = Path.Combine(Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(ordinary))!, "localization"),
            Rows = rows,
            Keys = keys.Keys.ToHashSet(StringComparer.Ordinal)
        };
    }
}
