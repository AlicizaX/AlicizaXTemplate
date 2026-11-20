using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic.UI
{
    public sealed class RvGroupItemViewHolder : ViewHolder<RvGroupData>, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI title;

        protected override void OnBind(RvGroupData data, int index)
        {
            if (title == null)
            {
                return;
            }

            title.text = data.TemplateId == 0 ? FormatGroupTitle(data) : data.Text;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CurrentData != null && CurrentData.TemplateId == 0)
            {
                UIRecyclerViewExampleWindow.ActivateGroup(CurrentIndex);
                return;
            }

            SetSelect();
        }

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
