using System.Collections.Generic;
using AlicizaX;
using AlicizaX.Localization;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Game.Config;
using Game.UI;
using GameLogic.Player;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.UI
{
    [Window(UILayer.UI, false, 3)]
    public class UIShopWindow : UITabWindow<ui_UIShopWindow>
    {
        private readonly List<ShopGoodsData> _goods = new(1024);
        private UGList<ShopGoodsData> _goodsList;
        private IFakePlayerDataService _playerDataService;
        private IConfigService _configService;

        protected override void OnInitialize()
        {
            _goodsList = UGListCreateHelper.Create<ShopGoodsData>(baseui.ScrollViewGoodsList);
            _goodsList.RegisterItemRender<ShopGoodsItemRender>();
            _playerDataService = AppServices.Require<IFakePlayerDataService>();
            _configService = AppServices.Require<IConfigService>();

            baseui.TogRecommend.onValueChanged.AddListener(OnTogRecommendChanged);
            baseui.TogItem.onValueChanged.AddListener(OnTogItemChanged);
            baseui.TogSkin.onValueChanged.AddListener(OnTogSkinChanged);
            baseui.TogPack.onValueChanged.AddListener(OnTogPackChanged);
            baseui.BtnClose.onClick.AddListener(OnBtnCloseClick);
            if (baseui.TextCurrency is UXTextMeshPro currencyText)
            {
                currencyText.SetLocalization(string.Empty);
            }

            baseui.TogRecommend.isOn = true;
            RefreshStaticText();
            RefreshCurrencyText();
            SwitchCategory(EShopCategory.TEST);
        }

        protected override void OnRegisterEvent(EventListenerProxy proxy)
        {
            proxy.AddUIEvent<PlayerDataChangedEvent>(OnPlayerDataChanged);
            proxy.AddUIEvent<LocalizationChangeEvent>(OnLocalizationChanged);
        }

        private void OnPlayerDataChanged(in PlayerDataChangedEvent evt)
        {
            RefreshCurrencyText();
        }

        private void OnLocalizationChanged(in LocalizationChangeEvent evt)
        {
            RefreshStaticText();
            RefreshCurrencyText();
            _goodsList.Adapter.NotifyDataChanged();
        }

        private void RefreshStaticText()
        {
            SetLocalizedText(baseui.TogRecommend, LocalizationKey.UI.SHOP_TABRECOMMEND);
            SetLocalizedText(baseui.TogItem, LocalizationKey.UI.SHOP_TABITEM);
            SetLocalizedText(baseui.TogSkin, LocalizationKey.UI.SHOP_TABSKIN);
            SetLocalizedText(baseui.TogPack, LocalizationKey.UI.SHOP_TABPACK);
        }

        private void RefreshCurrencyText()
        {
            baseui.TextCurrency.text = LocalizationKey.UI.SHOP_CURRENCY(_playerDataService.Credit.ToString());
        }

        private static void SetLocalizedText(Selectable selectable, string text)
        {
            var textMeshPro = selectable.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (textMeshPro != null)
            {
                textMeshPro.text = text;
            }
        }

        private void OnTogRecommendChanged(bool isOn)
        {
            if (isOn)
            {
                SwitchCategory(EShopCategory.TEST);
            }
        }

        private void OnTogItemChanged(bool isOn)
        {
            if (isOn)
            {
                SwitchCategory(EShopCategory.MATERIAL);
            }
        }

        private void OnTogSkinChanged(bool isOn)
        {
            if (isOn)
            {
                SwitchCategory(EShopCategory.EQUIPMENT);
            }
        }

        private void OnTogPackChanged(bool isOn)
        {
            if (isOn)
            {
                SwitchCategory(EShopCategory.CONSUMABLE);
            }
        }

        private void OnBtnCloseClick()
        {
            CloseSelf();
        }

        private void SwitchCategory(EShopCategory category)
        {
            BuildGoods(category);
            _goodsList.Data = _goods;
        }

        private void BuildGoods(EShopCategory category)
        {
            _goods.Clear();

            var configs = _configService.Shop.DataList;
            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];
                if (config.Type != category || config.ItemId_Ref == null)
                {
                    continue;
                }

                _goods.Add(new ShopGoodsData(config, GetCategoryTag(category), GetAccentColor(i)));
            }
        }

        private static readonly Color[] AccentColors =
        {
            new(0.18f, 0.92f, 0.88f, 1f),
            new(0.58f, 0.8f, 0.55f, 1f),
            new(0.95f, 0.78f, 0.32f, 1f),
            new(0.7f, 0.55f, 1f, 1f)
        };

        private static Color GetAccentColor(int index) => AccentColors[index % AccentColors.Length];

        private static string GetCategoryTag(EShopCategory category)
        {
            return category switch
            {
                EShopCategory.MATERIAL => LocalizationKey.UI.SHOP_TAG_ITEM_Raw,
                EShopCategory.EQUIPMENT => LocalizationKey.UI.SHOP_TAG_SKIN_Raw,
                EShopCategory.CONSUMABLE => LocalizationKey.UI.SHOP_TAG_PACK_Raw,
                _ => LocalizationKey.UI.SHOP_TAG_RECOMMEND_Raw
            };
        }
    }
}
