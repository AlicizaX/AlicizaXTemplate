using System.Threading;
using AlicizaX;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 清理不再使用的资源包缓存。
    /// </summary>
    internal sealed class ProcedureClearCacheState : AsyncProcedureBase
    {
        protected override async UniTask RunAsync(CancellationToken cancellationToken)
        {
            var options = new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles);
            await GameApp.ResourcePackage.ClearCacheAsync(options).ToUniTask(cancellationToken: cancellationToken);
            Log.Info("Unused bundle cache cleared.");
            SwitchProcedure<ProcedureLoadAssemblyState>();
        }
    }
}
