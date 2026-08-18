# GameObjectPool 模块

GameObjectPool 只池化 Unity `GameObject` 实例。资源系统管 Prefab 引用，池管 `Instantiate` / 回收 / 按策略销毁。

适合特效、掉落物、HUD 控件、通知条。不要把 FishNet `NetworkObject`、整窗 UI、纯 ECS 实体塞进来。

源码：

- `Client/Packages/com.alicizax.unity.framework/Runtime/Modules/GameObjectPool`
- `Client/Packages/com.alicizax.unity.framework/Editor/Modules/GameObjectPool`

## 使用前提

场景框架根节点需要：

- `ResourceComponent`
- `GameObjectPoolComponent`

`GameObjectPoolComponent` 在 `Awake` 注册 `IGameObjectPoolService`。Prefab 源资产走内部 `IPrefabLoader`：`LoadLease<GameObject>` 持有租约，`UnloadPrefab` 成对 `Dispose`。**不会**走 `LoadGameObject`（那个 API 会实例化并绑 `ResourceOwner`，和池生命周期冲突）。

```csharp
using AlicizaX;

IGameObjectPoolService pool = GameApp.GameObjectPool;
```

启动时必须先加载目录，未登记的 location 一律拒绝进池：

```csharp
GameApp.GameObjectPool.LoadCatalog("GameObjectPoolConfig");
// 或
GameApp.GameObjectPool.LoadCatalog(configAsset);
```

`LoadCatalog(string)` 的参数是 YooAsset location，不是磁盘路径。内部用 `LoadLease<PoolConfigScriptableObject>` 读配置，`BuildCatalog` 完立刻 `Dispose`，规则已编译进内存。

## 公开接口

```csharp
GameObject Spawn(string location, Transform parent = null);
T Spawn<T>(string location, Transform parent = null) where T : Component;
bool TrySpawn(string location, Transform parent, out GameObject instance);

UniTask<GameObject> SpawnAsync(string location, Transform parent = null, CancellationToken ct = default);
UniTask<T> SpawnAsync<T>(string location, Transform parent = null, CancellationToken ct = default) where T : Component;

GameObject LoadPrefab(string location);
UniTask<GameObject> LoadPrefabAsync(string location, CancellationToken ct = default);

UniTask WarmupAsync(string location, int count, CancellationToken ct = default);

void Despawn(GameObject instance);
void Despawn(GameObjectPoolHandle handle);

void Flush(string location);
void FlushGroup(string group);
void FlushAll();

void LoadCatalog(PoolConfigScriptableObject config);
void LoadCatalog(string poolConfigPath);
```

| API | 会加载 Prefab | 会 Instantiate | 硬顶 / 未登记 |
| --- | --- | --- | --- |
| `TrySpawn` / `Spawn` | 否 | 仅当 Prefab 已在内存且无 idle | `null` / `false` |
| `SpawnAsync` | 会 | idle 或新建 | `null` |
| `LoadPrefab*` | 会 | 否 | `null` |
| `WarmupAsync` | 会 | 填 idle，分帧 | 停在 hard |

同步 `Spawn` **不会** `LoadPrefab`，也不会转异步。Prefab 没在内存时直接 `null`；已加载且有 idle 则弹尾复用，没有 idle 则 `Instantiate` 直到 hard。要先钉住 Prefab 再同步出对象，调 `LoadPrefab` / `LoadPrefabAsync` / `WarmupAsync` / `SpawnAsync`。

`LoadCatalog`（string 或资源引用）会重编目录并 **清掉当前所有运行时池**。硬顶不会排队。没有空闲实例且 `total == hard` 时，同步和异步都立刻 `null`。

## 创建 PoolConfig

```text
Create > AlicizaX > PoolConfig
```

双击资源打开配置窗。推荐放到 YooAsset 已收集的配置目录，例如：

```text
Client/Assets/Bundles/Configs/sciptableObject/GameObjectPoolConfig.asset
```

默认包是 `AddressByFileName` 时，启动代码传：

```text
GameObjectPoolConfig
```

不要把 Windows 路径或 `Assets/...` 当 `LoadCatalog` 参数，除非 Collector 地址规则明确支持。

模板里 `Assets/Bundles/Configs/sciptableObject/` 目录已存在，但 **没有内置 `GameObjectPoolConfig.asset`**，`Assets/Bundles/Entity` / `Effects` 也是空的。要自己建配置和 Prefab，再 `LoadCatalog`。

