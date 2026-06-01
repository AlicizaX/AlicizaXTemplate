using AlicizaX;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Unity.Startup.Procedure
{
    internal sealed class ProcedurePatchDoneState : ProcedureBase
    {
        protected override void OnEnter()
        {
            ClearCacheFilesOperation operation = GameApp.Resource.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            operation.Completed += ClearCacheCompleted;
        }


        private void ClearCacheCompleted(AsyncOperationBase obj)
        {
            Log.Info($"清理包裹缓存完成");
            SwitchProcedure<ProcedureLoadAssembly>();
        }
    }
}
