using Cysharp.Threading.Tasks;
using AlicizaX;
using YooAsset;

namespace Unity.Startup.Procedure
{
    internal sealed class ProcedureInitPackageState : ProcedureBase
    {
        private int maxFailedCount = 0;
        private const int MAX_FAILED_TRYCOUNT = 3;

        protected override void OnEnter()
        {
            InitPackageAsync().Forget();
        }

        private async UniTask InitPackageAsync()
        {
            string hostUrl = string.Empty;
            if (GameApp.Resource.PlayMode == EPlayMode.HostPlayMode || GameApp.Resource.PlayMode == EPlayMode.WebPlayMode)
            {
                hostUrl = StartupSetting.CDNUrl;
            }

            await GameApp.Resource.InitPackageAsync(string.Empty, hostUrl, hostUrl);
            await UniTask.DelayFrame();
            UpdateStaticVersion().Forget();
        }

        private async UniTask UpdateStaticVersion()
        {
            var buildInOperation = GameApp.Resource.RequestPackageVersionAsync();
            await buildInOperation.ToUniTask();

            if (buildInOperation.Status == EOperationStatus.Succeeded)
            {
                //更新成功
                string packageVersion = buildInOperation.PackageVersion;
                GameApp.Resource.PackageVersion = packageVersion;

                Log.Info($"Updated package Version : {packageVersion}");
                maxFailedCount = 0;
                UpdateManifest().Forget();
            }
            else
            {
                maxFailedCount++;
                //更新失败
                Log.Error(buildInOperation.Error);

                if (maxFailedCount <= MAX_FAILED_TRYCOUNT)
                {
                    await UniTask.Delay(3000);
                    UpdateStaticVersion().Forget();
                    Log.Info($"Retry Update Static Version...{maxFailedCount}");
                }
            }
        }


        private async UniTask UpdateManifest()
        {
            LoadPackageManifestOperation buildInOperation;
            string packageVersion = "Simulate";
            if (GameApp.Resource.PlayMode != EPlayMode.EditorSimulateMode)
            {
                packageVersion = GameApp.Resource.PackageVersion;
            }

            buildInOperation = GameApp.Resource.LoadPackageManifestAsync(packageVersion);
            await buildInOperation.ToUniTask();

            if (buildInOperation.Status == EOperationStatus.Succeeded)
            {
                if (GameApp.Resource.PlayMode == EPlayMode.OfflinePlayMode)
                {
                    SwitchProcedure<ProcedurePatchDoneState>();
                    return;
                }

                SwitchProcedure<ProcedureDownloadBundleState>();
            }
            else
            {
                maxFailedCount++;
                Log.Error(buildInOperation.Error);
                if (maxFailedCount <= MAX_FAILED_TRYCOUNT)
                {
                    await UniTask.Delay(3000);
                    UpdateManifest().Forget();
                    Log.Info($"Retry Update Manifest Version...{maxFailedCount}");
                }
            }
        }
    }
}
