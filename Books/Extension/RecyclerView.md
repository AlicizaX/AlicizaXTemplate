# RecyclerView 列表

`RecyclerView` 是 `com.alicizax.unity.ui.extension` 中的虚拟滚动列表组件，负责列表项复用、滚动、布局、吸附、滚动条和局部刷新。业务层通常通过 `UGList`、`UGMixedList`、`UGLoopList`、`UGGroupList` 使用它，而不是直接操作内部 `IAdapter`。

源码位置：

```text
Client/Packages/com.alicizax.unity.ui.extension/Runtime/RecyclerView
```

## 基本结构

一个可用的 `RecyclerView` 需要以下配置：

| 配置 | 说明 |
| --- | --- |
| `RecyclerView` | 挂在滚动区域根节点上 |
| `Content` | 承载运行时列表项实例的 `RectTransform`；未配置时会尝试使用第一个子节点 |
| `Templates` | 一个或多个列表项模板；模板根节点必须挂 `ViewHolder` 子类 |
| `LayoutManager` | 布局管理器，例如线性、网格、分页、混合尺寸、圆形布局 |
| `Scroller` | 处理拖拽、滚轮、惯性、回弹和平滑滚动 |
| `Scrollbar` | 可选滚动条组件 |

运行时模板会被隐藏，实例由对象池创建和回收。不要直接把 `Templates` 中的模板对象当作真实显示项操作。

## 数据接口

普通列表数据只需要实现空接口 `ISimpleViewData`：

```csharp
public interface ISimpleViewData
{
}
```

多模板和分组列表使用模板下标，而不是模板名称：

```csharp
public interface IMixedViewData : ISimpleViewData
{
    int TemplateId { get; set; }
}

public interface IGroupViewData : IMixedViewData
{
    bool Expanded { get; set; }
    int Type { get; set; }
}
```

`TemplateId` 对应 `RecyclerView.Templates` 数组下标。当前代码中不存在 `TemplateName` 匹配机制。

## ViewHolder

列表项渲染逻辑写在 `ViewHolder<TData>` 子类中。当前模块没有独立的 `ItemRender<TData, THolder>` 类。

常用成员：

| 成员 | 说明 |
| --- | --- |
| `CurrentData` | 当前绑定的数据 |
| `CurrentIndex` | 当前数据索引 |
| `CurrentLayoutIndex` | 当前布局索引；循环或圆形布局中可能不同于数据索引 |
| `CurrentBindingVersion` | 当前绑定版本，可用于异步加载回调校验 |
| `IsBindingCurrent(version)` | 判断异步回调是否仍对应当前绑定 |
| `SetSelect()` | 将当前 `DataIndex` 提交为列表选择项 |
| `OnSelectionChange(bool)` | 选择状态变化回调 |
| `OnClear()` | Holder 被回收或重新绑定前的清理回调 |

如果异步加载图片，建议在 `OnBind` 记录 `CurrentBindingVersion`，回调时用 `IsBindingCurrent(version)` 判断当前 Holder 是否仍绑定同一份数据。

## UGList 普通列表

适用场景：背包、邮件列表、排行榜、任务子项列表等单一模板列表。

### Prefab 配置

```text
ScrollView
├── Content
└── Templates
    └── BagItemTemplate  // Templates[0]，挂 BagItemHolder
```

推荐配置：

| 项 | 值 |
| --- | --- |
| `Templates` | 只放一个模板 |
| `LayoutManager` | `LinearLayoutManager` 或 `GridLayoutManager` |
| `Direction` | 竖向列表用 `Vertical`，横向列表用 `Horizontal` |

### 数据

```csharp
using AlicizaX.UI;

public sealed class BagItemData : ISimpleViewData
{
    public int ItemId;
    public string Name;
    public int Count;
    public bool Locked;
}
```

### Holder

```csharp
using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BagItemHolder : ViewHolder<BagItemData>
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private GameObject lockedMask;

    protected override void OnBind(BagItemData data, int index)
    {
        nameText.text = data.Name;
        countText.text = data.Count.ToString();
        lockedMask.SetActive(data.Locked);
    }

    protected override void OnSelectionChange(bool select)
    {
        selectedFrame.SetActive(select);
        icon.color = select ? Color.cyan : Color.white;
    }

    protected override void OnClear()
    {
        selectedFrame.SetActive(false);
        lockedMask.SetActive(false);
        icon.color = Color.white;
    }

    public void OnClick()
    {
        if (!CurrentData.Locked)
        {
            SetSelect();
        }
    }
}
```

