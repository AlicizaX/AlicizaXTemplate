# Audio 模块

Audio 模块提供基于 Unity `AudioSource` 的统一播放服务，支持 2D 音效、3D 音效、跟随目标播放、分组音量、分组启用/禁用、句柄控制、淡入淡出、异步地址加载、显式预加载、显式卸载和可配置 Clip 缓存。

它不是 Wwise 等价封装。当前没有事件库、RTPC、Switch/State、Bank、虚拟声部和高级音乐转场。

源码位置：

- `Client/Packages/com.alicizax.unity.framework/Runtime/Modules/Audio`
- `Client/Packages/com.alicizax.unity.framework/Editor/Modules/Audio`

## 使用前提

场景中的框架根节点需要挂载：

- `ObjectPoolComponent`
- `ResourceComponent`
- `AudioComponent`

`AudioComponent` Inspector 中必须指定：

- `AudioMixer`
- `AudioListener`
- 每个 `AudioType` 对应的 `AudioGroupConfig`

编辑器下 `AudioComponent` 会补齐默认分组配置。每个分组需要有效的 `MixerGroup`。

## 音频分类

```csharp
public enum AudioType
{
    Sound = 0,
    UISound = 1,
    Music = 2,
    Voice = 3,
    Ambient = 4,
    Max = 5
}
```

常见用法：

- `Sound`：普通 2D/3D 音效。
- `UISound`：UI 点击、弹窗等音效。
- `Music`：背景音乐。
- `Voice`：角色语音。
- `Ambient`：环境声。

## 获取服务

```csharp
using AlicizaX;
using AlicizaX.Audio.Runtime;

IAudioService audio = AppServices.Require<IAudioService>();
```

如果代码可能早于 Audio 初始化执行：

```csharp
if (!AppServices.TryGet<IAudioService>(out var audio))
{
    return;
}
```

## 基础播放

通过地址播放 2D 音效：

```csharp
audio.Play(AudioType.UISound, "Assets/Bundles/Audios/ui_click.wav");
```

通过 `AudioClip` 播放：

```csharp
audio.Play(AudioType.UISound, clickClip, loop: false, volume: 1f);
```

`Play` 返回 `ulong` 句柄。返回 `0UL` 表示播放失败。

## 异步播放

地址播放默认 `Play` 走同步加载路径。需要避免首次加载卡顿时，使用 async 接口：

```csharp
ulong handle = audio.PlayAsync(
    AudioType.Sound,
    "Assets/Bundles/Audios/hit.wav",
    loop: false,
    volume: 1f);
```

3D 和跟随播放也有对应 async 快捷接口：

```csharp
audio.Play3DAsync(AudioType.Sound, "Assets/Bundles/Audios/explosion.wav", position);
audio.PlayFollowAsync(AudioType.Sound, "Assets/Bundles/Audios/engine.wav", target, Vector3.zero, loop: true);
```

## 播放选项

使用 `AudioPlayOptions` 设置异步加载、缓存策略、音高、淡入、淡出和抢占优先级：

```csharp
var options = new AudioPlayOptions
{
    Async = true,
    CachePolicy = AudioCachePolicy.Ttl,
    Pitch = 1.05f,
    FadeInSeconds = 0.15f,
    FadeOutSeconds = 0.25f,
    Priority = 180
};

ulong handle = audio.Play(
    AudioType.Sound,
    "Assets/Bundles/Audios/skill_cast.wav",
    loop: false,
    volume: 1f,
    options);
```

`Priority` 用于内部声部抢占，数值越大越重要。分类满载时会优先抢占低优先级声音；同优先级抢占最早播放的声音。如果新声音优先级低于所有活跃声音，播放会失败并返回 `0UL`。

## 3D 参数覆盖

默认 3D 参数来自 `AudioGroupConfig`。需要单次覆盖时使用 `AudioSpatialOptions`：

```csharp
var spatial = new AudioSpatialOptions
{
    Override = true,
    SpatialBlend = 1f,
    MinDistance = 2f,
    MaxDistance = 40f,
    RolloffMode = AudioRolloffMode.Logarithmic
};

var options = new AudioPlayOptions
{
    Async = true,
    CachePolicy = AudioCachePolicy.Ttl
};

audio.Play3D(
    AudioType.Sound,
    "Assets/Bundles/Audios/explosion.wav",
    position,
    loop: false,
    volume: 1f,
    spatial,
    options);
```

`default(AudioSpatialOptions)` 表示使用分组配置。

## 跟随目标播放

