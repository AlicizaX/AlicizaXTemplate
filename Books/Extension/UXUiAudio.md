# UX UI 音效

UI 控件不再自己持有 hover / click clip，也不再注入 `IUXAudioHelper`。控件只上报语义 Cue，由 `UXUiAudioRouter` 按当前输入设备和 `UXUiAudioProfile` 决定播不播。

源码位置：

- `Client/Packages/com.alicizax.unity.ui.extension/Runtime/UXComponent/Feedback`
- 编辑器：`Client/Packages/com.alicizax.unity.ui.extension/Editor/UX/Feedback`
- 默认主题资产：`Client/Assets/Resources/UX/UXUiAudioProfile.asset`

命名空间：`AlicizaX.UI.UXFeedback`。

## 为什么这样拆

导航开窗默认选中、关窗回焦、手柄强制补选都会走 `EventSystem.SetSelectedGameObject`，同步触发 `OnSelect`。如果按钮把 `OnSelect` 当成“玩家移到这个控件”来播 hover，程序化补焦点也会响一声。

导航包只负责给这次焦点变化打原因：`UXFocusChange.Cause.Programmatic`。音频模块看到 Programmatic 的 Focus Cue 就丢掉。玩家拨杆 / Tab 不会打这个戳，Focus 音正常播。

## 启动绑定

`UXUiAudioRouter` 不会 `Resources.Load`，也不会写死路径。没人 `Bind` 就不播。

在启动场景的 `Entry/UI` 上挂 `UXUiAudioBinder`，把 Profile 拖到 `_profile`。`Awake` 注入，`OnDestroy` 解绑。当前模板预制体：

```text
Packages/com.alicizax.unity.framework/Runtime/Prefabs/Entry.prefab/UI
```

运行时换主题：

```csharp
using AlicizaX.UI.UXFeedback;

UXUiAudioRouter.Bind(otherProfile);
```

## 控件发什么 Cue

| 控件 | 事件 | Cue |
| --- | --- | --- |
| `UXSelectable` | 指针进入 / 离开 | `PointerEnter` / `PointerExit` |
| `UXSelectable` | 非指针选中 / 取消选中 | `FocusEnter` / `FocusExit` |
| `UXButton` | 左键点击、Submit、`PlayClickFeedback` | `Press` |
| `UXToggle` | 点击或 Submit 后 | `ToggleOn` / `ToggleOff` |

鼠标点按钮时 `OnSelect` 的 `eventData` 是 `PointerEventData`，基类不会再发 `FocusEnter`，避免和 `PointerEnter` 双响。

## Profile

`UXUiAudioProfile` 是 ScriptableObject。每条 Rule：

| 字段 | 说明 |
| --- | --- |
| `Cue` | 语义事件 |
| `Devices` | 哪些输入类型播。`KeyboardMouse` / `Gamepad` / `Joystick` / `Touch` |
| `IgnoreProgrammatic` | 导航程序化补焦点时是否丢掉。Focus 默认 true |
| `Clip` | 主题 clip |

默认表：

| Cue | 设备 | Programmatic | 默认 clip |
| --- | --- | --- | --- |
| `PointerEnter` | 键鼠 | 播 | hover |
| `FocusEnter` | 手柄 / 摇杆 | 不播 | hover |
| `Press` | 全部 | 播 | click |
| `ToggleOn` / `ToggleOff` | 全部 | 播 | click |
| `PointerExit` / `FocusExit` | 未配 | — | 不播 |

键鼠 Tab 选中默认不响，因为 `FocusEnter` 没勾 `KeyboardMouse`。要键盘也有选中音，给 `FocusEnter` 加上 `KeyboardMouse`。

当前设备来自 `UXInput.Watch.CurrentInputType`。没装 Input System 时按键鼠处理。

## 单个按钮怎么出声

普通按钮什么都不用配，走全局 Profile。

两个按钮声音不同，或这个按钮不要音，在该控件上加 `UXUiAudioOverride`（`Add Component > UI/UX Audio Override`）。

| Mode | 行为 |
| --- | --- |
| `Overlay` | 只覆盖列出的 Cue×设备，没列的走 Profile |
| `Silent` | 这个控件任何 Cue 都不播，条目不用填 |
| `Exclusive` | 只播列出的 Cue；没列的全静音 |

条目字段：

| 字段 | 说明 |
| --- | --- |
| `Cue` | 覆盖哪条语义 |
| `Devices` | 覆盖哪些设备。当前设备不在掩码里则当没匹配 |
| `Mute` | 匹配后静音 |
| `Clip` | 匹配后播放的 clip |

示例：

- 整钮静音：`Mode = Silent`
- 只要点击音：`Mode = Exclusive`，加一条 `Press` + clip
- 只换点击音、hover 仍用主题：`Mode = Overlay`，加一条 `Press` + clip

`Overlay` 匹配成功后完全接管这条，不再回退 Profile。`Mute` 或 Clip 为空都是静音。

`Exclusive` 不依赖 Profile 里先有这条 Cue，可以单独给某个按钮加主题没有的音。`Overlay` 仍是“换主题已经会播的音”。

## 导航程序化焦点

`UXNavigationSystem` 补选时：

```csharp
using (new UXFocusChange.Scope(UXFocusChange.Cause.Programmatic))
    eventSystem.SetSelectedGameObject(selected);
```

业务如果自己 `SetSelectedGameObject` 且不想播 Focus 音，同样包一层。不要再使用已删除的 `UXSelectionAudio`。

## API 速查

| API | 说明 |
| --- | --- |
| `UXUiAudioBinder` | 场景组件，序列化注入 Profile |
| `UXUiAudioRouter.Bind(UXUiAudioProfile)` | 运行时绑定或清空 |
| `UXUiFeedback.Raise(Component, UXUiCue)` | 控件上报语义；业务一般不用直接调 |
| `UXUiAudioOverride` | 单控件覆盖 |
| `UXFocusChange` | 位于 `AlicizaX.UI.UXNavigation`，只标记焦点原因 |

## 注意事项

1. 不要把 clip 再写回 `UXButton` / `UXToggle`。主题进 Profile，特例进 Override。
2. 没挂 `UXUiAudioBinder` 或 Profile 为空时全部静音。
3. 播放仍走 `GameApp.Audio.Play(AudioType.UISound, clip)`，需要音频模块已启动。
4. 程序化 Focus 是否出声由 Profile 的 `IgnoreProgrammatic` 控制，不是导航包里的静音开关。