`OnClick` 可以绑定到模板上的 `Button.onClick`。如果没有按钮，也可以由其它点击组件调用这个公开方法。

### 窗口使用

```csharp
using System.Collections.Generic;
using AlicizaX;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine;

[Window(UILayer.UI)]
public sealed class BagWindow : UIWindow<ui_BagWindow>
{
    private UGList<BagItemData> _items;

    protected override void OnInitialize()
    {
        _items = UGListCreateHelper.Create<BagItemData>(baseui.ScrollViewItemList);
        _items.OnChoiceIndexChanged += OnChoiceChanged;
        _items.ScrollStopped += OnScrollStopped;

        _items.Data = CreateItems();
        _items.ChoiceIndex = 0;
        _items.ScrollToChoice(ScrollAlignment.Start);
    }

    protected override void OnClose()
    {
        if (_items == null)
        {
            return;
        }

        _items.OnChoiceIndexChanged -= OnChoiceChanged;
        _items.ScrollStopped -= OnScrollStopped;
    }

    private void OnChoiceChanged(int index)
    {
        BagItemData data = _items.Adapter.GetData(index);
        if (data == null)
        {
            return;
        }

        Log.Info($"Select item: {data.ItemId}");
    }

    private void OnScrollStopped()
    {
        Log.Info($"Scroll stopped at {_items.ScrollPosition}");
    }

    private static List<BagItemData> CreateItems()
    {
        return new List<BagItemData>
        {
            new BagItemData { ItemId = 1001, Name = "Potion", Count = 5 },
            new BagItemData { ItemId = 1002, Name = "Key", Count = 1 },
            new BagItemData { ItemId = 1003, Name = "Gem", Count = 3, Locked = true },
        };
    }
}
```

### 常见操作

```csharp
// 替换整份数据。
_items.Data = CreateItems();

// 添加单项。
_items.Adapter.Add(new BagItemData
{
    ItemId = 1004,
    Name = "Ticket",
    Count = 2,
});

// 修改已有数据，尺寸不变时只重绑可见项。
BagItemData item = _items.Adapter.GetData(1);
if (item != null)
{
    item.Count += 1;
    _items.Adapter.NotifyItemChanged(1);
}

// 如果修改会影响尺寸或布局，使用 relayout。
_items.Adapter.NotifyItemChanged(1, relayout: true);

// 滚动。
_items.ScrollToStart(0);
_items.ScrollToCenter(20, smooth: true);
_items.ScrollTo(20, ScrollAlignment.End, offset: 0f, smooth: true, duration: 0.3f);
```

## UGMixedList 混合模板列表

适用场景：一条列表中混合标题、普通项、奖励项、广告位、分割线等不同模板。

### Prefab 配置

```text
ScrollView
├── Content
└── Templates
    ├── MailTextTemplate    // Templates[0]，挂 MailTextHolder
    └── MailRewardTemplate  // Templates[1]，挂 MailRewardHolder
```

推荐使用 `MixedLayoutManager`，它会按每一项的 `TemplateId` 读取对应模板尺寸。

### 数据

```csharp
using AlicizaX.UI;

public enum MailTemplate
{
    Text = 0,
    Reward = 1,
}

public sealed class MailData : IMixedViewData
{
    public int MailId;
    public string Title;
    public string Content;
    public bool HasAttachment;
    public bool Claimed;
    public int TemplateId { get; set; }

    public static MailData Text(int id, string title, string content)
    {
        return new MailData
        {
            MailId = id,
            Title = title,
            Content = content,
            TemplateId = (int)MailTemplate.Text,
        };
    }

    public static MailData Reward(int id, string title, string content, bool claimed)
    {
        return new MailData
        {
            MailId = id,
            Title = title,
            Content = content,
            HasAttachment = true,
            Claimed = claimed,
            TemplateId = (int)MailTemplate.Reward,
        };
    }
}
```

### Holder

