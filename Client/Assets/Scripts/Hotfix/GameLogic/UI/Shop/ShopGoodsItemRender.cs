using AlicizaX.UI;
using Game.Config.Tables;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic.UI
{
    public sealed class ShopGoodsData : ISimpleViewData
    {
        private readonly ShopConfig _shopConfig;
        private readonly ItemConfig _itemConfig;

        public ShopGoodsData(ShopConfig shopConfig, string tagKey, Color accentColor)
        {
            _shopConfig = shopConfig;
            _itemConfig = shopConfig.ItemId_Ref;
            Tag = tagKey;
            AccentColor = accentColor;
        }

        public int Id => _shopConfig.Id;
        public int ItemId => _itemConfig.Id;
        public string Name => _itemConfig.Name;
        public string Description => _itemConfig.Desc;
        public int Price => _itemConfig.Price;
        public string Icon => _itemConfig.Icon;
        public string Tag { get;private set; }
        public Color AccentColor { get; }
        public string PriceText => LocalizationKey.UI.COMMON_CREDITPRICE(Price.ToString());
    }

    public sealed class ShopGoodsItemRender : ItemRender<ShopGoodsData, UIShopGoodsItemViewHolder>
    {
        public override ItemInteractionFlags InteractionFlags => ItemInteractionFlags.PointerNavigation;

        private static readonly Color BackgroundColor = new(0.035f, 0.04f, 0.04f, 0.9f);
        private string _iconLocation;

        protected override void OnBind(ShopGoodsData data, int index)
        {
            baseui.NameText.text = data.Name;
            baseui.DescriptionText.text = data.Description;
            baseui.PriceText.text = data.PriceText;
            baseui.TagText.text = data.Tag;
            if (_iconLocation != data.Icon)
            {
                _iconLocation = data.Icon;
                baseui.ItemIcon.SetSprite(data.Icon);
            }

            baseui.Icon.color = data.AccentColor;
            baseui.Background.color = BackgroundColor;
            baseui.SelectedFrame.color = new Color(data.AccentColor.r, data.AccentColor.g, data.AccentColor.b, 0.18f);
        }

        protected override void OnPointerClick(PointerEventData eventData)
        {
            GameApp.UI.ShowUISync<UIBuyAlertWindow>(CurrentData);
        }

        protected override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
        }

        protected override void OnSelectionChanged(bool selected)
        {
            if (CurrentData == null)
            {
                return;
            }

            baseui.SelectedFrame.color = selected
                ? new Color(CurrentData.AccentColor.r, CurrentData.AccentColor.g, CurrentData.AccentColor.b, 0.5f)
                : new Color(CurrentData.AccentColor.r, CurrentData.AccentColor.g, CurrentData.AccentColor.b, 0.18f);
        }
    }
}
