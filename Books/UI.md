# UI 模块

UI 模块负责窗口创建、显示、关闭、层级、缓存、生命周期、Widget、Tab 窗口和 UI 事件自动解绑。

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

窗口逻辑类使用 `WindowAttribute` 描述显示层级和缓存时间。

```csharp
[Window(UILayer.UI, 30)]
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
    int cacheTime = 0)
```

参数说明：

- `windowLayer`：窗口所在层级。
- `cacheTime`：缓存时间，`-1` 永久缓存，`0` 不缓存，`>= 1` 按秒缓存。

同层多个窗口可以同时打开，框架只做深度排序，不会自动隐藏或关闭被盖住的窗口。页面流若需要“打开下一页并处理上一页 / 返回上一页 / 回根”，请使用 `UIRouter`。

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
[Window(UILayer.UI, 30)]
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
[Window(UILayer.UI, 30)]
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

`IUIService` 负责实际 UI 实例、栈、层级、深度排序、缓存和生命周期。

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
- 一次事务内批量关闭目标，最终每个变更层只刷新一次深度排序。
- preflight 不会反射注册 UI，也不会创建 metadata；未知 handle 返回 `UnknownHandle`。
- 重复 handle 会被折叠，已缓存或不在栈中的 handle 会被跳过。

## UIRouter

`IUIRouter` 管理 Page 级导航历史。Router 只负责 history 和导航事务，不负责 UI 实例、层级、缓存；实际开关窗仍通过 UIService。

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
UIRouteResult result = await GameApp.UI.Router.NavigateTo<UITestAWindow>();
if (!result.Success)
{
    // result.Status: RejectedBusy / RejectedDirty / OpenFailed / CloseFailed ...
}