Yoo 初始化后：

```csharp
GameApp.GameObjectPool.LoadCatalog("GameObjectPoolConfig");
var fx = await GameApp.GameObjectPool.SpawnAsync("Explosion", transform);
GameApp.GameObjectPool.Despawn(fx);
```

规则示例（数字仅供参考，不是工程里的现成配置）：

| 规则 | pattern | group | policy | min-soft-hard | 用途 |
| --- | --- | --- | --- | --- | --- |
| Explosion (Burst) | `Explosion` | FX | Burst | 0-8-16 | 文件名地址的精确匹配 |
| Effects (Burst glob) | `Effects/**` | FX | Burst | 0-16-64 | 路径型地址下的特效 |
| HUD (Fixed) | `HUD*` | HUD | Fixed | 2-8-16 | 文件名以 HUD 开头的控件 |
| Entity (Sticky) | `Entity/**` | Entity | Sticky | 0-4-8 | 路径型实体；文件名地址请写精确规则 |

当前 Collector 是 `AddressByFileName`，`Explosion.prefab` 的请求必须是 `Explosion`，不会命中 `Effects/**` 或 `Entity/**`。

## 规则字段

每条 `PoolEntry` 对应一类 location：

| 字段 | 说明 |
| --- | --- |
| `entryName` | 调试名 |
| `group` | 空闲实例挂到 `[Group]` 节点。空值回落 `DefaultGroup` |
| `assetPath` | YooAsset location 或 glob。不含通配符 = 精确匹配 |
| `policy` | `Fixed` / `Burst` / `Sticky`，默认 `Burst` |
| `minIdle` | 维护后至少保留的空闲数 |
| `softCapacity` | 空闲修剪目标上限 |
| `hardCapacity` | 总实例硬顶（含在场）。到达后 Spawn 返回 `null` |
| `idleSeconds` | 仅 Burst：最老空闲超过该秒才剪 |
| `unloadPrefab` | 池被剪空后是否 `Dispose` Prefab 源租约 |
| `priority` | 配置窗拖拽顺序自动维护，越靠上越先匹配 |

路径 Normalize：

