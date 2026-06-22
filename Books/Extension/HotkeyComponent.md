# HotkeyComponent

`HotkeyComponent` 基于 Unity Input System，用配置的 `InputActionReference` 触发 UI 热键行为。

当前热键模块已经收敛为组件式设计：所有热键响应组件都必须继承 `HotkeyComponentBase`。原来的 `IHotkeyTrigger` 接口已删除。

源码位置：

- 本地包：`file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`
- 运行时代码：`Client/Packages/com.alicizax.unity.input/Runtime/Hotkey`
- 编辑器代码：`Client/Packages/com.alicizax.unity.input/Editor/Hotkey`

## 组件结构

| 类型 | 作用 |
| --- | --- |
| `HotkeyComponentBase` | 热键组件基类，负责通用配置、自动查找 `UIHolderObjectBase`、注册和反注册生命周期 |
| `HotkeyComponent` | 默认 Submit 适配器，把热键转换成目标组件的 `ISubmitHandler.OnSubmit` 调用 |
| `HotkeyPassThrough` | 标记组件，让某个 Holder 不参与热键焦点竞争，热键继续向下层可用 Holder 穿透 |
| `UXHotkeySystem` | 热键注册、作用域解析、InputAction 事件订阅和分发系统 |

## HotkeyComponentBase

`HotkeyComponentBase` 是所有热键响应组件的基类。

它保留的通用能力：

- `HotkeyAction`：热键对应的 `InputActionReference`
- `HotkeyPressType`：触发阶段，支持 `Started`、`Performed`、`Canceled`
- `HotkeyActionOwnershipMode`：InputAction 启用策略
- `HotkeyConsumesInput`：触发后是否消费当前热键
- `HotkeyHolder`：所属 `UIHolderObjectBase`
- `AutoAssignHolder()`：自动向父级查找 Holder
- `OnEnable` 注册热键，`OnDisable` / `OnDestroy` 反注册热键
- 应用失焦或暂停时清理热键按压锁定状态

业务层自定义热键时继承它：

```csharp
using UnityEngine.UI;

public sealed class CloseWindowHotkey : HotkeyComponentBase
{
    public override void HotkeyActionTrigger()
    {
        // 执行业务逻辑，例如关闭窗口
    }
}
```

## HotkeyComponent

`HotkeyComponent` 是默认的 Submit 适配器，适合挂在 `UXButton`、`UXToggle` 等实现了 `ISubmitHandler` 的控件上。

触发流程：

```text
InputAction 触发
-> UXHotkeySystem 分发到当前有效 Holder 的热键组件
-> HotkeyComponent.HotkeyActionTrigger()
-> _submitHandler.OnSubmit(_eventData)
```

`HotkeyComponent` 不直接调用 `onClick`。如果目标是 `UXButton`，通常由 `UXButton.OnSubmit` 再触发按钮点击逻辑。

## Inspector 字段

| 字段 | 说明 |
| --- | --- |
| `Component` | `HotkeyComponent` 专用字段，实际接收 Submit 的组件，必须实现 `ISubmitHandler` |
| `Holder` | 所属 `UIHolderObjectBase`，由 `HotkeyComponentBase` 自动查找 |
| `Input Action` | `InputActionReference`，例如 Submit、Cancel、Close |
| `Press Type` | 热键触发阶段，默认 `Performed` |
| `Action Ownership` | `ObserveOnly` 或 `EnableWhileRegistered` |
| `Consumes Input` | 当前热键触发后是否阻止继续向父级 scope 传播 |

## 配置方式

1. 在按钮或控件节点上挂 `HotkeyComponent`。
2. `Component` 指向同节点上的 `UXButton`、`UXToggle` 或其他 `ISubmitHandler`。
3. `Holder` 会自动查找父级 `UIHolderObjectBase`。
4. `Input Action` 指向对应热键。
5. 窗口显示时热键生效，窗口关闭或节点禁用时自动解绑。

示例：

```text
BtnConfirm (UXButton + HotkeyComponent)
|- Component: BtnConfirm 上的 UXButton
|- Holder: 自动找到 ConfirmWindow 的 UIHolderObjectBase
|- Input Action: UI/Submit
|- Press Type: Performed
|- Consumes Input: true
```

## 自定义业务热键

如果热键不是 Submit 行为，不要把逻辑塞进 `ISubmitHandler`。直接继承 `HotkeyComponentBase`：

```csharp
using UnityEngine.UI;

public sealed class SwitchTabHotkey : HotkeyComponentBase
{
    public override void HotkeyActionTrigger()
    {
        // 切换页签
    }
}
```

这样业务组件仍然享受统一的 Holder 自动查找、注册、解绑和焦点分发规则。