```csharp
private ulong _engineHandle;

private void OnEnable()
{
    var audio = AppServices.Require<IAudioService>();
    _engineHandle = audio.PlayFollow(
        AudioType.Sound,
        "Assets/Bundles/Audios/engine_loop.wav",
        transform,
        Vector3.zero,
        loop: true,
        volume: 0.7f);
}

private void OnDisable()
{
    if (AppServices.TryGet<IAudioService>(out var audio))
    {
        audio.Stop(_engineHandle, fadeout: true);
    }

    _engineHandle = 0UL;
}
```

目标对象失效或停止播放时，内部会释放对应 clip 引用。

## 句柄控制

```csharp
bool playing = audio.IsPlaying(handle);

audio.SetVolume(handle, 0.4f, fadeSeconds: 0.5f);
audio.Stop(handle, fadeout: true);
audio.Stop(handle, fadeOutSeconds: 0.3f);
```

`SetVolume` 控制用户音量倍率。遮挡音量和淡入淡出仍由运行时叠加。

## 全局和分组控制

```csharp
audio.Volume = 0.8f;
audio.Enable = true;

audio.SetCategoryVolume(AudioType.Music, 0.5f);
audio.SetCategoryEnable(AudioType.Voice, false);

float musicVolume = audio.GetCategoryVolume(AudioType.Music);
bool voiceEnabled = audio.GetCategoryEnable(AudioType.Voice);
```

分组音量写入 `AudioMixer` 暴露参数。默认参数名：

- `SoundVolume`
- `UISoundVolume`
- `MusicVolume`
- `VoiceVolume`
- `AmbientVolume`

如果 `AudioGroupConfig.ExposedVolumeParameter` 不为空，优先使用配置中的参数名。

## 停止播放

```csharp
audio.Stop(handle, fadeout: true);
audio.Stop(AudioType.Sound, fadeout: false);
audio.StopAll(fadeout: true);
```

禁用某个分类时，该分类当前播放中的音频会停止：

```csharp
audio.SetCategoryEnable(AudioType.Ambient, false);
```

## 缓存策略

地址播放使用内部 Clip 缓存。缓存策略由 `AudioCachePolicy` 指定：

```csharp
public enum AudioCachePolicy
{
    Default = 0,
    None = 1,
    Ttl = 2,
    Pin = 3
}
```

- `Default`：使用 `AudioComponent` Inspector 中的默认缓存策略。
- `None`：引用归零后立即释放。
- `Ttl`：引用归零后进入 LRU，超过 TTL 或容量压力时释放。
- `Pin`：不会被自动淘汰，只能通过显式卸载或强制清理释放。

`AudioComponent` Inspector 中可以配置：

- Clip Cache Capacity
- Clip Cache TTL
- Default Cache Policy

## 预热和预加载

动态音源默认只创建 `Initial Sources`，运行时按需增长到 `Max Sources`。如果某个场景进入后会立刻有大量声音，可以在加载阶段预热：

```csharp
audio.Warmup(AudioType.Sound, 16);
audio.Warmup(AudioType.UISound, 8);
```

预加载地址资源：

```csharp
bool ok = audio.Preload(
    "Assets/Bundles/Audios/bgm_main.wav",
    AudioCachePolicy.Pin);

audio.PreloadAsync(
    "Assets/Bundles/Audios/ui_click.wav",
    AudioCachePolicy.Ttl,
    success =>
    {
        if (!success)
        {
            return;
        }
    });
```

卸载未引用的缓存：

```csharp
audio.Unload("Assets/Bundles/Audios/bgm_main.wav");
audio.ClearCache(force: true);
```

`Unload` 和公开 `ClearCache` 不会释放正在播放、加载中或仍有 pending 请求的 clip。

## AudioEmitter 组件

`AudioEmitter` 是挂在场景物体上的播放组件，适合环境音、机关声、场景循环声等。

常用字段：

- `Audio Type`：音频分类。
- `Clip Mode`：使用地址或直接引用 `AudioClip`。
- `Address` / `Clip`：播放资源。
- `Play On Enable`：启用时自动播放。
- `Loop`：循环播放。
- `Volume`：播放音量。
- `Async`：地址模式下异步加载。
- `Cache Policy`：地址模式下的缓存策略。
- `Stop With Fadeout`：禁用时是否淡出停止。
- `Follow Self`：声音是否跟随自身 Transform。
- `Spatial Blend`：2D/3D 混合。
- `Min Distance` / `Max Distance`：3D 衰减距离。
- `Use Trigger Range`：根据 Listener 距离自动播放/停止。

代码控制：

```csharp
using AlicizaX.Audio.Runtime;
using UnityEngine;

public sealed class AudioEmitterExample : MonoBehaviour
{
    [SerializeField] private AudioEmitter emitter;

    public void Play()
    {
        emitter.Play();
    }

    public void Stop()
    {
        emitter.Stop();
    }
}
```

## 完整示例：音频设置面板