```csharp
using AlicizaX.UI;
using TMPro;
using UnityEngine;

public sealed class MailTextHolder : ViewHolder<MailData>
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private GameObject selectedFrame;

    protected override void OnBind(MailData data, int index)
    {
        titleText.text = data.Title;
        contentText.text = data.Content;
    }

    protected override void OnSelectionChange(bool select)
    {
        selectedFrame.SetActive(select);
    }

    protected override void OnClear()
    {
        selectedFrame.SetActive(false);
    }

    public void OnClick()
    {
        SetSelect();
    }
}
```

```csharp
using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MailRewardHolder : ViewHolder<MailData>
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject claimedMark;
    [SerializeField] private GameObject selectedFrame;

    protected override void OnBind(MailData data, int index)
    {
        titleText.text = data.Title;
        contentText.text = data.Content;
        claimButton.interactable = !data.Claimed;
        claimedMark.SetActive(data.Claimed);
    }

    protected override void OnSelectionChange(bool select)
    {
        selectedFrame.SetActive(select);
    }

    protected override void OnClear()
    {
        claimButton.interactable = false;
        claimedMark.SetActive(false);
        selectedFrame.SetActive(false);
    }

    public void OnClick()
    {
        SetSelect();
    }
}
```

### 窗口使用

```csharp
using System.Collections.Generic;
using AlicizaX;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine;

[Window(UILayer.UI)]
public sealed class MailWindow : UIWindow<ui_MailWindow>
{
    private UGMixedList<MailData> _mails;

    protected override void OnInitialize()
    {
        _mails = UGListCreateHelper.CreateMixed<MailData>(baseui.ScrollViewMailList);
        _mails.OnChoiceIndexChanged += OnMailSelected;
        _mails.Data = CreateMails();
    }

    protected override void OnClose()
    {
        if (_mails != null)
        {
            _mails.OnChoiceIndexChanged -= OnMailSelected;
        }
    }

    private void OnMailSelected(int index)
    {
        MailData mail = _mails.Adapter.GetData(index);
        if (mail == null)
        {
            return;
        }

        Log.Info($"Open mail: {mail.MailId}");
    }

    public void ClaimSelectedReward()
    {
        int index = _mails.ChoiceIndex;
        MailData mail = _mails.Adapter.GetData(index);
        if (mail == null || !mail.HasAttachment || mail.Claimed)
        {
            return;
        }

        mail.Claimed = true;
        _mails.Adapter.NotifyItemChanged(index);
    }

    private static List<MailData> CreateMails()
    {
        return new List<MailData>
        {
            MailData.Text(1, "System Notice", "Server maintenance completed."),
            MailData.Reward(2, "Login Reward", "Claim your daily reward.", claimed: false),
            MailData.Text(3, "Arena Result", "You reached rank 12."),
        };
    }
}
```

### 常见操作

```csharp
// 插入一个奖励邮件。TemplateId 决定使用哪个模板。
_mails.Adapter.Insert(0, MailData.Reward(4, "Compensation", "Thanks for waiting.", claimed: false));

// 修改模板类型时需要重布局，因为可见实例可能要换模板。
MailData mail = _mails.Adapter.GetData(1);
if (mail != null)
{
    mail.TemplateId = (int)MailTemplate.Reward;
    mail.HasAttachment = true;
    _mails.Adapter.NotifyItemChanged(1, relayout: true);
}
```

注意：`TemplateId` 必须是 `Templates` 数组的合法下标。`Templates[0]`、`Templates[1]` 的顺序改了，数据里的枚举值也要同步调整。

## UGLoopList 循环列表

适用场景：轮播图、角色预览、循环选择器、无限横向滚动。

`UGLoopList<TData>` 使用 `LoopAdapter<TData>`，当真实数据数量大于 0 时，显示数量返回 `int.MaxValue`。绑定时会用 `index % list.Count` 映射回真实数据。

### Prefab 配置

```text
ScrollView
├── Content
└── Templates
    └── BannerTemplate  // Templates[0]，挂 BannerHolder
```

推荐配置：

| 项 | 值 |
| --- | --- |
| `LayoutManager` | `LinearLayoutManager`、`PageLayoutManager` 或 `CircleLayoutManager` |
| `Direction` | 轮播通常用 `Horizontal` |
| `Snap` | 通常开启 |
| `Inertia` | 按手感决定 |

### 数据

```csharp
using AlicizaX.UI;

public sealed class BannerData : ISimpleViewData
{
    public int BannerId;
    public string Title;
    public string ImagePath;
}
```

