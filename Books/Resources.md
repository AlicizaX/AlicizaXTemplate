# Resources 模块

Resources 模块是框架对 YooAsset 的资源服务封装，负责资源包初始化、资源查询、同步/异步加载、Prefab 实例化、资源租约、组件绑定、下载器创建、缓存清理和资源回收。

源码位置：

- `Client/Packages/com.alicizax.unity.framework/Runtime/Resource`

运行时主要入口：

- `ResourceComponent`：Unity 场景组件，负责注册 `IResourceService`、初始化 YooAsset、驱动自动回收。
- `IResourceService`：资源服务接口，业务代码通过 `AppServices.Require<IResourceService>()` 获取。
- `IResourceBindingService`：资源绑定服务，管理 `Image`、`SpriteRenderer`、`Renderer` 等组件上的资源引用。
- `ResourceOwner`：绑定生命周期组件，目标对象销毁时自动释放绑定资源。

## 使用前提

场景中的框架根节点需要挂载：

- `ObjectPoolComponent`
- `ResourceComponent`

`ResourceComponent.Awake()` 会注册 `IResourceService`，初始化 YooAsset，创建默认资源包，并把 Inspector 参数写入资源服务。业务代码通常这样获取服务：

```csharp
using AlicizaX;
using AlicizaX.Resource.Runtime;

IResourceService resources = AppServices.Require<IResourceService>();
```

## ResourceComponent Inspector 参数

`ResourceComponent` 菜单路径为 `Game Framework/Resource`，默认执行顺序为 `-700`。

| 字段 | 默认值 | 作用 |
| --- | --- | --- |
| `Min Unload Unused Assets Interval` | `60` | 最小自动回收间隔，单位秒。预留回收请求只有超过该间隔后才会执行。当前代码中主要配合内部预回收标记使用。 |
| `Max Unload Unused Assets Interval` | `300` | 最大自动回收间隔，单位秒。超过该时间后会自动调用 `IResourceService.UnloadUnusedAssets()`。 |
| `Use System Unload Unused Assets` | `true` | 强制回收并请求 GC 时，是否额外调用 Unity 的 `Resources.UnloadUnusedAssets()`。普通自动回收只执行 YooAsset/资源服务回收。 |
| `Min GC Collect Interval` | `30` | `GC.Collect()` 的最小触发间隔，单位秒。即使多次请求 GC，也会受该间隔保护。 |
| `Decryption Services` | 空 | YooAsset 解密服务类型名。非空时通过 `AlicizaX.Utility.Assembly.GetType()` 查找类型并创建 `IDecryptionServices` 实例。 |
| `Auto Unload Bundle When Unused` | `false` | 传给 YooAsset 初始化参数的 `AutoUnloadBundleWhenUnused`。开启后，资源包引用计数归零时 YooAsset 可自动卸载 Bundle。 |
| `Play Mode` | `EditorSimulateMode` | YooAsset 运行模式。编辑器中会从 `EditorPrefs` 读取 `GamePlayMode`。支持 `EditorSimulateMode`、`OfflinePlayMode`、`HostPlayMode`、`WebPlayMode`。 |
| `Package Name` | `DefaultPackage` | 默认资源包名称。`IResourceService.DefaultPackageName` 和 YooAsset 默认包都使用这个值。 |
| `Milliseconds` | `30` | YooAsset 操作系统每帧最大时间片，调用 `YooAssets.SetOperationSystemMaxTimeSlice()`，单位毫秒。 |
| `Downloading Max Num` | `10` | 创建资源下载器时的最大并发下载数量。 |
| `Failed Try Again` | `3` | 创建资源下载器时单个下载失败后的最大重试次数。 |
| `Asset Record Capacity` | `64` | 资源记录预热容量。用于资源记录表、加载 Key 索引等内部结构，降低运行中扩容。 |
| `Asset Lease Capacity` | `128` | 资源租约槽预热容量。直接租约和绑定租约都会消耗该容量。 |
| `Binding Owner Capacity` | `64` | 绑定 Owner 预热容量。一个 `ResourceOwner` 对应一个 Owner 记录。 |
| `Binding Slot Capacity` | `128` | 绑定槽预热容量。每个被绑定的组件属性占用一个绑定槽，例如 `Image.sprite` 或 `Renderer.sharedMaterial`。 |
| `Registered Target Capacity` | `128` | 已注册目标组件预热容量，用于快速根据组件定位所属 Owner。 |
| `Idle Asset Expire Time` | `60` | 无引用资源进入 Idle 状态后的过期时间，单位秒。过期后才会释放底层 YooAsset 句柄。 |

