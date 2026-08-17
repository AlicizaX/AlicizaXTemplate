# Navigation 导航模块

`Navigation` 在 Input System 之上管理 UI 焦点域：哪个窗口还活着、谁能当手柄/键盘焦点、谁只挡下层、关闭进缓存后为什么不能再被方向键走进去。

源码位置：

- 本地包：`file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`
- 运行时：`Client/Packages/com.alicizax.unity.input/Runtime/Navigation`
- 编辑器：`Client/Packages/com.alicizax.unity.input/Editor/Navigation`

安装：先装 `com.alicizax.unity.ui.extension`。需要导航、快捷键或 InputGlyph 时再装 `com.alicizax.unity.input`。

编译条件：`com.alicizax.unity.input` 的 asmdef 检测到 `com.unity.inputsystem` 后自动生成 `INPUTSYSTEM_SUPPORT`。导航代码受 `#if INPUTSYSTEM_SUPPORT` 保护，不需要手加 `UX_NAVIGATION`。

## 核心概念

| 概念 | 类型 | 说明 |
| --- | --- | --- |
| 导航系统 | `UXNavigationSystem` | 静态管理器。`InputActionProvider` 初始化时启动，关闭时 Shutdown。 |
| 导航域 | `UXNavigationScope` | 挂在窗口或独立面板根上。管理烘焙/运行时 `Selectable`，并按策略改 `Navigation.Mode`。 |
| 存活 | Alive | 节点激活、`canvas.enabled`、Holder/Canvas 处于 `UIComponent.UIShowLayer`。 |
| 可焦点 | Focus / `Navigable` | Alive 且勾选 Focus、且有可用 Selectable 时，才能成为 Top。 |
| 挡板 | Block / `BlockLowerScopes` | Alive 时挡住优先级更低的域。自己可以不当焦点（Loading）。 |
| 选中音抑制 | `UXSelectionAudio` | 程序化补选时压住选中音。 |

已删除：`UXNavigationSkip`、`UXNavigationManager`、`UXNavigationModeListener`。输入设备切换走 `UXInput.Watch`。

## 使用前提

1. 已安装 `com.alicizax.unity.ui.extension` 与 `com.alicizax.unity.input`。
2. 已安装 `com.unity.inputsystem`，并切到 Input System 后端。
3. 场景里有 `EventSystem`。
4. 常驻节点上有 `InputActionProvider`（它会 `UXNavigationSystem.Initialize()`）。

不需要再挂 Navigation Manager。Scope 在 `Awake` 里自己注册。

## 快速接入

### 1. 为窗口添加导航域

在窗口 Prefab 根节点（或独立焦点面板根）上挂 `UXNavigationScope`。

Inspector：

| 字段 | 说明 |
| --- | --- |
| `Default` | 进入该域时默认选中的控件 |
| `Holder` | 绑定 `UIHolderObjectBase`。打开/关闭会刷新导航。点 Refresh 会自动绑。 |
| `Selectables` | 烘焙的静态控件列表。点刷新按钮按层级收集。 |
| `Remember` | 重新打开时优先恢复上次选中 |
| `Focus` | 开：本域可以成为手柄/键盘焦点。关：本域不接收选中。 |
| `Block` | 开：本域存活时挡住下层导航。关：不挡下层。 |

旧 Prefab 没有 `Focus` 字段时，反序列化会当成开启。

### 2. 烘焙静态 Selectable

点 Inspector 上的 Refresh。会收集本 Scope 子树里、导航模式不是 `None` 的 `Selectable`。Prefab 结构变了再点一次。

局部不想进图：不要 bake、不要 `RegisterSelectable`，或把该控件 `Navigation.Mode` 设为 `None`。不要再挂 Skip 组件。

### 3. 动态注册运行时控件

虚拟列表等运行时生成的控件要手动注册：

```csharp
using AlicizaX.UI.UXNavigation;
using UnityEngine;
using UnityEngine.UI;

public sealed class VirtualListItem : MonoBehaviour
{
    private UXNavigationScope _scope;
    private Selectable _selectable;

    private void Awake()
    {
        _selectable = GetComponent<Selectable>();
    }

    public void AttachToScope(UXNavigationScope scope)
    {
        _scope = scope;
        _scope.RegisterSelectable(_selectable, rememberable: true);
    }

    private void OnDestroy()
    {
        if (_scope != null)
        {
            _scope.UnregisterSelectable(_selectable);
        }
    }
}
```

`RecyclerViewNavigationController` 会把自己注册进父级 Scope，见 `RecyclerView.Navigation.md`。

### 4. 强制补选策略

```csharp
UXNavigationSystem.GamepadRequireSelection = true;   // 默认 true
UXNavigationSystem.KeyboardRequireSelection = false; // 默认 false
UXNavigationSystem.SetRequireSelection(gamepad: true, keyboard: false);
```

