# UI 模块

UI 模块负责窗口创建、显示、关闭、层级、缓存、遮挡、生命周期、Widget、Tab 窗口和 UI 事件自动解绑。

主要代码位置：

- `Client/Packages/com.alicizax.unity.framework/Runtime/Modules/UI`
- 示例业务代码：`Client/Assets/Scripts/Hotfix/GameLogic/UI`

常用入口：

```csharp
using AlicizaX;
using AlicizaX.UI.Runtime;

IUIService ui = GameApp.UI;
IUIRouter router = GameApp.UI.Router;
```

## 使用前提

启动场景需要挂载并初始化这些组件：

- `ObjectPoolComponent`
- `TimerComponent`
- `ResourceComponent`
- `UIComponent`

`UIComponent` Inspector 中需要配置 `uiRoot` 预制体。运行时会实例化 UI 根节点，注册 `IUIService`，并按 `UILayer` 创建层级节点。

## UI 层级

```csharp
public enum UILayer
{
    Background = 0,
    Scene = 1,
    UI = 2,
    Popup = 3,
    Tips = 4,
    Top = 5,
    All = 6,
}
```

约定：

- `Background`：背景层。
- `Scene`：场景 2D 信息，例如血条、飘字。
- `UI`：普通主界面、全屏界面。
- `Popup`：弹窗层。
- `Tips`：提示、Toast、跑马灯。
- `Top`：最高层，例如新手引导遮罩。

## WindowAttribute

窗口逻辑类使用 `WindowAttribute` 描述显示层级、遮挡模式和缓存时间。

```csharp
[Window(UILayer.UI, UIOcclusionMode.None, 30)]
public sealed class HomeWindow : UIWindow<ui_HomeWindow>
{
    protected override void OnInitialize()
    {
    }
}
```

当前构造函数是：

```csharp
public WindowAttribute(
    UILayer windowLayer,
    UIOcclusionMode occlusionMode = UIOcclusionMode.None,
    int cacheTime = 0)
```

参数说明：

- `windowLayer`：窗口所在层级。
- `occlusionMode`：遮挡模式。
- `cacheTime`：缓存时间，`-1` 永久缓存，`0` 不缓存，`>= 1` 按秒缓存。

遮挡模式：

```csharp
public enum UIOcclusionMode : byte
{
    None,
    Visible,
    Lifecycle,
}
```

- `None`：只做普通显示排序，不主动遮挡下层窗口生命周期。
- `Visible`：通过可见性隐藏被遮挡窗口。
- `Lifecycle`：被遮挡窗口会走关闭生命周期，重新露出时再打开。路由深回退时要用 Router 的批量关闭能力避免中间页先被恢复再关闭。

需要每帧更新时添加：

```csharp
[UIUpdate]
[Window(UILayer.UI)]
public sealed class BattleHudWindow : UIWindow<ui_BattleHudWindow>
{
    protected override void OnUpdate()
    {
        RefreshHpBar();
    }
}
```

## 窗口初始化

初始化生命周期内联在 `UIBase` 中，通过虚方法重写：

```csharp
protected virtual void OnInitialize()
{
}

protected virtual UniTask OnInitializeAsync()
{
    OnInitialize();
    return UniTask.CompletedTask;
}
```

同步初始化示例：

```csharp
[Window(UILayer.UI, UIOcclusionMode.None, 30)]
public sealed class TestWindow : UIWindow<ui_TestWindow>
{
    protected override void OnInitialize()
    {
        baseui.BtnClose.onClick.AddListener(CloseSelf);
    }
}
```

异步初始化示例：

```csharp
[Window(UILayer.UI, UIOcclusionMode.None, 30)]
public sealed class HomeWindow : UITabWindow<ui_HomeWindow>
{
    private HomeWidget _homeWidget;

    protected override async UniTask OnInitializeAsync()
    {
        _homeWidget = await CreateWidgetAsync<HomeWidget>(baseui.RectTransform, false);
        baseui.BtnShop.onClick.AddListener(OnShopClick);
    }

    private void OnShopClick()
    {
        GameApp.UI.Router.NavigateTo<ShopWindow>().Forget();
    }
}
```

注意：

