using System;
using System.Threading;
using AlicizaX;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 初始化资源包：初始化 YooAsset 包，请求资源版本并加载对应清单。
    /// </summary>
    internal sealed class ProcedureInitPackageState : AsyncProcedureBase
    {
        private readonly LaunchContext _context;

        public ProcedureInitPackageState(LaunchContext context) => _context = context;

        protected override async UniTask RunAsync(CancellationToken cancellationToken)
        {
            string hostUrl = _context.CdnUrl;
            bool initialized = await GameApp.ResourcePackage.InitPackageAsync(string.Empty, hostUrl, hostUrl)
                .AttachExternalCancellation(cancellationToken);
            if (!initialized) throw new InvalidOperationException("Initialize resource package failed.");

            await RetryAsync(UpdatePackageVersionAsync, cancellationToken);
            await RetryAsync(LoadPackageManifestAsync, cancellationToken);

            if (GameApp.ResourcePackage.PlayMode == EPlayMode.OfflinePlayMode)
                SwitchProcedure<ProcedureClearCacheState>();
            else
                SwitchProcedure<ProcedureDownloadBundleState>();
        }

        private static async UniTask UpdatePackageVersionAsync(CancellationToken cancellationToken)
        {
            RequestPackageVersionOperation operation = GameApp.ResourcePackage.RequestPackageVersionAsync();
            await operation.ToUniTask(cancellationToken: cancellationToken);
            EnsureSucceeded(operation);

            GameApp.ResourcePackage.PackageVersion = operation.PackageVersion;
            Log.Info($"Package version: {operation.PackageVersion}");
        }

        private static async UniTask LoadPackageManifestAsync(CancellationToken cancellationToken)
        {
            string packageVersion = GameApp.ResourcePackage.PlayMode == EPlayMode.EditorSimulateMode
                ? "Simulate"
                : GameApp.ResourcePackage.PackageVersion;

            LoadPackageManifestOperation operation = GameApp.ResourcePackage.LoadPackageManifestAsync(packageVersion);
            await operation.ToUniTask(cancellationToken: cancellationToken);
            EnsureSucceeded(operation);
        }

        private static void EnsureSucceeded(AsyncOperationBase operation)
        {
            if (operation.Status != EOperationStatus.Succeeded) throw new InvalidOperationException(operation.Error);
        }
    }
}