### Holder

```csharp
using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BannerHolder : ViewHolder<BannerData>
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject selectedFrame;

    protected override void OnBind(BannerData data, int index)
    {
        titleText.text = data.Title;

        uint version = CurrentBindingVersion;
        LoadImageAsync(data.ImagePath, sprite =>
        {
            if (IsBindingCurrent(version))
            {
                image.sprite = sprite;
            }
        });
    }

    protected override void OnSelectionChange(bool select)
    {
        selectedFrame.SetActive(select);
    }

    protected override void OnClear()
    {
        image.sprite = null;
        selectedFrame.SetActive(false);
    }

    public void OnClick()
    {
        SetSelect();
    }

    private void LoadImageAsync(string path, System.Action<Sprite> onLoaded)
    {
        // 接入项目自己的异步图片加载逻辑。
        onLoaded?.Invoke(null);
    }
}
```

### 窗口使用

```csharp
using System.Collections.Generic;
using AlicizaX;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine;

[UIUpdate]
[Window(UILayer.UI)]
public sealed class BannerWindow : UIWindow<ui_BannerWindow>
{
    private UGLoopList<BannerData> _banners;
    private float _autoScrollTimer;

    protected override void OnInitialize()
    {
        _banners = UGListCreateHelper.CreateLoop<BannerData>(baseui.ScrollViewBannerList);
        _banners.OnChoiceIndexChanged += OnBannerSelected;
        _banners.ScrollStopped += OnScrollStopped;
        _banners.Data = CreateBanners();

        if (_banners.DataCount > 0)
        {
            _banners.ChoiceIndex = 0;
            _banners.ScrollToIndex(0);
        }
    }

    protected override void OnClose()
    {
        if (_banners == null)
        {
            return;
        }

        _banners.OnChoiceIndexChanged -= OnBannerSelected;
        _banners.ScrollStopped -= OnScrollStopped;
    }

    protected override void OnUpdate()
    {
        if (_banners == null || _banners.DataCount <= 0)
        {
            return;
        }

        _autoScrollTimer += Time.deltaTime;
        if (_autoScrollTimer < 3f)
        {
            return;
        }

        _autoScrollTimer = 0f;
        int next = (_banners.ChoiceIndex + 1) % _banners.DataCount;
        _banners.ChoiceIndex = next;
        _banners.ScrollToCenter(next, smooth: true, duration: 0.25f);
    }

    private void OnBannerSelected(int index)
    {
        BannerData banner = _banners.Adapter.GetData(index);
        if (banner != null)
        {
            Log.Info($"Select banner: {banner.BannerId}");
        }
    }

    private void OnScrollStopped()
    {
        // 开启 Snap 时，RecyclerView 会在停止后吸附到最近项。
        // 如果需要把当前停靠项同步成业务选择，可在这里按业务规则计算并设置 ChoiceIndex。
    }

    private static List<BannerData> CreateBanners()
    {
        return new List<BannerData>
        {
            new BannerData { BannerId = 1, Title = "Event A", ImagePath = "event_a" },
            new BannerData { BannerId = 2, Title = "Event B", ImagePath = "event_b" },
            new BannerData { BannerId = 3, Title = "Event C", ImagePath = "event_c" },
        };
    }
}
```

### 常见操作

```csharp
// 循环列表的滚动索引传真实数据索引即可。
_banners.ScrollToCenter(2, smooth: true);

// 替换轮播数据后重新定位。
_banners.Data = CreateBanners();
_banners.ChoiceIndex = 0;
_banners.ScrollToIndex(0);
```

注意：循环列表真实数据数量来自 `DataCount` 和 `Adapter.GetRealCount()`。业务逻辑不要遍历 `Adapter.GetItemCount()`，因为循环列表显示数量可能是 `int.MaxValue`。

## UGGroupList 分组列表

适用场景：任务分组、背包分类、设置页分段、成就分类等“组头 + 子项”的列表。

`UGGroupList<TData>` 要求数据实现 `IGroupViewData`，并且 `TData` 必须有无参构造函数。创建时需要传入组头模板下标 `groupTemplateId`。

### Prefab 配置

