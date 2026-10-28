using System;
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
            TextAsset options = Resources.Load<TextAsset>("Localization/RuntimeOptions");
            string json = options.text;
            Resources.UnloadAsset(options);
            await LocalizationKit.InitializeAsync(json, new ResourcesCatalogSource("Localization/Update_{locale}"));

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
