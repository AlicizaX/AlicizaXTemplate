# Resources 模块

Resources 模块是框架对 YooAsset 的资源服务封装，负责资源包初始化、资源查询、同步/异步加载、Prefab 实例化、资源租约、组件绑定、下载器创建、缓存清理和资源回收。

底层 YooAsset 句柄只活在资源记录（AssetSlot）里。业务不要直接拿 `AssetHandle`。同一地址的同步/异步加载会合流到同一条记录。

源码位置：

- `Client/Packages/com.alicizax.unity.framework/Runtime/Modules/Resource/Resource`

运行时主要入口：

- `ResourceComponent`：Unity 场景组件，负责注册 `IResourceService`、初始化 YooAsset、驱动自动回收。
- `IResourceService`：资源服务接口，业务代码通过 `AppServices.Require<IResourceService>()` 或 `GameApp.Resource` 获取。
- `IResourceBindingService`：资源绑定服务，管理 `Image`、`SpriteRenderer`、`Renderer` 等组件上的资源引用。
- `ResourceOwner`：绑定生命周期组件，目标对象销毁时自动释放绑定资源。运行时字段不序列化。

## 使用前提

场景中的框架根节点需要挂载：

- `ObjectPoolComponent`
- `ResourceComponent`

`ResourceComponent.Awake()` 会注册 `IResourceService`，初始化 YooAsset，创建默认资源包，并把 Inspector 参数写入资源服务。业务代码通常这样获取服务：

```csharp
using AlicizaX;
using AlicizaX.Resource.Runtime;

IResourceService resources = AppServices.Require<IResourceService>();
// 或 GameApp.Resource
```

编辑器 Play Mode 注意：`Assets/Scenes/Main.unity` 里 `Procedure`、`Localization`、`UI`、`Audio`、`GameObjectPool` 默认可能是关闭的。不启用 `Procedure` 时包不会走启动流程初始化；不启用 `Localization` 时 Hotfix 读表会失败。

## ResourceComponent Inspector 参数

`ResourceComponent` 菜单路径为 `Game Framework/Resource`，默认执行顺序为 `-700`。

| 字段 | 默认值 | 作用 |
| --- | --- | --- |
| `Min Unload Unused Assets Interval` | `60` | 最小自动回收间隔，单位秒。预留回收请求只有超过该间隔后才会执行。 |
| `Max Unload Unused Assets Interval` | `300` | 最大自动回收间隔，单位秒。超过该时间后会自动调用 `UnloadUnusedAssets()`。 |
| `Use System Unload Unused Assets` | `true` | 强制回收并请求 GC 时，是否额外调用 Unity 的 `Resources.UnloadUnusedAssets()`。普通自动回收只执行资源服务回收。 |
| `Min GC Collect Interval` | `30` | `GC.Collect()` 的最小触发间隔，单位秒。即使多次请求 GC，也会受该间隔保护。 |
| `Decryption Services` | 空 | YooAsset 解密服务类型名。非空时通过 `AlicizaX.Utility.Assembly.GetType()` 查找类型并创建 `IDecryptionServices` 实例。 |
| `Auto Unload Bundle When Unused` | `false` | 传给 YooAsset 初始化参数的 `AutoUnloadBundleWhenUnused`。开启后，资源包引用计数归零时 YooAsset 可自动卸载 Bundle。 |
| `Play Mode` | `EditorSimulateMode` | YooAsset 运行模式。编辑器中会从 `EditorPrefs` 读取 `GamePlayMode`。支持 `EditorSimulateMode`、`OfflinePlayMode`、`HostPlayMode`、`WebPlayMode`。 |
| `Package Name` | `DefaultPackage` | 默认资源包名称。`IResourceService.DefaultPackageName` 和 YooAsset 默认包都使用这个值。 |
| `Milliseconds` | `30` | YooAsset 操作系统每帧最大时间片，调用 `YooAssets.SetOperationSystemMaxTimeSlice()`，单位毫秒。 |
| `Downloading Max Num` | `10` | 创建资源下载器时的最大并发下载数量。 |
| `Failed Try Again` | `3` | 创建资源下载器时单个下载失败后的最大重试次数。 |
| `Asset Record Capacity` | `64` | 资源记录预热容量。用于资源记录表、加载 Key 索引等内部结构，降低运行中扩容。 |
| `Asset Lease Capacity` | `128` | 资源租约槽预热容量。直接租约和绑定租约会消耗该容量。 |
| `Binding Owner Capacity` | `64` | 绑定 Owner 预热容量。一个 `ResourceOwner` 对应一个 Owner 记录。 |
| `Binding Slot Capacity` | `128` | 绑定槽预热容量。每个被绑定的组件属性占用一个绑定槽。 |
| `Registered Target Capacity` | `128` | 已注册目标组件预热容量，用于快速根据组件定位所属 Owner。 |
| `Idle Asset Expire Time` | `60` | 无引用资源进入 Idle 后的过期秒数。这是释放底层句柄的权威 TTL。 |
| `Expire Process Count Per Frame` | `16` | 每帧处理 Idle / KeepAlive 过期桶的上限。 |
| `Expire Process Count When Unloading` | `256` | 触发 `UnloadUnusedAssets` 当帧允许处理的过期数量。 |

