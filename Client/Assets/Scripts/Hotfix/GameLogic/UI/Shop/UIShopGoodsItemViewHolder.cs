using AlicizaX.Resource.Runtime;
using AlicizaX.UI;
using Cysharp.Threading.Tasks;
using Game.Config;
using Game.Config.Tables;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class ShopGoodsData : ISimpleViewData
    {
        private readonly ShopConfig _offer;
        private ItemConfig Item => _offer.ItemId_Ref;

        public ShopGoodsData(ShopConfig offer) => _offer = offer;

        public int ItemId => Item.Id;
        public EShopCategory Category => _offer.Type;
        public string NameKey => Item.NameKey;
        public string Name => Item.Name;
        public string Description => Item.Desc;
        public int Price => Item.Price;
        public string Icon => Item.Icon;
    }

    public sealed class UIShopGoodsItemViewHolder : NavigationViewHolder<ShopGoodsData>, IPointerClickHandler
    {
        [SerializeField] private Image selectedFrame;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI priceText;

        protected override void OnBind(ShopGoodsData data, int index)
        {
            nameText.SetText(data.Name);
            itemIcon.SetSprite(data.Icon);
            priceText.SetText(data.Price.ToString("N0"));
            selectedFrame.gameObject.SetActive(IsSelected);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) SetSelect();
        }

        protected override void OnNavigationFocused(bool focused)
        {
            if (focused) SetSelect();
        }

        protected override void OnSelectionChange(bool selected) => selectedFrame.gameObject.SetActive(selected);

        public override bool HandleNavigationSubmit()
        {
            if (CurrentData == null) return false;
            GameApp.UI.ShowUI<UIBuyAlertWindow>(CurrentData).Forget();
            return true;
        }
    }
}
