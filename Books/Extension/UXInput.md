# UXInput 输入系统

`UXInput` 是 `com.alicizax.unity.input` 的通用输入入口，包含设备监听、动作解析、输入读取、运行时重绑定、手柄震动和输入诊断。图标数据库和图标 UI 组件见 `InputGlyph.md`。

源码位置：

| 内容 | 路径 |
| --- | --- |
| 通用输入 API | `Client/Packages/com.alicizax.unity.input/Runtime/Input` |
| Glyph 和 Provider | `Client/Packages/com.alicizax.unity.input/Runtime/InputGlyph` |
| 诊断组件 | `Client/Packages/com.alicizax.unity.input/Runtime/Input/Diagnostics/InputVisualizer.cs` |

## 启动方式

在常驻输入节点上挂载：

```text
Add Component > Input > Input Action Provider
```

`InputActionProvider` 是当前输入系统的手动启动入口。它会按顺序执行：

1. `UXInput.Watch.Initialize()`
2. `UXInput.Glyph.SetDatabase(glyphDatabase)`
3. 建立 `InputActionProvider` 的 action 查询表
4. `UXInput.Rebind.Initialize(actions)`
5. `actions.Enable()`
6. `UXNavigationSystem.Initialize()`

销毁时会反向关闭导航、重绑定、动作查询、Glyph 数据库和 Watch。当前设计不依赖 `RuntimeInitializeOnLoadMethod` 自动启动，业务需要保证 `InputActionProvider` 早于依赖输入的系统创建。

## Action 查询

`InputActionProvider` 会把 `InputActionAsset` 中的 Action 注册为完整路径：

```text
MapName/ActionName
```

示例：

```csharp
InputAction submit = InputActionProvider.ResolveAction("UI/Submit");

if (InputActionProvider.TryResolveAction("Gameplay/Jump", out InputAction jump))
{
    jump.Enable();
}
```

如果多个 ActionMap 中有同名 Action，使用完整路径最稳定。

## Watch 设备监听

`UXInput.Watch` 负责监听当前输入设备和输入上下文。

常用状态：

| API | 说明 |
| --- | --- |
| `UXInput.Watch.Current` | 当前完整输入上下文。 |
| `CurrentDeviceType` | 当前设备类型，例如 `PC`、`Phone`、`SteamDeck`。 |
| `CurrentInputType` | 当前输入类型，例如 `KeyboardMouse`、`Gamepad`、`Touch`。 |
| `CurrentInputProfile` | 当前图标和平台 Profile，例如 `Xbox`、`PlayStation`。 |
| `CurrentControlScheme` | 当前控制方案字符串。 |
| `IsNavigationInput` | 当前是否为 Gamepad 或 Joystick 输入。 |
| `IsTouchInput` | 当前是否为 Touch 输入。 |
| `IsKeyboardMouseInput` | 当前是否为键鼠输入。 |

事件：

| 事件 | 触发时机 |
| --- | --- |
| `OnContextChanged` | 当前输入上下文变化时。 |
| `OnInputActivity` | 检测到输入活动时。 |
| `OnDeviceTypeChanged` | 设备类型变化时。 |
| `OnInputTypeChanged` | 输入类型变化时。 |
| `OnInputProfileChanged` | 输入 Profile 变化时。 |

示例：

```csharp
private void OnEnable()
{
    UXInput.Watch.OnContextChanged += OnInputContextChanged;
}

private void OnDisable()
{
    UXInput.Watch.OnContextChanged -= OnInputContextChanged;
}

private void OnInputContextChanged(UXInput.Watch.InputContext context)
{
    Debug.Log($"{context.InputProfile} / {context.DeviceName}");
}
```

`InputContext` 包含：

| 字段 | 说明 |
| --- | --- |
| `DeviceType` | 平台或设备类型。 |
| `InputType` | 输入类型。 |
| `InputProfile` | 当前 Profile。 |
| `ControlScheme` | 控制方案。 |
| `DeviceId` | Input System 设备 ID。 |
| `VendorId` / `ProductId` | 设备 VID/PID。 |
| `DeviceName` | 设备显示名。 |
| `Layout` | Input System layout。 |
| `InterfaceName` | 设备接口名。 |
| `Manufacturer` / `Product` | 设备厂商和产品名。 |

## Rebind 运行时重绑定

`UXInput.Rebind` 负责运行时交互式重绑定、冲突检测、暂存确认和绑定持久化。

状态：

| API | 说明 |
| --- | --- |
| `IsInitialized` | 是否已绑定 `InputActionAsset`。 |
| `IsRebinding` | 是否正在监听一次交互式重绑定。 |
| `HasPreparedRebinds` | 是否存在等待确认的重绑定变更。 |
| `HasSavedBindings` | 本地绑定覆盖文件是否存在。 |
| `BindingFilePath` | 当前绑定覆盖 JSON 文件路径。 |

事件：

