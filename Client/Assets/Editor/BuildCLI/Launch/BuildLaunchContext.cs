using System.Collections.Generic;

/// <summary>
/// Jenkins / AppBuild 在 BuildPlayer 前写入的启动参数邮箱。
/// 各模块 bake 按 key 自取；没有 key 则跳过，不改场景字段。
/// 只存在于本次 Editor 构建进程内存中，不会写回工程资产。
/// </summary>
public static class BuildLaunchContext
{
    public const string ResourcePlayMode = "resource.playMode";
    public const string ResourceDecryption = "resource.decryption";
    public const string LocalizationLanguage = "localization.language";

    private static Dictionary<string, string> _values;

    public static void Set(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (_values == null)
        {
            _values = new Dictionary<string, string>();
        }

        _values[key] = value ?? string.Empty;
    }

    public static bool TryGet(string key, out string value)
    {
        if (_values != null && _values.TryGetValue(key, out value))
        {
            return true;
        }

        value = null;
        return false;
    }

    public static void Clear()
    {
        _values = null;
    }
}
