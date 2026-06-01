using System;
using System.Collections.Generic;
using AlicizaX;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Unity.Startup.Procedure
{
    public class ProcedureEntry : MonoBehaviour
    {
        private async UniTaskVoid Start()
        {
            await UniTask.WaitUntil(() => YooAsset.YooAssets.Initialized);
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
