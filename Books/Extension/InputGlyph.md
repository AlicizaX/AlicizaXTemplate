# InputGlyph 输入图标与按键重绑定

`InputGlyph` 模块基于 Unity Input System，用于把当前输入设备和 `InputAction` 绑定解析成 UI 图标、TMP Sprite Tag 或可读文本。模块同时提供输入动作注册、动作查询、设备类型监听、运行时按键重绑定和输入轮询工具。

源码位置：

- `Client/Packages/com.alicizax.unity.ui.extension/Runtime/InputGlyph`
- 编辑器入口：`AlicizaX/Extension/Input/InputGlyph`
- 示例目录：`Client/Packages/com.alicizax.unity.ui.extension/Samples~/InputGlyph`

## 组成

| 类型 | 作用 |
| --- | --- |
| `InputActionProvider` | 在运行时注册 `IInputActionProvider` 服务，持有并启用 `InputActionAsset` |
| `InputBindingManager` | 基于 `IInputActionProvider` 管理绑定缓存、重绑定、保存和恢复 |
| `InputGlyphDatabase` | `ScriptableObject`，保存不同设备分类下控制路径到 Sprite 的映射 |
| `InputGlyphService` | 静态查询工具，把 Action 或控制路径解析为 Sprite、TMP Tag 或显示名 |
| `InputGlyphComponent` | UI 组件，自动根据设备切换和绑定变更刷新 Image 或 TMP 文本 |
| `InputDeviceWatcher` | 监听键鼠、Xbox、PlayStation、Switch 和其他手柄的当前输入设备 |
| `InputActionReader` | 业务层轮询输入动作的工具类 |

## 使用前提

项目需要启用 Unity Input System，并准备：

1. 一个 `InputActionAsset`。
2. 一个场景常驻节点挂载 `InputActionProvider`，并在 Inspector 中配置 `Actions`。
3. 一个场景常驻节点挂载 `InputBindingManager`。它不再直接配置 `InputActionAsset`，会从 `IInputActionProvider` 获取动作资产。
4. 一个 `InputGlyphDatabase` 资源，并在运行时调用 `InputGlyphService.SetDatabase(...)` 注入。

推荐启动顺序是：先让 `InputActionProvider` 注册服务，再初始化 `InputBindingManager`。如果二者在同一场景，确保 Provider 所在对象先创建，或放在更早加载的常驻根节点。

## 注册 InputActionAsset

在启动场景或常驻输入节点上添加：

```text
Add Component > Input > Input Action Provider
```

Inspector 配置：

| 字段 | 说明 |
| --- | --- |
| `Actions` | 项目的 `InputActionAsset` |

运行时 `InputActionProvider` 会：

- 注册 `IInputActionProvider` 到 `AppServices.App`。
- 建立 `ActionName` 与 `MapName/ActionName` 查询表。
- 自动启用 `InputActionAsset`。
- 当多个 ActionMap 中有同名 Action 时，短名称会被标记为歧义，必须使用 `MapName/ActionName`。

动作查询示例：

```csharp
using UnityEngine.InputSystem;

InputAction submit = InputActionResolver.Action("UI/Submit");

if (InputActionResolver.TryGetAction("Submit", out InputAction action))
{
    // 仅当 Submit 没有重名歧义时能成功。
}
```

## 挂载 InputBindingManager

`InputBindingManager` 继承框架服务组件，建议放在启动场景的框架根节点或常驻输入节点上。

Inspector 配置：

| 字段 | 说明 |
| --- | --- |
| `debugMode` | 输出加载、保存、重绑定等调试日志 |

运行时获取：

```csharp
using AlicizaX;

InputBindingManager input = AppServices.Require<InputBindingManager>();
```

也可以按名称查找 Action：

```csharp
InputAction move = InputBindingManager.Action("Player/Move");
InputAction submit = InputBindingManager.Action("UI/Submit");
```

如果不同 ActionMap 下存在同名 Action，请使用 `MapName/ActionName`，不要只写短名称。

## 创建 InputGlyphDatabase

打开编辑器窗口：

```text
AlicizaX/Extension/Input/InputGlyph
```

如果项目里还没有 `InputGlyphDatabase`，窗口会提示创建资源。也可以双击已有 `InputGlyphDatabase` 资源打开专用编辑器。

设备表是固定分类：