低内存回调 `Application.lowMemory` 会触发 `ResourceService.OnLowMemory()`，最终请求强制释放未使用资源并执行 GC。Audio 不再单独挂 `Application.lowMemory`。

## 所有权模型

YooAsset / Unity 都不知道业务还在不在用这份资源。句柄必须有明确 Owner，不能靠 `Resources.UnloadUnusedAssets()` 去猜。

| 场景 | 推荐 API | 谁释放 |
| --- | --- | --- |
| 临时读表、配置、音频 Clip | `LoadLease<T>` / `LoadLeaseAsync<T>` | `Dispose()`。引用归零后进入 Idle，等 `IdleAssetExpireTime` 再卸句柄 |
| 绑到 Image / Renderer | Binding 扩展方法 | 物体销毁或换绑时由 `ResourceOwner` 自动放 |
| 实例化到场景的 Prefab | `LoadGameObject` / `LoadGameObjectAsync` | `Destroy(instance)`，实例上的 `ResourceOwner` 自动解除源 Prefab 绑定 |
| 对象池 Prefab 源 | `IGameObjectPoolService`（内部已用 `LoadLease`） | 池空且策略允许，或 Shutdown 时放租约 |

不要把对象池的源 Prefab 改成 `LoadGameObject`。那条路会实例化并挂 `ResourceOwner`，和池生命周期冲突。

资源记录状态：

- `Active`：存在直接租约、旧式直接引用或绑定引用。
- `KeepAlive`：内部过渡态。绑定上的 `KeepAliveOnRelease` 已并入 Idle TTL，Release 不再进独立 KeepAlive 队列。
- `Idle`：没有引用，等待 `IdleAssetExpireTime` 到期后释放底层句柄。
- `Released`：记录已释放。

`UnloadUnusedAssets()` 只回收引用为零且已过期的记录。`UnloadUnusedAssets(true)` 忽略 Idle TTL，立即卸无引用记录。`ForceUnloadAllAssets()` 清空全部记录，并复用 BindingService 的 Owner 重注册路径。

## 初始化资源包

`ResourceComponent.Awake()` 只创建和注册默认包对象，实际资源包初始化需要调用 `InitPackageAsync()`：

```csharp
using AlicizaX;
using AlicizaX.Resource.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class ResourceInitExample : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        IResourceService resources = AppServices.Require<IResourceService>();

        bool succeed = await resources.InitPackageAsync(
            packageName: "DefaultPackage",
            hostServerURL: "https://cdn.example.com/Android",
            fallbackHostServerURL: "https://fallback-cdn.example.com/Android");

        Debug.Log($"Init package succeed: {succeed}");
    }
}
```

不同运行模式的初始化参数：

- `EditorSimulateMode`：编辑器模拟构建 + 编辑器文件系统。收集路径以 `AssetBundleCollectorSetting` 为准（当前工程是 `Assets/Bundles/...`）。
- `OfflinePlayMode`：使用内置文件系统，可配置解密服务。
- `HostPlayMode`：同时使用内置文件系统和远端缓存文件系统。必须传入非空 `hostServerURL`。
- `WebPlayMode`：使用 WebServer 文件系统；WebGL 微信小游戏宏下会创建微信缓存文件系统。必须传入非空 `hostServerURL`。

YooAsset 3 注意：

- `InitPackageAsync` 成功不等于 ActiveManifest 已就绪。`GetPackageVersion()` 在无 Manifest 时会抛 `Active package manifest not found.`。服务内部会在 `PackageValid` 为假时跳过版本刷新，并在 `LoadPackageManifestAsync` 成功后再写版本。
- 同一包的并发 `InitPackageAsync` 会合流到同一个 `TaskCompletionSource<bool>`，可以多次等待。不要自己对内部 UniTask 做 `Preserve()` + `WhenAll`。

