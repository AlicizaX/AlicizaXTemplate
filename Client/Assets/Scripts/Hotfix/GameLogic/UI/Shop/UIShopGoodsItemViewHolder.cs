using AlicizaX.UI;
using Cysharp.Threading.Tasks;
using Game.Config.Tables;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
            PriceText = LocalizationKey.UI.COMMON_CREDITPRICE(_itemConfig.Price.ToStringNonAlloc());
        }

        public int Id => _shopConfig.Id;
        public int ItemId => _itemConfig.Id;
        public string Name => _itemConfig.Name;
        public string Description => _itemConfig.Desc;
        public int Price => _itemConfig.Price;
        public string Icon => _itemConfig.Icon;
        public string Tag { get; private set; }
        public Color AccentColor { get; }
        public string PriceText { get; }
    }

    public sealed class UIShopGoodsItemViewHolder : ViewHolder<ShopGoodsData>, IPointerClickHandler, IRecyclerViewNavigationViewHolder
    {
        private static readonly Color BackgroundColor = new(0.035f, 0.04f, 0.04f, 0.9f);

        [SerializeField] private Image background;
        [SerializeField] private Image selectedFrame;
        [SerializeField] private Image icon;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI tagText;

        private string _iconLocation;

        protected override void OnBind(ShopGoodsData data, int index)
        {
            nameText.text = data.Name;
            descriptionText.text = data.Description;
            priceText.text = data.PriceText;
            tagText.text = data.Tag;
            if (_iconLocation != data.Icon)
            {
                _iconLocation = data.Icon;
                itemIcon.SetSprite(data.Icon);
            }

            icon.color = data.AccentColor;
            background.color = BackgroundColor;
        }

        protected override void OnClear()
        {
            _iconLocation = null;
        }

        protected override void OnSelectionChange(bool select)
        {
            if (CurrentData != null)
            {
                Color accentColor = CurrentData.AccentColor;
                selectedFrame.color = new Color(accentColor.r, accentColor.g, accentColor.b, select ? 0.78f : 0.18f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CurrentData != null)
            {
                SetSelect();
                GameApp.UI.ShowUI<UIBuyAlertWindow>(CurrentData).Forget();
            }
        }

        public void HandleNavigationFocused(bool focused)
        {
            OnSelectionChange(focused);
        }

        public bool HandleNavigationMove(AxisEventData eventData)
        {
            return false;
        }

        public bool HandleNavigationSubmit()
        {
            return false;
        }

        public bool IsNavigationFocusable(int dataIndex)
        {
            return true;
        }
    }
}
