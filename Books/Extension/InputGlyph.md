# InputGlyph 输入图标

`InputGlyph` 用于把 Unity Input System 的 `InputAction` 绑定或控制路径解析成 UI 图标、TMP Sprite 标签或可读文本。它只负责图标查询和 UI 展示，不负责设备监听、读取输入、重绑定或震动，这些通用输入能力见 `UXInput.md`。

源码位置：

| 内容 | 路径 |
| --- | --- |
| 运行时代码 | `Client/Packages/com.alicizax.unity.input/Runtime/InputGlyph` |
| 图标查询入口 | `Client/Packages/com.alicizax.unity.input/Runtime/Input/UXInput.Glyph.cs` |
| 数据库编辑器 | `Client/Packages/com.alicizax.unity.input/Editor/InputGlyph/InputGlyphDatabaseEditor.cs` |
| 数据库窗口 | `AlicizaX/Extension/Input/Input Glyph Database` |
| 创建数据库 | `AlicizaX/Extension/Input/Create Input Glyph Database` |

## 组成

| 类型 | 作用 |
| --- | --- |
| `InputActionProvider` | 场景入口组件。持有 `InputActionAsset` 和 `InputGlyphDatabase`，负责初始化 `UXInput.Watch`、`UXInput.Rebind`、`UXInput.Glyph` 和导航系统。 |
| `InputGlyphDatabase` | `ScriptableObject` 数据库，保存不同输入 Profile 下的控制路径到 Sprite/TMP 名称映射。 |
| `UXInput.Glyph` | 静态查询入口，根据当前 `UXInput.Watch.CurrentInputProfile` 查询图标、TMP 标签和显示文本。 |
| `InputGlyphImage` | UI Image 输出组件，把当前动作绑定显示为 `Sprite`。 |
| `InputGlyphText` | TMP Text 输出组件，把当前动作绑定显示为 `<sprite name="...">` 或文本回退。 |
| `InputGlyphBehaviourBase` | 图标 UI 组件基类，处理 Action 来源、Composite Part、Profile 事件和刷新时机。 |
| `InputGlyphPathUtility` | 控制路径规范化工具，把 Input System 路径转换成数据库 glyph key。 |

## 启动方式

在常驻输入节点上挂载：

```text
Add Component > Input > Input Action Provider
```

Inspector 配置：

| 字段 | 说明 |
| --- | --- |
| `Actions` | 项目的 `InputActionAsset`。 |
| `Glyph Database` | 项目的 `InputGlyphDatabase`。 |

运行时 `InputActionProvider` 会执行：

1. 初始化 `UXInput.Watch`。
2. 注入 `UXInput.Glyph.SetDatabase(glyphDatabase)`。
3. 建立 action 查询表。
4. 初始化 `UXInput.Rebind`。
5. 启用 `InputActionAsset`。
6. 初始化 `UXNavigationSystem`。

不要在业务代码里单独传入 `InputGlyphDatabase`。当前设计只有一个全局数据库入口：`UXInput.Glyph.SetDatabase(...)`，通常由 `InputActionProvider` 自动设置。

## InputGlyphDatabase

打开编辑器窗口：

```text
AlicizaX/Extension/Input/Input Glyph Database
```

如果项目还没有数据库资源：

```text
AlicizaX/Extension/Input/Create Input Glyph Database
```

数据库全局字段：

| 字段 | 说明 |
| --- | --- |
| `Placeholder` | 找不到图标或条目无有效图标时使用的占位 Sprite。可以为空。 |

每个 Profile 包含：

| 字段 | 说明 |
| --- | --- |
| `id` | Profile 名称，例如 `Xbox`、`PlayStation`。 |
| `fallbackProfileIds` | 当前 Profile 找不到图标时按顺序查找的兜底 Profile。 |
| `bindingGroupHints` | 用于从 Action 的 binding groups 中优先选择当前 Profile 对应绑定。 |
| `entries` | 控制路径到 Sprite/TMP 名称的映射。 |

每个 Entry 包含：

| 字段 | 说明 |
| --- | --- |
| `controlPaths` | 一个或多个 Input System 控制路径，例如 `<Gamepad>/buttonSouth`。 |
| `sprite` | UI Image 模式使用的图标。 |
| `tmpSpriteName` | TMP Sprite Asset 中的 sprite 名称。为空时尝试使用 `sprite.name`。 |

## 默认 Profile

`Sync Profiles` 会生成固定 Profile。当前默认不包含 `Touch`，因为触摸输入不应该作为通用按键图标数据库默认配置。

| Profile | 默认 Fallback |
| --- | --- |
| `KeyboardMouse` | 无 |
| `GenericGamepad` | 无 |
| `GenericJoystick` | `GenericGamepad` |
| `Xbox` | `GenericGamepad` |
| `PlayStation` | `GenericGamepad` |
| `Switch` | `GenericGamepad`，`Xbox` |
| `SteamDeck` | `Xbox`，`GenericGamepad` |
| `SteamController` | `SteamDeck`，`Xbox`，`GenericGamepad` |

