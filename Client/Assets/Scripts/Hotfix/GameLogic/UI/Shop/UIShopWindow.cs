using System.Collections.Generic;
using AlicizaX;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Game.UI;
using GameLogic.Player;
using UnityEngine;

namespace GameLogic.UI
{
    [Window(UILayer.UI, false, 3)]
    public class UIShopWindow : UITabWindow<ui_UIShopWindow>
    {
        private readonly List<ShopGoodsData> _goods = new(12);
        private UGList<ShopGoodsData> _goodsList;
        private IFakePlayerDataService _playerDataService;
        private ShopCategory _category;

        protected override void OnInitialize()
        {
            _goodsList = UGListCreateHelper.Create<ShopGoodsData>(baseui.ScrollViewGoodsList);
            _goodsList.RegisterItemRender<ShopGoodsItemRender>();
            _playerDataService = AppServices.Require<IFakePlayerDataService>();

            baseui.TogRecommend.onValueChanged.AddListener(OnTogRecommendChanged);
            baseui.TogItem.onValueChanged.AddListener(OnTogItemChanged);
            baseui.TogSkin.onValueChanged.AddListener(OnTogSkinChanged);
            baseui.TogPack.onValueChanged.AddListener(OnTogPackChanged);
            baseui.BtnClose.onClick.AddListener(OnBtnCloseClick);

            baseui.TogRecommend.isOn = true;
            RefreshCurrencyText();
            SwitchCategory(ShopCategory.Recommend);
        }

        protected override void OnRegisterEvent(EventListenerProxy proxy)
        {
            proxy.AddUIEvent<PlayerDataChangedEvent>(OnPlayerDataChanged);
        }

        private void OnPlayerDataChanged(in PlayerDataChangedEvent evt)
        {
            RefreshCurrencyText();
        }

        private void RefreshCurrencyText()
        {
            baseui.TextCurrency.text = $"信用点：{_playerDataService.Credit}";
        }

        private void OnTogRecommendChanged(bool isOn)
        {
            if (isOn)
            {
                SwitchCategory(ShopCategory.Recommend);
            }
        }

        private void OnTogItemChanged(bool isOn)
        {
            if (isOn)
            {
                SwitchCategory(ShopCategory.Item);
            }
        }

        private void OnTogSkinChanged(bool isOn)
        {
            if (isOn)
            {
                SwitchCategory(ShopCategory.Skin);
            }
        }

        private void OnTogPackChanged(bool isOn)
        {
            if (isOn)
            {
                SwitchCategory(ShopCategory.Pack);
            }
        }

        private void OnBtnCloseClick()
        {
            CloseSelf();
        }

        private void SwitchCategory(ShopCategory category)
        {
            _category = category;
            BuildGoods(category);
            _goodsList.Data = _goods;
            baseui.TextInfoTitle.text = GetCategoryTitle(category);
            baseui.TextInfoContent.text = GetCategoryDescription(category);
        }

