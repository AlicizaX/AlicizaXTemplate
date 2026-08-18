# UXButton

`UXButton` 继承 `UXSelectable`，用法接近 Unity `Button`，仍然通过 `onClick` 注册点击事件。额外支持：

- 向 UI 音效模块上报 Pointer / Focus / Press，本身不持有 clip。
- 子节点随按钮状态同步颜色或图片。
- Input System 导航选中通知。

源码位置：

- `Client/Packages/com.alicizax.unity.ui.extension/Runtime/UXComponent`

## 基础用法

```csharp
using AlicizaX;
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine.UI;

[Window(UILayer.UI)]
public sealed class LoginWindow : UIWindow<ui_LoginWindow>
{
    protected override void OnInitialize()
    {
        baseui.BtnLogin.onClick.AddListener(OnLoginClick);
        baseui.BtnClose.onClick.AddListener(CloseSelf);
    }

    private void OnLoginClick()
    {
        // 执行登录逻辑。
    }
}
```

临时禁用按钮：

```csharp
baseui.BtnConfirm.interactable = CanDeleteItem(_targetId);
```

确认弹窗示例：

```csharp
using AlicizaX.UI.Runtime;
using Game.UI;

[Window(UILayer.Popup)]
public sealed class DeleteConfirmWindow : UIWindow<ui_DeleteConfirmWindow>
{
    private int _targetId;

    public void SetTarget(int targetId)
    {
        _targetId = targetId;
    }

    protected override void OnInitialize()
    {
        baseui.BtnConfirm.onClick.AddListener(OnConfirmClick);
        baseui.BtnCancel.onClick.AddListener(CloseSelf);
    }

    private void OnConfirmClick()
    {
        GameApp.Inventory.DeleteItem(_targetId);
        CloseSelf();
    }
}
```

## 音效

`UXButton` 不再序列化 hover / click clip，也不再注入 `IUXAudioHelper`。

- 指针进入 / 离开、非指针选中 / 取消选中由 `UXSelectable` 上报。
- 左键点击、`OnSubmit`、`PlayClickFeedback` 上报 `Press`。

主题 clip 配在 `UXUiAudioProfile`，启动场景用 `UXUiAudioBinder` 注入。单个按钮要换音、只要点击音或完全静音，挂 `UXUiAudioOverride`。详见 [UXUiAudio](UXUiAudio.md)。

## Inspector 配置

| 配置 | 说明 |
| --- | --- |
| `Transition` | 沿用 `Selectable` 的状态过渡，通常用 `ColorTint` 或 `SpriteSwap` |
| `Navigation` | 沿用 UGUI 导航配置，可与 `UXNavigationScope` 配合 |
| `Child Transitions` | 继承自 `UXSelectable`，让图标、文本等子节点跟随状态变化 |

## 与 Unity Button 的差异

1. 只响应左键点击，右键不会触发 `onClick`。
2. `OnSubmit` 会触发 `onClick`，所以键盘、手柄和 `HotkeyComponent` 都可以复用同一套按钮逻辑。
3. `onClick` 的类型仍然是 `Button.ButtonClickedEvent`，旧代码里 `baseui.BtnConfirm.onClick.AddListener(...)` 的写法可以保留。
4. 按钮禁用或节点未激活时，`Press()` 会直接返回，不会触发业务回调。
5. 音效走全局 Profile。没挂 `UXUiAudioBinder` 或不想出声时静默跳过，不报错。

## UXSelectable 子节点状态

`UXSelectable` 继承 `Selectable`，并增加子节点过渡列表。适合一个按钮包含图标、文本、背景多层视觉时统一处理状态。

Inspector 配置：

| 配置 | 说明 |
| --- | --- |
| `Transition` | 沿用 UGUI 的 `ColorTint` 或 `SpriteSwap` |
| `Child Transitions` | 需要跟随按钮状态变化的子 `Graphic` |
| `TransitionData.colors` | 子节点各状态颜色 |
| `TransitionData.spriteState` | 子节点各状态图片 |

带图标和文字的页签按钮结构：

```text
BtnBag (UXButton)
├── Bg      (Image，作为按钮根节点 Target Graphic 或普通背景)
├── Icon    (Image，加入 Child Transitions)
└── Label   (TextMeshProUGUI，加入 Child Transitions)
```

推荐配置：

| 节点 | 配置 |
| --- | --- |
| `BtnBag` | `Transition = ColorTint`，根背景使用按钮自己的 `Colors` |
| `Icon` | 在 `Child Transitions` 里配置普通、悬停、按下、选中、禁用颜色 |
| `Label` | 在 `Child Transitions` 里配置同一套文本颜色 |

业务代码只需要处理点击：

```csharp
protected override void OnInitialize()
{
    baseui.BtnBag.onClick.AddListener(() => SwitchTab(0));
    baseui.BtnEquip.onClick.AddListener(() => SwitchTab(1));
}
```

子节点过渡类型跟随根节点 `UXSelectable.transition`，不能在同一控件内混用"根节点颜色过渡、子节点图片过渡"。需要混用时，建议拆成两个控件或用 `UXController + UXBinding` 控制额外状态。

导航行为增强：当 `Navigation.Mode` 是 `Explicit` 时，如果显式配置的目标不可交互，`FindSelectableOnLeft/Right/Up/Down` 会返回 `null`，避免焦点跳到禁用按钮上。

## API 速查

| API | 说明 |
| --- | --- |
| `UXButton.onClick` | 按钮点击事件，类型为 `Button.ButtonClickedEvent` |
| `UXSelectable.navigation` | 配置方向导航 |
| `UXSelectable.interactable` | 启用/禁用按钮 |
| `PlayClickFeedback()` | 上报 `Press` 并做按下视觉反馈 |

## 注意事项

1. `UXButton` 不继承 Unity `Button`，但保留了 `Button.ButtonClickedEvent` 类型的 `onClick`，业务调用方式基本一致。
2. `UXSelectable` 的子节点状态使用当前控件的 `transition` 类型决定是颜色还是图片切换，同一控件内不要混用两种过渡类型。
3. 不要在按钮上再挂 clip 字段。主题进 Profile，特例进 `UXUiAudioOverride`。