| 事件 | 说明 |
| --- | --- |
| `OnRebindStarted` | 开始监听输入。 |
| `OnRebindEnded` | 完成或取消监听。 |
| `OnRebindPrepared` | 捕获到新绑定并进入暂存。 |
| `OnBindingConflict` | 暂存绑定与已有绑定冲突。 |
| `OnBindingsChanged` | 绑定覆盖发生变化。Glyph UI 会监听它刷新。 |
| `OnApply` | 确认或丢弃暂存变更。 |
| `OnBindingsLoaded` | 从磁盘加载绑定。 |
| `OnBindingsSaved` | 写入绑定文件。 |

开始重绑定：

```csharp
UXInput.Rebind.BeginRebind("UI/Submit");
UXInput.Rebind.BeginRebind("UI/Submit", RebindTarget.KeyboardMouse);
UXInput.Rebind.BeginRebind("UI/Submit", RebindTarget.Gamepad);
```

Composite Part 重绑定：

```csharp
UXInput.Rebind.BeginCompositePartRebind("Gameplay/Move", "Up");
UXInput.Rebind.BeginCompositePartRebind("Gameplay/Move", "Up", RebindTarget.KeyboardMouse);
```

确认或丢弃：

```csharp
bool applied = UXInput.Rebind.ConfirmApply(clearConflicts: true, save: true);
UXInput.Rebind.DiscardPrepared();
```

取消当前交互式监听：

```csharp
UXInput.Rebind.CancelRebinding();
```

重置绑定：

```csharp
UXInput.Rebind.ResetBinding("UI/Submit");
UXInput.Rebind.ResetCompositePartBinding("Gameplay/Move", "Up");
UXInput.Rebind.ResetActionBindings("UI/Submit");
UXInput.Rebind.ResetAllBindings();
UXInput.Rebind.ResetToDefault();
```

导入导出：

```csharp
string json = UXInput.Rebind.ExportBindingsJson();
bool imported = UXInput.Rebind.ImportBindingsJson(json);
```

显示绑定文本：

```csharp
string text = UXInput.Rebind.GetBindingDisplayString("UI/Submit");
string gamepadText = UXInput.Rebind.GetBindingDisplayString("UI/Submit", RebindTarget.Gamepad);
string upText = UXInput.Rebind.GetBindingDisplayString("Gameplay/Move", "Up");
```

`RebindTarget`：

| 值 | 说明 |
| --- | --- |
| `KeyboardMouse` | 键盘和鼠标。默认会排除鼠标移动、滚轮、Pointer position/delta。 |
| `Gamepad` | 手柄。 |
| `Joystick` | 摇杆。 |
| `Any` | 不限制设备。 |

绑定文件路径：

| 环境 | 位置 |
| --- | --- |
| Editor | `Assets/input_bindings.json` |
| Player | `Application.persistentDataPath/input_bindings.json` |

## Reader 输入读取

`UXInput.Reader` 是业务层轮询读取工具，适合在 `Update` 中读 Action。

解析 Action：

```csharp
InputAction jump = UXInput.Reader.ResolveAction("Gameplay/Jump");
```

读取值：

```csharp
Vector2 move = UXInput.Reader.ReadValue<Vector2>("Gameplay/Move");

if (UXInput.Reader.TryReadValue<Vector2>("Gameplay/Aim", out Vector2 aim))
{
    Aim(aim);
}
```

一次触发和 Toggle：

```csharp
if (UXInput.Reader.ReadPressedOnce(this, "Gameplay/Interact"))
{
    Interact();
}

bool inventoryOpen = UXInput.Reader.ReadPressedToggle(this, "UI/Inventory");
SetInventoryVisible(inventoryOpen);
```

Button 专用读取：

```csharp
bool pressed = UXInput.Reader.ReadButton("UI/Submit");
bool once = UXInput.Reader.ReadButtonOnce(this, "UI/Submit");
bool toggled = UXInput.Reader.ReadButtonToggle(this, "UI/TogglePanel");
```

Composite Part：

```csharp
bool up = UXInput.Reader.ReadCompositePartButton("Gameplay/Move", "Up");

if (UXInput.Reader.ReadCompositePartButtonOnce(this, "Gameplay/Move", "Left"))
{
    StepLeft();
}
```

重置 Toggle 状态：

```csharp
UXInput.Reader.ResetToggledButton("UI/Inventory");
UXInput.Reader.ResetToggledCompositePartButton("player", "Gameplay/Move", "Up");
UXInput.Reader.ResetToggledButtons();
```

诊断事件：

| 事件 | 说明 |
| --- | --- |
| `OnRead` | `Once`、`Toggle`、`ValueOnce` 等离散读取发生时触发。 |
| `OnContinuousRead` | `ReadPressed`、`ReadButton`、`TryReadValue` 等连续读取为 true 时触发，可能高频。 |

`Reader` 的状态缓存是懒加载的。普通 `ReadPressed`、`ReadButton`、`TryReadValue` 不会创建 once/toggle/composite 缓存。

## Haptics 震动

`UXInput.Haptics` 负责当前 `Gamepad.current` 的马达反馈。

状态：

| API | 说明 |
| --- | --- |
| `Intensity` | 全局强度，范围 0 到 1。 |
| `Enabled` | 是否启用震动。设为 false 时会停止当前震动。 |
| `IsPlaying` | 是否正在播放震动。 |

