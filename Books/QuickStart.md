# QuickStart 快速入门

本篇用于把框架的启动链路串起来。各模块的详细 API、完整示例和注意事项请看同目录下对应文档。

源码入口：

- `Client/Packages/com.alicizax.unity.framework/Runtime`
- 启动示例：`Client/Assets/Scripts/Startup`
- 热更逻辑示例：`Client/Assets/Scripts/Hotfix/GameLogic`
- 示例场景：`Client/Assets/Scenes/Main.unity`

## 启动链路

项目当前的推荐启动顺序如下：

1. 启动场景中放置框架根节点，挂载 `RootModule` 和各模块 `Component`。
2. `RootModule.Awake()` 创建 `ServiceWorld`，之后各模块组件在 `Awake()` 中注册服务。
3. `ResourceComponent.Awake()` 初始化 YooAsset 并创建默认资源包。
4. `ProcedureEntry.Start()` 等待 `YooAssets.Initialized`，然后注册启动流程。
5. 启动流程完成资源包初始化、版本检查、资源下载、缓存清理。
6. 如果开启 HybridCLR，加载热更程序集并反射调用 `GameLogic.HotfixEntry.Entrance`。
7. 业务层通过 `GameApp` 或 `AppServices` 调用 UI、资源、音频、场景等模块。

项目已有入口代码：

```csharp
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
#if ENABLE_HYBRIDCLR
                    new ProcedureLoadAssembly(),
#endif
                    new ProcedureUpdateFinishState(),
                },
                typeof(ProcedureEntryState));

            Destroy(gameObject);
        }
    }
}
```

## 场景根节点

新建或检查启动场景时，建议准备一个框架根节点，例如 `Entry`。核心组件如下：

| 组件 | 作用 |
| --- | --- |
| `RootModule` | 创建服务世界，驱动 `Tick/LateTick/FixedTick`，设置帧率、后台运行、休眠策略 |
| `MemoryPoolSetting` | 驱动内存池回收和严格检查开关 |
| `ObjectPoolComponent` | 注册普通对象池服务 |
| `TimerComponent` | 注册全局计时器服务 |
| `ResourceComponent` | 注册资源服务并初始化 YooAsset |
| `UIComponent` | 注册 UI 服务并实例化 UI 根节点 |
| `AudioComponent` | 注册音频服务 |
| `SceneComponent` | 注册场景服务并创建 Scene 作用域 |
| `LocalizationComponent` | 注册并初始化本地化服务 |
| `GameObjectPoolComponent` | 注册 GameObject 池服务 |
| `DebuggerComponent` | 可选，注册运行时调试窗口 |

常用依赖顺序已经通过组件上的 `DefaultExecutionOrder` 处理：`RootModule` 最早执行，随后是内存池、对象池、计时器、资源、UI、音频等组件。业务脚本仍建议在 `Start()` 之后再访问框架服务。

关键 Inspector 配置：

- `ResourceComponent.PackageName` 默认使用 `DefaultPackage`。
- `ResourceComponent.PlayMode` 控制 YooAsset 运行模式，编辑器下会读取 YooAsset 播放模式配置。
- `UIComponent.uiRoot` 必须指定 UI 根节点预制体。
- `AudioComponent` 必须指定 `AudioMixer` 和 `AudioListener`。
- `LocalizationComponent` 可指定默认语言，例如 `ChineseSimplified`。

## 获取服务

业务层优先使用 `GameApp` 快捷入口：

```csharp
GameApp.Resource
GameApp.UI
GameApp.Timer
GameApp.Audio
GameApp.Scene
GameApp.Localization
GameApp.ObjectPool
GameApp.GameObjectPool
GameApp.Base
```

如果需要依赖抽象接口，使用 `AppServices`：

```csharp
using AlicizaX;
using AlicizaX.Resource.Runtime;
using AlicizaX.UI.Runtime;

IResourceService resources = AppServices.Require<IResourceService>();
IUIService ui = AppServices.Require<IUIService>();
```

服务可能尚未注册时使用 `TryGet`：

