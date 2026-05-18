# GameObjectPool 模块

GameObjectPool 模块用于池化 Unity `GameObject` 实例，适合特效、怪物、建筑、UI 临时对象等频繁创建和回收的预制体。模块按 `PoolConfigScriptableObject` 中的规则建立运行时池，支持 AssetBundle 和 Resources 两种加载器，提供同步获取、异步获取、预热、释放和强制清理。

源码位置：

- `Client/Packages/com.alicizax.unity.framework/Runtime/ABase/GameObjectPool`
- 编辑器辅助：`Client/Packages/com.alicizax.unity.framework/Editor/GameObjectPool`

## 使用前提

场景中的框架根节点需要挂载：

- `ResourceComponent`
- `GameObjectPoolComponent`

`GameObjectPoolComponent` 会在 `Awake` 中注册 `IGameObjectPoolService` 和内部调试服务。池化实例的资源加载依赖 `IResourceService`。

```csharp
using AlicizaX;

IGameObjectPoolService pool = AppServices.App.Require<IGameObjectPoolService>();
```

如果项目层提供了快捷入口，也可以通过快捷入口访问：

```csharp
IGameObjectPoolService pool = GameApp.GameObjectPool;
```

## 公开接口

当前公开接口 `IGameObjectPoolService` 暴露的是字符串路径接口：

```csharp
GameObject GetGameObject(string assetName, Transform parent = null);
UniTask<GameObject> GetGameObjectAsync(string assetName, Transform parent = null, CancellationToken cancellationToken = default);
UniTask PreloadAsync(string assetName, int count = 1, CancellationToken cancellationToken = default);
void Release(GameObject gameObject);
void ForceCleanup();
```

源码内部存在 `GameObjectPoolService.LoadCatalog(string poolConfigPath)` 用于加载 `PoolConfigScriptableObject`，但 `GameObjectPoolService` 是 `internal`，该方法未暴露到 `IGameObjectPoolService`。业务代码应以前置启动流程已加载 PoolConfig 为前提使用。

## 创建 PoolConfig

在 Project 面板创建：

```text
Create > GameplaySystem > PoolConfig
```

每条 `PoolEntry` 代表一条池化匹配规则：

| 字段 | 说明 |
| --- | --- |
| `entryName` | 规则名，调试展示使用 |
| `group` | 分组名，运行时会创建对应分组根节点 |
| `assetPath` | 资源路径、路径前缀或 glob 规则 |
| `loaderType` | `AssetBundle` 或 `Resources`；非法值会在 Normalize 时修正为 `AssetBundle` |
| `softCapacity` | 软容量，用于回收保留策略 |
| `hardCapacity` | 硬容量，池中最多实例数 |
| `priority` | 规则优先级，越大越优先匹配；同优先级下路径越长越优先 |

路径会被 Normalize：

