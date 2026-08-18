# UXToggle 和 UXGroup

`UXToggle` 类似 Unity `Toggle`，通过 `isOn` 和 `onValueChanged` 控制状态。`UXGroup` 管理一组选项，支持默认选中、禁止全部关闭、切换到上一项或下一项。

源码位置：

- `Client/Packages/com.alicizax.unity.ui.extension/Runtime/UXComponent`

## 基础用法

```csharp
using AlicizaX;
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine.UI;

[Window(UILayer.UI)]
public sealed class SettingWindow : UIWindow<ui_SettingWindow>
{
    protected override void OnInitialize()
    {
        baseui.TglMusic.onValueChanged.AddListener(OnMusicChanged);
        baseui.TglSfx.onValueChanged.AddListener(OnSfxChanged);
    }

    private void OnMusicChanged(bool isOn)
    {
        GameApp.Audio.SetCategoryEnable(AlicizaX.Audio.Runtime.AudioType.Music, isOn);
    }

    private void OnSfxChanged(bool isOn)
    {
        GameApp.Audio.SetCategoryEnable(AlicizaX.Audio.Runtime.AudioType.UISound, isOn);
    }
}
```

手动控制分组：

```csharp
UXGroup group = baseui.TabGroup;

group.allowSwitchOff = false;
group.Next();
group.Previous();
group.SetAllTogglesOff(sendCallback: false);
```

## UXToggle 配置

| 配置 | 说明 |
| --- | --- |
| `Is On` | 当前是否选中 |
| `Graphic` | 选中标记，例如勾选图、页签底线、高亮框 |
| `Toggle Transition` | `None` 立即切换，`Fade` 使用透明度淡入淡出 |
| `Group` | 所属 `UXGroup`，有分组时自动互斥 |
| 音效 | 不持有 clip。点击 / Submit 上报 `ToggleOn` / `ToggleOff`，悬停和焦点由 `UXSelectable` 上报。见 [UXUiAudio](UXUiAudio.md) |

`UXToggle` 选中后会把自身视觉状态强制当作 `Selected`，所以 `UXSelectable` 的子节点状态也能用于"选中页签文字变色、图标变亮"这类效果。`SetIsOnWithoutNotify` 适合初始化或同步服务端状态，避免触发业务回调。

## UXGroup 配置

| 配置 | 说明 |
| --- | --- |
| `Allow Switch Off` | 是否允许组内没有任何 Toggle 选中 |
| `Default Toggle` | 没有选中项时默认恢复到哪个 Toggle |
| `Toggles` | 当前组管理的 Toggle 数组 |

编辑器辅助按钮：

| 按钮 | 作用 |
| --- | --- |
| `Collect Children` | 收集当前节点子树下的 `UXToggle` |
| `Clean Nulls` | 清理数组里的空引用 |
| `Sort By Hierarchy` | 按层级顺序排序，影响 `Next()` 和 `Previous()` 的切换顺序 |

## 页签窗口完整示例

```csharp
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine.UI;

[Window(UILayer.UI)]
public sealed class RoleWindow : UIWindow<ui_RoleWindow>
{
    private UXGroup _tabGroup;

    protected override void OnInitialize()
    {
        _tabGroup = baseui.TabGroup;
        _tabGroup.allowSwitchOff = false;

        baseui.TglInfo.onValueChanged.AddListener(isOn =>
        {
            if (isOn) SwitchTab(0);
        });
        baseui.TglSkill.onValueChanged.AddListener(isOn =>
        {
            if (isOn) SwitchTab(1);
        });
        baseui.TglEquip.onValueChanged.AddListener(isOn =>
        {
            if (isOn) SwitchTab(2);
        });

        baseui.TglInfo.SetIsOnWithoutNotify(true);
        SwitchTab(0);
    }

    private void SwitchTab(int index)
    {
        baseui.PageInfo.SetActive(index == 0);
        baseui.PageSkill.SetActive(index == 1);
        baseui.PageEquip.SetActive(index == 2);
    }

    private void SelectNextTab()
    {
        _tabGroup.Next();
    }

    private void SelectPreviousTab()
    {
        _tabGroup.Previous();
    }
}
```

## API 速查

| API | 说明 |
| --- | --- |
| `UXToggle.isOn` | 读取或设置 Toggle 状态 |
| `UXToggle.onValueChanged` | 状态变化事件 |
| `UXToggle.SetIsOnWithoutNotify(bool)` | 不触发回调地设置 Toggle |
| `UXGroup.Next()` | 选中下一项，跳过不可用项 |
| `UXGroup.Previous()` | 选中上一项，跳过不可用项 |
| `UXGroup.GetFirstActiveToggle()` | 获取当前选中的 Toggle |
| `UXGroup.SetAllTogglesOff(bool)` | 清空选中状态，参数控制是否触发回调 |

## 注意事项

1. `SetIsOnWithoutNotify` 适合初始化或同步服务端状态，避免触发业务回调。
2. `UXGroup.Next()` 和 `UXGroup.Previous()` 会跳过不可用或不可交互的 Toggle。
3. `UXGroup` 的 `Sort By Hierarchy` 影响 `Next()` / `Previous()` 的遍历顺序，Prefab 结构调整后记得重新排序。
