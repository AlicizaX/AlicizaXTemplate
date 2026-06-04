# HotkeyComponent

`HotkeyComponent` 依赖 Unity Input System，把配置的 `InputActionReference` 转换为目标组件的 `ISubmitHandler.OnSubmit` 调用。常见目标是 `UXButton` 或 `UXToggle`。

源码位置：

- 本地包：`file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`
- 源码目录：`Client/Packages/com.alicizax.unity.input/Runtime/Hotkey`
- 编辑器目录：`Client/Packages/com.alicizax.unity.input/Editor/Hotkey`

安装说明：先安装 `com.alicizax.unity.ui.extension`。如果项目需要快捷键、导航或 InputGlyph，再安装 `com.alicizax.unity.input`。Unity Package Manager 中本地包会显示为 `com.alicizax.unity.input@file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`。

编译条件：不需要手动添加宏。`com.alicizax.unity.input` 的 asmdef 会在检测到 `com.unity.inputsystem` 后自动生成 `INPUTSYSTEM_SUPPORT`，`HotkeyComponent` 受 `#if INPUTSYSTEM_SUPPORT` 保护。

## 配置方式

1. 给按钮节点挂 `HotkeyComponent`。
2. `Component` 指向同节点上的 `UXButton`。
3. `Holder` 会自动查找父级 `UIHolderObjectBase`。
4. `HotkeyAction` 指向 Input Action。
5. 窗口显示时快捷键生效，窗口关闭或节点禁用时自动解绑。

## 基础示例

```csharp
using AlicizaX.UI.Runtime;
using Game.UI;

[Window(UILayer.Popup)]
public sealed class ConfirmWindow : UIWindow<ui_ConfirmWindow>
{
    protected override void OnInitialize()
    {
        baseui.BtnConfirm.onClick.AddListener(OnConfirm);
        baseui.BtnCancel.onClick.AddListener(CloseSelf);
    }

    private void OnConfirm()
    {
        CloseSelf();
    }
}
```

Prefab 配置：

```text
BtnConfirm: HotkeyAction = UI/Submit
BtnCancel : HotkeyAction = UI/Cancel
```

按钮上的 `HotkeyComponent` 会直接触发 `BtnConfirm` 的提交逻辑，不需要窗口额外轮询输入。

## Inspector 字段

| 字段 | 说明 |
| --- | --- |
| `Component` | 实际接收提交事件的组件，必须实现 `ISubmitHandler` |
| `Holder` | 所属 `UIHolderObjectBase`，用于判断窗口生命周期和优先级 |
| `HotkeyAction` | `InputActionReference`，例如 Confirm、Cancel、Submit |
| `Hotkey Press Type` | `Started` 或 `Performed`，决定在 Input Action 哪个阶段触发 |

## 运行时生命周期

1. `Awake` 和 `OnEnable` 会自动查找同节点上的 `ISubmitHandler`，并查找父级 `UIHolderObjectBase`。
2. `OnEnable` 注册快捷键，`OnDisable` 和 `OnDestroy` 解绑快捷键。
3. 如果 `InputAction` 原本没有启用，快捷键系统会临时启用；最后一个注册者解绑后会恢复禁用。
4. 触发快捷键时，组件会调用目标组件的 `OnSubmit(BaseEventData)`，不会直接调用 `onClick`。

因为 `UXButton.OnSubmit` 会触发 `onClick`，所以按钮快捷键通常只要这样配：

```text
BtnConfirm (UXButton + HotkeyComponent)
├── Component: BtnConfirm 上的 UXButton
├── Holder: 自动找到 ConfirmWindow 的 UIHolderObjectBase
├── HotkeyAction: UI/Submit
└── HotkeyPressType: Performed
```

## 快捷键优先级

快捷键优先级由窗口作用域决定，不是所有打开窗口都会同时响应。系统会在当前可见 UI 中选择最上层叶子窗口：

| 优先规则 | 说明 |
| --- | --- |
| `Canvas.sortingOrder` 更高 | 优先响应 |
| 层级更深 | 当 sortingOrder 相同，子窗口优先 |
| 更晚激活 | 当前两项仍相同，后显示的窗口优先 |
| Canvas 层必须等于 `UIComponent.UIShowLayer` | 不在 UI 显示层的窗口不会成为快捷键作用域 |

同一个窗口内多个控件绑定同一个 `InputAction` 时，后注册的触发器优先。

## 带取消快捷键的弹窗示例

```csharp
using AlicizaX.UI.Runtime;
using Game.UI;

[Window(UILayer.Popup)]
public sealed class ConfirmWindow : UIWindow<ui_ConfirmWindow>
{
    protected override void OnInitialize()
    {
        baseui.BtnConfirm.onClick.AddListener(OnConfirm);
        baseui.BtnCancel.onClick.AddListener(OnCancel);
    }

    private void OnConfirm()
    {
        GameApp.Save.ApplyPendingChange();
        CloseSelf();
    }

    private void OnCancel()
    {
        GameApp.Save.DiscardPendingChange();
        CloseSelf();
    }
}
```

## API 速查

| API | 说明 |
| --- | --- |
| `HotkeyComponent.HotkeyAction` | 设置快捷键 Input Action |
| `UXHotkeyExtension.BindHotKey()` | 手动注册快捷键触发器 |
| `UXHotkeyExtension.UnBindHotKey()` | 手动解绑快捷键触发器 |

## 快捷键不触发排查清单

1. 是否已安装 `com.alicizax.unity.input` 和 `com.unity.inputsystem`。
2. `HotkeyAction` 是否为空，Input Action 是否能正常触发。
3. `Component` 是否实现 `ISubmitHandler`。
4. 按钮是否处于激活和可交互状态。
5. 当前窗口的 Canvas 是否在 `UIComponent.UIShowLayer`。
6. 是否有更上层弹窗拦截了同一个快捷键。
