using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class UIShopGoodsItemViewHolder : ViewHolder
    {
        [SerializeField] private Image background;
        [SerializeField] private Image selectedFrame;
        [SerializeField] private Image icon;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI tagText;

        public Image Background => background;
        public Image SelectedFrame => selectedFrame;
        public Image Icon => icon;

        public Image ItemIcon => itemIcon;
        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI DescriptionText => descriptionText;
        public TextMeshProUGUI PriceText => priceText;
        public TextMeshProUGUI TagText => tagText;
    }
}