启动流程通常是：`InitPackageAsync` → `RequestPackageVersionAsync` → `LoadPackageManifestAsync` → 创建下载器。

## 查询资源

```csharp
using AlicizaX;
using AlicizaX.Resource.Runtime;
using UnityEngine;

public sealed class ResourceCheckExample : MonoBehaviour
{
    private void Start()
    {
        IResourceService resources = AppServices.Require<IResourceService>();

        string location = "UIHomeWindow";
        if (!resources.IsLocationValid(location))
        {
            Debug.LogError($"Invalid location: {location}");
            return;
        }

        HasAssetResult result = resources.HasAsset(location);
        Debug.Log($"Asset state: {result}");
    }
}
```

`HasAsset` 内部只查一次 `GetAssetInfo`。`HasAssetResult` 取值：

- `NotExist`：资源不存在。
- `AssetOnline`：资源存在，但需要从远端更新下载。
- `AssetOnDisk`：资源存在并位于磁盘。
- `AssetOnFileSystem`：资源存在并位于文件系统。
- `BinaryOnDisk`：二进制资源存在并位于磁盘。
- `BinaryOnFileSystem`：二进制资源存在并位于文件系统。
- `Valid`：资源定位地址无效。

## 加载普通资源（推荐租约）

新代码一律用 `LoadLease` / `LoadLeaseAsync`。租约是一次明确所有权；`Dispose()` 只交还所有权，不会立刻从内存抠掉资源。引用归零后走 Idle TTL。

同步：

```csharp
using (ResourceAssetLease<Texture2D> lease = resources.LoadLease<Texture2D>("icon_start"))
{
    Texture2D icon = lease.Asset;
}
```

异步建议传入 `CancellationToken`，对象销毁或界面关闭时取消：

```csharp
using System.Threading;
using AlicizaX;
using AlicizaX.Resource.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class LoadSpriteExample : MonoBehaviour
{
    private CancellationTokenSource _cts;
    private ResourceAssetLease<Sprite> _avatar;

    private async UniTaskVoid OnEnable()
    {
        _cts = new CancellationTokenSource();
        IResourceService resources = AppServices.Require<IResourceService>();
        _avatar = await resources.LoadLeaseAsync<Sprite>(
            "avatar_default",
            _cts.Token);
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _avatar.Dispose();
    }
}
```

也可以只拿句柄：

```csharp
ResourceLeaseHandle handle = await resources.AcquireDirectAsync(
    ResourceKey.Asset<Sprite>("avatar_default"),
    cancellationToken);

if (handle.IsValid && resources.TryGetLeaseAsset(handle, out UnityEngine.Object asset))
{
    Sprite sprite = asset as Sprite;
}

resources.Release(handle);
```

同一地址正在异步加载时，同步 `LoadLease` 会加入同一个 Provider，最终只保留一条记录。合流后必须从记录读资源，不要再碰可能已被 Dispose 的临时句柄。

## 旧式 LoadAsset（已 Obsolete）

`LoadAsset` / `LoadAssetAsync` / `UnloadAsset` 仍保留，因为业务和启动代码还在用，但已标 Obsolete。它们按 Unity 对象指针配对释放，多条记录共享同一 instanceId 时可能卸错。新代码不要再写：

```csharp
Texture2D icon = resources.LoadAsset<Texture2D>("icon_start");
resources.UnloadAsset(icon);
```

回调式 `LoadAssetAsync(..., LoadAssetCallbacks)` 同样 Obsolete，请改成 `LoadLeaseAsync`。

## 加载并实例化 Prefab

`LoadGameObject()` 和 `LoadGameObjectAsync()` 会加载 Prefab 并实例化到场景中。实例对象会自动挂载或复用 `ResourceOwner`，用于绑定 Prefab 源资源租约。销毁实例时，`ResourceOwner.OnDestroy()` 会释放绑定。

```csharp
GameObject window = await resources.LoadGameObjectAsync(
    "UIHomeWindow",
    parent);

Destroy(window);
```

同步版本：

```csharp
GameObject window = resources.LoadGameObject("UIHomeWindow", parent);
```

对象池不要走这两条 API。`GameObjectPool` 内部用 `LoadLease<GameObject>` 持有源 Prefab，按池策略 `Dispose`。

## 组件资源绑定

资源绑定服务用于把资源绑定到组件属性，并把生命周期交给 `ResourceOwner` 管理。目标对象销毁或调用 `ResourceOwner.ReleaseBindings()` 时，会清空组件槽位、释放租约，并销毁必要的运行时材质实例。

扩展方法位于 `Resource/Extension/ResourceBindingExtensions.cs`：