- 去掉首尾空白。
- `\` 转成 `/`。
- 去掉尾部 `/` 或 `\`。
- AssetBundle 路径会去掉 `Assets/Bundle/` 或 `Assets/Bundles/` 根路径。
- Resources 路径会去掉 `Assets/Resources/` 或中间的 `/Resources/`。
- 文件扩展名会被去掉。

例如：

```text
Assets/Bundles/Effects/Explosion.prefab -> Effects/Explosion
Assets/Resources/UI/DamageText.prefab -> UI/DamageText
```

## 规则匹配

`assetPath` 支持两类规则：

- 无通配符：按路径前缀匹配，例如 `Effects` 可匹配 `Effects/Explosion`。
- glob：支持 `*`、`?`、`**`。

glob 规则示例：

```text
Effects/*          // 匹配 Effects 下一级资源
Effects/**         // 递归匹配 Effects 下所有资源
UI/DamageText_?    // ? 匹配单个字符
```

解析时先按 `priority` 降序、路径长度降序排序，再按 loader 类型匹配。无前缀请求会分别尝试 `Resources` 和 `AssetBundle` 规则；如果同一个逻辑路径同时命中两种 loader，运行时会输出歧义错误，建议显式加前缀。

## 请求路径

业务层直接使用字符串路径请求：

```csharp
GameObject go = GameApp.GameObjectPool.GetGameObject("Effects/Explosion", transform);
```

可以用前缀强制指定加载器：

```csharp
GameObject abEffect = GameApp.GameObjectPool.GetGameObject("ab:Effects/Explosion");
GameObject resText = GameApp.GameObjectPool.GetGameObject("res:UI/DamageText");
```

如果请求路径没有命中 PoolConfig 规则，模块会退化为直接加载。直接加载出来的对象不会进入池，调用 `Release` 时会被销毁。Editor 和 Development Build 下会对同一路径只警告一次。

## 预热

```csharp
using AlicizaX;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class PoolPreloadExample : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        await GameApp.GameObjectPool.PreloadAsync("Effects/Explosion", 10);
    }
}
```

预热会提前加载 prefab，并创建指定数量的非激活实例。实际创建数量不会超过规则的 `hardCapacity`。预热过程会分批让出帧，避免一次性实例化造成明显卡顿。

## 同步获取和释放

```csharp
using AlicizaX;
using UnityEngine;

public sealed class ExplosionSpawner : MonoBehaviour
{
    public GameObject Spawn(Vector3 position)
    {
        GameObject instance = GameApp.GameObjectPool.GetGameObject("Effects/Explosion", transform);
        if (instance == null)
        {
            return null;
        }

        instance.transform.position = position;
        return instance;
    }

    public void Despawn(GameObject instance)
    {
        GameApp.GameObjectPool.Release(instance);
    }
}
```

同步获取命中池时会优先复用非激活实例；没有可复用实例且未达到 `hardCapacity` 时会创建新实例；达到 `hardCapacity` 时返回 `null`。

`Release` 会检查对象上是否有 `GameObjectPoolHandle`。如果对象仍属于池，会回收到池；否则会直接销毁。

## 异步获取

```csharp
using System.Threading;
using AlicizaX;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class AsyncMonsterSpawner : MonoBehaviour
{
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _cts = new CancellationTokenSource();
    }

    public UniTask<GameObject> SpawnAsync()
    {
        return GameApp.GameObjectPool.GetGameObjectAsync(
            "Enemies/EnemySoldier",
            transform,
            _cts.Token);
    }

    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
```

异步获取会异步加载 prefab。池已达到 `hardCapacity` 且没有空闲实例时，异步请求会进入等待队列，后续有实例释放时再完成。取消请求通过主线程消费，避免跨线程修改池内部链表。

## 池化对象生命周期

预制体上的组件可以实现 `IGameObjectPoolable` 接收池化生命周期回调：

```csharp
using AlicizaX;
using UnityEngine;

public sealed class PooledEffect : MonoBehaviour, IGameObjectPoolable
{
    public void OnPoolGet(in PoolSpawnContext context)
    {
        // 对象已经 SetParent(context.Parent) 并 SetActive(true)。
        // 在这里重置播放状态、位置、计时器等运行时状态。
    }

    public void OnPoolRelease()
    {
        // 对象即将被 SetActive(false) 并移回分组根节点。
        // 在这里停止粒子、动画、音效或清理临时状态。
    }

    public void OnPoolDestroy()
    {
        // 池销毁或裁剪实例前调用。
    }
}
```

`PoolSpawnContext` 包含：

- `AssetPath`：逻辑资源路径。
- `Group`：池规则分组。
- `Parent`：本次获取时传入的父节点。
- `SpawnFrame`：获取发生的帧。

模块会缓存 `IGameObjectPoolable` 绑定关系，避免每次获取/释放时重新扫描组件。

## 回收和内存策略

运行时池使用分页 slot 存储实例，空页释放后会保留页索引用于后续复用，避免反复创建/销毁导致页元数据持续膨胀。

维护逻辑会根据以下因素裁剪空闲实例并卸载冷 prefab：

- `softCapacity`
- `hardCapacity`
- 空闲时长
- 短期和长期活跃峰值
- 低内存事件

```csharp
GameApp.GameObjectPool.ForceCleanup();
```

`ForceCleanup` 会立即执行维护逻辑，尝试裁剪空闲实例和卸载冷 prefab。Unity 低内存事件也会触发更激进的维护。

## 调试查看

运行时调试服务会提供：

- 池数量、已加载 prefab 数量。
- 总实例数、激活实例数、空闲实例数。
- 每条规则的命中、未命中、扩容、销毁、峰值等计数。
- 每个实例的激活状态、空闲时长、生命周期时长和对象引用。

调试快照对象来自框架 `MemoryPool`，读取后由服务内部回收。

## 性能说明

- 请求路径解析结果有缓存，缓存上限为 `4096` 条；超过后仍可解析，但不再继续增长缓存。
- 规范路径会走 fast path，不产生额外字符串；只有首尾空白、反斜杠、尾部分隔符等非规范输入才会分配新字符串。
- 全局首条字面规则支持快速直查；复杂 glob 仍按排序后的规则顺序扫描，保证优先级语义不变。
- `Release` 对非池对象会走销毁路径，不会尝试池化。

## API 速查

| API | 说明 |
| --- | --- |
| `GetGameObject(assetName, parent)` | 同步获取池化实例，失败返回 `null` |
| `GetGameObjectAsync(assetName, parent, token)` | 异步获取池化实例，可等待释放或异步加载 |
| `PreloadAsync(assetName, count, token)` | 预热实例 |
| `Release(gameObject)` | 释放池对象；非池对象直接销毁 |
| `ForceCleanup()` | 立即执行池维护 |
| `IGameObjectPoolable.OnPoolGet(...)` | 对象取出回调 |
| `IGameObjectPoolable.OnPoolRelease()` | 对象回收回调 |
| `IGameObjectPoolable.OnPoolDestroy()` | 对象销毁回调 |

## 注意事项

1. 业务层不要手动销毁池化对象，统一调用 `Release`。
2. `hardCapacity` 达到上限后，同步获取可能失败，业务代码必须处理 `null`。
3. 同一路径可能同时命中 AssetBundle 和 Resources 规则时，使用 `ab:` 或 `res:` 前缀消除歧义。
4. 未配置到 PoolConfig 的路径会直接加载，释放时销毁，不会享受池化收益。
5. `LoadCatalog` 目前不是公开接口，配置加载应由框架启动流程完成。
