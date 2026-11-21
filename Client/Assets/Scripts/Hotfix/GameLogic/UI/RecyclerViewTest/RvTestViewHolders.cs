using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class RvTextItemViewHolder : NavigationViewHolder<RvTextData>, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image artwork;
        [SerializeField] private Sprite[] illustrations;
        [SerializeField] private GameObject focusFrame;

        protected override void OnBind(RvTextData data, int index)
        {
            title.text = data.Title;
            if (artwork != null)
            {
                artwork.sprite = illustrations[index % illustrations.Length];
            }

            focusFrame.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SetSelect();
        }

        protected override void OnNavigationFocused(bool focused) => focusFrame.SetActive(focused);
    }
}