- 只重写 `OnInitialize`：同步 API 和异步 API 都会执行这段初始化。
- 重写 `OnInitializeAsync`：异步 API 会等待它完成；同步 API 只调用 `OnInitialize`，不会执行异步初始化逻辑。
- 只写异步初始化的 UI 不建议用 `ShowUISync` 打开，除非明确不依赖该异步初始化结果。
- `OnOpen`、`OnRefresh`、`OnClose`、`OnDestroy`、`OnUpdate` 也是 `UIBase` 的虚方法。
- `OnRefresh` 用于重复打开或重复传参后的刷新，调用前参数已经写入 `UserData` / `UserDatas`。

## UIService API

`IUIService` 负责实际 UI 实例、栈、层级、遮挡、缓存和生命周期。

### 打开 UI

无参异步打开：

```csharp
HomeWindow home = await GameApp.UI.ShowUI<HomeWindow>();
```

带参数异步打开：

```csharp
ItemTipsWindow tips = await GameApp.UI.ShowUI<ItemTipsWindow>(10001, "from_bag");
```

需要精确状态时使用 `ShowUIResult`：

```csharp
UIShowResult<HomeWindow> result = await GameApp.UI.ShowUIResult<HomeWindow>();
if (result.IsAccepted)
{
    HomeWindow view = result.View;
}
```

可用重载：

```csharp
UniTask<T> ShowUI<T>() where T : UIBase;
UniTask<T> ShowUI<T>(params object[] userDatas) where T : UIBase;
UniTask<UIShowResult<T>> ShowUIResult<T>() where T : UIBase;
UniTask<UIShowResult<T>> ShowUIResult<T>(params object[] userDatas) where T : UIBase;
UniTask<UIBase> ShowUI(string type, params object[] userDatas);
UniTask<UIShowResult> ShowUIResult(string type, params object[] userDatas);
UniTask<UIBase> ShowUI(RuntimeTypeHandle handle, params object[] userDatas);
UniTask<UIShowResult> ShowUIResult(RuntimeTypeHandle handle, params object[] userDatas);
```

同步打开：

```csharp
LoginWindow login = GameApp.UI.ShowUISync<LoginWindow>();
LoginWindow loginWithArgs = GameApp.UI.ShowUISync<LoginWindow>("startup");
```

同步打开的语义是“同步拿到可操作 View，并启动打开流程”：

- 同步完成 UI 实例创建、资源绑定、参数写入和 `OnInitialize`。
- 打开动画仍会正常播放，但同步 API 不等待动画完成；返回时状态可能是 `Opening`。
- 动画完成后才进入稳定 `Opened`，并触发 `OnWindowAfterShowEvent`。
- 需要等待动画完成和稳定打开时，使用 `await ShowUI<T>()` 或 `ShowUIResult<T>()`。
- 同步打开只适合资源可同步加载的场景；如果初始化依赖 `OnInitializeAsync`，应使用异步 API。

重复 `ShowUI` 打开同一个正在打开或已打开的窗口时，不会重新初始化。框架会刷新参数，并在初始化完成后的状态调用 `OnRefresh`；`Opening` 阶段也会触发刷新。

```csharp
protected override void OnRefresh()
{
    RefreshView(UserDatas);
}
```

### 关闭 UI

```csharp
GameApp.UI.CloseUI<LoginWindow>();
GameApp.UI.CloseUI<LoginWindow>(force: true);
bool ok = await GameApp.UI.CloseUIAsync<LoginWindow>();
```

`force: true` 会强制跳过缓存策略。

窗口内部关闭自身：

```csharp
CloseSelf();
ForceCloseSelf();
```

`CloseUI` / `CloseSelf` 进入 UIService 后会先检查目标是否为 Router 当前页。
如果是当前 routed page，会转交给 `Router.CloseCurrent(expectedHandle)`，由 Router 维护 history；
如果不是当前 routed page，则继续按普通 UIService 关闭流程处理。

routed page 内部仍建议直接表达路由语义：

```csharp
GameApp.UI.Router.CloseCurrent().Forget();
```

### 查询 UI

```csharp
bool opened = GameApp.UI.IsOpen<LoginWindow>();
LoginWindow login = GameApp.UI.GetUI<LoginWindow>();
RectTransform uiLayer = GameApp.UI.GetLayer(UILayer.UI);
```