```csharp
if (!AppServices.TryGet<IUIService>(out var ui))
{
    return;
}

ui.ShowUISync<UILoadUpdate>();
```

`Require<T>()` 找不到服务会抛出异常，适合“必须存在”的启动链路。`TryGet<T>()` 适合可选模块或不确定初始化时机的逻辑。

## 最小启动流程

Procedure 适合管理启动、热更新、登录、加载战斗等串行阶段。一个简化流程如下：

```csharp
using AlicizaX;
using UnityEngine;

public sealed class ProcedureBoot : ProcedureBase
{
    protected internal override void OnEnter()
    {
        Debug.Log("Boot");
        SwitchProcedure<ProcedureEnterGame>();
    }
}

public sealed class ProcedureEnterGame : ProcedureBase
{
    protected internal override void OnEnter()
    {
        GameApp.UI.ShowUISync<UILoadUpdate>();
    }
}
```

注册流程：

```csharp
using System.Collections.Generic;
using AlicizaX;
using UnityEngine;

public sealed class SimpleProcedureEntry : MonoBehaviour
{
    private void Start()
    {
        ProcedureBuilder.InitializeProcedure(
            new List<ProcedureBase>
            {
                new ProcedureBoot(),
                new ProcedureEnterGame(),
            },
            typeof(ProcedureBoot));
    }

    private void OnDestroy()
    {
        ProcedureBuilder.DestroyProcedure();
    }
}
```

项目当前启动流程已经放在 `Client/Assets/Scripts/Startup/Procedure`，可以直接参考：

- `ProcedureEntryState`：检查运行模式、网络状态和远端版本。
- `ProcedureInitPackageState`：初始化 YooAsset 包、请求包版本、更新清单。
- `ProcedureDownloadBundleState`：创建下载器并下载资源。
- `ProcedurePatchDoneState`：清理未使用缓存。
- `ProcedureLoadAssembly`：开启 HybridCLR 时加载热更程序集。
- `ProcedureUpdateFinishState`：销毁启动流程。

## 资源初始化和下载

`ResourceComponent.Awake()` 只负责创建资源服务、初始化 YooAsset 系统和默认包对象。真正进入可加载资源状态前，启动流程需要调用 `InitPackageAsync`、请求包版本并更新清单。

简化示例：

```csharp
using AlicizaX;
using Cysharp.Threading.Tasks;
using YooAsset;

public sealed class ProcedureInitPackage : ProcedureBase
{
    protected internal override void OnEnter()
    {
        InitAsync().Forget();
    }

    private async UniTaskVoid InitAsync()
    {
        string hostUrl = string.Empty;

        if (GameApp.Resource.PlayMode == EPlayMode.HostPlayMode ||
            GameApp.Resource.PlayMode == EPlayMode.WebPlayMode)
        {
            hostUrl = StartupSetting.CDNUrl;
        }

        bool initialized = await GameApp.Resource.InitPackageAsync(
            packageName: string.Empty,
            hostServerURL: hostUrl,
            fallbackHostServerURL: hostUrl);

        if (!initialized)
        {
            Log.Error("Init package failed.");
            return;
        }

        var versionOperation = GameApp.Resource.RequestPackageVersionAsync();
        await versionOperation.ToUniTask();

        if (versionOperation.Status != EOperationStatus.Succeed)
        {
            Log.Error(versionOperation.Error);
            return;
        }

        string packageVersion = versionOperation.PackageVersion;
        GameApp.Resource.PackageVersion = packageVersion;

        var manifestOperation = GameApp.Resource.UpdatePackageManifestAsync(packageVersion);
        await manifestOperation.ToUniTask();

        if (manifestOperation.Status != EOperationStatus.Succeed)
        {
            Log.Error(manifestOperation.Error);
            return;
        }

        SwitchProcedure<ProcedureEnterGame>();
    }
}
```

创建下载器：

