namespace AlicizaX.LubanLocalization;

internal sealed class IniConfig
{
    private readonly Dictionary<string, Dictionary<string, string>> _sections = new(StringComparer.OrdinalIgnoreCase);

    internal string DirectoryPath { get; }
    internal IEnumerable<KeyValuePair<string, Dictionary<string, string>>> Sections => _sections;

    internal IniConfig(string path)
    {
        path = Path.GetFullPath(path);
        DirectoryPath = Path.GetDirectoryName(path)!;
        Dictionary<string, string>? section = null;
        int number = 0;
        foreach (string raw in File.ReadAllLines(path))
        {
            number++;
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#')) continue;
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                string name = line[1..^1].Trim();
                if (name.Length == 0 || !_sections.TryAdd(name, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)))
                    throw new FormatException($"Invalid or repeated config section at {path}:{number}: {line}");
                section = _sections[name];
                continue;
            }
            int equals = line.IndexOf('=');
            if (section == null || equals < 1 || !section.TryAdd(line[..equals].Trim(), line[(equals + 1)..].Trim()))
                throw new FormatException($"Invalid or repeated config key at {path}:{number}: {line}");
        }
    }

    internal bool HasSection(string name) => _sections.ContainsKey(name);

    internal string Get(string section, string key, bool required = false)
    {
        string value = _sections.TryGetValue(section, out var values) && values.TryGetValue(key, out string? found)
            ? found : string.Empty;
        if (required && value.Length == 0) throw new FormatException($"Missing [{section}] {key} in config.ini.");
        return value;
    }

    internal string Resolve(string value) => Path.GetFullPath(Path.Combine(DirectoryPath, value));
}
