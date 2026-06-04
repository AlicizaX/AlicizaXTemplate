# Navigation 导航模块

`Navigation` 模块在 Input System 之上提供一套多输入设备的 UI 焦点管理系统。它解决的核心问题是：当玩家在鼠标、键盘、手柄之间切换时，UI 焦点应该跟随哪个窗口、选中哪个控件、以及如何防止焦点泄漏到被遮挡的窗口。

源码位置：

- 本地包：`file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`
- 源码目录：`Client/Packages/com.alicizax.unity.input/Runtime/Navigation`
- 编辑器目录：`Client/Packages/com.alicizax.unity.input/Editor/Navigation`

安装说明：先安装 `com.alicizax.unity.ui.extension`。如果项目需要导航、快捷键或 InputGlyph，再安装 `com.alicizax.unity.input`。Unity Package Manager 中本地包会显示为 `com.alicizax.unity.input@file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`。

编译条件：不再需要手动添加 `UX_NAVIGATION`。`com.alicizax.unity.input` 的 asmdef 会在检测到 `com.unity.inputsystem` 后自动生成 `INPUTSYSTEM_SUPPORT`，导航文件受 `#if INPUTSYSTEM_SUPPORT` 保护。

## 核心概念

| 概念 | 类型 | 说明 |
| --- | --- | --- |
| 输入模式 | `UXInputMode` | 当前使用的输入设备类别：Keyboard / Gamepad / Touch |
| 输入模式监听 | `UXNavigationModeListener` | 自动检测设备输入并维护 `CurrentMode`，设备切换时广播 `OnModeChanged` |
| 导航管理器 | `UXNavigationManager` | 管理所有 `UXNavigationScope`，决定哪个 Scope 是当前顶层焦点域，并执行模式切换处理器 |
| 导航域 | `UXNavigationScope` | 挂在窗口或面板根节点上，定义一个焦点域，管理域内的 `Selectable` 列表 |
| 导航跳过标记 | `UXNavigationSkip` | 挂在节点上，让该节点子树内的所有 `UXNavigationScope` 被排除在导航系统之外 |
| 模式切换处理器 | `IUXNavigationModeChangeProcessor` | 当输入模式或顶层 Scope 变化时执行项目自定义逻辑，例如鼠标显示、背景点击失焦策略 |

## 使用前提

1. 工程已安装 `com.alicizax.unity.ui.extension`。
2. 需要导航功能时，再安装本地包 `file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`。
3. 工程已安装 `com.unity.inputsystem` 并切换到 Input System 输入后端。
4. 启动场景中存在 `EventSystem`（UGUI 标准要求）。
5. 启动场景或常驻服务节点上存在 `UXNavigationManager`。

`UXNavigationModeListener` 会在 `BeforeSceneLoad` 自动创建并标记 `DontDestroyOnLoad`。`UXNavigationManager` 是导航域管理器，需要作为项目服务或常驻组件挂载；它会在初始化时收集已加载的 `UXNavigationScope`。

## 快速接入

### 1. 添加导航管理器

在启动场景或常驻服务节点上添加：

```text
Add Component > UI > UX Navigation Manager
```

默认 `Processor Type` 是 `DefaultUXNavigationModeChangeProcessor`，可配置键盘/手柄模式下鼠标是否显示，以及是否禁止空白区域点击导致失焦。

### 2. 为窗口添加导航域

在窗口 Prefab 的根节点（或需要独立焦点域的面板根节点）上挂 `UXNavigationScope`。

Inspector 常用配置：

| 字段 | 说明 |
| --- | --- |
| `Default Selectable` | 进入该域时默认聚焦的控件 |
| `Holder` | 绑定 `UIHolderObjectBase`，窗口打开/关闭时自动触发刷新 |
| `Baked Selectables` | 编辑器烘焙的静态 `Selectable` 列表，点击 Inspector 上的刷新按钮生成 |
| `Remember Last Selection` | 是否记住上次选中的控件，下次进入时恢复 |
| `Block Lower Scopes` | 为 `true` 时，压制所有优先级更低的 Scope 的导航，防止焦点泄漏 |

### 3. 烘焙静态 Selectable 列表

在 Inspector 中点击 `UXNavigationScope` 组件上的 **Refresh** 按钮，系统会扫描子树内所有 `Selectable` 并写入 `_bakedSelectables`。静态窗口只需烘焙一次；如果 Prefab 结构变化，重新点击即可。