| 表 | 说明 |
| --- | --- |
| `PlayStation` | DualShock、DualSense、PlayStation 布局 |
| `Xbox` | Xbox、XInput、通用 Gamepad 布局 |
| `Switch` | Switch、Nintendo、Joy-Con 布局 |
| `Keyboard` | Keyboard 与 Mouse 布局 |
| `Other` | 运行时支持，但默认编辑窗口优先维护上述四类；可在资源序列化数据中保留 |

数据库全局设置：

| 字段 | 说明 |
| --- | --- |
| `placeholderSprite` | 找到绑定但没有匹配图标时返回的占位 Sprite |

每条 `GlyphEntry` 需要配置：

| 字段 | 说明 |
| --- | --- |
| `Sprite` | 显示用图标 |
| `action` | 用来提供绑定路径的 `InputAction` |

数据库会读取 `action.bindings` 中的 `path` 和 `effectivePath`，归一化后建立控制路径到 Sprite 的缓存。例如 `<XInputController>/buttonSouth`、`<Gamepad>/buttonSouth`、`<DualShockGamepad>/buttonSouth` 会按设备布局规则归一化到可匹配的路径。

运行时注入数据库：

```csharp
using UnityEngine;

public sealed class InputGlyphBootstrap : MonoBehaviour
{
    [SerializeField] private InputGlyphDatabase glyphDatabase;

    private void Awake()
    {
        InputGlyphService.SetDatabase(glyphDatabase);
    }
}
```

## 显示按键图标

在 UI 节点上添加：

```text
Add Component > UI > Input Glyph
```

`InputGlyphComponent` 会在启用时自动监听：

- `InputDeviceWatcher.OnDeviceChanged`
- `InputBindingManager.BindingsChanged`

设备切换或绑定保存后，组件会自动刷新。

### Source

| 模式 | 说明 |
| --- | --- |
| `ActionReference` | 直接指定 `InputActionReference` |
| `HotkeyTrigger` | 从同节点或指定组件上的 `IHotkeyTrigger` 读取 `HotkeyAction` |
| `ActionName` | 通过 `InputActionResolver.Action(actionName)` 查询 |

`Composite Part` 只在 Action 包含 Composite Binding 时显示，常见值是 `Up`、`Down`、`Left`、`Right`。它用于显示或重绑定 2DVector 里的某一个方向。

### Output

| 模式 | 说明 |
| --- | --- |
| `Image` | 将解析到的 Sprite 设置到 `targetImage` |
| `Text` | 将 TMP Sprite Tag 或显示名填入 `targetText` |

`Image` 模式如果没有手动指定 `targetImage`，组件会尝试 `GetComponent<Image>()`。

`Text` 模式如果没有手动指定 `targetText`，组件会尝试 `GetComponent<TMP_Text>()`。当前 TMP 文本会被当作模板，并用 `Utility.Text.Format(template, token)` 替换 `{0}`：

```text
Press {0} to confirm
```

当找到 Sprite 时，文本里会插入：

```text
<sprite name="SpriteName">
```

找不到 Sprite 时，会回退为 Input System 的可读显示名。

### 平台事件

`Platform Events` 可以为指定设备分类配置两个 UnityEvent：

| 事件 | 触发时机 |
| --- | --- |
| `On Matched` | 当前设备分类等于该条配置的 `category` |
| `On Not Matched` | 当前设备分类不等于该条配置的 `category` |

适合用于切换不同平台提示、布局节点或手柄专属 UI。

## 手动查询图标

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class ActionGlyphView : MonoBehaviour
{
    [SerializeField] private InputActionReference action;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string compositePartName;

    private void OnEnable()
    {
        InputDeviceWatcher.OnDeviceChanged += OnDeviceChanged;
        InputBindingManager.BindingsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        InputDeviceWatcher.OnDeviceChanged -= OnDeviceChanged;
        InputBindingManager.BindingsChanged -= Refresh;
    }

    private void OnDeviceChanged(InputDeviceWatcher.InputDeviceCategory category)
    {
        Refresh();
    }

    private void Refresh()
    {
        InputDeviceWatcher.InputDeviceCategory device = InputDeviceWatcher.CurrentCategory;
        InputAction inputAction = action != null ? action.action : null;

        if (InputGlyphService.TryGetUISpriteForActionPath(inputAction, compositePartName, device, out Sprite sprite))
        {
            icon.sprite = sprite;
        }
        else
        {
            icon.sprite = null;
        }

        label.text = InputGlyphService.GetDisplayNameFromInputAction(inputAction, compositePartName, device);
    }
}
```

查询 TMP Sprite Tag：

```csharp
InputAction action = InputBindingManager.Action("UI/Submit");
InputDeviceWatcher.InputDeviceCategory device = InputDeviceWatcher.CurrentCategory;

