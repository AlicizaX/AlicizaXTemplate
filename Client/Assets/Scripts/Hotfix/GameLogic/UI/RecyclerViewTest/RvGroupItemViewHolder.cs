using AlicizaX.UI;
using TMPro;
using UnityEngine;

namespace GameLogic.UI
{
    public sealed class RvGroupItemViewHolder : InteractiveViewHolder<RvGroupData>
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject focusFrame;

        // 点击与提交都交给 GroupAdapter.Activate：组头展开/收起，子项才设为业务选中。
        protected override bool SelectOnActivate => false;

        protected override void OnBind(RvGroupData data, int index)
        {
            title.text = data.TemplateId == 0 ? FormatGroupTitle(data) : data.Text;
            focusFrame.SetActive(false);
        }

        protected override void OnNavigationFocused(bool focused) => focusFrame.SetActive(focused);

        protected override void OnItemClick(RvGroupData data, int index) => UIRecyclerViewExampleWindow.ActivateGroup(index);

        private static string FormatGroupTitle(RvGroupData data)
        {
            string name = data.Type switch
            {
                0 => LocalizationKey.UI.RECYCLER_DATA_DAILY,
                1 => LocalizationKey.UI.RECYCLER_DATA_WORLD,
                2 => LocalizationKey.UI.RECYCLER_DATA_STORY,
                _ => data.Type.ToString()
            };
            return (data.Expanded ? "−    " : "+    ") + name;
        }
    }
}