`IsOpen` 只表示该 UI 类型当前处于稳定 `Opened` 状态，不表示它一定是 Router 当前页。
同步打开返回后如果动画仍在播放，`IsOpen<T>()` 可能暂时为 `false`。

### 关闭最上层匹配 UI

```csharp
bool closed = await GameApp.UI.TryCloseTopAsync(
    handle => handle.Equals(typeof(SettingsWindow).TypeHandle));
```

`TryCloseTopAsync` 从最高层、最高栈位开始查找，关闭第一个满足谓词的 UI。

### CloseManyAsync

`CloseManyAsync` 是 UIService 内部栈事务能力，主要给 Router 深回退使用。普通业务通常不直接调用。

```csharp
RuntimeTypeHandle[] handles =
{
    typeof(ShopWindow).TypeHandle,
    typeof(TestWindow).TypeHandle,
};

UICloseManyMode[] modes =
{
    UICloseManyMode.Transition,
    UICloseManyMode.SilentFinalize,
};

UICloseManyResult result = await GameApp.UI.CloseManyAsync(handles, modes, 2);
```

语义：

- `Transition`：走正常关闭动画。
- `SilentFinalize`：不播放关闭动画，但仍完成 UIBase 关闭状态机。
- 一次事务内不会在每个窗口关闭后重复刷新遮挡。
- 最终每个变更层只刷新一次可见性和深度。
- preflight 不会反射注册 UI，也不会创建 metadata；未知 handle 返回 `UnknownHandle`。
- 重复 handle 会被折叠，已缓存或不在栈中的 handle 会被跳过。

## UIRouter

`IUIRouter` 管理 Page 级导航历史。Router 只负责 history 和导航事务，不负责 UI 实例、层级、遮挡、缓存。

入口：

```csharp
IUIRouter router = GameApp.UI.Router;
```

### 初始化 root

启动主界面建议用 Router 建立 root history：

```csharp
GameApp.UI.Router.ResetHistory();
await GameApp.UI.Router.NavigateTo<UIHomeWindow>();
```

不要用 `ShowUI<UIHomeWindow>()` 打开主 Page 后再期待 Router 能自动知道 history。若确实已有 UI 是手动打开的，可以用 `SyncFromCurrentUI` 重建 Router history。

### 前进导航

```csharp
await GameApp.UI.Router.NavigateTo<UITestAWindow>();
await GameApp.UI.Router.NavigateTo<UIShopWindow>(shopId);
```

规则：

- 每次成功导航会加入一条 Router history。
- 参数会浅拷贝保存到 history，避免调用方后续修改数组影响回放。
- 如果目标类型等于当前 history 顶部类型，会刷新当前页参数，不新增 history。
- `A -> B -> C -> D -> NavigateTo<A>()` 是正常前进导航，history 会变成 `A, B, C, D, A`，此后 `Back()` 返回 `D`。
- 当旧页和目标页在同一层，且目标页是 `Lifecycle` 遮挡窗口时，Router 会在目标开始显示后先加入 pending history，让遮挡系统负责旧页关闭；目标打开失败时会移除 pending history 并恢复本次 trim 掉的旧记录。
- 跨层 `Lifecycle` 目标不会走这个 shortcut，仍会按普通流程打开目标并主动关闭旧页。

### Replace

```csharp
await GameApp.UI.Router.Replace<SettingsWindow>();
await GameApp.UI.Router.Replace<SettingsWindow>("from_home");
```

`Replace` 用目标页替换当前 history 顶部。目标打开失败时不修改 history；旧页关闭失败时会按事务规则回滚或进入 dirty。

### Back

```csharp
await GameApp.UI.Router.Back();
```

返回上一条 history。Router Back 不处理弹窗优先关闭，弹窗应由业务、输入系统或弹窗管理逻辑处理。

相邻回退时，如果目标页已经被遮挡系统恢复打开，Router 会跳过重复 `ShowByRouter`。

### CloseCurrent

```csharp
await GameApp.UI.Router.CloseCurrent();
await GameApp.UI.Router.CloseCurrent(force: true);
```

用于 routed page 内部关闭当前页面：