if (InputGlyphService.TryGetTMPTagForActionPath(
        action,
        null,
        device,
        out string tag,
        out string fallback))
{
    text.text = $"Press {tag}";
}
else
{
    text.text = $"Press {fallback}";
}
```

查询 Composite 方向图标：

```csharp
InputAction move = InputBindingManager.Action("Player/Move");
InputGlyphService.TryGetUISpriteForActionPath(
    move,
    "Up",
    InputDeviceWatcher.CurrentCategory,
    out Sprite upSprite);
```

## 按键重绑定

重绑定由 `InputBindingManager` 负责。它会选择目标 Action 上最适合键盘的 Binding，排除鼠标移动和滚轮，并通过 `Escape` 取消。

开始重绑定：

```csharp
using AlicizaX;

InputBindingManager input = AppServices.Require<InputBindingManager>();
input.StartRebind("UI/Submit");
```

重绑定 Composite 中的某个方向：

```csharp
input.StartRebind("Player/Move", "Up");
```

取消正在进行的交互式重绑定：

```csharp
input.CancelRebind();
```

重绑定完成后，新绑定会先进入暂存区。调用 `ConfirmApply` 后才会写入 `InputActionAsset` override、保存到磁盘，并触发 `InputBindingManager.BindingsChanged`：

```csharp
bool saved = await input.ConfirmApply();
```

如果不想应用暂存修改：

```csharp
input.DiscardPrepared();
```

恢复默认绑定：

```csharp
await input.ResetToDefaultAsync();
```

保存位置：

| 环境 | 文件 |
| --- | --- |
| Unity Editor | `Assets/input_bindings.json` |
| Player | `Application.persistentDataPath/input_bindings.json` |

### 重绑定 UI 示例

```csharp
using AlicizaX;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class RebindButtonView : MonoBehaviour
{
    [SerializeField] private string actionName = "UI/Submit";
    [SerializeField] private string compositePartName;
    [SerializeField] private TextMeshProUGUI bindingLabel;
    [SerializeField] private TextMeshProUGUI stateLabel;

    private InputBindingManager input;

    private void OnEnable()
    {
        input = AppServices.Require<InputBindingManager>();
        input.OnRebindStart += OnRebindStart;
        input.OnRebindEnd += OnRebindEnd;
        input.OnRebindPrepare += OnRebindPrepare;
        input.OnRebindConflict += OnRebindConflict;
        input.OnApply += OnApply;
        InputBindingManager.BindingsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.OnRebindStart -= OnRebindStart;
            input.OnRebindEnd -= OnRebindEnd;
            input.OnRebindPrepare -= OnRebindPrepare;
            input.OnRebindConflict -= OnRebindConflict;
            input.OnApply -= OnApply;
        }

        InputBindingManager.BindingsChanged -= Refresh;
    }

    public void StartRebind()
    {
        input.StartRebind(actionName, string.IsNullOrEmpty(compositePartName) ? null : compositePartName);
    }

    public async void Confirm()
    {
        bool saved = await input.ConfirmApply();
        stateLabel.text = saved ? "Saved" : "No changes";
    }

    public void Discard()
    {
        input.DiscardPrepared();
        Refresh();
    }

    private void Refresh()
    {
        InputAction action = InputBindingManager.Action(actionName);
        bindingLabel.text = InputGlyphService.GetDisplayNameFromInputAction(
            action,
            string.IsNullOrEmpty(compositePartName) ? null : compositePartName,
            InputDeviceWatcher.CurrentCategory);
    }

    private void OnRebindStart()
    {
        stateLabel.text = "Press a key...";
    }

    private void OnRebindEnd(bool success, InputBindingManager.RebindContext context)
    {
        stateLabel.text = success ? "Ready to save" : "Canceled";
    }

    private void OnRebindPrepare(InputBindingManager.RebindContext context)
    {
        if (IsTarget(context))
        {
            stateLabel.text = $"Prepared: {context.overridePath}";
        }
    }

    private void OnRebindConflict(
        InputBindingManager.RebindContext prepared,
        InputBindingManager.RebindContext conflict)
    {
        if (IsTarget(prepared) || IsTarget(conflict))
        {
            stateLabel.text = "Conflict detected";
        }
    }

    private void OnApply(bool success, InputBindingManager.RebindContext[] contexts)
    {
        Refresh();
    }

    private bool IsTarget(InputBindingManager.RebindContext context)
    {
        if (context == null || context.action == null)
        {
            return false;
        }

        InputAction action = InputBindingManager.Action(actionName);
        if (context.action != action)
        {
            return false;
        }

        if (string.IsNullOrEmpty(compositePartName))
        {
            return true;
        }

        if (context.bindingIndex < 0 || context.bindingIndex >= action.bindings.Count)
        {
            return false;
        }

        return string.Equals(
            action.bindings[context.bindingIndex].name,
            compositePartName,
            System.StringComparison.OrdinalIgnoreCase);
    }
}
```

## 输入读取工具

`InputActionReader` 适合在业务逻辑中按名称或 `InputAction` 轮询输入。

```csharp
using UnityEngine;