```text
ScrollView
├── Content
└── Templates
    ├── QuestGroupTemplate  // Templates[0]，挂 QuestGroupHolder
    └── QuestItemTemplate   // Templates[1]，挂 QuestItemHolder
```

推荐使用 `MixedLayoutManager`，因为组头和子项通常高度不同。

### 数据

原始 `Data` 只放真实子项。`GroupAdapter` 会扫描子项的 `Type`，为每个类型创建一个临时组头数据：

```csharp
new TData
{
    TemplateId = groupTemplateId,
    Type = type
};
```

```csharp
using AlicizaX.UI;

public enum QuestGroupType
{
    Main = 1,
    Daily = 2,
    Achievement = 3,
}

public sealed class QuestRowData : IGroupViewData
{
    public int QuestId;
    public string Title;
    public int Current;
    public int Target;
    public bool Completed;

    public int TemplateId { get; set; }
    public bool Expanded { get; set; }
    public int Type { get; set; }

    public QuestGroupType Group => (QuestGroupType)Type;

    public static QuestRowData Item(
        QuestGroupType group,
        int questId,
        string title,
        int current,
        int target,
        bool completed = false)
    {
        return new QuestRowData
        {
            QuestId = questId,
            Title = title,
            Current = current,
            Target = target,
            Completed = completed,
            Type = (int)group,
            TemplateId = 1,
        };
    }
}
```

### Holder

```csharp
using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuestGroupHolder : ViewHolder<QuestRowData>
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image arrow;
    [SerializeField] private GameObject selectedFrame;

    protected override void OnBind(QuestRowData data, int index)
    {
        titleText.text = GetGroupTitle(data.Group);
        arrow.rectTransform.localEulerAngles = data.Expanded
            ? new Vector3(0f, 0f, 90f)
            : Vector3.zero;
    }

    protected override void OnSelectionChange(bool select)
    {
        selectedFrame.SetActive(select);
    }

    protected override void OnClear()
    {
        arrow.rectTransform.localEulerAngles = Vector3.zero;
        selectedFrame.SetActive(false);
    }

    public void OnClick()
    {
        SetSelect();
    }

    private static string GetGroupTitle(QuestGroupType group)
    {
        return group switch
        {
            QuestGroupType.Main => "主线任务",
            QuestGroupType.Daily => "日常任务",
            QuestGroupType.Achievement => "成就任务",
            _ => "其他任务",
        };
    }
}
```

```csharp
using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuestItemHolder : ViewHolder<QuestRowData>
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Button rewardButton;
    [SerializeField] private GameObject selectedFrame;

    protected override void OnBind(QuestRowData data, int index)
    {
        titleText.text = data.Title;
        progressText.text = $"{data.Current}/{data.Target}";
        rewardButton.interactable = data.Completed;
    }

    protected override void OnSelectionChange(bool select)
    {
        selectedFrame.SetActive(select);
    }

    protected override void OnClear()
    {
        selectedFrame.SetActive(false);
        rewardButton.interactable = false;
    }

    public void OnClick()
    {
        SetSelect();
    }
}
```

### 窗口使用

```csharp
using System.Collections.Generic;
using AlicizaX;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine;

[Window(UILayer.UI)]
public sealed class QuestWindow : UIWindow<ui_QuestWindow>
{
    private const int GroupTemplateId = 0;

    private UGGroupList<QuestRowData> _quests;

    protected override void OnInitialize()
    {
        _quests = UGListCreateHelper.CreateGroup<QuestRowData>(
            baseui.ScrollViewQuestList,
            groupTemplateId: GroupTemplateId);

        _quests.OnChoiceIndexChanged += OnQuestChoiceChanged;
        _quests.Data = CreateQuestRows();

        ExpandFirstGroup();
    }

    protected override void OnClose()
    {
        if (_quests != null)
        {
            _quests.OnChoiceIndexChanged -= OnQuestChoiceChanged;
        }
    }

    private void OnQuestChoiceChanged(int displayIndex)
    {
        if (!_quests.TryGetDisplayData(displayIndex, out QuestRowData row))
        {
            return;
        }

        if (_quests.IsGroupIndex(displayIndex))
        {
            _quests.Activate(displayIndex);
            return;
        }

        Log.Info($"Open quest detail: {row.QuestId}");
    }

    private void ExpandFirstGroup()
    {
        if (_quests.TryGetDisplayData(0, out _) && _quests.IsGroupIndex(0))
        {
            _quests.Expand(0);
        }
    }

    public void AddDailyQuest()
    {
        _quests.Data.Add(QuestRowData.Item(
            QuestGroupType.Daily,
            questId: 2003,
            title: "完成一次钓鱼",
            current: 0,
            target: 1));

        _quests.Adapter.NotifyDataChanged();
    }

    private static List<QuestRowData> CreateQuestRows()
    {
        return new List<QuestRowData>
        {
            QuestRowData.Item(QuestGroupType.Main, 1001, "前往王城", 1, 1, completed: true),
            QuestRowData.Item(QuestGroupType.Main, 1002, "拜访骑士团长", 0, 1),
            QuestRowData.Item(QuestGroupType.Daily, 2001, "完成三次副本", 1, 3),
            QuestRowData.Item(QuestGroupType.Daily, 2002, "赠送一次礼物", 0, 1),
            QuestRowData.Item(QuestGroupType.Achievement, 3001, "累计登录七天", 4, 7),
        };
    }
}
```

