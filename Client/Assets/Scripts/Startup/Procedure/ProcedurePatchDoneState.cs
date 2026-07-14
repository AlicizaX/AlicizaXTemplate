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
            var options = new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles);
            ClearCacheOperation operation = GameApp.Resource.ClearCacheAsync(options);
            operation.Completed += ClearCacheCompleted;
        }


        private void ClearCacheCompleted(AsyncOperationBase obj)
        {
            Log.Info($"清理包裹缓存完成");
            SwitchProcedure<ProcedureLoadAssembly>();
        }
    }
}
