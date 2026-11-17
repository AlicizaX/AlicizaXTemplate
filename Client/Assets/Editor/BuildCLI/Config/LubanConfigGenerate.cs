using System;
using System.Diagnostics;
using System.IO;
using AlicizaX.Editor.Toolbar;
using UnityEditor;
using UnityEngine;

public static class LubanConfigGenerate
{
    // Hardcoded Luban config folder: <AlicizaXTemplate>/Config, relative to the Unity project folder (<AlicizaXTemplate>/Client).
    private const string ConfigRelativeRoot = "../../Config";

    private static string ConfigRoot
    {
        get => Path.GetFullPath(Path.Combine(Application.dataPath, ConfigRelativeRoot));
    }

    [EditorToolFunction("Gen-Config")]
    public static void GenerateConfig()
    {
        RunConfigScript();
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Luban config generation completed.");
    }
    
    private static void RunConfigScript()
    {
        string root = ConfigRoot;
        bool windows = Application.platform == RuntimePlatform.WindowsEditor;
        string script = Path.Combine(root, windows ? "gen-all.bat" : "gen-all.command");
        if (!File.Exists(script)) throw new FileNotFoundException("gen-all script not found.", script);
        var start = new ProcessStartInfo(windows ? "cmd.exe" : "/bin/bash",
            windows ? "/d /s /c \"\"" + script + "\"" + "\""
                : "\"" + script + "\"")
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