### 常见操作

```csharp
// 展开或收起显示索引上的组头。
_quests.Expand(0);
_quests.Collapse(0);
_quests.SetExpanded(0, true);

// Activate：组头会切换展开，子项会设置 ChoiceIndex。
_quests.Activate(displayIndex);

// 获取当前显示列表数据。显示列表包含组头和已展开子项。
if (_quests.TryGetDisplayData(displayIndex, out QuestRowData row))
{
    bool isGroup = _quests.IsGroupIndex(displayIndex);
}

// 分组数据变更后通常使用全量刷新，因为显示列表需要重建。
_quests.Adapter.NotifyDataChanged();
```

注意：

1. `groupTemplateId` 是组头模板下标，不是显示文本。
2. 子项的 `TemplateId` 不要等于 `groupTemplateId`，否则 `IsGroupIndex` 会把子项识别为组头。
3. `UGGroupList` 的索引是显示索引，不是原始 `Data` 的子项索引。展开和收起会改变显示索引。
4. 仅靠 `SetSelect()` 时，同一个已选中的组头再次点击不会触发 `OnChoiceIndexChanged`。如果业务要求重复点击同一组头也切换，需要让窗口层直接调用 `_quests.Activate(displayIndex)`。

## 数据更新

`UGListBase` 暴露 `Adapter`，可以直接增删改数据并刷新。

```csharp
_items.Adapter.Add(new BagItemData
{
    ItemId = 1003,
    Name = "Gem",
    Count = 3,
});

_items.Adapter.RemoveAt(0);
_items.Adapter.NotifyItemChanged(1);
_items.Adapter.NotifyItemChanged(1, relayout: true);
_items.Adapter.NotifyDataChanged();
```

常用策略：

| 场景 | 推荐 API |
| --- | --- |
| 替换整份数据 | `_items.Data = newList` |
| 可见项内容变化且尺寸不变 | `_items.Adapter.NotifyItemChanged(index)` |
| 可见项尺寸或模板变化 | `_items.Adapter.NotifyItemChanged(index, relayout: true)` |
| 一段可见项内容变化 | `_items.Adapter.NotifyItemRangeChanged(index, count)` |
| 添加单项 | `_items.Adapter.Add(data)` |
| 插入单项 | `_items.Adapter.Insert(index, data)` |
| 删除单项 | `_items.Adapter.RemoveAt(index)` |
| 清空列表 | `_items.Adapter.Clear()` |
| 反转顺序 | `_items.Adapter.Reverse()` |

`AddRange`、`InsertRange`、`RemoveAll`、`Sort` 当前是 `internal`，不要在程序集外文档示例中当作公开 API 使用。

## 选择、滚动和事件

`ChoiceIndex` 是业务选择索引。它只会在显式设置、`SetSelect()` 或分组子项 `Activate(index)` 时变化。

```csharp
_items.ChoiceIndex = 3;
_items.ClearChoice();

_items.ScrollToIndex(10);
_items.ScrollToStart(10, offset: 0f, smooth: true);
_items.ScrollToCenter(10, offset: 0f, smooth: true, duration: 0.3f);
_items.ScrollToEnd(10);
_items.ScrollTo(10, ScrollAlignment.Center, offset: 0f, smooth: true);
_items.ScrollToChoice(ScrollAlignment.Center, smooth: true);
```

