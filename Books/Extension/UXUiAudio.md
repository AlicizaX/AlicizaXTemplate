# UX UI 音效

`UXButton`、`UXToggle` 和 `UXSelectable` 上报语义 Cue。`UXUiAudioRouter` 先读取控件的 `UXUiAudioOverride`，未命中时再读取全局 `UXUiAudioProfile`，最后交给框架的 `UISound` 类别播放。

源码位于 `Client/Packages/com.alicizax.unity.ui.extension/Runtime/UXComponent/Feedback`，Inspector 位于同一包的 `Editor/UX/Feedback`，命名空间为 `AlicizaX.UI.UXFeedback`。

## 加载和切换主题

包不固定资产路径，也不自动加载。应用自行加载 Profile 并绑定；当前模板在 `HotfixEntry.EntranceAsync` 首次打开窗口前执行：

```csharp
UXUiAudioRouter.Bind(Resources.Load<UXUiAudioProfile>("UX/UXUiAudioProfile"));
```

默认资产位于 `Client/Assets/Resources/UX/UXUiAudioProfile.asset`。运行时调用 `Bind(otherProfile)` 切换主题；`Bind(null)` 清除全局主题。Router 只持有引用，资产的加载与释放由调用方负责。没有全局 Profile 时，控件上命中的 Override 仍会播放；两者都没有时静音。

## Cue 与默认规则

| 控件 | 事件 | Cue |
| --- | --- | --- |
| `UXSelectable` | 指针进入 | `PointerEnter` |
| `UXSelectable` | 非指针选中 | `FocusEnter` |
| `UXButton` | 左键点击、Submit、`PlayClickFeedback` | `Press` |
| `UXToggle` | 点击或 Submit，不区分开关状态 | `Press` |

当前 Profile 为键鼠的 `PointerEnter`、手柄和摇杆的 `FocusEnter` 配 hover 音；为所有设备的 `Press` 配 click 音。键盘 Tab 的 Focus 默认没有音，要启用时在 `FocusEnter` 规则中加入 `KeyboardMouse`。设备类型来自 `UXInput.Watch.CurrentInputType`；未安装 Input System 时使用 `KeyboardMouse`。

Profile 每条规则包含 `Cue`、`Devices`、`Clip`，Focus 规则还可设置 `AllowProgrammaticFocus`，默认关闭。程序化补焦点由 `UXFocusChange.Scope(Programmatic)` 标记；Router 只对命中的 Focus 规则检查该选项。鼠标点击造成的 `OnSelect` 不另发 Focus Cue。

## 控件 Override

需要单控件换音或静音时，挂 `UXUiAudioOverride`：

| 模式 | 行为 |
| --- | --- |
| `Overlay` | 命中的规则接管；未命中时使用全局 Profile |
| `Silent` | 此控件所有 Cue 静音 |
| `Exclusive` | 仅使用列出的规则；未命中时静音 |

每条 Override 规则指定 `Cue`、`Devices`、`Clip`，可选择 `Mute`；Focus 规则还可设置 `AllowProgrammaticFocus`。匹配到 `Mute` 或空 Clip 时静音，不回退全局 Profile。组件被禁用时不参与解析。即使未绑定 Profile，`Overlay` 和 `Exclusive` 中命中的有效 Clip 也可以播放。

Profile 与 Override 的 Inspector 会限制同一 Cue 可用的设备，提示空 Clip，并能修复旧资产的规则重叠。若旧数据仍有重叠，运行时使用列表中第一条匹配规则。

## 使用边界

- 不可交互的按钮和 Toggle 不上报按下 Cue。
- 音频服务尚未注册时跳过播放。
- `UXUiFeedback.Raise(Component, UXUiCue)` 供控件上报事件；业务代码通常只需调用 `UXUiAudioRouter.Bind`。