## 运行时生命周期

1. `Awake` / `OnEnable` 自动查找父级 `UIHolderObjectBase`。
2. `OnEnable` 注册热键。
3. `OnDisable` / `OnDestroy` 解绑热键。
4. `OnApplicationFocus(false)` 和 `OnApplicationPause(true)` 会清理 `_pressTargets`，避免移动端后台恢复或失焦时保留旧的按压目标。
5. `HotkeyComponent` 在 EventSystem 变化或恢复焦点时重建 `BaseEventData`。

## CacheEventData

`HotkeyComponent` 需要调用：

```csharp
_submitHandler.OnSubmit(_eventData);
```

`ISubmitHandler.OnSubmit` 要求传入 `BaseEventData`，所以组件会缓存一个 `BaseEventData`，避免每次热键触发都 `new BaseEventData(...)`。

当 `EventSystem.current` 变化、组件初始化或应用恢复焦点时，会重新创建缓存，避免继续引用旧的 EventSystem。

## HotkeyPassThrough

`HotkeyPassThrough` 是空的标记组件。

如果某个 `UIHolderObjectBase` 所在对象挂了 `HotkeyPassThrough`，它不会成为热键焦点 Holder。系统会跳过它，继续查找下层可用 Holder。

适合用于：

- Loading 遮罩
- Toast
- Tooltip
- 纯展示浮层
- 不希望拦截热键的顶层 UI

## 热键作用域和优先级

热键按当前 UI Holder 作用域分发，不是所有打开窗口都会同时响应。

系统会在当前可见 UI 中选择最合适的 scope：

| 规则 | 说明 |
| --- | --- |
| `Canvas.sortingOrder` 更高 | 优先响应 |
| 层级更深 | 当 sortingOrder 相同时，子 Holder 优先 |
| 更晚激活 | 当前两项仍相同时，后激活 Holder 优先 |
| Canvas layer 等于 `UIComponent.UIShowLayer` | 只有显示层 Holder 才可响应 |

## 重复注册规则

同一个 `UIHolder scope + InputAction + PressType` 只允许一个有效热键注册。

如果重复注册：

- 保留先注册者
- 后注册者会被拒绝
- Editor 下会输出 warning

如果同一个窗口内有多个 widget 想使用同一个热键，应由业务层控制启用状态，确保同一时间只有一个 widget/component 处于启用并注册状态。

## InputAction Ownership

| 模式 | 说明 |
| --- | --- |
| `ObserveOnly` | 热键系统只监听 action，不负责启用 action。外部 input map 必须已经启用 |
| `EnableWhileRegistered` | 注册期间如果 action 未启用，热键系统会临时启用；最后一个注册者解绑后恢复 |

建议：

- 热键专用 action 可以使用 `EnableWhileRegistered`。
- 如果 action 还被其他系统或 input map 管理，优先使用 `ObserveOnly`，避免 enabled 状态争用。

## InputGlyph 集成

`InputGlyphImage` 和 `InputGlyphText` 的 `Action Source Mode` 设为 `HotkeyTrigger` 时，需要引用 `HotkeyComponentBase` 组件。

它会从 `HotkeyComponentBase.HotkeyAction` 读取 action，用于显示对应输入图标。

## API 速查

| API | 说明 |
| --- | --- |
| `HotkeyComponentBase.HotkeyAction` | 设置或读取热键 Input Action |
| `HotkeyComponentBase.HotkeyActionTrigger()` | 热键触发回调，业务层重写 |
| `UXHotkeyExtension.BindHotKey(this HotkeyComponentBase)` | 手动注册热键 |
| `UXHotkeyExtension.UnBindHotKey(this HotkeyComponentBase)` | 手动解绑热键 |
| `UXHotkeyExtension.BindHotKeyBatch(this HotkeyComponentBase[])` | 批量注册 |
| `UXHotkeyExtension.UnBindHotKeyBatch(this HotkeyComponentBase[])` | 批量解绑 |

## 排查清单

1. 是否安装 `com.alicizax.unity.input` 和 `com.unity.inputsystem`。
2. 是否生成了 `INPUTSYSTEM_SUPPORT`。
3. `Input Action` 是否为空，Input Action 所在 map 是否启用。
4. 当前组件是否继承 `HotkeyComponentBase`。
5. 如果使用 `HotkeyComponent`，`Component` 是否实现 `ISubmitHandler`。
6. 当前节点和 Holder 是否 active。
7. 当前 Holder 的 Canvas 是否在 `UIComponent.UIShowLayer`。
8. 是否有更上层非 `HotkeyPassThrough` Holder 抢占热键焦点。
9. 同一个 Holder 内是否重复注册了相同 action 和 press type。
