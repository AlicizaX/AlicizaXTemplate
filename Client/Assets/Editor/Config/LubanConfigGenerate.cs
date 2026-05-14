using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using AlicizaX.Editor.Extension;
using UnityEngine.UI;

public static class LubanConfigGenerate
{
    [InitializeOnLoadMethod]
    private static void InitializeUXTextMeshPro()
    {
        LocalizationRefreshHelper.ResolveLocalizationConstPath =
            () => "Assets/Editor/Config/LocalizationConst.json";
    }

    static string ConfigPath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Config"));

    [EditorToolFunction("Gen-Config")]
    static void Gen_Config()
    {
        int choice = EditorUtility.DisplayDialogComplex(
            "Gen Config",
            "Select generation mode:",
            "Normal", "Cancel", "Lazy");
        if (choice == 1) return;

        string template = choice == 2 ? "ClientTemplateLazy" : "ClientTemplate";
        RunScript("gen-all", template);
    }

    [EditorToolFunction("Open-LocalizationConfig")]
    static void Open_LocalizationConfig()
    {
        string localizationPath = Path.Combine(ConfigPath, "Excels", "Localization/");
        EditorUtility.RevealInFinder(localizationPath);
    }

    static void RunScript(string name, string template)
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (isWindows)
            Execute(Path.Combine(ConfigPath, name + ".bat"), template);
        else
            Execute("bash", $"\"{Path.Combine(ConfigPath, name + ".sh")}\" {template}");
    }

    static void Execute(string fileName, string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = ConfigPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();

                if (process.ExitCode == 0)
                    UnityEngine.Debug.Log($"Executed: {fileName} {arguments}");
                else
                    UnityEngine.Debug.LogError($"Execute failed: {fileName} {arguments}\nExitCode: {process.ExitCode}");

                AssetDatabase.Refresh();
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Execute failed: {fileName} {arguments}\n{e}");
        }
    }
}
