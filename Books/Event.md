# Event 模块

Event 模块提供轻量级事件总线，用于模块之间的低耦合通知，例如资源下载进度、语言切换、UI 刷新、战斗结算通知等。

源码位置：

- `Client/Packages/com.alicizax.unity.framework/Runtime/Modules/Event`
- `Client/Packages/com.alicizax.unity.framework/Editor/Modules/Event/EventMonitorWindow.cs`

## 使用前提

事件总线是静态 API，不需要在场景中挂组件。事件参数必须是 `struct`。Payload 事件实现 `IPayloadEventArgs`，无参 marker 事件实现 `IEmptyEventArgs`。

Payload 事件只支持 `InEventHandler<T>`，也就是 `in T` 参数订阅。旧的 `Action<T>` 值传参订阅已经移除，避免大 struct 在每个订阅者回调时复制。

无参事件仍然使用 `Action` 订阅，通过 `EventBus.Publish<T>()` 或 `SafePublisher.Publish<T>()` 发布。

## 定义事件

```csharp
using AlicizaX;

[Prewarm(8)]
public readonly struct PlayerLevelUpEvent : IPayloadEventArgs
{
    public readonly int PlayerId;
    public readonly int OldLevel;
    public readonly int NewLevel;

    public PlayerLevelUpEvent(int playerId, int oldLevel, int newLevel)
    {
        PlayerId = playerId;
        OldLevel = oldLevel;
        NewLevel = newLevel;
    }
}
```

`PrewarmAttribute` 用于声明事件初始容量。高频事件应按预期峰值提前设置容量，减少运行时扩容。

## 订阅和取消订阅

`Subscribe` 返回 `EventRuntimeHandle`。对象销毁或逻辑结束时必须调用 `Dispose()` 取消订阅。

```csharp
using AlicizaX;
using UnityEngine;

public sealed class PlayerLevelView : MonoBehaviour
{
    private EventRuntimeHandle _levelUpHandle;

    private void OnEnable()
    {
        _levelUpHandle = EventBus.Subscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }

    private void OnDisable()
    {
        _levelUpHandle.Dispose();
    }

    private void OnPlayerLevelUp(in PlayerLevelUpEvent evt)
    {
        Debug.Log($"Player {evt.PlayerId}: {evt.OldLevel} -> {evt.NewLevel}");
    }
}
```

无参事件示例：

```csharp
public readonly struct InventoryChangedEvent : IEmptyEventArgs
{
}

private EventRuntimeHandle _handle;

private void OnEnable()
{
    _handle = EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
}

private void OnDisable()
{
    _handle.Dispose();
}

private void OnInventoryChanged()
{
    RefreshInventory();
}
```

## 普通发布

普通 `Publish` 是极限性能路径，不做每个回调的异常隔离，也不支持派发期间修改同一个事件类型的订阅结构。

```csharp
var evt = new PlayerLevelUpEvent(playerId, oldLevel, newLevel);
EventBus.Publish(in evt);
```

也可以直接传临时值：

```csharp
EventBus.Publish(new PlayerLevelUpEvent(1001, 9, 10));
```

无参事件：

```csharp
EventBus.Publish<InventoryChangedEvent>();
```

普通 `Publish` 的硬约束：

1. 回调内部不要对同一个事件类型执行订阅、取消订阅、`ClearPayload`/`ClearEmpty` 或 `EnsurePayloadCapacity`/`EnsureEmptyCapacity`。
2. 回调不应抛异常；如果抛异常，普通发布不会保证后续订阅者继续执行。
3. Editor 和 Development Build 会尽量拦截派发期变更；Release Player 为性能路径，违反约束属于未定义行为。

## SafePublisher

如果必须在派发期间订阅、取消订阅、清空、扩容，或者希望某个订阅者异常不影响后续订阅者，使用 `SafePublisher`。

```csharp
var evt = new PlayerLevelUpEvent(playerId, oldLevel, newLevel);
SafePublisher.Publish(in evt);
```

无参事件：

```csharp
SafePublisher.Publish<InventoryChangedEvent>();
```

`EventBus.SafePublish(in evt)` 和 `EventBus.SafePublish<T>()` 是等价的便捷入口；为了代码语义清晰，业务中推荐优先写 `SafePublisher.Publish`。

`SafePublisher` 语义：

1. 当前这次发布使用稳定订阅快照。
2. 发布期间产生的订阅、取消订阅、`ClearPayload`/`ClearEmpty`、`EnsurePayloadCapacity`/`EnsureEmptyCapacity` 会进入 pending 队列。
3. 最外层 `SafePublisher.Publish` 结束后立即 flush pending 队列，不延迟到下一帧。
4. 每个订阅者单独捕获异常，记录日志后继续派发后续订阅者。
5. Safe 路径包含 `try/catch` 和 pending 处理，不应替代高频热路径上的普通 `Publish`。

## UI 内自动管理事件

UI 模块提供 `EventListenerProxy`。在 `UIBase.OnRegisterEvent` 中注册的事件会在窗口销毁时自动移除。