```csharp
using AlicizaX;
using AlicizaX.Audio.Runtime;
using UnityEngine;
using UnityEngine.UI;

public sealed class AudioSettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle voiceToggle;

    private IAudioService _audio;

    private void OnEnable()
    {
        _audio = AppServices.Require<IAudioService>();

        masterSlider.SetValueWithoutNotify(_audio.Volume);
        musicSlider.SetValueWithoutNotify(_audio.GetCategoryVolume(AudioType.Music));
        voiceToggle.SetIsOnWithoutNotify(_audio.GetCategoryEnable(AudioType.Voice));

        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        voiceToggle.onValueChanged.AddListener(OnVoiceEnableChanged);
    }

    private void OnDisable()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        voiceToggle.onValueChanged.RemoveListener(OnVoiceEnableChanged);
    }

    private void OnMasterVolumeChanged(float value)
    {
        _audio.Volume = value;
    }

    private void OnMusicVolumeChanged(float value)
    {
        _audio.SetCategoryVolume(AudioType.Music, value);
    }

    private void OnVoiceEnableChanged(bool enabled)
    {
        _audio.SetCategoryEnable(AudioType.Voice, enabled);
    }
}
```

## API 速查

```csharp
float Volume { get; set; }
bool Enable { get; set; }

float GetCategoryVolume(AudioType type);
void SetCategoryVolume(AudioType type, float value);
bool GetCategoryEnable(AudioType type);
void SetCategoryEnable(AudioType type, bool value);

ulong Play(AudioType type, string path, bool loop = false, float volume = 1f);
ulong PlayAsync(AudioType type, string path, bool loop = false, float volume = 1f);
ulong Play(AudioType type, string path, bool loop, float volume, in AudioPlayOptions options);
ulong Play(AudioType type, AudioClip clip, bool loop = false, float volume = 1f);
ulong Play(AudioType type, AudioClip clip, bool loop, float volume, in AudioPlayOptions options);

ulong Play3D(AudioType type, string path, in Vector3 position, bool loop = false, float volume = 1f);
ulong Play3DAsync(AudioType type, string path, in Vector3 position, bool loop = false, float volume = 1f);
ulong Play3D(AudioType type, string path, in Vector3 position, bool loop, float volume, in AudioSpatialOptions spatial, in AudioPlayOptions options);
ulong Play3D(AudioType type, AudioClip clip, in Vector3 position, bool loop = false, float volume = 1f);
ulong Play3D(AudioType type, AudioClip clip, in Vector3 position, bool loop, float volume, in AudioSpatialOptions spatial, in AudioPlayOptions options);

ulong PlayFollow(AudioType type, string path, Transform target, in Vector3 localOffset, bool loop = false, float volume = 1f);
ulong PlayFollowAsync(AudioType type, string path, Transform target, in Vector3 localOffset, bool loop = false, float volume = 1f);
ulong PlayFollow(AudioType type, string path, Transform target, in Vector3 localOffset, bool loop, float volume, in AudioSpatialOptions spatial, in AudioPlayOptions options);
ulong PlayFollow(AudioType type, AudioClip clip, Transform target, in Vector3 localOffset, bool loop = false, float volume = 1f);
ulong PlayFollow(AudioType type, AudioClip clip, Transform target, in Vector3 localOffset, bool loop, float volume, in AudioSpatialOptions spatial, in AudioPlayOptions options);

bool Stop(ulong handle, bool fadeout = false);
bool Stop(ulong handle, float fadeOutSeconds);
bool SetVolume(ulong handle, float volume, float fadeSeconds = 0f);
bool IsPlaying(ulong handle);
void Stop(AudioType type, bool fadeout);
void StopAll(bool fadeout);

void Warmup(AudioType type, int count);
bool Preload(string address, AudioCachePolicy policy = AudioCachePolicy.Pin);
void PreloadAsync(string address, AudioCachePolicy policy, Action<bool> completed = null);
bool Unload(string address, bool force = false);
void ClearCache(bool force = false);
```

## 注意事项

- `AudioComponent` 必须指定 `AudioMixer` 和 `AudioListener`。
- 每个 `AudioType` 都要有对应 `AudioGroupConfig`，且 `MixerGroup` 不能为空。
- 地址播放依赖 Resource 模块，资源包必须已初始化。
- `Play` 返回 `0UL` 表示播放失败，不要继续用这个句柄做状态判断。
- 背景音乐、循环环境音应保存播放句柄，并在生命周期结束时停止。
- 分组音量最小会被限制到 `0.0001f`；禁用分组时才会写入静音值。
- `Pin` 缓存会保留内存，离开场景时应配合 `Unload` 或 `ClearCache`。
- `Warmup` 只创建音源对象，不加载音频资源；音频资源预加载请使用 `Preload` 或 `PreloadAsync`。