```csharp
using AlicizaX;
using AlicizaX.Resource.Runtime;
using Cysharp.Threading.Tasks;
using YooAsset;

public sealed class ProcedureDownload : ProcedureBase
{
    private ResourceDownloaderOperation _downloader;

    protected internal override void OnEnter()
    {
        _downloader = GameApp.Resource.CreateResourceDownloader();

        if (_downloader.TotalDownloadCount == 0)
        {
            SwitchProcedure<ProcedureEnterGame>();
            return;
        }

        DownloadAsync().Forget();
    }

    protected internal override void OnLeave()
    {
        _downloader?.CancelDownload();
        _downloader = null;
    }

    private async UniTaskVoid DownloadAsync()
    {
        _downloader.DownloadUpdateCallback = OnDownloadUpdate;
        _downloader.BeginDownload();
        await _downloader;

        if (_downloader.Status != EOperationStatus.Succeed)
        {
            Log.Error("Download failed.");
            return;
        }

        SwitchProcedure<ProcedureEnterGame>();
    }

    private void OnDownloadUpdate(DownloadUpdateData data)
    {
        EventBus.Publish(AssetDownloadProgressUpdateEventArgs.Create(
            data.PackageName,
            data.TotalDownloadCount,
            data.CurrentDownloadCount,
            data.TotalDownloadBytes,
            data.CurrentDownloadBytes));
    }
}
```

下载进度订阅可以参考 `UpdateProgressUtils`：

```csharp
private EventRuntimeHandle _handle;

private void OnEnable()
{
    _handle = EventBus.Subscribe<AssetDownloadProgressUpdateEventArgs>(OnProgress);
}

private void OnDisable()
{
    _handle.Dispose();
}

private void OnProgress(in AssetDownloadProgressUpdateEventArgs evt)
{
    float progress = evt.TotalDownloadSizeBytes <= 0
        ? 1f
        : evt.CurrentDownloadSizeBytes / (float)evt.TotalDownloadSizeBytes;

    Log.Info($"Download progress: {progress:P0}");
}
```

## 热更入口

热更入口由 `StartupSetting` 指定：

```csharp
public const string EntranceDll = "GameLogic.dll";
public const string EntranceClass = "GameLogic.HotfixEntry";
public const string EntranceMethod = "Entrance";

public static readonly List<string> HotUpdateAssemblies = new List<string>
{
    "GameLib.dll",
    "GameProto.dll",
    "GameBase.dll",
    "GameLogic.dll"
};
```

项目示例中的热更入口：

```csharp
using System.Collections.Generic;
using System.Reflection;
using AlicizaX;

namespace GameLogic
{
    public static class HotfixEntry
    {
        private static List<Assembly> _hotfixAssembly;
        public static List<Assembly> HotfixAssembly => _hotfixAssembly;

        public static void Entrance(object[] objects)
        {
            Log.Info("HotFix Logic Entry!");
            _hotfixAssembly = (List<Assembly>)objects[0];
            GameApp.UI.ShowUISync<UILoadUpdate>();
        }
    }
}
```

如果没有开启 HybridCLR，可以在 `ProcedureUpdateFinishState` 后进入自己的 AOT 业务入口。开启 HybridCLR 时，确保 `StartupSetting.HotUpdateAssemblies`、YooAsset 资源收集配置和构建产物中的 DLL 名称一致。

## 打开第一个 UI

UI 逻辑类继承 `UIWindow<T>`、`UITabWindow<T>` 或 `UIWidget<T>`，并通过 `WindowAttribute` 指定层级。

```csharp
using AlicizaX;
using AlicizaX.UI.Runtime;
using Game.UI;

[Window(UILayer.UI, false, 3)]
public sealed class LoginWindow : UIWindow<ui_LoginWindow>
{
    protected override void OnInitialize()
    {
        baseui.BtnLogin.onClick.AddListener(OnLoginClick);
    }

    private void OnLoginClick()
    {
        CloseSelf();
    }
}
```

打开 UI：

```csharp
using Cysharp.Threading.Tasks;

public static class GameEntryExample
{
    public static async UniTask EnterGameAsync()
    {
        LoginWindow login = await GameApp.UI.ShowUI<LoginWindow>();
    }
}
```

同步打开适合资源已经在本地且可以同步加载的窗口：

