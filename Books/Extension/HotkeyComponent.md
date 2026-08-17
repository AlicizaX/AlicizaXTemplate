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
- `HotkeyConsumesInput`：触发后是否消费当前热键
- `HotkeyHolder`：所属 `UIHolderObjectBase`
- `AutoAssignHolder()`：自动向父级查找 Holder
- `OnEnable` 注册热键，`OnDisable` 反注册热键

热键系统只观察 InputAction，不负责 Enable/Disable。Action 必须由输入层（如 `InputActionProvider`）启用。

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
| `Consumes Input` | 当前热键触发成功后，是否停止沿同一焦点 Holder 的父级 scope 继续分发。不吞全局 Input System 事件 |

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

1. `Reset` / `Awake` / `OnEnable` 查找父级 `UIHolderObjectBase`。
2. `OnEnable` 注册热键。节点 `SetActive(false)` 或组件禁用会走 `OnDisable` 并解绑；重新启用会再注册。
3. 热键必须从 `InputActionProvider` 解析到同一份已启用的 `InputAction`。解析失败则不注册（Development 下 warning），不会回退到 Prefab 上的 `InputActionReference.action`。
4. `UXHotkeySystem` 在应用失焦 / 暂停时清掉当前按压锁定。
5. `HotkeyComponent` 在 `EventSystem.current` 变化时才重建 `BaseEventData`。

## HotkeyPassThrough 与焦点

热键焦点按 UI 栈从高到低取**第一个可见且未挂 `HotkeyPassThrough` 的 Holder**。焦点锁在这一层后，只在该 Holder 及其子 Holder 里找热键，**不会自动落到更下面的窗口**。

`HotkeyPassThrough` 是空的标记组件，挂在某个 `UIHolderObjectBase` 上后，这一层不当热键焦点，系统继续向下找。

窗口例子：

```text
AWindow 有 Cancel 热键
打开更高一层的 BWindow
```

| BWindow | 按 Cancel 的结果 |
| --- | --- |
| 没挂 `HotkeyPassThrough`，也没有热键 | 焦点在 B，A 的热键**不会**触发 |
| 挂了 `HotkeyPassThrough` | 跳过 B，热键打到 A |
| 自己有热键 | 打到 B |

适合挂 `HotkeyPassThrough`：

- Loading 遮罩
- Toast / Tooltip
- HUD、纯展示浮层
- 明确不该拦截下层热键的顶层 UI

不要依赖“B 没配热键就自动透传”。需要透传时必须主动挂 `HotkeyPassThrough`。

## 热键作用域和优先级

热键按当前焦点 Holder 分发，不是所有打开窗口同时响应。

先锁定栈顶可用 Holder（见上节），再在该 Holder 内部选叶 scope：

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

## InputAction 启用

热键系统不启用 / 禁用 InputAction。请确保：

- 运行时由 `InputActionProvider`（或等价入口）启用对应 `InputActionAsset`
- Development 下若 action 未启用，注册时会输出 warning

## 显示 Canvas 回退

- Holder 自身有 Canvas：用自身 `layer` / `sortingOrder`
- Widget 等无自身 Canvas：回退到最近祖先 Canvas（正常路径，不警告）
- 整条父链都没有 Canvas：仅依赖生命周期 + `activeInHierarchy`，Development 下警告一次

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

## 排查清单

1. 是否安装 `com.alicizax.unity.input` 和 `com.unity.inputsystem`。
2. 是否生成了 `INPUTSYSTEM_SUPPORT`。
3. `Input Action` 是否为空，Input Action 所在 map 是否启用。
4. 当前组件是否继承 `HotkeyComponentBase`。
5. 如果使用 `HotkeyComponent`，`Component` 是否实现 `ISubmitHandler`。
6. 当前节点和 Holder 是否 active。
7. 当前 Holder 的 Canvas 是否在 `UIComponent.UIShowLayer`。
8. 是否有更上层、未挂 `HotkeyPassThrough` 的窗口抢走了焦点（该窗没有热键时也会挡住下层）。
9. 同一个 Holder 内是否重复注册了相同 action 和 press type。