低内存回调 `Application.lowMemory` 会触发 `ResourceService.OnLowMemory()`，最终请求强制释放未使用资源并执行 GC。

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

- `EditorSimulateMode`：使用 `EditorSimulateModeHelper.SimulateBuild(packageName)` 和编辑器文件系统。
- `OfflinePlayMode`：使用内置文件系统，可配置解密服务。
- `HostPlayMode`：同时使用内置文件系统和远端缓存文件系统。
- `WebPlayMode`：使用 WebServer 文件系统；WebGL 微信小游戏宏下会创建微信缓存文件系统。

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

        string location = "Assets/Bundles/UI/Login.prefab";
        if (!resources.CheckLocationValid(location))
        {
            Debug.LogError($"Invalid location: {location}");
            return;
        }

        HasAssetResult result = resources.HasAsset(location);
        Debug.Log($"Asset state: {result}");
    }
}
```

`HasAssetResult` 取值：

- `NotExist`：资源不存在。
- `AssetOnline`：资源存在，但需要从远端更新下载。
- `AssetOnDisk`：资源存在并位于磁盘。
- `AssetOnFileSystem`：资源存在并位于文件系统。
- `BinaryOnDisk`：二进制资源存在并位于磁盘。
- `BinaryOnFileSystem`：二进制资源存在并位于文件系统。
- `Valid`：资源定位地址无效。

## 加载普通资源

同步加载适合启动阶段或确认已经在本地的小资源：

```csharp
Texture2D icon = resources.LoadAsset<Texture2D>("Assets/Bundles/Icons/icon_start.png");
resources.UnloadAsset(icon);
```

异步加载建议传入 `CancellationToken`，对象销毁或界面关闭时取消：

```csharp
using System.Threading;
using AlicizaX;
using AlicizaX.Resource.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class LoadSpriteExample : MonoBehaviour
{
    private CancellationTokenSource _cts;
    private Sprite _avatar;

    private async UniTaskVoid OnEnable()
    {
        _cts = new CancellationTokenSource();
        IResourceService resources = AppServices.Require<IResourceService>();
        _avatar = await resources.LoadAssetAsync<Sprite>(
            "Assets/Bundles/UI/avatar_default.png",
            _cts.Token);
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        if (_avatar != null && AppServices.TryGet<IResourceService>(out var resources))
        {
            resources.UnloadAsset(_avatar);
            _avatar = null;
        }
    }
}
```

也可以使用回调式异步加载：

```csharp
var callbacks = new LoadAssetCallbacks(
    loadAssetSuccessCallback: (assetName, asset, duration, userData) =>
    {
        Debug.Log($"Load success: {assetName}, duration={duration:F3}");
    },
    loadAssetFailureCallback: (assetName, status, errorMessage, userData) =>
    {
        Debug.LogError($"Load failure: {assetName}, status={status}, error={errorMessage}");
    },
    loadAssetUpdateCallback: (assetName, progress, userData) =>
    {
        Debug.Log($"Loading {assetName}: {progress:P0}");
    });

resources.LoadAssetAsync(
    location: "Assets/Bundles/UI/Login.prefab",
    priority: 0,
    loadAssetCallbacks: callbacks,
    userData: this).Forget();
```

## 加载并实例化 Prefab

`LoadGameObject()` 和 `LoadGameObjectAsync()` 会加载 Prefab 并实例化到场景中。实例对象会自动挂载或复用 `ResourceOwner`，用于绑定 Prefab 源资源租约。销毁实例时，`ResourceOwner.OnDestroy()` 会释放绑定。

```csharp
GameObject window = await resources.LoadGameObjectAsync(
    "Assets/Bundles/UI/Login.prefab",
    parent);

Destroy(window);
```

同步版本：

```csharp
GameObject window = resources.LoadGameObject("Assets/Bundles/UI/Login.prefab", parent);
```

## 使用 AssetHandle

如果需要直接控制 YooAsset 句柄生命周期，可以使用 Handle API。使用这些 API 时，调用方负责 `Dispose()`。

```csharp
using YooAsset;