public sealed class PlayerInputLoop : MonoBehaviour
{
    private void Update()
    {
        Vector2 move = InputActionReader.ReadValue<Vector2>("Player/Move");

        if (InputActionReader.ReadPressedOnce(this, "Player/Interact"))
        {
            Interact();
        }

        bool inventoryOpen = InputActionReader.ReadPressedToggle(this, "UI/OpenInventory");
        SetInventoryVisible(inventoryOpen);

        if (InputActionReader.ReadCompositePartButtonOnce(this, "Player/Move", "Up"))
        {
            StepUp();
        }
    }

    private void Interact() { }
    private void SetInventoryVisible(bool visible) { }
    private void StepUp() { }
}
```

常用 API：

| API | 说明 |
| --- | --- |
| `ReadValue<T>(InputAction/string)` | 直接读取 Action 当前值 |
| `TryReadValue<T>(InputAction/string, out T)` | 仅在 Action 处于按下状态时读取值 |
| `TryReadValueOnce<T>(owner, InputAction/string, out T)` | 仅在本次按下第一帧读取值 |
| `ReadButton(InputAction/string)` | 读取 Button 类型 Action，非 Button 会抛异常 |
| `ReadPressed(InputAction/string)` | 对任意类型 Action 读取 `IsPressed()` |
| `ReadPressedOnce(owner/int/string, InputAction/string)` | 对任意类型 Action 做单次按下触发 |
| `ReadPressedToggle(owner/int/string, InputAction/string)` | 对任意类型 Action 做按下切换 |
| `ReadButtonOnce(owner/int/string, InputAction/string)` | Button 类型单次触发 |
| `ReadButtonToggle(owner/int/string, InputAction/string)` | Button 类型按下切换 |
| `ReadCompositePartButton(InputAction/string, part)` | 读取 Composite 指定 part 是否按下 |
| `ReadCompositePartButtonOnce(owner/int/string, InputAction/string, part)` | Composite 指定 part 单次触发 |
| `ReadCompositePartButtonToggle(owner/int/string, InputAction/string, part)` | Composite 指定 part 按下切换 |
| `ResetToggledButton(...)` | 重置指定切换状态 |
| `ResetToggledCompositePartButton(...)` | 重置指定 Composite part 的切换状态 |
| `ResetToggledButtons()` | 清空全部切换状态 |

## 设备切换

`InputDeviceWatcher` 会在 `BeforeSceneLoad` 自动初始化。默认设备是 `Keyboard`，设备名为 `Keyboard&Mouse`。模块会监听键盘、鼠标、Gamepad 和 Joystick，并根据布局、设备描述、VendorId/ProductId 判断分类。

监听当前设备：

```csharp
private void OnEnable()
{
    InputDeviceWatcher.OnDeviceContextChanged += OnDeviceContextChanged;
}

private void OnDisable()
{
    InputDeviceWatcher.OnDeviceContextChanged -= OnDeviceContextChanged;
}