```csharp
GameApp.UI.ShowUISync<UILoadUpdate>();
```

## 常用模块调用

延迟执行：

```csharp
ulong timer = GameApp.Timer.AddTimer(() =>
{
    Log.Info("One second later.");
}, 1f);
```

发布和订阅事件：

```csharp
public readonly struct LoginSuccessEvent : IPayloadEventArgs
{
    public readonly long UserId;

    public LoginSuccessEvent(long userId)
    {
        UserId = userId;
    }
}

EventRuntimeHandle handle = EventBus.Subscribe<LoginSuccessEvent>(OnLoginSuccess);
EventBus.Publish(new LoginSuccessEvent(10001));

private static void OnLoginSuccess(in LoginSuccessEvent evt)
{
    Log.Info($"Login success: {evt.UserId}");
}
```

加载资源：

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

public static async UniTask<Sprite> LoadIconAsync()
{
    return await GameApp.Resource.LoadAssetAsync<Sprite>(
        "Assets/Bundles/UIRaw/Raw/icon_start.png");
}
```

播放音效：

```csharp
using AlicizaX.Audio.Runtime;

ulong handle = GameApp.Audio.Play(
    AudioType.UISound,
    "Assets/Bundles/Audios/ui_click.wav");
```

切换场景：

```csharp
using UnityEngine.SceneManagement;

await GameApp.Scene.LoadSceneAsync(
    "Assets/Bundles/Scenes/Battle.unity",
    LoadSceneMode.Single);
```

切换语言：

```csharp
await GameApp.Localization.SwitchLanguageAsync("English");
```

## 调试建议

启动阶段常见检查点：

1. `RootModule` 是否在启动场景中，并且场景里没有多个框架根节点。
2. `UIComponent.uiRoot` 是否已配置，否则运行时会抛出 `UIRoot Prefab is invalid.`。
3. `AudioComponent` 是否配置了 `AudioMixer` 和 `AudioListener`。
4. `ResourceComponent.PackageName` 是否与 YooAsset 构建包名一致。
5. `StartupSetting.CDNUrl` 在 Host/Web 模式下是否能访问。
6. `StartupSetting.HotUpdateAssemblies` 是否与热更 DLL 名称一致。
7. 业务调用 `GameApp.xxx` 前，对应组件是否已经完成 `Awake()` 注册。

运行时调试可以挂载 `DebuggerComponent`，查看日志、系统信息、资源、对象池、内存池、音频和计时器状态。

## 文档索引

| 文档 | 内容 |
| --- | --- |
| `Service.md` | 服务容器、作用域、自定义服务 |
| `MemoryPool.md` | 内存池、引用对象生命周期 |
| `Timer.md` | 延迟、循环、暂停恢复计时器 |
| `ObjectPool.md` | 普通对象池 |
| `GameObjectPool.md` | GameObject 实例池 |
| `Resources.md` | YooAsset 资源加载、下载、卸载 |
| `Audio.md` | 音效、音乐、音量分组 |
| `Event.md` | 事件总线 |
| `Procedure.md` | 启动流程和流程状态机 |
| `Scene.md` | 场景加载、挂起、卸载 |
| `Localization.md` | 本地化表和语言切换 |
| `UI.md` | UI 窗口、Widget、Tab、事件自动释放 |
| `Debugger.md` | 运行时调试面板 |

## 接入顺序建议

新项目接入时建议按下面顺序验证：

1. 只挂 `RootModule`、`MemoryPoolSetting`、`ObjectPoolComponent`、`TimerComponent`，确认服务世界和 Tick 正常。
2. 加 `ResourceComponent`，确认 YooAsset 默认包能初始化。
3. 加 `UIComponent`，打开一个最简单窗口。
4. 加 `ProcedureEntry`，把资源初始化和下载流程放入 Procedure。
5. 接入 `AudioComponent`、`LocalizationComponent`、`SceneComponent` 等业务模块。
6. 最后接入 HybridCLR 热更入口和远端版本配置。

这样每一步的依赖边界都比较清晰，排查启动问题会简单很多。