- history 大于 1 时等价于 `Back()`。
- 只有 root 时关闭 root 并移除 history。
- `CloseSelf()` / `GameApp.UI.CloseUI<T>()` 如果命中 Router 当前页，也会转交给本流程。

### BackToRoot

```csharp
await GameApp.UI.Router.BackToRoot();
```

返回当前导航流的 root。

深回退示例：

```text
A -> B -> C -> D -> BackToRoot()
```

行为：

- `D` 播放关闭动画。
- `B`、`C` silent finalize，不播放关闭动画。
- 中间页不会因为 Lifecycle 遮挡先被 reopen 再 close。
- root `A` 只恢复或打开一次。

这依赖 UIService 的 `CloseManyAsync`。Router 会先让 UIService 批量关闭目标以上的非 target UI，再根据目标是否已被遮挡恢复决定是否需要显式 `ShowByRouter(target)`。

### BackTo

```csharp
await GameApp.UI.Router.BackTo<UIHomeWindow>();
await GameApp.UI.Router.BackTo<UIHomeWindow>(openIfMissing: false);
await GameApp.UI.Router.BackTo<UIHomeWindow>(openIfMissing: true, "arg");
```

规则：

- 命中 history 时，返回最近的目标类型，并恢复该 history entry 保存的参数。
- 未命中且 `openIfMissing == true` 时，执行 `ResetTo<T>(args)`。
- 未命中且 `openIfMissing == false` 时返回 `false`。
- 深回退不会关闭 target TypeHandle；重复 UI 类型只关闭一次。

### ResetTo

```csharp
await GameApp.UI.Router.ResetTo<UIHomeWindow>();
await GameApp.UI.Router.ResetTo<UIHomeWindow>("startup");
```

重建导航栈，只保留目标页作为 root entry。目标打开失败时不破坏旧页面和旧 history。

### ResetHistory

```csharp
GameApp.UI.Router.ResetHistory();
```

只清空 Router history 并清除 dirty 状态，不关闭任何实际 UI。
如果当前正在导航，或存在尚未完成的 pending route show，调用会被忽略，避免同步修改 history 破坏异步导航事务。

### SyncFromCurrentUI

```csharp
GameApp.UI.Router.SyncFromCurrentUI(typeof(UIHomeWindow));
GameApp.UI.Router.SyncFromCurrentUI(typeof(UIHomeWindow).TypeHandle, "arg");
```

用于把当前实际显示的 UI 类型设为新的 Router root。它只修复 Router history，不打开或关闭实际 UI。
如果当前正在导航，或存在尚未完成的 pending route show，调用会被忽略。

### Router 状态

```csharp
bool canBack = GameApp.UI.Router.CanBack;
Type currentType = GameApp.UI.Router.Current;
UIRouteEntry currentEntry = GameApp.UI.Router.CurrentEntry;
```

`CurrentEntry` 返回快照，不要修改后期待影响 Router 内部 history。

### Router 使用建议

- Page 级 UI 使用 Router：`NavigateTo`、`Back`、`BackToRoot`、`CloseCurrent`。
- 非 routed 弹窗、Tips、临时 UI 可以继续使用 `ShowUI` / `CloseSelf`。
- 不要让 UIService 参与 Router history；UIService 只管理实际 UI 生命周期。
- routed page 的直接关闭入口会在 UIService 源头转交 Router；关键业务流程仍建议显式调用 Router API。
- Router API 是异步事务，按钮里可以 `.Forget()`，但关键流程建议 `await` 并检查返回值。
- `BackTo` / `BackToRoot` 遇到同层事务正在播放动画时可能返回 `false`；如果没有实际关闭任何 UI，不会把 Router 标记为 dirty，调用方可稍后重试。

## 接收打开参数

`ShowUI`、`ShowUISync`、Router `NavigateTo`、`Replace`、`ResetTo` 的 `params object[]` 会传到窗口内部。

```csharp
public sealed class ItemTipsWindow : UIWindow<ui_ItemTipsWindow>
{
    protected override void OnOpen()
    {
        int itemId = UserData is int value ? value : 0;
        object[] args = UserDatas;
    }
}
```

Router 会复制参数数组保存到 history。`Back` / `BackTo` 恢复旧页面时会使用对应 history entry 保存的参数。

## UI 事件自动解绑

