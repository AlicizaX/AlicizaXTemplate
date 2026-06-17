using System.Collections.Generic;
using AlicizaX;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 模拟Http请求远端 CDN地址版本号等 包括热更入口等..
/// </summary>
public static class StartupSetting
{
    public const string EntranceDll = "GameLogic.dll";
    public const string EntranceClass = "GameLogic.HotfixEntry";
    public const string EntranceMethod = "Entrance";

    public static readonly List<string> HotUpdateAssemblies = new List<string>()
        { "GameLib.dll", "GameProto.dll", "GameBase.dll", "GameLogic.dll" };



    public const string VersionApi = "http://localhost:5000/api/Version?channel=Standlone";
    public static string Version = string.Empty;
    public static string CDNUrl = string.Empty;
    public static string AppDownloadUrl = string.Empty;

    [System.Serializable]
    private class RemoteVersionInfo
    {
        public string version;
        public string cdnUrl;
        public string appDownloadUrl;
    }

    public static async UniTask GetRemoteVersion()
    {
        var updateDataStr = await Utility.Http.Get(VersionApi);
        var versionInfo = JsonUtility.FromJson<RemoteVersionInfo>(updateDataStr);
        Version = versionInfo.version;
        CDNUrl = versionInfo.cdnUrl;
        AppDownloadUrl = versionInfo.appDownloadUrl;
        Debug.Log(updateDataStr);
    }
}
