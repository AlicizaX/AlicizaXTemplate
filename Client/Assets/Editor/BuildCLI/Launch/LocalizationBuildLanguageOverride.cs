#if ALICIZAX_LOCALIZATION
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 构建语言校验。语言文件由 AppBuildHelper 在构建前写入 Resources，构建结束后删除。
/// </summary>
internal static class LocalizationBuildLanguageOverride
{
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

    [Serializable]
    private sealed class BuildLocalizationSettings
    {
        public string[] Locales;
    }
}
#endif
