using AlicizaX;
using AlicizaX.Localization.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 启动入口（挂在 Main 场景）：显示启动界面、加载启动多语言表，然后组装启动流程。
    /// <para>CheckVersion → InitPackage → DownloadBundle → ClearCache → LoadAssembly → EnterGame</para>
    /// <para>流程之间不互相引用，只通过 <see cref="LaunchContext"/> 交换数据；
    /// 与界面之间只通过 <c>Event/LaunchEvents.cs</c> 中的事件通信。</para>
    /// </summary>
    public sealed class ProcedureEntry : MonoBehaviour
    {
        private async UniTaskVoid Start()
        {
            GameApp.UI.ShowUISync<UILaunchWindow>();
            LocalizationKit.Initialize();
            await LocalizationKit.AddTableAsync("StartupTable", new ResourcesTableSource("Localization/StartupTable_{locale}"));
            EventBus.Publish(new LaunchStateChangedEvent(LaunchState.Checking));

            await UniTask.WaitUntil(() => YooAssets.IsInitialized);

            var context = new LaunchContext();
            ProcedureBuilder.InitializeProcedure(new ProcedureBase[]
            {
                new ProcedureCheckVersionState(context),
                new ProcedureInitPackageState(context),
                new ProcedureDownloadBundleState(),
                new ProcedureClearCacheState(),
                new ProcedureLoadAssemblyState(context),
                new ProcedureEnterGameState(context),
            }, typeof(ProcedureCheckVersionState));

            Destroy(gameObject);
        }
    }
}
