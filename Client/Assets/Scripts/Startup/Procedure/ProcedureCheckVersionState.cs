using System;
using System.Threading;
using AlicizaX;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 检查版本：请求远端版本信息，App 版本落后时强制更新，并把资源服务器地址写入 <see cref="LaunchContext"/>。
    /// 离线 / 编辑器模拟模式不需要远端，直接进入下一步。
    /// </summary>
    internal sealed class ProcedureCheckVersionState : AsyncProcedureBase
    {
        private readonly LaunchContext _context;

        public ProcedureCheckVersionState(LaunchContext context) => _context = context;

        protected override async UniTask RunAsync(CancellationToken cancellationToken)
        {
            EPlayMode playMode = GameApp.ResourcePackage.PlayMode;
            Log.Info($"PlayMode: {playMode}");

            if (playMode is EPlayMode.OfflinePlayMode or EPlayMode.EditorSimulateMode)
            {
                SwitchProcedure<ProcedureInitPackageState>();
                return;
            }

            RemoteVersionInfo remote = await RetryAsync(FetchRemoteVersionAsync, cancellationToken);

            // Web 端每次都加载最新页面，没有需要更新的安装包。
            if (playMode != EPlayMode.WebPlayMode && remote.version != Application.version)
            {
                ForceAppUpdate(remote);
                return;
            }

            _context.CdnUrl = remote.cdnUrl;
            SwitchProcedure<ProcedureInitPackageState>();
        }

        private static async UniTask<RemoteVersionInfo> FetchRemoteVersionAsync(CancellationToken cancellationToken)
        {
            string json = await Utility.Http.Get(StartupSetting.VersionApi).AttachExternalCancellation(cancellationToken);
            RemoteVersionInfo info = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<RemoteVersionInfo>(json);
            if (info == null || string.IsNullOrEmpty(info.version) || string.IsNullOrEmpty(info.cdnUrl))
                throw new FormatException($"Invalid version response from {StartupSetting.VersionApi}: '{json}'.");

            Log.Info($"Remote version: {json}");
            return info;
        }

        private static void ForceAppUpdate(RemoteVersionInfo remote)
        {
            Log.Warning($"App version mismatch: {Application.version} -> {remote.version}");
#if !UNITY_EDITOR
            Utility.Platform.OpenURL(remote.appDownloadUrl);
#endif
            Utility.Platform.Quit();
        }

        [Serializable]
        private sealed class RemoteVersionInfo
        {
            public string version;
            public string cdnUrl;
            public string appDownloadUrl;
        }
    }
}
