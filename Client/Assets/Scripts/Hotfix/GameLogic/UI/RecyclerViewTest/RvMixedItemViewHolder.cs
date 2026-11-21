using AlicizaX.UI;
using TMPro;
using UnityEngine;

namespace GameLogic.UI
{
    public sealed class RvMixedItemViewHolder : NavigationViewHolder<RvMixedData>
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject focusFrame;

        protected override void OnBind(RvMixedData data, int index)
        {
            title.text = data.Text;
            focusFrame.SetActive(false);
        }

        protected override void OnNavigationFocused(bool focused) => focusFrame.SetActive(focused);
    }
}