### 4. 动态注册运行时控件

虚拟列表等运行时生成的控件需要手动注册：

```csharp
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
        _scope.RegisterSelectable(_selectable);
    }

    private void OnDestroy()
    {
        if (_scope != null)
            _scope.UnregisterSelectable(_selectable);
    }
}
```

### 5. 监听输入模式变化

```csharp
using AlicizaX.UI.UXNavigation;
using UnityEngine;
using UnityEngine.UI;

public sealed class CursorVisibilityController : MonoBehaviour
{
    private void OnEnable()
    {
        UXNavigationModeListener.OnModeChanged += OnModeChanged;
        Refresh(UXNavigationModeListener.CurrentMode);
    }

    private void OnDisable()
    {
        UXNavigationModeListener.OnModeChanged -= OnModeChanged;
    }

    private void OnModeChanged(UXInputMode mode)
    {
        Refresh(mode);
    }

    private void Refresh(UXInputMode mode)
    {
        // 手柄模式下隐藏鼠标光标，切回键鼠或触屏模式时恢复。
        bool showCursor = mode == UXInputMode.Keyboard || mode == UXInputMode.Touch;
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
```

### 6. 实现模式切换处理器

当顶层 Scope 或输入模式发生变化时，`UXNavigationManager` 会回调 `IUXNavigationModeChangeProcessor`。适合在这里驱动手柄光标显示、位置更新、动画或项目自定义失焦策略。

```csharp
using AlicizaX.UI.UXNavigation;
using UnityEngine;

[System.Serializable]
public sealed class GamepadCursorPolicy : IUXNavigationModeChangeProcessor
{
    public void OnNavigationModeChanged(
        UXInputMode mode,
        UXNavigationScope previousTopScope,
        UXNavigationScope currentTopScope)
    {
        bool isGamepad = mode == UXInputMode.Gamepad;
        Cursor.visible = !isGamepad;
        Cursor.lockState = isGamepad ? CursorLockMode.Locked : CursorLockMode.None;
        // 根据 currentTopScope 和 mode 更新手柄光标的目标位置。
    }
}
```

实现后，在 `UXNavigationManager` 的 `Processor Type` 中选择该类型。处理器需要可序列化、非 `MonoBehaviour`，并提供无参构造函数。

## 顶层 Scope 选择规则

`UXNavigationManager` 在每次状态刷新时从所有已注册的 `UXNavigationScope` 中选出优先级最高的一个作为顶层 Scope。选择条件：

1. Scope 所在节点处于激活状态。
2. Scope 所在 Canvas 处于 `UIComponent.UIShowLayer`（UI 显示层）。
3. Scope 未被 `UXNavigationSkip` 标记排除。
4. Scope 内至少有一个 `IsActive() && IsInteractable()` 的 `Selectable`。

满足以上条件的 Scope 之间按以下规则比较优先级（依次降级）：

| 优先级 | 规则 |
| --- | --- |
| 1 | `Canvas.sortingOrder` 更高的 Scope 优先 |
| 2 | 层级深度（父节点数量）更深的 Scope 优先 |
| 3 | `ActivationSerial`（最近一次变为可用的序号）更大的 Scope 优先 |

## 导航压制（Block Lower Scopes）

当顶层 Scope 的 `BlockLowerScopes = true` 时，运行时会把所有优先级更低的 Scope 内的 `Selectable.navigation.mode` 临时设为 `None`，阻止手柄焦点跳入被遮挡的窗口。顶层 Scope 切换时，被压制的 Scope 会自动恢复原始导航配置。

典型场景：弹窗打开时，弹窗的 Scope 成为顶层，背景窗口的所有按钮导航被压制，手柄无法越过弹窗操作背景。

```text
MainWindow (UXNavigationScope, BlockLowerScopes = false)
└── ConfirmPopup (UXNavigationScope, BlockLowerScopes = true)  ← 顶层
    ├── BtnConfirm
    └── BtnCancel
```

## 焦点恢复（Remember Last Selection）

```csharp
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine.UI;

[Window(UILayer.UI)]
public sealed class InventoryWindow : UIWindow<ui_InventoryWindow>
{
    protected override void OnInitialize()
    {
        // UXNavigationScope 挂在 Prefab 根节点，Inspector 中已配置：
        //   RememberLastSelection = true
        //   DefaultSelectable = baseui.BtnFirstItem
        //   BlockLowerScopes = true
    }
}
```