```csharp
using UnityEngine.UI;

image.SetSprite("icon_start", setNativeSize: true);
image.SetSubSprite("CommonAtlas", "btn_start");
image.SetMaterial("ui_mask", isAsync: true);

spriteRenderer.SetSprite("player");
spriteRenderer.SetSubSprite("atlas", "idle_0");
spriteRenderer.SetMaterial("shared");

meshRenderer.SetMaterial("role", needInstance: true, isAsync: true);
meshRenderer.SetSharedMaterial("shared");
```

绑定槽类型：

- `ImageSprite`：绑定 `Image.sprite`。
- `SubSprite`：从图集子资源中取 Sprite 并绑定到 `Image.sprite` 或 `SpriteRenderer.sprite`。
- `SpriteRendererSprite`：绑定 `SpriteRenderer.sprite`。
- `ImageMaterial`：绑定 `Image.material`。
- `RendererSharedMaterial`：绑定 `Renderer.sharedMaterial`。
- `RendererMaterialInstance`：从资源材质创建运行时实例，并绑定到 `Renderer.sharedMaterial`。
- `PrefabSource`：Prefab 实例和源 Prefab 之间的绑定。

可选项：

- `SetNativeSize`：绑定成功后对 `Image` 调用 `SetNativeSize()`。
- `KeepAliveOnRelease`：释放后进入 Idle，TTL 使用 `IdleAssetExpireTime`，避免组件频繁换图时反复加载。

## 多资源包

大多数 API 的最后一个参数是 `packageName` 或 `customPackageName`。不传时使用 `ResourceComponent.PackageName` 指定的默认包。

```csharp
await resources.InitPackageAsync(
    packageName: "DlcPackage",
    hostServerURL: "https://cdn.example.com/DLC",
    fallbackHostServerURL: "https://fallback-cdn.example.com/DLC");

using ResourceAssetLease<Texture2D> lease = await resources.LoadLeaseAsync<Texture2D>(
    "icon_dlc",
    packageName: "DlcPackage");
```

## 下载、版本和清单

创建下载器时会使用 Inspector 中的 `Downloading Max Num` 和 `Failed Try Again`：

```csharp
using YooAsset;

ResourceDownloaderOperation downloader = resources.CreateResourceDownloader();
if (downloader.TotalDownloadCount > 0)
{
    downloader.DownloadErrorCallback = data =>
    {
        Debug.LogError($"Download error: {data.PackageName}/{data.FileName}, {data.ErrorInfo}");
    };

    downloader.DownloadUpdateCallback = data =>
    {
        Debug.Log($"Download: {data.CurrentDownloadCount}/{data.TotalDownloadCount}");
    };

    downloader.BeginDownload();
    await downloader;
}
```

更新版本和清单：

```csharp
RequestPackageVersionOperation versionOperation = resources.RequestPackageVersionAsync();
await versionOperation.ToUniTask();

if (versionOperation.Status == EOperationStatus.Succeeded)
{
    resources.PackageVersion = versionOperation.PackageVersion;
    LoadPackageManifestOperation manifestOperation =
        resources.LoadPackageManifestAsync(versionOperation.PackageVersion);
    await manifestOperation.ToUniTask();
}
```

编辑器模拟模式下清单版本通常是 `"Simulate"`，不要用远端版本号去加载模拟清单。

获取当前包版本：

```csharp
string version = resources.GetPackageVersion();
```

仅在 `PackageValid`（ActiveManifest 已就绪）后调用。初始化刚成功时请用 `RequestPackageVersionAsync` / `PackageVersion`，不要立刻 `GetPackageVersion()`。

## 缓存清理

```csharp
using YooAsset;

ClearCacheOperation clearUnused =
    resources.ClearCacheAsync(new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles));
await clearUnused.ToUniTask();

resources.ClearAllBundleFiles("DlcPackage");
```

`ClearAllBundleFiles()` 内部调用 `ClearCacheAsync(ClearCacheMethods.ClearAllBundleFiles)`，不会等待操作完成；需要等待结果时使用 `ClearCacheAsync()`。

## 资源回收

```csharp
lease.Dispose();

resources.UnloadUnusedAssets();
resources.UnloadUnusedAssets(force: true);

resources.ForceUnloadUnusedAssets(performGCCollect: true);

resources.ForceUnloadAllAssets();
```

自动回收流程：