Fallback 使用广度优先队列，并会去重。以 `SteamController` 为例，查找顺序是：

```text
SteamController -> SteamDeck -> Xbox -> GenericGamepad
```

以 `Xbox` 找 `<Gamepad>/buttonSouth` 为例：

```text
Xbox -> GenericGamepad -> Placeholder
```

不会自动去找 `PlayStation`、`Switch` 或 `KeyboardMouse`，除非你把它们写进 `fallbackProfileIds`。

## 控制路径和按钮含义

Gamepad 的控制路径表达的是物理位置，不是按钮文字。不同 Profile 可以使用相同 `controlPaths`，但配置不同图标。

| 控制路径 | Xbox | PlayStation | Switch | Steam Deck |
| --- | --- | --- | --- | --- |
| `<Gamepad>/buttonSouth` | A | Cross | B | A |
| `<Gamepad>/buttonEast` | B | Circle | A | B |
| `<Gamepad>/buttonWest` | X | Square | Y | X |
| `<Gamepad>/buttonNorth` | Y | Triangle | X | Y |
| `<Gamepad>/leftShoulder` | LB | L1 | L | LB |
| `<Gamepad>/rightShoulder` | RB | R1 | R | RB |
| `<Gamepad>/leftTrigger` | LT | L2 | ZL | LT |
| `<Gamepad>/rightTrigger` | RT | R2 | ZR | RT |

因此推荐做法是每个 Profile 都配置同一批常用 `controlPaths`，只换 `sprite` 和 `tmpSpriteName`。

## Glyph Key 规范化

数据库不是直接用原始控制路径查找，而是先转换成 glyph key。

常见例子：

```text
<Gamepad>/buttonSouth           -> gamepad/buttonsouth
<XInputController>/buttonSouth  -> gamepad/buttonsouth
<DualShockGamepad>/buttonSouth  -> gamepad/buttonsouth
<Keyboard>/space                -> keyboard/space
<Mouse>/leftButton              -> mouse/leftbutton
```

`Gamepad`、`XInput`、`DualShock`、`DualSense`、`Switch`、`Nintendo`、`Controller` 等布局会归一为 `gamepad`。这让不同手柄布局可以共享同一批路径，再由 Profile 决定实际图标。

## UI 组件

### InputGlyphImage

用于把 Action 绑定显示为 `Image.sprite`。

```text
Add Component > UI > Input Glyph Image
```

字段：

| 字段 | 说明 |
| --- | --- |
| `Action Source Mode` | 图标来源。 |
| `Action Reference` | 直接指定 `InputActionReference`。 |
| `Hotkey Trigger` | 从同节点或指定 `HotkeyComponentBase` 读取 Hotkey Action。 |
| `Action Name` | 通过 `InputActionProvider.ResolveAction(actionName)` 查询。 |
| `Composite Part Name` | 组合绑定部分，例如 `Up`、`Down`、`Left`、`Right`。 |
| `Target Image` | 要写入 Sprite 的 Image。为空时自动取同节点 `Image`。 |
| `Profile Events` | 当前 Profile 匹配或不匹配时触发 UnityEvent。 |

刷新时机：

| 事件 | 行为 |
| --- | --- |
| `OnEnable` | 自动解析目标并立即刷新。 |
| `UXInput.Watch.OnContextChanged` | 设备 Profile 切换后刷新。 |
| `UXInput.Rebind.OnBindingsChanged` | 绑定改变后刷新。 |

### InputGlyphText

用于把 Action 绑定显示为 TMP Sprite 标签或文本回退。

```text
Add Component > UI > Input Glyph Text
```

字段和 `InputGlyphImage` 基本一致，输出目标为 `Target Text`。为空时自动取同节点 `TMP_Text`。

启用时会缓存当前 TMP 文本作为模板，刷新时用 `{0}` 填入图标或回退文本：

```text
Press {0} to confirm
```

如果找到 TMP Sprite 名称：

```text
Press <sprite name="xbox_a"> to confirm
```

如果找不到图标：

```text
Press A to confirm
```

## Action 来源

| 模式 | 说明 |
| --- | --- |
| `ActionReference` | 直接使用 Inspector 中的 `InputActionReference`。 |
| `HotkeyTrigger` | 从 `HotkeyComponentBase.HotkeyAction` 读取动作，适合按钮热键提示。 |
| `ActionName` | 使用 `InputActionProvider.ResolveAction` 按名称查找。 |

`ActionName` 推荐使用完整路径：

```text
UI/Submit
Gameplay/Jump
```

`InputActionProvider` 只注册 `MapName/ActionName` 形式的完整路径，短名称不会被解析。

## 运行时查询