玩家在背包中选中第 5 格后打开装备详情弹窗，关闭弹窗后焦点会自动回到第 5 格，而不是重置到第 1 格。

## 排除子树（UXNavigationSkip）

在不希望参与导航系统的节点上挂 `UXNavigationSkip`。该节点子树内的所有 `UXNavigationScope` 都会被排除，不会成为顶层 Scope，也不会被压制。

常见用途：

- 常驻 HUD 上的辅助按钮，不希望被弹窗的 `BlockLowerScopes` 压制。
- 调试面板，不参与正式导航流程。

```text
HUD (UXNavigationSkip)          ← 整个 HUD 子树排除在外
└── BtnMenu (UXNavigationScope)  ← 不会被任何 Scope 压制，也不会成为顶层
```

## 与 UIHolderObjectBase 集成

`UXNavigationScope` 的 `Holder` 字段绑定 `UIHolderObjectBase` 后，会自动订阅窗口的 `OnWindowAfterShowEvent` 和 `OnWindowAfterClosedEvent`，在窗口打开或关闭时触发导航状态刷新，无需业务代码手动通知。

```csharp
// Prefab Inspector 中配置：
// UXNavigationScope._holder = GetComponent<UIHolderObjectBase>()
// 运行时窗口打开/关闭时自动刷新，无需额外代码。
```

## API 速查

### UXInputMode

```csharp
public enum UXInputMode : byte
{
    Touch    = 0,   // 触屏
    Keyboard = 1,   // 键盘
    Gamepad  = 2,   // 手柄
}
```

### UXNavigationModeListener

| API | 说明 |
| --- | --- |
| `static UXInputMode CurrentMode` | 当前输入模式（只读） |
| `static event Action<UXInputMode> OnModeChanged` | 输入模式变化时触发 |
| `static bool GamepadRequireLowFocus` | 手柄模式下是否强制保持一个可导航焦点 |
| `static bool KeyBoardRequireLowFocus` | 键盘模式下是否强制保持一个可导航焦点 |

### UXNavigationManager

| API | 说明 |
| --- | --- |
| `static bool IsHolderWithinTopScope(UIHolderObjectBase)` | 判断某个 Holder 是否在当前顶层 Scope 内 |

### UXNavigationScope

| API | 说明 |
| --- | --- |
| `void RegisterSelectable(Selectable)` | 运行时注册动态控件 |
| `void UnregisterSelectable(Selectable)` | 运行时注销动态控件 |
| `Selectable GetPreferredSelectable()` | 获取该域的首选聚焦控件 |
| `bool BlockLowerScopes` | 是否压制低优先级 Scope 的导航 |
| `bool RememberLastSelection` | 是否记住上次选中项 |
| `int ActivationSerial` | 该 Scope 最近一次变为可用的序号，由运行时维护 |

### IUXNavigationModeChangeProcessor

```csharp
public interface IUXNavigationModeChangeProcessor
{
    void OnNavigationModeChanged(
        UXInputMode mode,
        UXNavigationScope previousTopScope,
        UXNavigationScope currentTopScope);
}
```

## 注意事项

1. `UXNavigationScope` 的 `_bakedSelectables` 在编辑器中烘焙，Prefab 结构变化后需要重新点击 Refresh，否则运行时可能找不到新增的控件。
2. 运行时动态生成的控件（虚拟列表项等）必须手动调用 `RegisterSelectable` / `UnregisterSelectable`，烘焙列表不会自动感知。
3. `BlockLowerScopes` 只压制导航，不影响鼠标点击。鼠标仍然可以点击被压制窗口的按钮，如需完全屏蔽需要配合 `CanvasGroup.blocksRaycasts`。
4. `UXNavigationModeListener` 对摇杆输入有噪声过滤（`sqrMagnitude >= 0.04f`），轻微抖动不会触发模式切换。合成控件（`synthetic`）也会被忽略，避免虚拟输入误触发。
5. 启动时有 30 帧的设备探测窗口（`InitialDeviceProbeFrames`），用于检测已连接的手柄。在此期间输入模式可能还未稳定，不要在 `Awake` 中依赖 `CurrentMode` 的最终值。
6. `UXNavigationManager` 内部有重入保护，`FlushStateIfDirty` 不会递归触发。但业务代码不应在 `IUXNavigationModeChangeProcessor.OnNavigationModeChanged` 回调中直接修改 Scope 的激活状态，否则会推迟到下一帧刷新。
