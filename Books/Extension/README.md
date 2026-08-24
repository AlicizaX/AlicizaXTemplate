# UI Extension 扩展包

`com.alicizax.unity.ui.extension` 是 UI 框架之外的一组常用 UI 控件和列表扩展。当前文档对应包版本 `2.1.11`。输入图标、快捷键和导航能力已拆到可选输入扩展包 `com.alicizax.unity.input`。它不负责窗口生命周期，窗口打开、关闭、层级和 Holder 生成仍然由框架 UI 模块处理。

源码位置：

- `Client/Packages/com.alicizax.unity.ui.extension/Runtime`
- `Client/Packages/com.alicizax.unity.ui.extension/Editor`
- 可选输入扩展：`file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/`
- 项目示例：`Client/Assets/Scripts/Hotfix/GameLogic/UI`

## 模块划分

| 文档 | 内容 |
| --- | --- |
| [UXComponent](UXComponent.md) | `UXButton`、`UXToggle`、`UXImage`、`UXTextMeshPro`、`UXDraggable` 索引 |
| [UXButton](UXButton.md) | `UXButton`、`UXSelectable` 子节点状态 |
| [UXToggle](UXToggle.md) | `UXToggle`、`UXGroup` 分组、页签切换 |
| [UXUiAudio](UXUiAudio.md) | UI 语义音效、`UXUiAudioProfile`、Binder、单控件 Override |
| [UXImage](UXImage.md) | 渐变绘制、镜像模式、进度条 |
| [UXTextMeshPro](UXTextMeshPro.md) | 本地化 key 绑定、本地化适配器注入 |
| [UXDraggable](UXDraggable.md) | 拖拽事件转发、可拖拽弹窗 |
| [HotkeyComponent](HotkeyComponent.md) | 可选 input 包：Input System 快捷键绑定、优先级规则 |
| [RecyclerView](RecyclerView.md) | 虚拟列表、`ViewHolder`、定高/变高、普通列表、循环列表、混合模板列表和分组列表 |
| [RecyclerView.Navigation](RecyclerView.Navigation.md) | 可选 input 包：RecyclerView 手柄/键盘导航、虚拟焦点、ViewHolder 导航接口和 UXNavigation 接入 |
| [InputGlyph](InputGlyph.md) | 可选 input 包：Input System 按键图标、TMP Sprite 标签和图标数据库 |
| [UXInput](UXInput.md) | 可选 input 包：设备监听、输入读取、运行时重绑定、震动和输入诊断 |
| [Navigation](Navigation.md) | 可选 input 包：多输入设备 UI 焦点管理、`UXNavigationScope`、顶层 Scope 选择、导航压制、`UXFocusChange` |

## 使用前提

工程需要已经接入基础 UI 模块，并在启动场景中配置好：

- `RootModule`
- `ObjectPoolComponent`
- `TimerComponent`
- `ResourceComponent`
- `UIComponent`

如果使用输入图标、快捷键、按键重绑定或导航，还需要安装并启用 Unity Input System，并安装 `com.alicizax.unity.input`：

```text
file:/G:/UnityProject/AlicizaXTemplate/Client/Packages/com.alicizax.unity.input/
```

不需要手动添加 `INPUTSYSTEM_SUPPORT`、`UX_NAVIGATION` 或 `UXNAVIGATION_SUPPORT`。相关 asmdef 会根据已安装包自动生成所需宏。

## 命名空间

常用类型分布如下：

| 类型 | 命名空间 |
| --- | --- |
| `UXButton`、`UXToggle`、`UXImage`、`UXTextMeshPro` | `UnityEngine.UI` |
| `RecyclerView`、`ViewHolder`、`UGList` | `AlicizaX.UI` |
| `UXInput`、`InputActionProvider`、`InputGlyphImage`、`InputGlyphText`、`InputVisualizer` | 全局命名空间，位于 `com.alicizax.unity.input` |
| `HotkeyComponent`、`HotkeyComponentBase` | `UnityEngine.UI`，位于 `com.alicizax.unity.input` |
| `UXNavigationScope`、`UXNavigationSystem`、`UXFocusChange` | `AlicizaX.UI.UXNavigation`，位于 `com.alicizax.unity.input` |
| `UXUiAudioBinder`、`UXUiAudioProfile`、`UXUiAudioOverride` | `AlicizaX.UI.UXFeedback` |

示例：

```csharp
using AlicizaX.UI;
using UnityEngine.UI;

public sealed class Demo
{
    private UXButton _button;
    private RecyclerView _list;
}
```

## 编辑器入口

扩展包提供了几个常用右键创建入口：

```text
GameObject/UI/UXButton
GameObject/UI/UXToggle
GameObject/UI/UXImage
GameObject/UI/UXTextMeshPro
GameObject/UI/UXInput Field
GameObject/UI/UXScrollView
GameObject/UI/UXTemplateWindow
```

输入图标数据库编辑窗口：

```text
AlicizaX/Extension/Input/Input Glyph Database
```

`UXScrollView` 会从包内模板创建一个已经带有 `RecyclerView` 结构的滚动列表，适合再按项目需要替换列表项模板。

## 与 UI 模块的关系

`Books/UI.md` 说明的是窗口框架和 Holder 生成；本目录说明的是具体控件和列表怎么使用。推荐接入顺序：

1. 先按 [UI 模块](../UI.md) 配好窗口、Holder 生成和 `baseui` 引用。
2. 在 Prefab 中使用 `UXButton`、`RecyclerView` 等扩展控件。
3. 在窗口逻辑里通过自动生成的 Holder 字段访问扩展控件。

示例：

```csharp
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine.UI;

[Window(UILayer.UI)]
public sealed class BagWindow : UIWindow<ui_BagWindow>
{
    private UGList<BagItemData> _items;

    protected override void OnInitialize()
    {
        baseui.BtnClose.onClick.AddListener(CloseSelf);

        _items = UGListCreateHelper.Create<BagItemData>(baseui.ScrollViewItems);
    }
}
```

## 注意事项

1. `UXButton`、`UXToggle` 等类型在 `UnityEngine.UI` 命名空间下，和 Unity UGUI 控件同一套使用方式。
2. `RecyclerView` 的 `SetAdapter`、`Refresh`、`RequestLayout` 是内部方法，业务层优先通过 `UGList`、`UGMixedList` 等包装类操作。
3. `InputGlyph` 运行时不会自动 `Resources.Load` 图标库，必须在启动场景挂载 `InputActionProvider`，由它注入 `InputGlyphDatabase` 并初始化 `UXInput`。
4. 输入读取、运行时重绑定、设备监听和震动统一走 `UXInput`；图标 UI 只监听 `UXInput.Watch` 和 `UXInput.Rebind` 的变化后刷新显示。
