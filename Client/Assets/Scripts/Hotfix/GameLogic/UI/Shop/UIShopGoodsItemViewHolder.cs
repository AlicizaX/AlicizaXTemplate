using AlicizaX.Resource.Runtime;
using AlicizaX.UI;
using AlicizaX.Localization.UI;
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
            TagKey = tagKey;
            AccentColor = accentColor;
        }

        public int Id => _shopConfig.Id;
        public int ItemId => _itemConfig.Id;
        public string NameKey => _itemConfig.NameKey;
        public string DescriptionKey => _itemConfig.DescKey;
        public string Name => _itemConfig.Name;
        public string Description => _itemConfig.Desc;
        public int Price => _itemConfig.Price;
        public string Icon => _itemConfig.Icon;
        public string TagKey { get; }
        public Color AccentColor { get; }
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
        private LocalizedText _nameBinding;
        private LocalizedText _descriptionBinding;
        private LocalizedText _priceBinding;
        private LocalizedText _tagBinding;

        protected override void OnBind(ShopGoodsData data, int index)
        {
            _nameBinding ??= EnsureBinding(nameText);
            _descriptionBinding ??= EnsureBinding(descriptionText);
            _priceBinding ??= EnsureBinding(priceText);
            _tagBinding ??= EnsureBinding(tagText);
            _nameBinding.SetKey(data.NameKey);
            _descriptionBinding.SetKey(data.DescriptionKey);
            _priceBinding.SetContent(LocalizationKey.UI.COMMON_CREDITPRICE_Raw, data.Price);
            _tagBinding.SetKey(data.TagKey);
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
            _nameBinding?.SetKey(null);
            _descriptionBinding?.SetKey(null);
            _priceBinding?.SetKey(null);
            _tagBinding?.SetKey(null);
        }

        private static LocalizedText EnsureBinding(TMP_Text text)
        {
            return text.GetComponent<LocalizedText>() ?? text.gameObject.AddComponent<LocalizedText>();
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
