using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class RvTextItemViewHolder : ViewHolder<RvTextData>, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI title;

        protected override void OnBind(RvTextData data, int index)
        {
            if (title != null)
            {
                title.text = data.Title;
            }

            if (background != null)
            {
                background.color = data.Color;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SetSelect();
        }
    }
}