窗口可重写 `OnRegisterEvent`，通过 `EventListenerProxy` 注册 UI 事件。窗口销毁时会自动移除。

```csharp
public sealed class SettingsWindow : UIWindow<ui_SettingsWindow>
{
    protected override void OnRegisterEvent(EventListenerProxy proxy)
    {
        proxy.AddUIEvent<LocalizationChangeEvent>(OnLanguageChanged);
    }

    private void OnLanguageChanged(in LocalizationChangeEvent evt)
    {
        RefreshText();
    }
}
```

## Widget

`UIWidget<T>` 适合窗口内部子界面、分页内容、列表详情。

```csharp
public sealed class BagWindow : UIWindow<ui_BagWindow>
{
    private BagDetailWidget _detail;

    protected override async UniTask OnInitializeAsync()
    {
        _detail = await CreateWidgetAsync<BagDetailWidget>(baseui.DetailRoot, false);
    }

    protected override void OnOpen()
    {
        _detail.Open(10001);
    }

    protected override void OnClose()
    {
        _detail.Close();
    }
}

public sealed class BagDetailWidget : UIWidget<ui_BagDetailWidget>
{
    protected override void OnOpen()
    {
        int itemId = UserData is int value ? value : 0;
    }
}
```

常用 API：

```csharp
T widget = await CreateWidgetAsync<T>(Transform parent, bool visible = true);
T widget = await CreateWidgetAsync<T>(UIHolderObjectBase holder, bool destroyHolderOnDispose = false);
T widget = CreateWidgetSync<T>(Transform parent, bool visible = true);
T widget = CreateWidgetSync<T>(UIHolderObjectBase holder, bool destroyHolderOnDispose = false);
await RemoveWidget(widget);

widget.Open(args);
await widget.OpenAsync(args);
widget.Close();
await widget.CloseAsync();
widget.Destroy();
```

`CreateWidgetAsync` 会等待可见 Widget 的打开动画完成后返回。`CreateWidgetSync` 会同步完成资源绑定和 `OnInitialize`，然后以后台任务播放打开动画；返回时可见 Widget 可能仍处于 `Opening`。

## Tab 窗口

`UITabWindow<T>` 内置虚拟 Tab 注册和切换。

```csharp
public sealed class RoleWindow : UITabWindow<ui_RoleWindow>
{
    protected override void OnInitialize()
    {
        InitTabVirtuallyView<RoleInfoTab>(baseui.TabRoot);
        InitTabVirtuallyView<RoleEquipTab>(baseui.TabRoot);

        baseui.BtnInfo.onClick.AddListener(() => SwitchTab(0));
        baseui.BtnEquip.onClick.AddListener(() => SwitchTab(1));
    }
}
```

第一次切换到某个 Tab 时会异步创建，后续切换复用已加载实例。

## Holder 生成

Holder 是 UI 绑定工具生成的类，继承 `UIHolderObjectBase`，挂在 UI 预制体上，负责暴露序列化控件引用。

生成示例：

```csharp
using AlicizaX.UI.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [UIRes(ui_LoginWindow.ResTag, EUIResLoadType.AssetBundle)]
    public class ui_LoginWindow : UIHolderObjectBase
    {
        public const string ResTag = "LoginWindow";

        [SerializeField] private Button mBtnLogin;
        public Button BtnLogin => mBtnLogin;
    }
}
```

`UIResAttribute` 写在 Holder 类上，不写在窗口逻辑类上。

生成入口：

```text
AlicizaX/UISetting Window
```

右键 UI Prefab：

```text
UI生成绑定
UI生成绑定 仅复制属性
```

常用命名：

```text
Btn@Login
Img@BackGround
Text@Title
ScrollView@ItemList
Btn#Img@Close
*Img@Star*0
*Img@Star*1
```

Widget 节点不需要写绑定符号。只要子节点上挂了 `UIHolderObjectBase`，生成器会把它作为 Widget 引用收集。

## 手动创建 Holder

少数场景可手动创建 Holder，例如临时 Tips 多实例。生命周期需要业务自行维护。

```csharp
await UIHolderFactory.CreateUIHolderAsync<ui_UILogicTestAlert>(parent);
ui_UILogicTestAlert holder = UIHolderFactory.CreateUIHolderSync<ui_UILogicTestAlert>(parent);
```