事件：

```csharp
_items.OnChoiceIndexChanged += index => { };
_items.ScrollValueChanged += position => { };
_items.ScrollStopped += () => { };
_items.ScrollDraggingChanged += dragging => { };
```

当前 `UGList` 没有 `FocusIndex`、`TryFocus`、`CommitFocusToChoice` 这些 API。

## Inspector 配置

`RecyclerView` 的主要配置项：

| 配置 | 说明 |
| --- | --- |
| `Direction` | `Vertical`、`Horizontal`、`Custom` |
| `Alignment` | `Left`、`Center`、`Top` |
| `Spacing` | 项间距 |
| `Padding` | 内容内边距 |
| `MovementType` | `Elastic` 回弹或 `Clamped` 硬限制 |
| `Scroll` | `AlwaysDisable`、`AlwaysEnable`、`WhenScrollable` |
| `Snap` | 停止滚动后吸附到最近项 |
| `Inertia` | 是否启用惯性滑动 |
| `DecelerationRate` | 惯性减速率，范围 `[0.001, 0.999]`，值越小减速越快 |
| `ScrollSpeed` | 平滑滚动速度系数 |
| `WheelSpeed` | 鼠标滚轮速度系数 |
| `ScrollbarVisibility` | `AlwaysHide`、`AlwaysShow`、`WhenScrollable` |
| `Templates` | 模板数组，混合和分组列表通过下标选择模板 |

布局类型：

| 类型 | 说明 |
| --- | --- |
| `LinearLayoutManager` | 单列或单行列表，所有项使用第一个模板尺寸 |
| `GridLayoutManager` | 网格列表，`cellCount` 控制每行或每列数量 |
| `PageLayoutManager` | 分页列表，继承线性布局并对可见项做缩放动画 |
| `MixedLayoutManager` | 多模板或不同尺寸列表，按每项 `TemplateId` 读取模板长度 |
| `CircleLayoutManager` | 圆形布局，使用虚拟布局区间并按角度摆放 |

滚动条在 `WhenScrollable` 模式下只会在内容尺寸大于视口尺寸时显示并允许交互。对 `Direction.Custom`，溢出检测不生效。

## API 速查

| API | 说明 |
| --- | --- |
| `UGListCreateHelper.Create<T>(RecyclerView)` | 创建普通列表 |
| `UGListCreateHelper.CreateMixed<T>(RecyclerView)` | 创建混合模板列表 |
| `UGListCreateHelper.CreateLoop<T>(RecyclerView)` | 创建循环列表 |
| `UGListCreateHelper.CreateGroup<T>(RecyclerView, int)` | 创建分组列表，第二个参数是组头模板下标 |
| `UGListBase.Data` | 替换数据源并刷新 |
| `UGListBase.DataCount` | 原始数据数量 |
| `UGListBase.ChoiceIndex` | 获取或设置业务选择 |
| `UGListBase.HasChoice` | 是否已有选择 |
| `UGListBase.ScrollPosition` | 当前滚动位置 |
| `UGListBase.ScrollTo(...)` | 按对齐方式滚动到索引 |
| `UGListBase.ScrollToChoice(...)` | 滚动到当前选择 |
| `Adapter.GetData(index)` | 获取原始数据 |
| `Adapter.NotifyDataChanged()` | 重新布局并刷新 |
| `Adapter.NotifyItemChanged(index, relayout)` | 重绑可见项或重新布局 |
| `RecyclerView.TrimInactivePool()` | 裁剪对象池中的非活动实例 |
| `RecyclerView.PoolStats` | 开发环境下查看对象池统计 |

## 注意事项

1. `Templates` 必须非空，且每个模板都必须挂 `ViewHolder` 子类。
2. 混合模板和分组列表的 `TemplateId` 必须是合法的模板数组下标。
3. 分组列表的 `groupTemplateId` 必须是组头模板下标，子项不要使用同一个模板下标，除非业务确实希望它被识别为组头。
4. 动态改变列表项尺寸或模板后，需要使用 `relayout: true` 或 `NotifyDataChanged()`。
5. `ScrollMode.AlwaysDisable` 会阻止 `ScrollTo` 定位。
6. 所有 Unity 对象和 UI 操作都应在 Unity 主线程执行。