只在「当前没有合法焦点」时补选。关掉策略不会清掉已有选中。切到手柄/摇杆且策略开启时才会 Ensure。

## Focus / Block 怎么配

两个开关正交，不要再用 Skip。

| 场景 | Focus | Block | 效果 |
| --- | --- | --- | --- |
| 普通窗 / 确认框 | 开 | 开 | 自己当焦点，挡住下层 |
| Loading / 全屏挡板 | **关** | **开** | 自己不选中，下层全部 `Mode.None`，当前选中会被清掉 |
| Toast / 飘字 | 关 | 关 | 当自己不存在，下层继续导航 |
| 可点但不抢手柄的装饰面 | 关 | 按需 | 有按钮也不进 Top |

Loading 示例：

```text
Loading (UXNavigationScope, Focus = 关, Block = 开)
HUD     (UXNavigationScope, Focus = 开, Block = 开)  ← 被挡住，不能导航
```

## 存活 / 焦点 / 压制

每次刷新：

1. **Alive**：`activeInHierarchy`、`canvas.enabled`、Holder（没有 Holder 则看 Canvas）layer == `UIComponent.UIShowLayer`。
2. **Focusable**：Alive && `Navigable` && 至少有一个可用 Selectable。
3. **Occluder**：Alive && `Block`。
4. **Top**：Focusable 里按 `sortingOrder` → 层级深度 → `ActivationSerial` 取最高。若最高挡板自己不可焦点（Loading），Top 为空。
5. **Suppress**：非 Alive、非 Focusable、或被更高 Occluder 挡住 → `Navigation.Mode = None`。

关窗进缓存时 UI 只改 Holder layer、关掉 `canvas.enabled`，对象仍激活。因此 **非 Alive 也必须抑制**，否则 UGUI 的 `FindSelectable` 仍会走进缓存窗。

优先级：

| 顺序 | 规则 |
| --- | --- |
| 1 | `Canvas.sortingOrder` 更高优先 |
| 2 | 层级更深优先 |
| 3 | 最近一次变为 Alive 的序号更大优先 |

## 焦点恢复

```csharp
[Window(UILayer.UI)]
public sealed class InventoryWindow : UIWindow<ui_InventoryWindow>
{
    protected override void OnInitialize()
    {
        // Prefab 上 UXNavigationScope：
        //   Remember = true
        //   Default = 第一格
        //   Focus = 开
        //   Block = 开
    }
}
```

在背包选中第 5 格后打开详情，关掉详情后焦点回到第 5 格。只有烘焙控件、或 `RegisterSelectable(..., rememberable: true)` 的运行时控件会被记住。

## 与 UIHolder 的关系

`Holder` 绑上后订阅 `OnWindowAfterShowEvent` / `OnWindowAfterClosedEvent`，开关窗自动刷新。不侵入 UI 框架的 WindowAttribute。

Alive 不靠 `UIState` 枚举，靠 Holder layer + `canvas.enabled`。缓存层会关 Canvas，关窗会把 Holder 改到 Hide 层，两者都会让域出局并保持抑制。

## API 速查

### UXNavigationSystem

| API | 说明 |
| --- | --- |
| `GamepadRequireSelection` | 手柄/摇杆是否强制补选。默认 true |
| `KeyboardRequireSelection` | 键鼠是否强制补选。默认 false |
| `SetRequireSelection(bool gamepad, bool keyboard)` | 一次改两套策略 |

`Initialize` / `Shutdown` 由 `InputActionProvider` 调用，业务不要自己开。

### UXNavigationScope

| API | 说明 |
| --- | --- |
| `RegisterSelectable(Selectable, bool rememberable = false)` | 注册运行时控件 |
| `UnregisterSelectable(Selectable)` | 注销运行时控件 |
| `NotifySelectableStateChanged()` | interactable/active 变化后通知刷新 |
| `Navigable` | 是否可当焦点（Inspector 名 Focus） |
| `BlockLowerScopes` | 是否挡住下层（Inspector 名 Block） |
| `RememberLastSelection` | 是否记住上次选中 |
| `DefaultSelectable` | 默认选中 |
| `NavigationSuppressed` | 当前是否被写成 `Mode.None` |

### UXSelectionAudio

程序化 `SetSelectedGameObject` 时自动 Begin/End。业务若自己补选且不想播选中音，也可包一层。

## 注意事项

1. Prefab 结构变了要重新 Refresh bake，否则新按钮不在列表里。
2. 运行时生成的控件必须 `RegisterSelectable` / `UnregisterSelectable`。
3. `Block` 只改导航图，不挡鼠标。要彻底点不透，配合 `CanvasGroup.blocksRaycasts` 或关 GraphicRaycaster。
4. 输入类型来自 `UXInput.Watch`，不是旧的 `UXNavigationModeListener`。
5. 不要在选中回调里反复改 Scope 激活状态；刷新有重入保护，但状态会推迟。
6. 局部排除用「不进列表」，不要恢复 `UXNavigationSkip`。