```csharp
using AlicizaX;
using AlicizaX.UI.Runtime;
using Game.UI;

public sealed class PlayerInfoWindow : UIWindow<ui_PlayerInfoWindow>
{
    protected override void OnRegisterEvent(EventListenerProxy proxy)
    {
        proxy.AddUIEvent<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }

    private void OnPlayerLevelUp(in PlayerLevelUpEvent evt)
    {
        baseui.TxtLevel.text = evt.NewLevel.ToString();
    }
}
```

无参 UI 事件仍然可以注册 `Action`：

```csharp
protected override void OnRegisterEvent(EventListenerProxy proxy)
{
    proxy.AddUIEvent<InventoryChangedEvent>(OnInventoryChanged);
}

private void OnInventoryChanged()
{
    RefreshInventory();
}
```

## 查询和清理

```csharp
int payloadCount = EventBus.GetPayloadSubscriberCount<PlayerLevelUpEvent>();
int emptyCount = EventBus.GetEmptySubscriberCount<InventoryChangedEvent>();

EventBus.EnsurePayloadCapacity<PlayerLevelUpEvent>(16);
EventBus.EnsureEmptyCapacity<InventoryChangedEvent>(16);

EventBus.ClearPayload<PlayerLevelUpEvent>();
EventBus.ClearEmpty<InventoryChangedEvent>();
```

`ClearPayload<T>()` / `ClearEmpty<T>()` 会移除某一种事件的所有订阅者，一般只在模块卸载、测试或热重载流程中使用。不要在普通 `Publish` 回调中调用；如确实需要，使用 `SafePublisher` 发布，让清理操作在本次派发结束后立刻执行。

## Event Monitor

编辑器菜单：

- `AlicizaX/Event Monitor`

Event Monitor 用于查看事件运行状态，主要包括：

- 当前订阅数、无参订阅数、`in` 订阅数、容量和容量利用率。
- 普通发布次数、Safe 发布次数。
- 订阅、取消订阅、扩容、清空次数。
- 普通发布期间非法变更次数。
- SafePublisher 捕获的回调异常次数。
- SafePublisher 延迟变更次数、Flush 次数、Pending 峰值。
- 订阅者列表、Unity 对象是否已销毁、lambda 或闭包提示。
- 快照对比和最近操作历史。

Event Monitor 只在 Editor 下生效，不进入 Player 热路径。Benchmark 的 release-like 模式会跳过调试统计，以便更接近 Release 性能。

## API 速查

| API | 说明 |
| --- | --- |
| `EventBus.Subscribe<T>(InEventHandler<T>)` | 订阅 payload 事件，参数以 `in` 传递 |
| `EventBus.Subscribe<T>(Action)` | 订阅无参事件 |
| `EventBus.Publish<T>(in T evt)` | 普通发布 payload 事件，极限性能路径 |
| `EventBus.Publish<T>()` | 普通发布无参事件 |
| `SafePublisher.Publish<T>(in T evt)` | Safe 发布 payload 事件 |
| `SafePublisher.Publish<T>()` | Safe 发布无参事件 |
| `EventBus.SafePublish<T>(in T evt)` | Safe 发布 payload 事件的便捷入口 |
| `EventBus.SafePublish<T>()` | Safe 发布无参事件的便捷入口 |
| `EventBus.GetPayloadSubscriberCount<T>()` | 获取 payload 事件订阅者数量 |
| `EventBus.GetEmptySubscriberCount<T>()` | 获取无参事件订阅者数量 |
| `EventBus.EnsurePayloadCapacity<T>(int capacity)` | 预分配 payload 事件订阅容量 |
| `EventBus.EnsureEmptyCapacity<T>(int capacity)` | 预分配无参事件订阅容量 |
| `EventBus.ClearPayload<T>()` | 清理指定 payload 事件所有订阅 |
| `EventBus.ClearEmpty<T>()` | 清理指定无参事件所有订阅 |
| `EventRuntimeHandle.Dispose()` | 取消单个订阅 |

## 注意事项

1. 事件参数必须是 `struct`；payload 事件实现 `IPayloadEventArgs`，无参 marker 事件实现 `IEmptyEventArgs`。
2. Payload 事件只使用 `in` 参数订阅，不再支持 `Action<T>` 值传参订阅。
3. 订阅句柄必须保存，并在不用时释放，否则会导致订阅者继续被调用。
4. 高频事件优先使用普通 `Publish`，并提前使用对应的 `EnsurePayloadCapacity` / `EnsureEmptyCapacity` 或使用 `PrewarmAttribute`。
5. 普通 `Publish` 回调中不要修改同一事件类型的订阅结构；需要这种语义时使用 `SafePublisher`。
6. 普通 `Publish` 不隔离回调异常；需要“一个回调报错不影响其它回调”时使用 `SafePublisher`。
7. 事件总线只负责通知，不负责状态保存；需要持久状态时应由业务服务或数据模块维护。
