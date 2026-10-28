using Luban;
using Luban.L10N;
using Luban.OutputSaver;
using Luban.Pipeline;
using System.Text.RegularExpressions;

namespace AlicizaX.LubanLocalization;

[Pipeline("alicizax-localization")]
public sealed class LocalizationPipeline : IPipeline
{
    public void Run(PipelineArguments args)
    {
        string configPath = EnvManager.Current.GetOptionOrDefaultRaw("alicizax.config", "config.ini");
        var ini = new IniConfig(configPath);
        LocalizationProject? project = LocalizationProject.Load(ini);
        Dictionary<string, byte[]> localization = project == null
            ? new Dictionary<string, byte[]>() : LocalizationOutputs.Create(project);
        bool check = EnvManager.Current.GetBoolOptionOrDefault("alicizax", "check", false, false);
        bool validationOnly = EnvManager.Current.GetBoolOptionOrDefault("alicizax", "validationOnly", false, false);
        if (EnvManager.Current.GetOptionOrDefaultRaw("outputSaver", "") != "alicizax-stage")
            throw new InvalidOperationException("The alicizax-localization pipeline requires -x outputSaver=alicizax-stage.");

        using var stage = new GenerationStage();
        GenerationStage.Current = stage;
        GenerationStage.Project = project;
        try
        {
            new DefaultPipeline().Run(args);
            if (GenerationContext.Current.AnyValidatorFail)
                throw new InvalidOperationException("Luban validation failed; generated files were not changed.");
            if (validationOnly) return;

            string codeOut = ini.Resolve(ini.Get("paths", "code_out", true));
            string dataOut = ini.Resolve(ini.Get("paths", "table_data_out", true));
            var outputs = new Dictionary<string, byte[]>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            AddDirectory(outputs, stage.CodeDirectory, codeOut);
            AddDirectory(outputs, stage.DataDirectory, dataOut);
            foreach ((string path, byte[] bytes) in localization)
                if (!outputs.TryAdd(path, bytes)) throw new InvalidOperationException($"Output collision: {path}");

            var obsolete = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            FindObsolete(obsolete, codeOut, stage.CodeDirectory, "*.cs");
            FindObsolete(obsolete, dataOut, stage.DataDirectory, "*.bytes");
            if (project != null) FindObsoleteLocalization(obsolete, project, localization);

            if (check)
            {
                string[] changed = outputs.Where(pair => !File.Exists(pair.Key) || !File.ReadAllBytes(pair.Key).AsSpan().SequenceEqual(pair.Value))
                    .Select(pair => pair.Key).Concat(obsolete).Order(StringComparer.Ordinal).ToArray();
                if (changed.Length != 0) throw new InvalidOperationException("Generated files are out of date:\n" + string.Join("\n", changed));
                Console.WriteLine($"Checked {outputs.Count} generated files.");
                return;
            }

            foreach ((string path, byte[] bytes) in outputs.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                if (File.Exists(path) && File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes)) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, bytes);
            }
            foreach (string path in obsolete)
            {
                File.Delete(path);
                if (File.Exists(path + ".meta")) File.Delete(path + ".meta");
            }
            Console.WriteLine($"Generated {outputs.Count} files ({localization.Count} localization files).");
        }
        finally
        {
            GenerationStage.Project = null;
            GenerationStage.Current = null;
        }
    }

    private static void AddDirectory(Dictionary<string, byte[]> outputs, string source, string destination)
    {
        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string path = Path.Combine(destination, Path.GetRelativePath(source, file));
            if (!outputs.TryAdd(path, File.ReadAllBytes(file))) throw new InvalidOperationException($"Output collision: {path}");
        }
    }

    private static void FindObsolete(HashSet<string> obsolete, string destination, string source, string pattern)
    {
        if (!Directory.Exists(destination)) return;
        foreach (string file in Directory.GetFiles(destination, pattern, SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(destination, file);
            if (!File.Exists(Path.Combine(source, relative))) obsolete.Add(file);
        }
    }

    private static void FindObsoleteLocalization(HashSet<string> obsolete, LocalizationProject project,
        Dictionary<string, byte[]> expected)
    {
        foreach (string template in project.DataTemplates)
        {
            string prefix = template[..template.IndexOf("{locale}", StringComparison.Ordinal)];
            string directory = Directory.Exists(prefix) ? prefix : Path.GetDirectoryName(prefix)!;
            if (!Directory.Exists(directory)) continue;
            string relativeTemplate = Path.GetRelativePath(directory, template).Replace('\\', '/');
            var pattern = new Regex("^" + Regex.Escape(relativeTemplate).Replace(Regex.Escape("{locale}"), "[^/]+") + "$",
                OperatingSystem.IsWindows() ? RegexOptions.IgnoreCase : RegexOptions.None);
            foreach (string file in Directory.GetFiles(directory, "*.bytes", SearchOption.AllDirectories))
                if (pattern.IsMatch(Path.GetRelativePath(directory, file).Replace('\\', '/')) && !expected.ContainsKey(file))
                    obsolete.Add(file);
        }
        foreach (string code in project.Rows.Where(row => row.CodeOut.Length > 0).Select(row => row.CodeOut).Distinct())
            if (File.Exists(code) && !expected.ContainsKey(code)) obsolete.Add(code);
    }
}

[TextProvider("alicizax")]
public sealed class LocalizationTextProvider : ITextProvider
{
    public bool ConvertTextKeyToValue => false;
    public void Load() { }
    public void ProcessDatas() { }
    public bool IsValidKey(string key) => GenerationStage.Project?.Keys.Contains(key) ?? true;
    public bool TryGetText(string key, out string text) { text = key; return false; }
    public void AddUnknownKey(string key) { }
}

[OutputSaver("alicizax-stage")]
public sealed class LocalizationStageSaver : LocalFileSaver
{
    protected override string GetOutputDir(OutputFileManifest manifest)
    {
        GenerationStage stage = GenerationStage.Current
            ?? throw new InvalidOperationException("The localization stage was not initialized.");
        return manifest.OutputType == OutputType.Code ? stage.CodeDirectory : stage.DataDirectory;
    }
}

internal sealed class GenerationStage : IDisposable
{
    internal static GenerationStage? Current;
    internal static LocalizationProject? Project;

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "AlicizaXLuban", Guid.NewGuid().ToString("N"));
    internal string CodeDirectory => Path.Combine(_directory, "code");
    internal string DataDirectory => Path.Combine(_directory, "data");

    internal GenerationStage()
    {
        Directory.CreateDirectory(CodeDirectory);
        Directory.CreateDirectory(DataDirectory);
    }

    public void Dispose() => Directory.Delete(_directory, true);
}