播放预设：

```csharp
UXInput.Haptics.Play(HapticPreset.Selection);
UXInput.Haptics.Play(HapticPreset.Success);
UXInput.Haptics.Play(HapticPreset.Error);
```

播放指定马达强度：

```csharp
UXInput.Haptics.Play(leftMotor: 0.4f, rightMotor: 0.8f, duration: 0.15f);
```

播放自定义曲线：

```csharp
UXInput.Haptics.Play(hapticPattern);
```

停止和重置：

```csharp
UXInput.Haptics.Stop();
UXInput.Haptics.Reset();
UXInput.Haptics.SetIntensity(0.5f);
```

`HapticPattern` 是 `ScriptableObject`，可以在项目中创建：

```text
AlicizaX/Input/Haptic Pattern
```

它包含左右马达曲线和持续时间。播放时系统会注册到 `InputSystem.onAfterUpdate`，结束后自动停止并注销更新。

## InputVisualizer 输入诊断

`InputVisualizer` 是运行时屏幕输入日志组件。

```text
Add Component > Input > Input Visualizer
```

日志来源：

| 字段 | 说明 |
| --- | --- |
| `logInputActions` | 显示 `InputSystem.onActionChange` 的 ActionPerformed。 |
| `logDeviceActivity` | 显示 `UXInput.Watch.OnInputActivity`。 |
| `logContextChanges` | 显示 `UXInput.Watch.OnContextChanged`。 |
| `logReaderReads` | 显示 `UXInput.Reader.OnRead`。默认开启。 |
| `logReaderContinuousReads` | 显示连续 Reader 读取。默认关闭，避免刷屏。 |

过滤：

| 字段 | 说明 |
| --- | --- |
| `logPointerMovementActions` | 是否显示鼠标和 Pointer 的 position/delta action。默认关闭。 |
| `logRepeatedDeviceActivity` | 是否重复显示同设备活动。默认关闭。 |
| `repeatedDeviceActivityInterval` | 重复设备活动的最小输出间隔。 |
| `mergeDuplicateEntries` | 是否合并短时间内相同日志。默认开启。 |
| `duplicateMergeWindow` | 重复日志合并窗口。 |

显示：

| 字段 | 说明 |
| --- | --- |
| `showOnScreen` | 是否绘制屏幕面板。 |
| `screenPosition` | 面板位置。 |
| `panelWidth` | 面板宽度。 |
| `entryHeight` | 单行高度。 |
| `fontSize` | 字号。 |
| `maxHistoryEntries` | 最大历史行数。 |
| `entryLifetime` | 日志保留时间。 |
| `showTimestamps` | 是否显示时间戳。 |

`InputVisualizer` 适合临时挂在调试场景里。正式 UI 不应该依赖它。

## 常见组合

### 按键设置界面

```csharp
public void StartRebindSubmit()
{
    UXInput.Rebind.BeginRebind("UI/Submit");
}

public void SavePrepared()
{
    UXInput.Rebind.ConfirmApply();
}

public void Cancel()
{
    UXInput.Rebind.CancelRebinding();
    UXInput.Rebind.DiscardPrepared();
}
```

刷新显示：

```csharp
private void OnEnable()
{
    UXInput.Rebind.OnBindingsChanged += Refresh;
    Refresh();
}

private void OnDisable()
{
    UXInput.Rebind.OnBindingsChanged -= Refresh;
}

private void Refresh()
{
    bindingLabel.text = UXInput.Rebind.GetBindingDisplayString("UI/Submit");
}
```

### 输入设备切换

```csharp
private void OnEnable()
{
    UXInput.Watch.OnInputProfileChanged += OnProfileChanged;
}

private void OnDisable()
{
    UXInput.Watch.OnInputProfileChanged -= OnProfileChanged;
}

private void OnProfileChanged(UXInput.Watch.InputProfile profile)
{
    Debug.Log($"Input profile: {profile}");
}
```

### 读取输入并震动

```csharp
private void Update()
{
    if (UXInput.Reader.ReadPressedOnce(this, "Gameplay/Interact"))
    {
        UXInput.Haptics.Play(HapticPreset.Selection);
        Interact();
    }
}
```

## 注意事项

1. `InputActionProvider` 是当前推荐的输入系统启动点。不要再依赖旧的自动初始化入口。
2. `UXInput.Glyph` 的数据库由 `InputActionProvider` 注入。图标查询方法不允许额外传入数据库。
3. `UXInput.Rebind` 的结果先进入暂存区，只有 `ConfirmApply` 后才会应用和保存。
4. `UXInput.Rebind.OnBindingsChanged` 是绑定变化的统一通知入口。
5. `UXInput.Reader.OnContinuousRead` 可能高频触发，诊断默认不显示。
6. `InputVisualizer` 默认过滤 Pointer position/delta 和重复设备活动，避免鼠标移动刷屏。
7. 手柄震动只作用于 `Gamepad.current`，没有当前手柄时调用会直接返回。