`UXInput.Glyph` 是当前唯一公开的图标查询入口。

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class GlyphExample : MonoBehaviour
{
    [SerializeField] private InputActionReference action;
    [SerializeField] private Image icon;

    private void Refresh()
    {
        InputAction inputAction = action != null ? action.action : null;
        if (UXInput.Glyph.TryGetUISpriteForActionPath(inputAction, null, out Sprite sprite))
        {
            icon.sprite = sprite;
        }
        else
        {
            icon.sprite = null;
        }
    }
}
```

查询 TMP 标签：

```csharp
if (UXInput.Glyph.TryGetTMPTagForActionPath(
        action.action,
        null,
        out string tag,
        out string fallback))
{
    label.text = $"Press {tag}";
}
else
{
    label.text = $"Press {fallback}";
}
```

查询 Composite Part：

```csharp
InputAction move = InputActionProvider.ResolveAction("Gameplay/Move");
UXInput.Glyph.TryGetUISpriteForActionPath(move, "Up", out Sprite upSprite);
```

直接查询控制路径：

```csharp
UXInput.Glyph.TryGetUISpriteForControlPath("<Gamepad>/buttonSouth", out Sprite sprite);
UXInput.Glyph.TryGetTMPTagForControlPath("<Keyboard>/space", out string tag, out string fallback);
```

显示文本：

```csharp
string displayName = UXInput.Glyph.GetDisplayNameFromInputAction(action.action);
string pathDisplay = UXInput.Glyph.GetDisplayNameFromControlPath("<Keyboard>/space");
```

## API 速查

| API | 说明 |
| --- | --- |
| `UXInput.Glyph.CurrentProfileId` | 当前输入 Profile 名称。 |
| `UXInput.Glyph.SetDatabase(InputGlyphDatabase)` | 设置全局图标数据库。通常由 `InputActionProvider` 调用。 |
| `UXInput.Glyph.GetBindingControlPath(...)` | 获取当前 Profile 下最匹配绑定的有效控制路径。 |
| `UXInput.Glyph.TryGetUISpriteForActionPath(...)` | 根据 Action 绑定获取 UI Sprite。 |
| `UXInput.Glyph.TryGetTMPTagForActionPath(...)` | 根据 Action 绑定获取 TMP Sprite 标签和文本回退。 |
| `UXInput.Glyph.TryGetUISpriteForControlPath(...)` | 根据控制路径获取 UI Sprite。 |
| `UXInput.Glyph.TryGetTMPTagForControlPath(...)` | 根据控制路径获取 TMP Sprite 标签和文本回退。 |
| `UXInput.Glyph.GetDisplayNameFromInputAction(...)` | 获取 Action 当前绑定的可读文本。 |
| `UXInput.Glyph.GetDisplayNameFromControlPath(...)` | 获取控制路径的可读文本。 |
| `UXInput.Glyph.TryGetBindingControl(...)` | 获取当前 Profile 下最匹配的 `InputBinding`。 |
| `UXInput.Glyph.GetGlyphKeyFromControlPath(...)` | 把控制路径转换成数据库 glyph key。 |

图标查询 API 不再允许额外传入 `InputGlyphDatabase`。这样可以避免 UI 局部传入不同数据库导致 Profile、Fallback 和缓存行为不一致。

## AI 辅助配置建议

可以让 AI 生成结构化配置草稿，但不建议让 AI 直接改 Unity `.asset` 中的 Sprite 引用。

推荐流程：

1. 统一图标资源命名，例如 `xbox_a`、`ps_cross`、`generic_button_south`。
2. 让 AI 生成 JSON/YAML 配置，内容包含 Profile、controlPaths、sprite 名称和 tmpSpriteName。
3. 写一个 Editor Importer，根据 sprite 名称在项目里查找 Sprite，再写入 `InputGlyphDatabase.asset`。

示例中间格式：

```yaml
profiles:
  Xbox:
    fallback:
      - GenericGamepad
    entries:
      - controlPaths:
          - "<Gamepad>/buttonSouth"
        sprite: "xbox_a"
        tmpSpriteName: "xbox_a"
  PlayStation:
    fallback:
      - GenericGamepad
    entries:
      - controlPaths:
          - "<Gamepad>/buttonSouth"
        sprite: "ps_cross"
        tmpSpriteName: "ps_cross"
```

## 注意事项

1. `Touch` 不在默认 `InputGlyphDatabase` Profile 中。触摸输入仍然可以被 `UXInput.Watch` 识别，但默认不生成触摸图标表。
2. 同一物理位置的 Gamepad 控制路径在不同平台显示不同按钮文字，差异应放在不同 Profile 的 Sprite 上。
3. `InputGlyphText` 会把启用时的文本作为模板，模板中建议保留 `{0}`。
4. `InputGlyphImage` 找不到图标时会清空 `Image.sprite`。
5. `InputGlyphText` 找不到图标时会优先回退到 Input System 的可读绑定文本。
6. 绑定变化由 `UXInput.Rebind.OnBindingsChanged` 通知，设备变化由 `UXInput.Watch.OnContextChanged` 通知。
