using System;
using System.Diagnostics;
using System.IO;
using AlicizaX.Editor.Toolbar;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class LubanConfigGenerate
{
    private const string ConfigPathPrefsKey = "AlicizaX.Config.Path";

    private static string ConfigPath
    {
        get
        {
            string selected = EditorPrefs.GetString(ConfigPathPrefsKey, string.Empty);
            return string.IsNullOrEmpty(selected)
                ? Path.GetFullPath(Path.Combine(Application.dataPath, "../../Config/config.ini"))
                : selected;
        }
    }

    private static string ConfigRoot
    {
        get => Path.GetDirectoryName(ConfigPath);
    }

    [MenuItem("AlicizaX/Config/Select Config")]
    public static void SelectConfig()
    {
        string selected = EditorUtility.OpenFilePanel("Select Config/config.ini", ConfigRoot, "ini");
        if (string.IsNullOrEmpty(selected)) return;
        if (!Path.GetFileName(selected).Equals("config.ini", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Select the Luban config.ini file.");
        EditorPrefs.SetString(ConfigPathPrefsKey, Path.GetFullPath(selected));
    }

    [MenuItem("AlicizaX/Config/Generate Config")]
    [EditorToolFunction("Gen-Config")]
    public static void GenerateConfig()
    {
        RunConfigScript(false);
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Luban config generation completed.");
    }

    [MenuItem("AlicizaX/Config/Check Generated Files")]
    public static void CheckGeneratedFiles()
    {
        if (!File.Exists(ConfigPath)) return;
        RunConfigScript(true);
    }

    private static void RunConfigScript(bool check)
    {
        string root = ConfigRoot;
        bool windows = Application.platform == RuntimePlatform.WindowsEditor;
        string script = Path.Combine(root, windows ? "gen-all.bat" : "gen-all.sh");
        if (!File.Exists(script)) throw new FileNotFoundException("gen-all script not found.", script);
        var start = new ProcessStartInfo(windows ? "cmd.exe" : "/bin/bash",
            windows ? "/d /s /c \"\"" + script + "\"" + (check ? " --check" : "") + "\""
                : "\"" + script + "\"" + (check ? " --check" : ""))
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        RunProcess(start, "gen-all");
    }

    private static void RunProcess(ProcessStartInfo start, string label)
    {
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start " + label + ".");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        string output = stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult();
        if (process.ExitCode != 0) throw new InvalidOperationException(label + " failed:\n" + output);
        if (output.Length != 0) UnityEngine.Debug.Log(output);
    }
}

internal sealed class LubanConfigBuildCheck : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report) => LubanConfigGenerate.CheckGeneratedFiles();
}