1. `ResourceComponent.Update()` 每帧调用 `ProcessKeepAlive`，按 `Expire Process Count Per Frame` 处理过期桶。
2. 达到 `Max Unload Unused Assets Interval` 后，调用 `UnloadUnusedAssets()`，当帧可用 `Expire Process Count When Unloading`。
3. 低内存或显式 `ForceUnloadUnusedAssets(true)` 会请求强制回收、Unity 系统卸载和 GC。
4. GC 受 `Min GC Collect Interval` 限制，避免短时间重复触发。

## 调试信息

可以通过资源服务和绑定服务读取当前资源记录、绑定记录、Owner 记录：

```csharp
ResourceAssetInfo[] assets = new ResourceAssetInfo[128];
int totalAssetCount = resources.GetAssetInfos(assets, 0, assets.Length);

ResourceOwnerInfo[] owners = new ResourceOwnerInfo[64];
int totalOwnerCount = resources.BindingService.GetOwnerInfos(owners, 0, owners.Length);

ResourceBindingInfo[] bindings = new ResourceBindingInfo[128];
int totalBindingCount = resources.BindingService.GetBindingInfos(bindings, 0, bindings.Length);
```

这些接口适合调试面板或编辑器工具使用，用于观察引用计数、Idle 过期时间、绑定目标和句柄状态。

## API 速查

```csharp
void Initialize();
UniTask<bool> InitPackageAsync(string packageName = "", string hostServerURL = "", string fallbackHostServerURL = "");

bool IsLocationValid(string location, string packageName = "");
HasAssetResult HasAsset(string location, string packageName = "");
AssetInfo GetAssetInfo(string location, string packageName = "");
AssetInfo[] GetAssetInfos(string resTag, string packageName = "");
AssetInfo[] GetAssetInfos(string[] tags, string packageName = "");

ResourceAssetLease<T> LoadLease<T>(string location, string packageName = "") where T : UnityEngine.Object;
UniTask<ResourceAssetLease<T>> LoadLeaseAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object;
ResourceLeaseHandle AcquireDirect(ResourceKey key);
UniTask<ResourceLeaseHandle> AcquireDirectAsync(ResourceKey key, CancellationToken cancellationToken = default);
bool TryAcquireDirect(ResourceKey key, out ResourceLeaseHandle handle);
void Release(ResourceLeaseHandle handle);
bool TryGetLeaseAsset(ResourceLeaseHandle handle, out UnityEngine.Object asset);

GameObject LoadGameObject(string location, Transform parent = null, string packageName = "");
UniTask<GameObject> LoadGameObjectAsync(string location, Transform parent = null, CancellationToken cancellationToken = default, string packageName = "");

[Obsolete] T LoadAsset<T>(string location, string packageName = "") where T : UnityEngine.Object;
[Obsolete] UniTask<T> LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object;
[Obsolete] void UnloadAsset(object asset);

void UnloadUnusedAssets();
void UnloadUnusedAssets(bool force);
void ForceUnloadUnusedAssets(bool performGCCollect);
void ForceUnloadAllAssets();

ResourceDownloaderOperation CreateResourceDownloader(string customPackageName = "");
RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks = false, int timeout = 60, string customPackageName = "");
LoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout = 60, string customPackageName = "");
string GetPackageVersion(string customPackageName = "");

ClearCacheOperation ClearCacheAsync(ClearCacheOptions options, string customPackageName = "");
void ClearAllBundleFiles(string customPackageName = "");
```

公开 API 不再提供 `LoadAssetSyncHandle` / `LoadAssetAsyncHandle`。Yoo 类型仍出现在 `IResourceService` 上，因为启动和热更流程要直接操作下载器、版本和清单。

## 注意事项

- 新代码优先 `LoadLease` / Binding / `LoadGameObject`。`LoadAsset` + `UnloadAsset` 只为兼容旧业务保留。
- `LoadGameObject()` / `LoadGameObjectAsync()` 实例化出的对象通过 `Destroy(instance)` 释放，实例上的 `ResourceOwner` 会自动解除 Prefab 源绑定。
- 组件图片、材质等资源优先使用绑定扩展方法，避免手动维护引用和释放。
- 异步加载建议传入 `CancellationToken`，对象销毁或界面关闭时取消加载。
- 多包资源必须显式传入 `packageName` / `customPackageName`，否则默认使用 `ResourceComponent.PackageName`。
- `Decryption Services` 必须填写可被框架程序集工具查找到的完整类型名，并且该类型需要实现 YooAsset 的 `IDecryptionServices`。
- `InitPackageAsync` 成功后仍要 `LoadPackageManifestAsync`，再读 `GetPackageVersion()`。
- 对象池、音频缓存已经改为内部持有租约，业务继续走各自模块 API 即可。
