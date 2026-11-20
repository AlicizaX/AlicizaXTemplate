using System.Collections.Generic;
using AlicizaX;
using AlicizaX.Localization.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Unity.Startup.Procedure
{
    public class ProcedureEntry : MonoBehaviour
    {
        private async UniTaskVoid Start()
        {
            GameApp.UI.ShowUISync<UILaunchWindow>();
            LocalizationKit.Initialize();
            await LocalizationKit.AddTableAsync("StartupTable", new ResourcesTableSource("Localization/StartupTable_{locale}"));
            EventBus.Publish(new LaunchStateChangedEvent(LaunchState.Checking));

            await UniTask.WaitUntil(() => YooAsset.YooAssets.IsInitialized);
            var loadAssembly = new ProcedureLoadAssembly();
            ProcedureBuilder.InitializeProcedure(
                new List<ProcedureBase>
                {
                    new ProcedureEntryState(),
                    new ProcedureGetAppVersionInfoState(),
                    new ProcedureInitPackageState(),
                    new ProcedureDownloadBundleState(),
                    new ProcedurePatchDoneState(),
                    loadAssembly,
                    new ProcedureUpdateFinishState(loadAssembly.InvokeMainLogicEntry),
                },
                typeof(ProcedureEntryState)
            );
            Destroy(gameObject);
        }
    }
}
