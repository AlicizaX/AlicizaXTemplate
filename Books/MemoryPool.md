# MemoryPool 模块

`MemoryPool` 是框架提供的 C# 对象池，用于复用频繁创建和释放的临时对象，例如事件参数、加载请求、战斗结算数据、UI 列表项数据等。

源码位置：

- `Client/Packages/com.alicizax.unity.framework/Runtime/Modules/MemoryPool`
- Inspector：`Client/Packages/com.alicizax.unity.framework/Editor/Common/Inspector/MemoryPoolComponentInspector.cs`

## 使用前提

池对象必须继承 `MemoryObject`。没有 `IMemory` 接口。

```csharp
using AlicizaX;

public sealed class BattleDamageInfo : MemoryObject
{
    public int AttackerId;
    public int TargetId;
    public int Damage;
    public bool Critical;

    public override void Clear()
    {
        AttackerId = 0;
        TargetId = 0;
        Damage = 0;
        Critical = false;
    }
}
```

硬性要求：

- 类型必须是 `class`。
- 类型必须继承 `MemoryObject`。
- 类型不能是 `abstract`。
- 类型不能是 open generic。
- 类型必须有 public 无参构造函数。
- 只能在 Unity 主线程使用。
- 使用完成后必须调用 `MemoryPool.Release` 归还。
- `Clear()` / `OnEvict()` 里禁止再 `Acquire` / `Release`。

对象真正被驱逐时（hard 溢出、Tick 缩容、`ClearAll` / tombstone），可选实现 `IPoolEvictable.OnEvict()` 做资源收取。`Release` 回池只调 `Clear()`，不调 `OnEvict()`。

## 获取和归还

推荐使用泛型 API：

```csharp
BattleDamageInfo info = MemoryPool.Acquire<BattleDamageInfo>();
info.AttackerId = 1001;
info.TargetId = 2001;
info.Damage = 350;
info.Critical = true;

MemoryPool.Release(info);
```

也可以直接使用类型专属池：

```csharp
BattleDamageInfo info = MemoryPool<BattleDamageInfo>.Acquire();

// 使用对象...

MemoryPool<BattleDamageInfo>.Release(info);
```

动态类型入口：

```csharp
MemoryObject memory = MemoryPool.Acquire(typeof(BattleDamageInfo));
MemoryPool.Release(memory);
```

重复使用动态类型时应提前缓存 `MemoryPoolHandle`，避免每次 Type 查表：

```csharp
MemoryPoolHandle handle = MemoryPool.GetHandle(typeof(BattleDamageInfo));
MemoryObject memory = handle.Acquire();
handle.Release(memory);
```

## MemoryPoolComponentInspector 配置参数

这些参数在 `MemoryPoolSetting` 组件上配置。运行时 Inspector 也可以直接修改全局值。

### Idle Trim

| 参数 | 默认值 | 作用 | 限制 |
|---|---:|---|---|
| `Short Decay Start` | `1800` | 池空闲多少帧后开始衰减目标空闲水位。60 FPS 下约 30 秒。实际每 Tick 驱逐数由 Phase 预算决定（Gameplay=2）。 | `>= 0` |
| `Long Decay Start` | `7200` | 池空闲多少帧后加速衰减 Acquire 速率预测。60 FPS 下约 2 分钟。 | `>= Short Decay Start` |
| `Zero Free Start` | `7200` | 池空闲多少帧后允许 `TargetFreeReserve` 最低降到 `0`。到达前仍保留 `MinKeep=4`。 | `>= Long Decay Start` |
| `Unschedule Idle` | `18000` | 池空闲多少帧后允许停止 Tick，减少 CPU 调度成本。 | `>= Zero Free Start` |
| `Auto Trim Native` | `18000` | 池空闲多少帧后，如果 `Using=0`、`Unused=0`、`Constructed=0`，自动释放 native metadata。`-1` 表示关闭自动 Trim。 | `-1` 或 `>= Zero Free Start` |

默认时间轴（60 FPS / Gameplay）：

- 0 到 30 秒：目标空闲不往下掉，超过目标的多余对象仍按 Phase 预算逐帧驱逐。
- 30 秒后：目标空闲开始衰减。
- 2 分钟后：目标空闲允许降到 `0`。
- 5 分钟后：停 Tick；若 `Using=0` 且对象也清完，自动释放 native metadata。

