using System.Text;
using System.Text.Json;
using Luban.Serialization;

namespace AlicizaX.LubanLocalization;

internal static class LocalizationOutputs
{
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    private static readonly UTF8Encoding Utf8 = new(false);

    internal static Dictionary<string, byte[]> Create(LocalizationProject project)
    {
        var outputs = new Dictionary<string, byte[]>(PathComparer);
        foreach (var group in project.Rows.GroupBy(row => row.DataOut, PathComparer).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            TextRow[] rows = group.OrderBy(row => row.Key, StringComparer.Ordinal).ToArray();
            foreach (string locale in project.Languages)
            {
                var buffer = new ByteBuf();
                buffer.WriteSize(rows.Length);
                foreach (TextRow row in rows)
                {
                    buffer.WriteString(row.Key);
                    string? value = row.Values[locale];
                    buffer.WriteBool(value != null);
                    if (value != null) buffer.WriteString(value);
                }
                Add(outputs, group.Key.Replace("{locale}", locale, StringComparison.Ordinal), buffer.CopyData());
            }
        }
        var classNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in project.Rows.Where(row => row.IsConstant && row.CodeOut.Length > 0)
                     .GroupBy(row => row.CodeOut, PathComparer).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            string className = Path.GetFileNameWithoutExtension(group.Key);
            if (!IsIdentifier(className)) throw new FormatException($"code_out file name is not a C# class name: {group.Key}");
            if (!classNames.Add(className)) throw new InvalidOperationException($"Constant class name is used by multiple code_out files: {className}");
            Add(outputs, group.Key, Utf8.GetBytes(Accessor(className, group)));
        }
        string optionsDirectory = Path.GetDirectoryName(project.RuntimeOptionsOut)!;
        string[] catalogs = project.DataTemplates.Select(template => Path.GetRelativePath(optionsDirectory, template))
            .Select(path => Path.IsPathRooted(path)
                ? throw new FormatException($"[localization] runtime_options_out and data_out must be on the same volume: {path}")
                : path.Replace('\\', '/')).Order(StringComparer.Ordinal).ToArray();
        string json = JsonSerializer.Serialize(new
        {
            Version = 1,
            Locales = project.Languages,
            DefaultLocale = project.DefaultLocale,
            Catalogs = catalogs,
        }, new JsonSerializerOptions { WriteIndented = true }).ReplaceLineEndings("\n") + "\n";
        Add(outputs, project.RuntimeOptionsOut, Utf8.GetBytes(json));
        return outputs;
    }

    private static void Add(Dictionary<string, byte[]> outputs, string path, byte[] content)
    {
        if (!outputs.TryAdd(path, content)) throw new InvalidOperationException($"Localization output path collision: {path}");
    }

    private static string Accessor(string className, IEnumerable<TextRow> rows)
    {
        var output = new StringBuilder("using AlicizaX.Localization.Runtime;\n\n");
        output.Append("public static class ").Append(className).AppendLine().AppendLine("{");
        var classNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in rows.OrderBy(row => row.Key, StringComparer.Ordinal)
                     .GroupBy(row => row.Key.Contains('.') ? row.Key[..row.Key.IndexOf('.')] : "Common", StringComparer.Ordinal))
        {
            string groupName = Identifier(group.Key);
            if (!classNames.Add(groupName)) throw new InvalidOperationException($"Constant class name collision: {group.Key}");
            output.Append("    public static class ").Append(groupName).AppendLine().AppendLine("    {");
            var members = new HashSet<string>(StringComparer.Ordinal);
            foreach (TextRow row in group)
            {
                string remainder = row.Key.Contains('.') ? row.Key[(row.Key.IndexOf('.') + 1)..] : row.Key;
                string name = Identifier(remainder.Replace('.', '_')).ToUpperInvariant();
                if (!members.Add(name)) throw new InvalidOperationException($"Constant member name collision: {row.Key}");
                output.Append("        public const string ").Append(name).Append("_Raw = ")
                    .Append(JsonSerializer.Serialize(row.Key)).AppendLine(";");
                if (row.ArgumentCount == 0)
                {
                    output.Append("        public static string ").Append(name).Append(" => LocalizationKit.Get(")
                        .Append(name).AppendLine("_Raw);");
                    continue;
                }
                output.Append("        public static string ").Append(name).Append('<')
                    .Append(string.Join(", ", Enumerable.Range(0, row.ArgumentCount).Select(i => "T" + i))).Append(">(")
                    .Append(string.Join(", ", Enumerable.Range(0, row.ArgumentCount).Select(i => "T" + i + " arg" + i)))
                    .Append(") => LocalizationKit.Format(").Append(name).Append("_Raw, ")
                    .Append(string.Join(", ", Enumerable.Range(0, row.ArgumentCount).Select(i => "arg" + i))).AppendLine(");");
            }
            output.AppendLine("    }");
        }
        output.AppendLine("}");
        return output.ToString().ReplaceLineEndings("\n");
    }

    private static bool IsIdentifier(string value) => value.Length > 0 && (char.IsLetter(value[0]) || value[0] == '_')
        && value.Skip(1).All(character => char.IsLetterOrDigit(character) || character == '_');

    private static string Identifier(string source)
    {
        var result = new StringBuilder();
        foreach (char character in source) result.Append(char.IsLetterOrDigit(character) || character == '_' ? character : '_');
        if (result.Length == 0 || char.IsDigit(result[0])) result.Insert(0, '_');
        return result.ToString();
    }
}
