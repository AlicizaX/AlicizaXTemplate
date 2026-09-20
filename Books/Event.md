# Event 模块

`EventBus` 是唯一公开门面，仅支持主线程同步调用。每个监听单独捕获异常并用 `UnityEngine.Debug.LogException` 记录，随后继续派发；调用方仍负责订阅的生命周期。

## 定义和使用

```csharp
public readonly struct InventoryChangedEvent : IEmptyEventArgs { }

[Prewarm(16)]
public readonly struct PlayerLevelUpEvent : IPayloadEventArgs
{
    public readonly int PlayerId;
    public readonly int NewLevel;
    public PlayerLevelUpEvent(int playerId, int level)
    {
        PlayerId = playerId;
        NewLevel = level;
    }
}

private EventRuntimeHandle _inventory;
private EventRuntimeHandle _level;

private void OnEnable()
{
    _inventory = EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
    _level = EventBus.Subscribe<PlayerLevelUpEvent>(OnLevelUp);
}

private void OnDisable()
{
    _inventory.Dispose();
    _level.Dispose();
}

private void OnInventoryChanged() { RefreshInventory(); }
private void OnLevelUp(in PlayerLevelUpEvent e) { ShowLevel(e.NewLevel); }

private void Notify()
{
    EventBus.Publish<InventoryChangedEvent>();
    var payload = new PlayerLevelUpEvent(1001, 10);
    EventBus.Publish(in payload);
}
```

事件必须为结构体，并且恰好实现一种标记接口。空事件不能含实例字段（包括自动属性的后备字段）。Source Generator 提供编译期诊断；绕过生成器的程序集在容器首次初始化时仍会被拒绝。空事件只能使用 `Action` 和无参 `Publish<T>()`；带参事件使用 `InEventHandler<T>` 和 `Publish(in T)`。

传入单个方法的委托是调用前置条件，不支持组合后的 multicast delegate。相同事件类型、相同目标和相同方法只能订阅一次；重复申请逐次记录 Warning 并返回默认句柄。不同实例的相同方法可以各自订阅。Dispose 后可立即重新申请；若正在派发，新申请仍需等待提交。

## 派发和变更时序

| 操作 | 当前派发中的效果 | 物理账本提交 |
| --- | --- | --- |
| Subscribe | 立即返回预留句柄；新监听不参与当前或同类型嵌套发布 | 当前事件最外层发布返回后按 pending 顺序加入 packed |
| Dispose | 立即取消后续调用，也影响同类型嵌套发布 | 延后 swap-remove 和槽位回收 |
| Clear | 已存在且未 Dispose 的监听继续当前及嵌套派发；逻辑订阅索引立即清空 | 按 pending 顺序清空；之后排队的订阅可重新建立 |
| ReserveCapacity | 容量请求排队；Subscribe 必要时仍可扩展预留槽位 | 不在派发中替换 packed 数组 |
| SubscriberCount | 返回已提交的 packed 数量，可能包含等待删除的监听 | 最外层结束后收敛 |

深度按事件类型维护。A→B 中 B 返回即可提交 B 的变更；A→B→A 中 A 的变更必须等最外层 A 返回。每轮按开始时的 packed 顺序调用，已取消的监听跳过；swap-remove 可以改变下一轮顺序。

退订将对应待调用回调置为 null，发布循环在每次调用前读取它；槽位、packed 顺序和数量直到 pending 提交才改变。槽位中的原始委托仍保留到回收，用于订阅索引所有权和诊断查询。Clear 不设置这些取消标记，因此不会打断已开始的派发。

Clear 不重置类型注册、容量或版本序列。默认、过期、重复释放及无效 typeId/槽位/版本的句柄不会改变订阅账本。不承诺防御反射篡改为其他合法 typeId 的伪造句柄。类型注册表在当前程序集域生命周期内只增不减。

## 容量和清理

```csharp
EventBus.ReserveEmptyCapacity<InventoryChangedEvent>(16);
EventBus.ReservePayloadCapacity<PlayerLevelUpEvent>(32);

int emptyCount = EventBus.GetEmptySubscriberCount<InventoryChangedEvent>();
int payloadCount = EventBus.GetPayloadSubscriberCount<PlayerLevelUpEvent>();

EventBus.ClearEmpty<InventoryChangedEvent>();
EventBus.ClearPayload<PlayerLevelUpEvent>();
```

容量只扩不缩，负数被拒绝，0 不会缩小已有存储。默认初始容量为 4。`[Prewarm(n)]` 生成的入口设置 `EventInitialSize<T>.Size` 并调用对应 Reserve API，真实分配存储；容器已经初始化时也可补足容量。直接修改 `EventInitialSize<T>.Size` 只影响尚未初始化的容器。

自动增长按倍数扩容，Editor 记录诊断警告，Player 不记录扩容警告。监听异常日志与异常隔离不受 `ENABLE_EXCEPTION_LOG` 或 Editor 诊断开关影响。

稳定 Publish 为 O(N)，常规订阅和退订为期望/均摊 O(1)，扩容为 O(C)。Clear 为 O(N+C) 上界（含订阅索引清空），pending 提交为 O(P + 被 Clear 处理的监听数 + 扩容量)。稳定派发和容量充足时使用已缓存单方法委托的订阅/退订要求框架自身 0 B；调用方创建委托、类型首次初始化、数组/字典增长、重复拒绝和异常日志分别计量。

带参数据在 Publish 到回调之间按 `in T` 传递，pending 和 Debug 不保存 payload。业务访问非 readonly 结构体成员仍可能触发 C# 防御性拷贝，推荐不可变 payload。

## Unity 和 UI 所有权

Disable、Destroy 本身不会让总线自动退订。委托仍持有托管目标；访问已销毁 Unity 对象可能抛出异常，后续监听仍继续执行，残留订阅仍能从账本和监视器观察到。订阅者应在适当的 OnDisable、OnDestroy 或池归还流程中 Dispose。

UI 的 `EventListenerProxy.AddUIEvent<T>` 统一调用 `EventBus.Subscribe`，由代理释放时逐一 Dispose 句柄；本次重构不改变 UI 的窗口生命周期。

## 迁移

| 已删除 API | 替代方式 |
| --- | --- |
| `SafePublisher.Publish` / `EventBus.SafePublish` | `EventBus.Publish`，默认逐监听隔离异常 |
| 公开 `EmptyEventContainer<T>` / `EventContainer<T>` | 所有订阅、发布、计数和清理均经过 EventBus |
| `EnsureEmptyCapacity<T>` | `ReserveEmptyCapacity<T>` |
| `EnsurePayloadCapacity<T>` | `ReservePayloadCapacity<T>` |
| 同目标同方法重复订阅 | 持有一个订阅句柄；需要独立所有权时使用不同监听实例 |

不保留旧名转发。运行时只有内部 `EventContainer<TEvent,THandler>`，两个 Publish 重载由生成器的一份模板生成，分别直接调用 `Action()` 和 `handler(in payload)`。

## Event Monitor

`AlicizaX/Event Monitor` 显示统一发布数、订阅数、容量、异常数、pending/flush、订阅目标及销毁状态。移除了旧派发模式计数。详细历史为固定 128 条环形记录；查询详情会创建诊断数组，不属于业务 Publish 热路径。测试的诊断关闭模式只关闭统计与扩容警告，不能视为实跑 Release Player。

测试、性能数据及执行边界见 [重构验收记录](EventRefactorVerification-20260920.md)；十万/三十万发布者、订阅和退订的耗时、分配及 GC 结果见 [大规模压力测试记录](EventStressVerification-20260920.md)。