`IdleFrames` 只在本帧有 `Acquire` / `Release` 或还有 miss 补仓时重置。**长租对象（`Using > 0`）不再锁死冷池倒计时**。租约本身不会被 Tick 杀掉，但同类型的空闲缓存会按上表缩容。

`TargetFreeReserve` 由近期突发量、近 8 帧 Acquire 预估、miss×2、保底（默认 4，2 分钟后可为 0）取最大，再夹到 `[MinKeep或0, min(Soft, Hard)]`。

### Phase 预算

`MemoryPoolSetting.Phase` / `MemoryPoolRegistry.Phase` 控制每 Tick 最多创建 / 驱逐多少个：

| Phase | Growth | Evict |
|---|---:|---:|
| Boot / Loading | 32 | 4 |
| Gameplay | 2 | 2 |
| Background | 8 | 16 |
| LowMemory | 0 | 32 |

`Acquire` 仍不会失败：缓存不够时当场 `new T()`。LowMemory 只是不预创建、加快驱逐。

### Capacity

| 参数 | 默认值 | 作用 | 限制 |
|---|---:|---|---|
| `Soft Free Limit` | `128` | 默认空闲缓存软上限。新池使用该值，运行时修改会同步到已创建池。 | `>= 4` |
| `Hard Free Limit` | `512` | 默认空闲缓存硬上限。释放对象时，如果空闲数已达到硬上限，对象会被驱逐，不回到 free 队列。 | `>= Soft Free Limit` |

`Hard Free Limit` 不是正在使用对象的上限。`Acquire` 不能失败，所以池里没有空闲对象时仍会应急 `new T()` 返回对象。硬上限只限制“归还后最多缓存多少空闲对象”。

## Inspector 调试面板

运行时选中场景里的 `MemoryPoolSetting` 组件，可以看到调试面板。

### Configuration

- `Memory Pool Count`：已经注册过的池类型数量。注意这是注册表数量，不等于当前缓存对象数量。
- `Show Full Class Name`：显示完整类型名，方便定位同名类。
- `Show Empty Pools`：默认关闭。关闭时隐藏 `Unused=0 && Using=0 && PageCap=0` 的空池。打开后可以看到只剩注册表 handle 的池类型。

空池仍出现在统计里的原因：某个类型一旦被 materialize，注册表会保留这个类型的 handle。即使对象和 native metadata 都清掉了，handle 仍用于后续 O(1) 再访问。这不是内存泄露。

### Overview

- `Total Cached`：所有池当前空闲对象总数，对应各池 `Unused` 之和。
- `Total In Use`：所有池当前借出未归还对象总数，对应各池 `Using` 之和。
- `Total Page Capacity`：所有池当前活跃 page 可容纳 slot 总数，对应各池 `PageCap` 之和。

判断是否存在明显泄露，优先看 `Total In Use`。如果场景退出、业务结束、窗口关闭后该值长期不回到 `0`，说明有对象没有归还。

### Pools 列表字段

| 字段 | 含义 | 分析方式 |
|---|---|---|
| `Unused` | 当前池内空闲对象数量。 | 长期很高表示缓存较多，不一定泄露。超过冷却时间后应逐步下降。 |
| `Using` | 当前借出未归还对象数量。 | 泄露排查第一字段。业务结束后长期大于 0，基本就是未归还。 |
| `Acquire` | 累计获取次数。 | 与 `Release` 对比，判断归还是否匹配。 |
| `Release` | 累计归还次数。 | 正常情况下长期应接近 `Acquire`。差值通常接近当前 `Using`。 |
| `Created` | 累计创建对象次数。 | 持续增长说明池容量不足、对象峰值上升，或释放太慢导致频繁 miss。 |
| `Target` | 当前目标空闲保留数量，即 `TargetFreeReserve`。 | 活跃期会升高，冷却后会下降。到 `Zero Free Start` 后允许降到 0。 |
| `MaxCap` | 当前硬空闲缓存上限。 | 归还时超过该值会直接驱逐。 |
| `Idle` | 当前空闲帧数。 | 判断是否进入冷池阶段。本帧有获取/归还/补仓时重置；仅 `Using > 0` 不会阻止增长。 |
| `PageCap` | 当前活跃 page 的 slot 容量。 | `Unused=0 && Using=0 && PageCap=0` 表示对象和 native metadata 都已经清干净，只剩注册表 handle。 |