- 去空白、`\` 转 `/`、去掉尾部分隔符和扩展名
- 去掉 `Assets/Bundles/` 或 `Assets/Bundle/` 前缀

```text
Assets/Bundles/Effects/Explosion.prefab  -> Effects/Explosion
Explosion                                -> Explosion
Effects/**                               -> 递归匹配 Effects 下所有 location
```

请求 location 也走同一套 Normalize。规则写成 `Effects/Explosion` 时，业务应请求 `Effects/Explosion`，或请求能 Normalize 成同一字符串的路径。当前项目若 Collector 用文件名地址，规则和请求都写 `Explosion`。

## 策略

| Policy | 行为 |
| --- | --- |
| **Fixed** | 可涨到 hard。一有空闲且 `total > retain` 就立刻剪空闲；`retain = clamp(minIdle, 0, soft)`。在场对象不剪。适合 HUD |
| **Burst** | 可涨到 hard。`total > soft` 时立刻剪空闲；未超 soft 时最老空闲超过 `idleSeconds` 再剪。适合特效 / 弹体 |
| **Sticky** | 只涨不自动剪，等 `Flush` 或 `Application.lowMemory`。适合关卡常驻 |

`Flush` / 低内存：所有策略（含 Sticky）按 `minIdle` 剪空闲；剪空后若 `unloadPrefab` 为真则释放 Prefab 源租约（进入资源模块 Idle TTL，不是立刻从内存抠掉）。普通 Tick 不会动 Sticky。每次维护有剪裁预算（约 `soft/4`，封顶 16；低内存 16），不一定一帧剪完。

## 请求

```csharp
using AlicizaX;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class ExplosionSpawner : MonoBehaviour
{
    public GameObject Spawn(Vector3 position)
    {
        GameObject instance = GameApp.GameObjectPool.Spawn("Explosion", transform);
        if (instance == null)
        {
            return null;
        }

        instance.transform.position = position;
        return instance;
    }

    public async UniTask<GameObject> SpawnAsync(Vector3 position)
    {
        GameObject instance = await GameApp.GameObjectPool.SpawnAsync("Explosion", transform);
        if (instance == null)
        {
            return null;
        }

        instance.transform.position = position;
        return instance;
    }

    public void Recycle(GameObject instance)
    {
        GameApp.GameObjectPool.Despawn(instance);
    }
}
```

热路径（命中）：`location` 精确表 -> 弹 idle 尾 -> `SetParent` -> `SetActive(true)` -> `OnSpawn`。创建时扫一次 `IGameObjectPoolable`，之后只用缓存。

`Despawn` 找 `GameObjectPoolHandle`。对得上就回池；否则 `Destroy`，Editor / Development Build 对同一名字只警告一次。外毁走 `OnDestroy` 摘槽，同样警告一次。

不要自己 `Destroy` 池对象。

## 预热

```csharp
await GameApp.GameObjectPool.WarmupAsync("Explosion", 10);
```

先异步钉 Prefab，再分帧造空闲实例（每帧约 8 个或 1ms）。数量卡在 `hardCapacity`。取消即停，已造的留在 idle。

只想钉 Prefab、不造实例：

```csharp
await GameApp.GameObjectPool.LoadPrefabAsync("Explosion");
```

之后同步 `Spawn` 才能命中。

## 生命周期

重置写在 Prefab 自己身上。池不帮你清 Trail、不停粒子、不扫丢失引用。

```csharp
using AlicizaX;
using UnityEngine;

public sealed class PooledMuzzleFlash : MonoBehaviour, IGameObjectPoolable
{
    [SerializeField] ParticleSystem _fx;
    [SerializeField] TrailRenderer _trail;

    public void OnSpawn(in PoolSpawnContext context)
    {
        _fx.Clear(true);
        _fx.Play(true);
    }

    public void OnDespawn()
    {
        _fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _trail.Clear();
    }

    public void OnPooledDestroy()
    {
    }
}
```

`PoolSpawnContext`：`Location`、`Group`、`Parent`、`SpawnFrame`。

一个 Prefab 可以挂多个接口。没有接口 = 只做显隐和挂接。

## 手动清理

```csharp
GameApp.GameObjectPool.Flush("Explosion");
GameApp.GameObjectPool.FlushGroup("FX");
GameApp.GameObjectPool.FlushAll();
```

`Flush` 按低内存计划剪到 `minIdle`，空池且允许时释放 Prefab 源租约。Sticky 也吃 Flush。

## 编辑器

配置窗：左列表一行 `group / pattern  Policy  min-soft-hard`。右栏编辑当前规则；`idleSeconds` 仅 Burst 显示。保存前看重复 `assetPath` 警告。

Play 模式选中 `Assets/Scenes/Main.unity` 里的 `Entry/GameObjectPool`：

- Runtime Summary：Ready / Pools / Loaded Prefabs / Instances / Active / Inactive / Pending Maintenance
- 每池一行：`[FX] Explosion` + `A12/I20/T32 | Hit 98/120`（Hit / Spawn）
- 展开才列出实例，可点到 Hierarchy
- `Flush This` / `Flush Group`

Hierarchy 本身也能看：`[FX]`、`[HUD]` 下挂休眠体。

计数器只加不减，Flush 不清零。

## 性能

- 命中是摊还 O(1)，和池里有 16 还是 8000 个无关。
- 第一次 glob 命中后写回精确表。字面规则直接查表。
- 维护堆只处理到期池，不每帧扫全部实例。
- 上限在 Unity `SetActive` / `SetParent`，不在目录。大物体不要每次整树显隐。

## 明确不做

- 未登记资产自动建池或直接 Instantiate
- 同步失败转异步
- 硬顶等待队列
- `res:` / `ab:` 双 Loader
- 回收时全树 `GetComponentsInChildren`
- 通用 Reset 基类、组件白名单

一次性物体继续用 `GameApp.Resource.LoadGameObject`。池和一次性物不互相兜底。

## 旧 API

已删除，不要再写：

- `GetGameObject` / `GetGameObjectAsync`
- `PreloadAsync`
- `Release`
- `ForceCleanup`
- `IGameObjectPoolable.OnPoolGet` / `OnPoolRelease` / `OnPoolDestroy`
- `PoolEntry.loaderType`

对应改为 `Spawn` / `SpawnAsync` / `WarmupAsync` / `Despawn` / `Flush`，生命周期改为 `OnSpawn` / `OnDespawn` / `OnPooledDestroy`。
