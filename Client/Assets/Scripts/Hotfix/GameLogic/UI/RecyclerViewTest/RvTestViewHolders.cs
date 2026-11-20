using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class RvTextItemViewHolder : ViewHolder<RvTextData>, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image artwork;
        [SerializeField] private Sprite[] illustrations;

        protected override void OnBind(RvTextData data, int index)
        {
            if (title != null)
            {
                title.text = data.Title;
            }

            if (artwork != null && illustrations.Length > 0)
            {
                artwork.sprite = illustrations[index % illustrations.Length];
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SetSelect();
        }
    }
}