## API 速查

| API | 说明 |
| --- | --- |
| `GameApp.UI.Router` | Page 级导航 Router |
| `IUIRouter.NavigateTo<T>()` | 前进导航到 Page |
| `IUIRouter.Replace<T>()` | 替换当前 Page |
| `IUIRouter.Back()` | 返回上一条 history |
| `IUIRouter.CloseCurrent()` | routed page 关闭当前页 |
| `IUIRouter.BackToRoot()` | 深回退到 root |
| `IUIRouter.BackTo<T>()` | 回退到最近的指定 Page |
| `IUIRouter.ResetTo<T>()` | 重建 Router history，只保留目标 root |
| `IUIRouter.ResetHistory()` | 只清空 Router history，不关闭 UI |
| `IUIRouter.SyncFromCurrentUI(...)` | 用当前实际 UI 重建 Router root |
| `IUIService.ShowUI<T>()` | 异步打开 UI |
| `IUIService.ShowUIResult<T>()` | 异步打开并返回精确状态 |
| `IUIService.ShowUISync<T>()` | 同步准备 UI 并启动打开动画，立即返回 View |
| `IUIService.CloseUI<T>(bool force)` | 关闭 UI |
| `IUIService.CloseUIAsync<T>(bool force)` | 异步关闭 UI |
| `IUIService.CloseManyAsync(...)` | 批量关闭 UIService 栈项，主要供 Router 使用 |
| `IUIService.TryCloseTopAsync(...)` | 关闭最上层匹配 UI |
| `IUIService.IsOpen<T>()` | 查询 UI 是否稳定打开 |
| `IUIService.GetUI<T>()` | 获取已打开 UI |
| `IUIService.GetLayer(UILayer)` | 获取层级根节点 |
| `UIWindow<T>.CloseSelf()` | 关闭自身；若自身是 Router 当前页，会转交 Router |
| `UIWindow<T>.ForceCloseSelf()` | 强制关闭自身；若自身是 Router 当前页，会转交 Router |
| `UITabWindow<T>.CloseSelf(bool)` | TabWindow 关闭自身 |
| `UIBase.CreateWidgetAsync<T>()` | 创建 Widget |
| `UIBase.CreateWidgetSync<T>()` | 同步准备 Widget 并启动打开动画 |
| `UIBase.RemoveWidget(UIBase)` | 移除 Widget |
| `UIWidget.Open/Close/Destroy` | Widget 自身打开、关闭、销毁 |
| `UIMetaRegistry.Register(...)` | 手动注册窗口元数据 |
| `UIResRegistry.Register(...)` | 手动注册 Holder 资源 |

## 注意事项

1. `WindowAttribute` 写在窗口逻辑类上；`UIResAttribute` 写在 Holder 类上。
2. 窗口初始化重写 `OnInitialize` / `OnInitializeAsync`；`OnInitializeAsync` 默认会调用 `OnInitialize`。
3. `ShowUISync` 会同步完成资源绑定和 `OnInitialize`，但不等待打开动画完成，也不会执行 `OnInitializeAsync` 中真正异步的逻辑；需要完整异步初始化和动画完成时使用 `ShowUI`。
4. routed page 使用 Router 打开；关闭建议用 Router API。`CloseSelf` / `CloseUI` 命中 Router 当前页时会自动转交 Router。
5. `IsOpen` 只表示 UIBase 状态，不代表 Router 当前页。
6. `Lifecycle` 遮挡会触发被遮挡窗口关闭和恢复打开。深回退必须走 Router，避免中间页被恢复后再关闭。
7. 重复 `ShowUI` 打开已初始化后的窗口会刷新参数并触发 `OnRefresh`，包括 `Opening` 阶段。
8. Router 发生事务失败时可能进入 dirty 状态。dirty 时导航 API 会返回 `false`，需要业务决定是 `ResetHistory`、`SyncFromCurrentUI` 还是重建 UI 流程。导航中或 pending route show 未完成时，`ResetHistory` / `SyncFromCurrentUI` 会被忽略。
9. UI 依赖 ObjectPool、Timer、Resource，启动场景需要保证组件注册顺序。
