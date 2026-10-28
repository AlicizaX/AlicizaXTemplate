using System;
using System.Collections.Generic;

/// <summary>
/// 构建参数及模块构建期间的临时覆盖。
/// </summary>
public static class BuildLaunchContext
{
    public const string ResourcePlayMode = "resource.playMode";
    public const string ResourceDecryption = "resource.decryption";

    private static Dictionary<string, string> _values;
    private static List<Func<IDisposable>> _buildOverrides;

    public static void RegisterBuildOverride(Func<IDisposable> factory)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        (_buildOverrides ??= new List<Func<IDisposable>>()).Add(factory);
    }

    public static IDisposable BeginBuildOverrides()
    {
        var scopes = new List<IDisposable>();
        try
        {
            if (_buildOverrides != null)
            {
                foreach (Func<IDisposable> factory in _buildOverrides)
                    scopes.Add(factory());
            }

            return new BuildOverrideScope(scopes);
        }
        catch
        {
            new BuildOverrideScope(scopes).Dispose();
            throw;
        }
    }

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

    private sealed class BuildOverrideScope : IDisposable
    {
        private List<IDisposable> _scopes;

        public BuildOverrideScope(List<IDisposable> scopes) => _scopes = scopes;

        public void Dispose()
        {
            List<IDisposable> scopes = _scopes;
            if (scopes == null) return;
            _scopes = null;
            for (int i = scopes.Count - 1; i >= 0; i--)
                scopes[i]?.Dispose();
        }
    }
}
