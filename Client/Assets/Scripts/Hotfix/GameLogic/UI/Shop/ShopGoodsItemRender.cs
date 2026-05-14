using AlicizaX.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic.UI
{
    public sealed class ShopGoodsData : ISimpleViewData
    {
        public int Id;
        public string NameKey;
        public string NameArg;
        public string DescriptionKey;
        public int Price;
        public string TagKey;
        public Color AccentColor;

        public string Name => GetLocalized(NameKey, NameArg);
        public string Description => GetLocalized(DescriptionKey);
        public string PriceText => LocalizationKey.UI.COMMON_CREDITPRICE(Price.ToString());
        public string Tag => GetLocalized(TagKey);

        private static string GetLocalized(string key, string arg = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return string.IsNullOrEmpty(arg)
                ? GameApp.Localization.GetString(key)
                : GameApp.Localization.GetString(key, arg);
        }

    }

    public sealed class ShopGoodsItemRender : ItemRender<ShopGoodsData, UIShopGoodsItemViewHolder>
    {
        public override ItemInteractionFlags InteractionFlags => ItemInteractionFlags.PointerNavigation;

        protected override void OnBind(ShopGoodsData data, int index)
        {
            baseui.NameText.text = data.Name;
            baseui.DescriptionText.text = data.Description;
            baseui.PriceText.text = data.PriceText;
            baseui.TagText.text = data.Tag;
            baseui.Icon.color = data.AccentColor;
            baseui.Background.color = new Color(0.035f, 0.04f, 0.04f, 0.9f);
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