        private void BuildGoods(ShopCategory category)
        {
            _goods.Clear();

            switch (category)
            {
                case ShopCategory.Item:
                    AddGoods(2001, LocalizationKey.UI.SHOP_GOODS_ITEM_MEDKIT_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_ITEM_MEDKIT_DESCRIPTION_Raw, 320, LocalizationKey.UI.SHOP_TAG_ITEM, new Color(0.45f, 0.9f, 0.62f, 1f));
                    AddGoods(2002, LocalizationKey.UI.SHOP_GOODS_ITEM_ENERGYBATTERY_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_ITEM_ENERGYBATTERY_DESCRIPTION_Raw, 480, LocalizationKey.UI.SHOP_TAG_ITEM, new Color(0.25f, 0.75f, 1f, 1f));
                    AddGoods(2003, LocalizationKey.UI.SHOP_GOODS_ITEM_ACCESSKEY_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_ITEM_ACCESSKEY_DESCRIPTION_Raw, 760, LocalizationKey.UI.SHOP_TAG_ITEM, new Color(0.9f, 0.82f, 0.3f, 1f));
                    AddGoods(2004, LocalizationKey.UI.SHOP_GOODS_ITEM_SIGNALMARKER_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_ITEM_SIGNALMARKER_DESCRIPTION_Raw, 600, LocalizationKey.UI.SHOP_TAG_ITEM, new Color(0.95f, 0.55f, 0.35f, 1f));
                    break;
                case ShopCategory.Skin:
                    AddGoods(3001, LocalizationKey.UI.SHOP_GOODS_SKIN_NEONCOAT_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_SKIN_NEONCOAT_DESCRIPTION_Raw, 1800, LocalizationKey.UI.SHOP_TAG_SKIN, new Color(0.18f, 0.92f, 0.88f, 1f));
                    AddGoods(3002, LocalizationKey.UI.SHOP_GOODS_SKIN_ASHHELMET_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_SKIN_ASHHELMET_DESCRIPTION_Raw, 1500, LocalizationKey.UI.SHOP_TAG_SKIN, new Color(0.68f, 0.7f, 0.67f, 1f));
                    AddGoods(3003, LocalizationKey.UI.SHOP_GOODS_SKIN_GHOSTMASK_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_SKIN_GHOSTMASK_DESCRIPTION_Raw, 2400, LocalizationKey.UI.SHOP_TAG_SKIN, new Color(0.85f, 0.9f, 0.82f, 1f));
                    AddGoods(3004, LocalizationKey.UI.SHOP_GOODS_SKIN_WANDERERBADGE_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_SKIN_WANDERERBADGE_DESCRIPTION_Raw, 900, LocalizationKey.UI.SHOP_TAG_SKIN, new Color(0.95f, 0.72f, 0.32f, 1f));
                    break;
                case ShopCategory.Pack:
                    AddGoods(4001, LocalizationKey.UI.SHOP_GOODS_PACK_STARTERPACK_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_PACK_STARTERPACK_DESCRIPTION_Raw, 980, LocalizationKey.UI.SHOP_TAG_PACK, new Color(0.4f, 0.82f, 1f, 1f));
                    AddGoods(4002, LocalizationKey.UI.SHOP_GOODS_PACK_EXPLORATIONPACK_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_PACK_EXPLORATIONPACK_DESCRIPTION_Raw, 1680, LocalizationKey.UI.SHOP_TAG_PACK, new Color(0.48f, 0.9f, 0.58f, 1f));
                    AddGoods(4003, LocalizationKey.UI.SHOP_GOODS_PACK_OUTFITPACK_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_PACK_OUTFITPACK_DESCRIPTION_Raw, 3600, LocalizationKey.UI.SHOP_TAG_PACK, new Color(0.92f, 0.48f, 1f, 1f));
                    AddGoods(4004, LocalizationKey.UI.SHOP_GOODS_PACK_WEEKLYSUPPLY_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_PACK_WEEKLYSUPPLY_DESCRIPTION_Raw, 2200, LocalizationKey.UI.SHOP_TAG_PACK, new Color(0.95f, 0.75f, 0.34f, 1f));
                    break;
                default:
                    AddGoods(1001, LocalizationKey.UI.SHOP_GOODS_RECOMMEND_DAILYSUPPLY_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_RECOMMEND_DAILYSUPPLY_DESCRIPTION_Raw, 680, LocalizationKey.UI.SHOP_TAG_HOT, new Color(0.18f, 0.92f, 0.88f, 1f));
                    AddGoods(1002, LocalizationKey.UI.SHOP_GOODS_RECOMMEND_TACTICALMODULE_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_RECOMMEND_TACTICALMODULE_DESCRIPTION_Raw, 1280, LocalizationKey.UI.SHOP_TAG_NEW, new Color(0.58f, 0.8f, 0.55f, 1f));
                    AddGoods(1003, LocalizationKey.UI.SHOP_GOODS_RECOMMEND_NIGHTOUTFIT_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_RECOMMEND_NIGHTOUTFIT_DESCRIPTION_Raw, 2100, LocalizationKey.UI.SHOP_TAG_DISCOUNT, new Color(0.7f, 0.55f, 1f, 1f));
                    AddGoods(1004, LocalizationKey.UI.SHOP_GOODS_RECOMMEND_CREDITCRATE_NAME_Raw, LocalizationKey.UI.SHOP_GOODS_RECOMMEND_CREDITCRATE_DESCRIPTION_Raw, 300, LocalizationKey.UI.SHOP_TAG_RECOMMEND, new Color(0.95f, 0.78f, 0.32f, 1f));
                    break;
            }

            for (int i = 0; i < 800; i++)
            {
                int baseId = (int)category * 1000 + 50 + i;
                int displayIndex = i + 1;
                AddGoods(baseId, LocalizationKey.UI.SHOP_GOODS_TEST_NAME(displayIndex.ToString("00")), LocalizationKey.UI.SHOP_GOODS_TEST_DESCRIPTION_Raw, 520 + i * 130, GetCategoryTag(category), GetAccentColor(i));
            }
        }

        private void AddGoods(int id, string name, string description, int price, string tag, Color color)
        {
            _goods.Add(new ShopGoodsData
            {
                Id = id,
                NameKey = name,
                DescriptionKey = description,
                Price = price,
                TagKey = tag,
                AccentColor = color,
            });
        }

        private static readonly Color[] AccentColors =
        {
            new(0.18f, 0.92f, 0.88f, 1f),
            new(0.58f, 0.8f, 0.55f, 1f),
            new(0.95f, 0.78f, 0.32f, 1f),
            new(0.7f, 0.55f, 1f, 1f)
        };

        private static Color GetAccentColor(int index) => AccentColors[index % AccentColors.Length];

        private static string GetCategoryTag(ShopCategory category)
        {
            return category switch
            {
                ShopCategory.Item => LocalizationKey.UI.SHOP_TAG_ITEM,
                ShopCategory.Skin => LocalizationKey.UI.SHOP_TAG_SKIN,
                ShopCategory.Pack => LocalizationKey.UI.SHOP_TAG_PACK,
                _ => LocalizationKey.UI.SHOP_TAG_HOT
            };
        }

        private static string GetCategoryTitle(ShopCategory category)
        {
            return category switch
            {
                ShopCategory.Item => LocalizationKey.UI.SHOP_CATEGORY_ITEM_TITLE,
                ShopCategory.Skin => LocalizationKey.UI.SHOP_CATEGORY_SKIN_TITLE,
                ShopCategory.Pack => LocalizationKey.UI.SHOP_CATEGORY_PACK_TITLE,
                _ => LocalizationKey.UI.SHOP_CATEGORY_RECOMMEND_TITLE
            };
        }

        private static string GetCategoryDescription(ShopCategory category)
        {
            return category switch
            {
                ShopCategory.Item => LocalizationKey.UI.SHOP_CATEGORY_ITEM_DESCRIPTION_Raw,
                ShopCategory.Skin => LocalizationKey.UI.SHOP_CATEGORY_SKIN_DESCRIPTION_Raw,
                ShopCategory.Pack => LocalizationKey.UI.SHOP_CATEGORY_PACK_DESCRIPTION_Raw,
                _ => LocalizationKey.UI.SHOP_CATEGORY_RECOMMEND_DESCRIPTION_Raw
            };
        }

        private enum ShopCategory
        {
            Recommend = 1,
            Item = 2,
            Skin = 3,
            Pack = 4
        }
    }
}