private void OnDeviceContextChanged(InputDeviceWatcher.DeviceContext context)
{
    AlicizaX.Log.Info(
        $"Device: {context.Category}, {context.DeviceName}, vid={context.VendorId}, pid={context.ProductId}");
}
```

设备分类：

```csharp
public enum InputDeviceCategory
{
    Keyboard,
    Xbox,
    PlayStation,
    Other,
    Switch
}
```

常用状态：

| 属性 | 说明 |
| --- | --- |
| `CurrentCategory` | 当前设备分类 |
| `CurrentDeviceName` | 当前设备显示名 |
| `CurrentDeviceId` | 当前 Input System 设备 ID |
| `CurrentVendorId` | 当前设备 VendorId |
| `CurrentProductId` | 当前设备 ProductId |
| `CurrentContext` | 完整设备上下文 |

## Hotkey 集成

当 UI 节点已经使用 `HotkeyComponent` 时，可以把 `InputGlyphComponent.Source` 设为 `HotkeyTrigger`。组件会从 `IHotkeyTrigger.HotkeyAction` 中读取要显示的 Action。

典型结构：

```text
Button GameObject
  - UXButton 或其他 ISubmitHandler
  - HotkeyComponent
  - Image / TMP_Text
  - InputGlyphComponent
```

配置：

1. 在 `HotkeyComponent.HotkeyAction` 中指定快捷键 Action。
2. 在 `InputGlyphComponent` 中选择 `Reference Mode = HotkeyTrigger`。
3. 如果 `Hotkey Trigger` 留空，组件会尝试从同节点自动查找实现 `IHotkeyTrigger` 的组件。

## API 速查

| API | 说明 |
| --- | --- |
| `InputGlyphService.SetDatabase(InputGlyphDatabase)` | 注入 Glyph 数据库 |
| `InputGlyphService.GetBindingControlPath(...)` | 获取当前设备下最匹配绑定的控制路径 |
| `InputGlyphService.TryGetUISpriteForActionPath(...)` | 获取 Action 或控制路径对应 Sprite |
| `InputGlyphService.TryGetTMPTagForActionPath(...)` | 获取 TMP Sprite Tag，并输出显示名兜底 |
| `InputGlyphService.GetDisplayNameFromInputAction(...)` | 获取 Action 当前绑定的可读显示名 |
| `InputGlyphService.GetDisplayNameFromControlPath(...)` | 将控制路径转换为可读文本 |
| `InputActionResolver.Action(string)` | 按名称查找 Action，失败会记录错误 |
| `InputActionResolver.TryGetAction(string, out InputAction)` | 尝试按名称查找 Action |
| `InputBindingManager.Action(string)` | `InputActionResolver.Action` 的便捷入口 |
| `InputBindingManager.StartRebind(string, string)` | 开始交互式键盘重绑定 |
| `InputBindingManager.CancelRebind()` | 取消当前交互式重绑定 |
| `InputBindingManager.ConfirmApply(bool)` | 应用并保存暂存重绑定 |
| `InputBindingManager.DiscardPrepared()` | 丢弃暂存重绑定 |
| `InputBindingManager.ResetToDefaultAsync()` | 恢复默认绑定并保存 |
| `InputBindingManager.GetBindingPath(...)` | 获取指定 Action/Binding 的路径对象 |
| `InputDeviceWatcher.CurrentCategory` | 当前设备分类 |
| `InputActionReader.ReadPressedOnce(...)` | 任意 Action 类型的单次按下读取 |

## 注意事项

1. 当前 API 名称是 `InputGlyphService`，不是旧文档里的 `GlyphService`。
2. `InputBindingManager` 不再直接配置 `InputActionAsset`，必须先通过 `InputActionProvider` 注册 `IInputActionProvider`。
3. `InputBindingManager.Instance` 不是当前模块的公开用法，请通过 `AppServices.Require<InputBindingManager>()` 获取服务实例。
4. `InputGlyphService.SetDatabase(...)` 需要项目启动逻辑主动调用，否则图标查询只能回退为显示名或返回空。
5. `InputGlyphComponent.Text` 模式会把启用时的 TMP 文本作为模板，建议模板中保留 `{0}`。
6. 同名 Action 存在多个 ActionMap 时，必须使用 `MapName/ActionName`。
7. `StartRebind` 当前按键重绑定限定键盘路径 `<Keyboard>`，并排除鼠标移动和滚轮，更适合键位设置界面。
8. 重绑定结果先进入暂存区，只有 `ConfirmApply` 后才会写入磁盘并触发 `BindingsChanged`。
9. `ReadButton(...)` 只支持 `InputActionType.Button`，Value/PassThrough/Composite 请使用 `ReadPressed(...)` 系列。