AssetHandle handle = resources.LoadAssetAsyncHandle<AudioClip>("Assets/Bundles/Audios/click.wav");
handle.Completed += completed =>
{
    AudioClip clip = completed.AssetObject as AudioClip;
};

// 不再使用时释放
if (handle is { IsValid: true })
{
    handle.Dispose();
}
```

## 使用资源租约

当前模块新增了显式租约模型。`ResourceLeaseHandle` 表示一次资源引用，释放租约会减少引用计数。相比旧的 `UnloadAsset(object)`，租约不会因为多个资源记录指向同一个 Unity 对象而产生歧义。

```csharp
ResourceLeaseHandle handle = await resources.AcquireDirectAsync(
    ResourceKey.Asset<Sprite>("Assets/Bundles/UI/avatar_default.png"),
    cancellationToken);

if (handle.IsValid && resources.TryGetLeaseAsset(handle, out UnityEngine.Object asset))
{
    Sprite sprite = asset as Sprite;
}

resources.Release(handle);
```

相关状态：

- `Active`：存在直接引用、旧式直接引用或绑定引用。
- `KeepAlive`：绑定释放后短暂保活，当前固定 5 秒，用于避免组件频繁切换资源时反复加载。
- `Idle`：没有引用，等待 `IdleAssetExpireTime` 到期后释放句柄。
- `Released`：资源记录已释放。

## 组件资源绑定

资源绑定服务用于把资源绑定到组件属性，并把生命周期交给 `ResourceOwner` 管理。目标对象销毁或调用 `ResourceOwner.ReleaseBindings()` 时，会清空组件槽位、释放租约，并销毁必要的运行时材质实例。

扩展方法位于 `Resource/Extension/ResourceBindingExtensions.cs`：

```csharp
using UnityEngine.UI;

image.SetSprite("Assets/Bundles/UI/icon_start.png", setNativeSize: true);
image.SetSubSprite("Assets/Bundles/UI/CommonAtlas.spriteatlas", "btn_start");
image.SetMaterial("Assets/Bundles/Materials/ui_mask.mat", isAsync: true);

spriteRenderer.SetSprite("Assets/Bundles/Sprites/player.png");
spriteRenderer.SetSubSprite("Assets/Bundles/Sprites/atlas.spriteatlas", "idle_0");
spriteRenderer.SetMaterial("Assets/Bundles/Materials/shared.mat");

meshRenderer.SetMaterial("Assets/Bundles/Materials/role.mat", needInstance: true, isAsync: true);
meshRenderer.SetSharedMaterial("Assets/Bundles/Materials/shared.mat");
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
- `KeepAliveOnRelease`：枚举已定义，当前绑定实现主要依赖统一的绑定释放后 5 秒 KeepAlive 机制。

## 多资源包

大多数 API 的最后一个参数是 `packageName` 或 `customPackageName`。不传时使用 `ResourceComponent.PackageName` 指定的默认包。

```csharp
await resources.InitPackageAsync(
    packageName: "DlcPackage",
    hostServerURL: "https://cdn.example.com/DLC",
    fallbackHostServerURL: "https://fallback-cdn.example.com/DLC");

Texture2D dlcIcon = await resources.LoadAssetAsync<Texture2D>(
    "Assets/DLC/Icons/icon_dlc.png",
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
await versionOperation;

if (versionOperation.Status == EOperationStatus.Succeed)
{
    UpdatePackageManifestOperation manifestOperation =
        resources.UpdatePackageManifestAsync(versionOperation.PackageVersion);
    await manifestOperation;
}
```

获取当前包版本：

```csharp
string version = resources.GetPackageVersion();
```

## 缓存清理

```csharp
// 清理未使用的 Bundle 缓存文件。
ClearCacheFilesOperation clearUnused =
    resources.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
await clearUnused;

// 清理指定资源包的所有 Bundle 缓存文件。
resources.ClearAllBundleFiles("DlcPackage");
```

`ClearAllBundleFiles()` 当前内部调用 `ClearCacheFilesAsync(EFileClearMode.ClearAllBundleFiles)`，不会等待操作完成；需要等待结果时使用 `ClearCacheFilesAsync()`。

## 资源回收

