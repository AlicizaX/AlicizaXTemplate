using Luban.Serialization;

namespace AlicizaX.LubanLocalization;

internal static class LocalizationOutputs
{
    internal static Dictionary<string, byte[]> Create(LocalizationProject project)
    {
        var outputs = new Dictionary<string, byte[]>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var rows = project.Rows.OrderBy(row => row.Key, StringComparer.Ordinal).ToArray();
        foreach (string locale in project.Languages)
        {
            var buffer = new ByteBuf();
            buffer.WriteSize(rows.Length);
            foreach (var row in rows)
            {
                buffer.WriteString(row.Key);
                string? value = row.Values[locale];
                buffer.WriteBool(value != null);
                if (value != null) buffer.WriteString(value);
            }
            outputs.Add(Path.Combine(project.OutputDirectory, $"Game_{locale}.bytes"), buffer.CopyData());
        }
        return outputs;
    }
}
