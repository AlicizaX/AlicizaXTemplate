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
            LocalizationKit.Initialize(Resources.Load<LocalizationSettings>("Localization/Settings"));
            await LocalizationKit.AddTableAsync("StartupTable", new ResourcesTableSource("Localization/StartupTable_{locale}"));

            await UniTask.WaitUntil(() => YooAsset.YooAssets.IsInitialized);
            ProcedureBuilder.InitializeProcedure(
                new List<ProcedureBase>
                {
                    new ProcedureEntryState(),
                    new ProcedureGetAppVersionInfoState(),
                    new ProcedureInitPackageState(),
                    new ProcedureDownloadBundleState(),
                    new ProcedurePatchDoneState(),
                    new ProcedureLoadAssembly(),
                    new ProcedureUpdateFinishState(),
                },
                typeof(ProcedureEntryState)
            );
            Destroy(gameObject);
        }
    }
}