await GameApp.UI.Router.NavigateTo<UIShopWindow>(shopId);
```

规则：

- 每次成功导航会加入一条 Router history。
- 参数会浅拷贝保存到 history，避免调用方后续修改数组影响回放。
- 如果目标类型等于当前 history 顶部类型，会刷新当前页参数，不新增 history。
- `A -> B -> C -> D -> NavigateTo<A>()` 是正常前进导航，history 会变成 `A, B, C, D, A`，此后 `Back()` 返回 `D`。
- 前进导航流程：先打开目标页，成功后再关闭旧 current（若类型不同），最后写入 history。
- Router 关闭必须真实完成；layer busy 时返回 `RejectedBusy`，不会把“入队成功”当关闭完成，也不会提交 history。
- history 上限 64；新增条目超限返回 `RejectedLimit`，不会静默截断。
- 目标打开失败时不修改 history；旧页关闭失败时会按事务规则回滚，必要时进入 dirty。

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

相邻回退时，如果目标页仍处于打开状态，Router 会跳过重复 `ShowByRouter`。

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
- root `A` 若仍打开则直接保留，否则再 `ShowByRouter` 一次。

这依赖 UIService 的 `CloseManyAsync`。Router 会先批量关闭目标以上的非 target UI，再根据目标是否仍打开决定是否需要显式 `ShowByRouter(target)`。

### BackTo

```csharp
await GameApp.UI.Router.BackTo<UIHomeWindow>();
await GameApp.UI.Router.BackTo<UIHomeWindow>(openIfMissing: false);
await GameApp.UI.Router.BackTo<UIHomeWindow>(openIfMissing: true, "arg");
```

规则：

- 命中 history 时，返回最近的目标类型，并恢复该 history entry 保存的参数。
- 未命中且 `openIfMissing == true` 时，执行 `ResetTo<T>(args)`。
- 未命中且 `openIfMissing == false` 时返回 `NotFound`。
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
如果当前正在导航，调用会被忽略，避免同步修改 history 破坏异步导航事务。

### SyncFromCurrentUI

```csharp
GameApp.UI.Router.SyncFromCurrentUI(typeof(UIHomeWindow));
GameApp.UI.Router.SyncFromCurrentUI(typeof(UIHomeWindow).TypeHandle, "arg");
```

用于把当前实际显示的 UI 类型设为新的 Router root。它只修复 Router history，不打开或关闭实际 UI。
如果当前正在导航，调用会被忽略。
目标类型必须当前处于 `Opened`；否则拒绝同步（Editor 会记 warning）。

### Router 状态

```csharp
bool canBack = GameApp.UI.Router.CanBack;
Type currentType = GameApp.UI.Router.Current;
UIRouteEntry currentEntry = GameApp.UI.Router.CurrentEntry;
UIRouteResult result = await GameApp.UI.Router.Back();
if (result.Status == UIRouteStatus.RejectedBusy)
{
    // 可稍后重试
}
```

`CurrentEntry` 返回快照，不要修改后期待影响 Router 内部 history。
`UIRouteResult` 可隐式转 `bool`，但关键流程建议读 `Status`：

| Status | 含义 | 建议 |
|---|---|---|
| `Success` | 成功 | - |
| `RejectedBusy` | 同层事务忙 | 稍后重试 |
| `RejectedDirty` | Router dirty | `ResetTo` / `SyncFromCurrentUI` / `ResetHistory` |
| `RejectedLimit` | history 已满 | 收敛导航深度或 `BackTo/ResetTo` |
| `NotFound` | 无目标/无 history | 检查调用时机 |
| `OpenFailed` / `CloseFailed` | 开关窗失败 | 查 UIService/资源；必要时 dirty 恢复 |
| `InvalidTarget` | 目标类型非法 | 检查类型 |

### Router 使用建议

- Page 级 UI 使用 Router：`NavigateTo`、`Back`、`BackToRoot`、`CloseCurrent`。
- 非 routed 弹窗、Tips、临时 UI 可以继续使用 `ShowUI` / `CloseSelf`。
- 不要让 UIService 参与 Router history；UIService 只管理实际 UI 生命周期。
- routed page 的直接关闭入口会在 UIService 源头转交 Router；关键业务流程仍建议显式调用 Router API。
- Router API 是异步事务，按钮里可以 `.Forget()`，但关键流程建议 `await` 并检查返回值。
- `BackTo` / `BackToRoot` 遇到同层事务忙时返回 `RejectedBusy`；未实际关闭 UI 时不会标 dirty，调用方可稍后重试。

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
| `IUIRouter.NavigateTo<T>()` | 前进导航到 Page，返回 `UIRouteResult` |
| `IUIRouter.Replace<T>()` | 替换当前 Page，返回 `UIRouteResult` |
| `IUIRouter.Back()` | 返回上一条 history，返回 `UIRouteResult` |
| `IUIRouter.CloseCurrent()` | routed page 关闭当前页，返回 `UIRouteResult` |
| `IUIRouter.BackToRoot()` | 深回退到 root，返回 `UIRouteResult` |
| `IUIRouter.BackTo<T>()` | 回退到最近的指定 Page，返回 `UIRouteResult` |
| `IUIRouter.ResetTo<T>()` | 重建 Router history，只保留目标 root |
| `IUIRouter.ResetHistory()` | 只清空 Router history，不关闭 UI |
| `IUIRouter.SyncFromCurrentUI(...)` | 用当前已 Opened 的 UI 重建 Router root |
| `UIRouteResult` / `UIRouteStatus` | 导航结果；可隐式转 `bool`，关键路径读 `Status` |
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

## 同层命令队列

同一 `UILayer` 上的异步 `ShowUI` / `CloseUI` 走**统一 FIFO 命令队列**，不要求业务对同层连续调用 `await`。

规则：

- 同层 `Show` / `Close` 按入队顺序串行执行，避免并发改栈。
- 同 type 折叠：
  - pending `Show` + 新 `Show`：覆盖参数并 join 同一 completion。
  - pending `Show` + `Close`：取消 pending `Show`（`Cancelled`）；若实例已在栈中仍继续真正 Close。
  - pending `Close` + `Close`：join 已有 Close completion；`force` 可升级。
- 正在进行的 `Show` 收到 `Close` 时：发 `RequestCancelShowLoad` 取消加载，**不插队**，等当前命令结束后再 drain Close。
- `ShowUISync` **不入队**：仅当该层完全 idle 时执行；层忙或已有异步 Show 时返回 `null`。
- `CloseManyAsync` 在 preflight 阶段若目标层有 pending 命令会拒绝，避免绕过 FIFO。

## Show 结果语义

`UIShowResultState`：

| State | 含义 | 典型场景 |
| --- | --- | --- |
| `Opened` | 成功打开（稳定或已接受） | 正常 `ShowUI` / `ShowUIResult` |
| `Cancelled` | 被业务取消或被后续操作顶替，**不是故障** | 加载中 `CloseUI`、pending Show 被 Close 折叠、服务销毁清空队列 |
| `Failed` | 真实失败 | 资源加载失败、初始化异常、层事务阻塞且无法开始、非法参数 |

业务侧：

```csharp
UIShowResult result = await GameApp.UI.ShowUIResult<HomeWindow>();
if (result.State == UIShowResultState.Opened) { /* 使用 result.View */ }
else if (result.State == UIShowResultState.Cancelled) { /* 正常取消，无需当错误处理 */ }
else { /* Failed：查日志/资源 */ }
```

`await ShowUI<T>()` 在 `Cancelled` / `Failed` 时返回 `null`，无法区分原因；需要精确语义时用 `ShowUIResult`。

## 日志与警告语义

原则：**取消静默，故障告警**。预期的打断/取消路径不打 Warning；真实错误才 `Error` / 有针对性的 `Warning`。

### Editor Warning（`WarnUIOperation`，可开关）

开关：`UIWarningSettings.OtherWarningsEnabled`（EditorPrefs，默认开）。仅 Editor 编译进包。

| 文案 | 何时出现 | 原因 | 是否预期 |
| --- | --- | --- | --- |
| `Show invalid after resource creation` | 资源创建/绑定结束后，元数据仍无效，且**不是**取消 | 资源加载成功但 View 未绑定、状态仍为 `CreatedUI`、或非法状态 | **否**。加载中被 Close 取消 → `Cancelled`，**不打此 Warning** |
| `Show init failed` | `InternalInitlized` 返回 false，且**不是**取消 | `OnInitialize`/`OnInitializeAsync` 失败，或初始化后状态非法 | **否**。版本失效/CTS 取消 → `Cancelled`，无 Warning；真实异常在 UIBase 已有 `Error` |
| `Show open rejected` | `InternalOpen` 未接受，且结果为 `Failed`、operation 仍当前 | 打开状态机拒绝、View 丢失、不在栈中等真实失败 | **否**。被 Close/新操作打断 → `Cancelled`，无 Warning |
| `ShowSync invalid after resource creation` | Sync 路径资源创建后无效 | 同步资源/绑定问题 | **否**（Sync 无取消令牌，一般不是“取消”） |
| `ShowSync init failed` | Sync 初始化失败 | 同步 `OnInitialize` 失败 | **否** |
| `Close interrupted` | `InternalClose` 后未进入 `Closed`，且 **operation 仍当前** | 关闭流程中途失败，当前关闭操作仍有效 | **否**。被新 Show/Close 顶替导致 version 变化 → **不告警** |

### Error（真实故障，运行时也保留）

| 文案/位置 | 原因 |
| --- | --- |
| `UI resource load failed` / `missing holder component` | 资源路径错误或预制体缺 Holder |
| `Failed to create UI instance` / Metadata 注册失败 | 类型/注册表问题 |
| `Async/Sync initialize failed` / `OnOpen` / `OnClose` / transition failed | 业务生命周期或动画抛异常 |
| `ShowUISync rejected while show is in progress` | 同实例异步 Show 进行中又调 Sync；Sync 不 join 半开 View |
| UI 根节点 / Canvas / Camera 缺失 | 启动配置问题 |

### 明确**不会**再告警的路径

这些属于正常并发/取消，控制台不应出现 Warning：

1. `ShowUI` 加载资源过程中调用 `CloseUI` 同一类型 → 取消加载、销毁未绑定实例 → `Cancelled`。
2. 同层 pending `Show` 被后续 `Close` 折叠 → completion 设为 `Cancelled`。
3. Opening / Closing 动画过程中被新操作打断生命周期版本 → 回滚状态，静默失败返回。
4. UI 服务销毁时清空未执行的队列命令 → pending Show 记为 `Cancelled`。

### 调试提示

- 若看到 `Show invalid after resource creation` 且业务刚调了 `CloseUI`：检查框架版本是否已按“取消→Cancelled”修复；正常不应再出现。
- 若 `ShowUI` 返回 `null` 但无任何 Error：多半是 `Cancelled` 或层 busy 的 `Failed`；用 `ShowUIResult` 看 `State`。
- Router 导航失败看 `UIRouteResult.Status`，不要只看控制台 Warning。

## 注意事项

1. `WindowAttribute` 写在窗口逻辑类上；`UIResAttribute` 写在 Holder 类上。
2. 窗口初始化重写 `OnInitialize` / `OnInitializeAsync`；`OnInitializeAsync` 默认会调用 `OnInitialize`。
3. `ShowUISync` 会同步完成资源绑定和 `OnInitialize`，但不等待打开动画完成，也不会执行 `OnInitializeAsync` 中真正异步的逻辑；需要完整异步初始化和动画完成时使用 `ShowUI`。
4. routed page 使用 Router 打开；关闭建议用 Router API。`CloseSelf` / `CloseUI` 命中 Router 当前页时会自动转交 Router。
5. `IsOpen` 只表示 UIBase 状态，不代表 Router 当前页。
6. 深回退（`BackTo` / `BackToRoot`）必须走 Router，依赖 `CloseManyAsync` 批量关闭中间页。
7. 重复 `ShowUI` 打开已初始化后的窗口会刷新参数并触发 `OnRefresh`，包括 `Opening` 阶段。
8. Router 发生事务失败时可能进入 dirty 状态。dirty 时导航 API 返回 `RejectedDirty`，需要业务决定是 `ResetHistory`、`SyncFromCurrentUI` 还是重建 UI 流程。导航中时，`ResetHistory` / `SyncFromCurrentUI` 会被忽略。
9. UI 依赖 ObjectPool、Timer、Resource，启动场景需要保证组件注册顺序。
10. 框架不再提供 `UIOcclusionMode`。同层多窗默认叠层显示；页面导航请用 Router，弹窗继续 `ShowUI` / `CloseSelf`。
11. 同层连续 `ShowUI`/`CloseUI` 可不必 `await` 排队；框架 FIFO 保证顺序。需要结果态时用 `ShowUIResult` / `CloseUIAsync`。
12. 日志语义：取消/打断不告警；资源、初始化、生命周期异常才 `Error`/`Warning`。详见上文「日志与警告语义」。
