using AlicizaX.Resource.Runtime;
using AlicizaX.UI;
using AlicizaX.Localization.UI;
using Cysharp.Threading.Tasks;
using Game.Config.Tables;
using TMPro;
using UnityEngine;
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

    public sealed class UIShopGoodsItemViewHolder : InteractiveViewHolder<ShopGoodsData>
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

        // 保持现状：点击/提交只开购买弹窗，不改业务选中。
        protected override bool SelectOnActivate => false;

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
            // 复用的实例可能残留上一条数据的焦点/选中边框：新绑定不持有焦点，按业务选中重画。
            ApplyFrame(IsSelected);
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

        protected override void OnItemClick(ShopGoodsData data, int index)
        {
            GameApp.UI.ShowUI<UIBuyAlertWindow>(data).Forget();
        }

        protected override void OnNavigationFocused(bool focused)
        {
            ApplyFrame(focused);
        }

        protected override void OnSelectionChange(bool select)
        {
            ApplyFrame(select);
        }

        private void ApplyFrame(bool highlighted)
        {
            Color accentColor = CurrentData.AccentColor;
            selectedFrame.color = new Color(accentColor.r, accentColor.g, accentColor.b, highlighted ? 0.78f : 0.18f);
        }
    }
}
