# RecyclerView 导航

`RecyclerView.Navigation` 为虚拟列表提供手柄/键盘导航。它不把焦点绑定到可回收的 `ViewHolder` 或 item `GameObject`，而是用数据索引维护一个虚拟焦点，并通过 `RecyclerViewNavigationController` 作为列表整体接入 `UXNavigationScope`。

源码位置：

```text
Client/Packages/com.alicizax.unity.ui.extension/Runtime/RecyclerView/Navigation
```

编译条件：导航代码受 `#if INPUTSYSTEM_SUPPORT && UX_NAVIGATION` 保护。需要启用 Unity Input System，并在 Scripting Define Symbols 中添加 `UX_NAVIGATION`。

## 核心类型

| 类型 | 说明 |
| --- | --- |
| `RecyclerViewNavigationController` | 挂在 `RecyclerView` 同物体上的导航入口，继承 `Selectable`，负责 Move/Submit/Cancel、焦点索引、滚动和层级恢复 |
| `IRecyclerViewNavigationViewHolder` | 模板 `ViewHolder` 实现该接口后才参与导航，同时负责焦点表现和当前项优先处理 Move |

## Prefab 配置

`RecyclerViewNavigationController` 必须和 `RecyclerView` 挂在同一个 GameObject 上：

```text
ScrollView
├── Content
├── Templates
└── 组件
    ├── RecyclerView
    ├── Scroller
    └── RecyclerViewNavigationController
```

`RecyclerViewNavigationController` 带有：

```csharp
[RequireComponent(typeof(RecyclerView))]
```

因此不要把它挂到父物体、子物体、`Content` 或模板 item 上。

父级窗口或面板需要挂 `UXNavigationScope`。在编辑器模式下，`RecyclerViewNavigationController.OnValidate()` 会从当前物体的父节点开始向上查找第一个 `UXNavigationScope` 并填入 `navigationScope` 字段。运行时不会自动查找 `navigationScope`，避免启用阶段遍历层级。

## Inspector 字段

| 字段 | 说明 |
| --- | --- |
| `Recycler View` | 同物体上的 `RecyclerView`，编辑器会自动赋值 |
| `Navigation Scope` | 父级最近的 `UXNavigationScope`，编辑器会自动赋值 |
| `Wrap Navigation` | 普通列表到首尾边界后是否允许焦点首尾环绕 |
| `Smooth Scroll` | 焦点变化时是否平滑滚动到目标 item |
| `Smooth Scroll Duration` | 平滑滚动时长 |
| `Focus Alignment` | 焦点 item 滚动到 Start / Center / End 等位置 |
| `Exit Left/Right/Up/Down` | 到达边界且不环绕时，跳出的外部 `Selectable` |

`Wrap Navigation` 不是 `LoopList` 开关。它只控制导航焦点是否首尾环绕。`LoopAdapter / UGLoopList` 会被自动识别为允许环绕；普通列表如果也需要从尾跳到头，可以手动勾选。

## ViewHolder 接入

只有模板 `ViewHolder` 实现 `IRecyclerViewNavigationViewHolder`，该模板对应的数据项才会被导航。未实现接口的模板会被跳过，适合分组标题、分隔线、装饰项等不可聚焦项。

```csharp
#if INPUTSYSTEM_SUPPORT && UX_NAVIGATION
using AlicizaX.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class BagItemHolder : ViewHolder<BagItemData>, IRecyclerViewNavigationViewHolder
{
    [SerializeField] private GameObject navigationFocusedFrame;

    protected override void OnBind(BagItemData data, int index)
    {
        // 正常绑定业务数据。
    }

    protected override void OnClear()
    {
        HandleNavigationFocused(false);
    }

    public void HandleNavigationFocused(bool focused)
    {
        navigationFocusedFrame.SetActive(focused);
    }

    public bool HandleNavigationMove(MoveDirection direction)
    {
        // item 内部如果有二级导航、滑条、页签等需要先处理方向输入，可在这里消费。
        // 返回 true 表示 RecyclerView 不再移动到其它 item。
        return false;
    }
}
#endif
```

焦点样式完全由 `HandleNavigationFocused` 内部实现。导航系统不会创建独立焦点框，也不会调用 `ViewHolder.ApplySelection`，避免混淆业务选中和导航焦点。

## 进入导航

`RecyclerViewNavigationController` 本身就是一个 `Selectable`。当 `UXNavigationScope` 选中它后，方向键或手柄方向输入会进入 `OnMove`，再由 controller 计算下一个可聚焦数据索引。

可以让 `UXNavigationScope` 自动选择，也可以在业务代码中主动选中：

```csharp
navigationController.Select();
```

或：

```csharp
EventSystem.current.SetSelectedGameObject(navigationController.gameObject);
```

Submit 会调用当前焦点数据索引的 `SetChoiceIndex`，因此可以和原有 `OnChoiceIndexChanged` 流程协作。

## 数据刷新

业务层不需要调用导航刷新 API。继续使用原有列表刷新方式：

```csharp
list.Data = newItems;
list.Adapter.NotifyDataChanged();
list.Adapter.NotifyItemChanged(index);
```

`Adapter` 内部会通过 internal 链路通知导航系统修正焦点：

```text
Adapter.Notify...
  -> RecyclerView.NotifyNavigationDataChanged()
  -> RecyclerViewNavigationController.NotifyDataSetChanged()
```

如果数据数量变少、当前焦点越界或当前模板变为不可导航，controller 会自动寻找最近的可导航项；如果没有可导航项，则清空导航焦点。

## 布局支持

| 布局 | 导航行为 |
| --- | --- |
| `LinearLayoutManager` | 按列表方向前后移动，可按 `Wrap Navigation` 环绕 |
| `GridLayoutManager` | 按行列移动；遇到不可导航项时在同行或同列内继续查找 |
| `MixedLayoutManager` | 按单轴列表处理，通过模板接口跳过不可导航项 |
| `CircleLayoutManager` | 按环形方向移动，天然环绕 |
| `LoopAdapter / UGLoopList` | 使用真实数据数量导航，焦点首尾环绕 |

导航计算只使用数据索引、模板接口和 `LayoutManager` 信息，不注册可回收 item，不在 Move 热路径创建临时集合。

## 层级恢复

当上层 UI 打开并成为更高优先级的 `UXNavigationScope` 时，底层 scope 会被压制，`RecyclerViewNavigationController` 会隐藏当前导航焦点表现并记住 `focusedDataIndex`。

上层 UI 关闭后，scope 恢复可导航，controller 会：

1. 重新选中自身 GameObject。
2. 根据之前保存的 `focusedDataIndex` 恢复焦点。
3. 滚动到焦点项并刷新 `HandleNavigationFocused`。

恢复依据是数据索引，不依赖 `ViewHolder` 或 item `GameObject`，因此不会被回收复用影响。

## 注意事项

1. `RecyclerViewNavigationController` 必须和 `RecyclerView` 同物体挂载。
2. 父级需要存在 `UXNavigationScope`，编辑器会自动填入最近的父级 scope。
3. 模板不实现 `IRecyclerViewNavigationViewHolder` 就不会参与导航。
4. `HandleNavigationMove` 返回 `true` 会阻止 RecyclerView 移动焦点。
5. 业务选中状态继续走 `ChoiceIndex` / `OnChoiceIndexChanged`，不要把导航焦点等同于业务选中。
6. 运行时不要手动调用 `NotifyDataSetChanged()`，数据刷新会自动触发导航修正。
