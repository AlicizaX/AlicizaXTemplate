# UXComponent 扩展组件

`UXComponent` 提供一组基于 UGUI 的增强控件。它们主要解决按钮音效、子节点状态切换、Toggle 分组、渐变图片、状态绑定和本地化文本等问题。快捷键、导航和 InputGlyph 已拆到独立输入扩展包中。

源码位置：

- `Client/Packages/com.alicizax.unity.ui.extension/Runtime/UXComponent`
- 编辑器入口：`Client/Packages/com.alicizax.unity.ui.extension/Editor/UX`

可选输入扩展：

- 本地包：`file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`
- Unity Package Manager 显示路径：`com.alicizax.unity.input@file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`
- 需要 `HotkeyComponent`、`Navigation` 或 `InputGlyph` 时再安装；只使用基础 UX 控件时不需要安装 input 包。

## 模块划分

| 文档 | 内容 |
| --- | --- |
| [UXButton](UXButton.md) | `UXButton`、`UXSelectable` 子节点状态、音效适配器 |
| [UXToggle](UXToggle.md) | `UXToggle`、`UXGroup` 分组、页签切换 |
| [UXImage](UXImage.md) | 渐变绘制、镜像模式、进度条 |
| [UXTextMeshPro](UXTextMeshPro.md) | 本地化 key 绑定、本地化适配器注入 |
| [UXController](UXController.md) | `UXController` 多状态管理、`UXBinding` 属性绑定 |
| [UXDraggable](UXDraggable.md) | 拖拽事件转发、可拖拽弹窗 |
| [HotkeyComponent](HotkeyComponent.md) | 可选 input 包：Input System 快捷键绑定、优先级规则 |

## 适配器注入

部分控件依赖项目注入适配器才能工作：

| 适配器 | 注入方法 | 影响控件 |
| --- | --- | --- |
| `IUXAudioHelper` | `UXComponentExtensionsHelper.SetAudioHelper(...)` | `UXButton`、`UXToggle` 音效 |
| `IUXLocalizationHelper` | `UXComponentExtensionsHelper.SetLocalizationHelper(...)` | `UXTextMeshPro` 本地化 |

建议在项目启动流程中统一注入，例如在 `RootModule` 初始化完成后：

```csharp
UXComponentExtensionsHelper.SetAudioHelper(new UXAudioAdapter());
UXComponentExtensionsHelper.SetLocalizationHelper(new UXLocalizationAdapter());
```

## 编译条件

| 宏 | 影响范围 |
| --- | --- |
| `TEXTMESHPRO_SUPPORT` | `UXTextMeshPro` |
| `INPUTSYSTEM_SUPPORT` | 由 `com.alicizax.unity.input` 自动生成，用于 input 包内的 `HotkeyComponent`、`Navigation`、`InputGlyph` |
| `UXNAVIGATION_SUPPORT` | UI extension 检测到 `com.alicizax.unity.input` 后自动生成，用于 RecyclerView.Navigation 接入 |

不需要在 Player Settings 手动添加 `INPUTSYSTEM_SUPPORT`、`UX_NAVIGATION` 或 `UXNAVIGATION_SUPPORT`。需要输入相关功能时，安装 `com.unity.inputsystem` 和 `com.alicizax.unity.input` 即可。

## 注意事项

1. `UXButton`、`UXToggle` 等类型在 `UnityEngine.UI` 命名空间下，和 Unity UGUI 控件同一套使用方式。
2. `UXButton` 不继承 Unity `Button`，但保留了 `Button.ButtonClickedEvent` 类型的 `onClick`，业务调用方式基本一致。
3. `HotkeyComponent` 位于 `com.alicizax.unity.input`，只转发到 `ISubmitHandler`，目标组件必须实现提交接口，例如 `UXButton`、`UXToggle`。
4. `UXImage.FlipMode.Horziontal` 的拼写来自源码枚举，代码里需要使用这个实际名称。
