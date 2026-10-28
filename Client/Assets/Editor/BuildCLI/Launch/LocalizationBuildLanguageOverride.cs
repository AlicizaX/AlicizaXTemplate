#if ALICIZAX_LOCALIZATION
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

[InitializeOnLoad]
internal static class LocalizationBuildLanguageOverride
{
    internal const string Define = "ALICIZAX_BUILD_LOCALE_FILE";
    internal const string StreamingAssetsPath = "AlicizaX/Localization/build-locale.txt";
    private const string LanguageKey = "localization.language";
    private static string _activeTempPath;

    internal static string ActiveTempPath => _activeTempPath;

    static LocalizationBuildLanguageOverride() => BuildLaunchContext.RegisterBuildOverride(Apply);

    internal static void ValidateLanguage(string language)
    {
        if (string.IsNullOrEmpty(language)) return;

        string optionsPath = Path.Combine(Application.dataPath, "Resources", "Localization", "RuntimeOptions.json");
        if (!File.Exists(optionsPath))
            throw new FileNotFoundException("Localization runtime options are missing.", optionsPath);

        var settings = JsonUtility.FromJson<BuildLocalizationSettings>(File.ReadAllText(optionsPath));
        if (settings?.Locales == null || Array.IndexOf(settings.Locales, language) < 0)
            throw new ArgumentException("Build language is not listed in localization settings: " + language);
    }

    private static IDisposable Apply()
    {
        if (!BuildLaunchContext.TryGet(LanguageKey, out string language) || string.IsNullOrEmpty(language))
            return null;

        ValidateLanguage(language);
        if (_activeTempPath != null)
            throw new InvalidOperationException("A build language file is already active.");

        string directory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Library", "AlicizaX", "Localization");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            File.WriteAllText(path, language, new UTF8Encoding(false));
            _activeTempPath = path;
            return new Cleanup(path);
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
    }

    [Serializable]
    private sealed class BuildLocalizationSettings
    {
        public string[] Locales;
    }

    private sealed class Cleanup : IDisposable
    {
        private readonly string _path;

        internal Cleanup(string path) => _path = path;

        public void Dispose()
        {
            try { File.Delete(_path); }
            finally { _activeTempPath = null; }
        }
    }
}

internal sealed class LocalizationBuildLocaleFileProcessor : BuildPlayerProcessor
{
    public override void PrepareForBuild(BuildPlayerContext context)
    {
        string path = LocalizationBuildLanguageOverride.ActiveTempPath;
        if (path == null) return;

        string[] defines = context.BuildPlayerOptions.extraScriptingDefines;
        if (defines == null || Array.IndexOf(defines, LocalizationBuildLanguageOverride.Define) < 0)
            throw new InvalidOperationException("Build language file requires ALICIZAX_BUILD_LOCALE_FILE.");

        context.AddAdditionalPathToStreamingAssets(path, LocalizationBuildLanguageOverride.StreamingAssetsPath);
    }
}
#endif