```csharp
// 卸载通过 LoadAsset / LoadAssetAsync 获取的旧式直接引用。
resources.UnloadAsset(sprite);

// 释放引用计数为 0 的资源记录，并触发 YooAsset 包回收。
resources.UnloadUnusedAssets();

// 请求 ResourceComponent 在 Update 中强制释放未使用资源，可选择是否 GC。
resources.ForceUnloadUnusedAssets(performGCCollect: true);

// 强制卸载所有资源包资源。WebGL 不支持。
resources.ForceUnloadAllAssets();
```

自动回收流程：

1. `ResourceComponent.Update()` 每帧调用 `ResourceService.ProcessKeepAlive(Time.unscaledTime)`，处理绑定释放后的 KeepAlive 和 Idle 过期队列。
2. 达到 `Max Unload Unused Assets Interval` 后，调用 `UnloadUnusedAssets()`。
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

bool CheckLocationValid(string location, string packageName = "");
HasAssetResult HasAsset(string location, string packageName = "");
AssetInfo GetAssetInfo(string location, string packageName = "");
AssetInfo[] GetAssetInfos(string resTag, string packageName = "");
AssetInfo[] GetAssetInfos(string[] tags, string packageName = "");

T LoadAsset<T>(string location, string packageName = "") where T : UnityEngine.Object;
UniTask<T> LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object;
UniTask LoadAsset<T>(string location, Action<T> callback, string packageName = "") where T : UnityEngine.Object;
UniTask LoadAssetAsync(string location, int priority, LoadAssetCallbacks callbacks, object userData, string packageName = "");
UniTask LoadAssetAsync(string location, Type assetType, int priority, LoadAssetCallbacks callbacks, object userData, string packageName = "");

GameObject LoadGameObject(string location, Transform parent = null, string packageName = "");
UniTask<GameObject> LoadGameObjectAsync(string location, Transform parent = null, CancellationToken cancellationToken = default, string packageName = "");

AssetHandle LoadAssetSyncHandle<T>(string location, string packageName = "") where T : UnityEngine.Object;
AssetHandle LoadAssetAsyncHandle<T>(string location, string packageName = "") where T : UnityEngine.Object;

ResourceLeaseHandle AcquireDirect(ResourceKey key);
UniTask<ResourceLeaseHandle> AcquireDirectAsync(ResourceKey key, CancellationToken cancellationToken = default);
bool TryAcquireDirect(ResourceKey key, out ResourceLeaseHandle handle);
void Release(ResourceLeaseHandle handle);
bool TryGetLeaseAsset(ResourceLeaseHandle handle, out UnityEngine.Object asset);

void UnloadAsset(object asset);
void UnloadUnusedAssets();
void ForceUnloadUnusedAssets(bool performGCCollect);
void ForceUnloadAllAssets();

ResourceDownloaderOperation CreateResourceDownloader(string customPackageName = "");
RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks = false, int timeout = 60, string customPackageName = "");
UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60, string customPackageName = "");
string GetPackageVersion(string customPackageName = "");

ClearCacheFilesOperation ClearCacheFilesAsync(EFileClearMode clearMode = EFileClearMode.ClearUnusedBundleFiles, string customPackageName = "");
void ClearAllBundleFiles(string customPackageName = "");
```

## 注意事项

- `LoadAsset<T>()` 和 `LoadAssetAsync<T>()` 属于旧式直接引用，用完后需要调用 `UnloadAsset(asset)`。新代码优先使用 `ResourceLeaseHandle`，引用关系更明确。
- `LoadGameObject()` / `LoadGameObjectAsync()` 实例化出的对象通过 `Destroy(instance)` 释放，实例上的 `ResourceOwner` 会自动解除 Prefab 源绑定。
- 组件图片、材质等资源优先使用绑定扩展方法，避免手动维护引用和释放。
- 直接使用 `AssetHandle` 时，调用方必须负责 `Dispose()`。
- 异步加载建议传入 `CancellationToken`，对象销毁或界面关闭时取消加载。
- 多包资源必须显式传入 `packageName` / `customPackageName`，否则默认使用 `ResourceComponent.PackageName`。
- `Decryption Services` 必须填写可被框架程序集工具查找到的完整类型名，并且该类型需要实现 YooAsset 的 `IDecryptionServices`。