## 如何判断是否泄露

### 1. 看 `Using`

业务结束后等待几帧，如果某个池：

- `Using > 0`
- `Release` 没追上 `Acquire`
- 对应对象逻辑上已经不应该存在

这是真正的高概率泄露：对象借出后没有 `Release`。

处理方式：沿着该类型的 `Acquire` 调用点检查所有异常分支、提前 return、取消流程、对象生命周期结束点，确保最终都会调用 `MemoryPool.Release`。

### 2. 区分缓存和泄露

`Unused > 0` 不是泄露。它表示对象已经归还，当前只是缓存起来等待复用。

默认策略下，如果池不再使用：

- 约 30 秒后开始缩容。
- 约 2 分钟后目标空闲允许降到 0。
- 缩容速度受当前 `MemoryPoolPhase` 的 evict budget 控制，不会一帧全部释放。
- 约 5 分钟后，如果完全空池，会自动 Trim native metadata。

### 3. 看 `PageCap`

`PageCap > 0` 表示该池仍持有活跃 page 或 native metadata。

常见情况：

- `Using > 0`：不能释放，正常。
- `Unused > 0`：还有空闲缓存，正常。
- `Using=0 && Unused=0 && PageCap>0`：对象已经清完，但 metadata 还没自动 Trim；达到 `Auto Trim Native` 后会清，或手动点 `Trim Native`。
- `Using=0 && Unused=0 && PageCap=0`：已经清干净，不是泄露。默认会被 `Show Empty Pools` 隐藏。

### 4. 看 `Created`

`Created` 持续上涨通常说明运行时发生 miss：

- 短时间峰值超过当前空闲缓存。
- `Soft Free Limit` 太低。
- 高频对象没有提前预热。
- 对象被归还得太晚，导致后续请求拿不到 free 对象。

如果目标是运行时零分配，压测时应关注 `Created` 是否在稳定阶段继续增长。稳定阶段继续增长就是性能问题。

### 5. 看 `Acquire - Release`

粗略判断：

```text
Acquire - Release ~= Using
```

如果差值持续扩大，且 `Using` 也持续上升，基本是未归还。

如果差值短时间扩大后又回落，是正常高峰。

## 调试按钮

- `Clear Cached`：清理所有池的空闲缓存。仍在使用的对象进入 tombstone，归还时驱逐并在最后一个租约归还后释放 native metadata。无租约时 `ClearAll` 会直接释放页表。
- `Trim Native`：在没有借出对象的池上释放 native metadata。`Using > 0` 的池不会被强行释放。
- `Reset Stats`：重置统计计数，例如 `Acquire`、`Release`、`Created`，方便重新压测一段业务。

推荐排查流程：

1. 进入目标业务前点 `Reset Stats`。
2. 执行业务流程。
3. 退出业务后观察 `Using` 是否回到 0。
4. 等待冷却，观察 `Unused` 是否下降。
5. 如需确认 metadata 是否能释放，点 `Trim Native` 或等待 `Auto Trim Native`。
6. 打开 `Show Empty Pools`，确认空池是否只是 `Unused=0 && Using=0 && PageCap=0` 的注册表残留。

## 手动清理 API

```csharp
// 降低某一类型的目标保留数量，并按预算淘汰多余 free 对象。
MemoryPool.Remove<BattleDamageInfo>(32);

// 清空某一类型对象池。
MemoryPool.RemoveAll<BattleDamageInfo>();

// 触发某一类型对象池淘汰多余 free 对象。
MemoryPool.Compact<BattleDamageInfo>();

// 清空所有对象池。
MemoryPool.ClearAll();

// 触发所有对象池压缩。
MemoryPool.CompactAll();

// 释放所有可释放池的 native metadata。
MemoryPool.TrimAllNativeMetadata();
```

通常业务代码不需要在退出场景时手动清理全部池。`RootModule` 关闭时调 `ClearAllNativeMetadata()`；`MemoryPoolSetting.OnDestroy` 只 `TrimAllNativeMetadata()`（有租约的池跳过，不会强杀已借出对象）。Editor domain reload 会强制释放 native 页表。手动清理主要用于调试、压测和低内存场景。
