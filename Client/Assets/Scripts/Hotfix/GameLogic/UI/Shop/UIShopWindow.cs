using System.Collections.Generic;
using System.Linq;
using AlicizaX;
using AlicizaX.Localization;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Cysharp.Threading.Tasks;
using Game.Config;
using Game.UI;
using GameLogic.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic.UI
{
    [Window(UILayer.UI, UIOcclusionMode.Lifecycle, 3)]
    public class UIShopWindow : UITabWindow<ui_UIShopWindow>
    {
        private readonly Dictionary<EShopCategory, List<ShopGoodsData>> _goodsByCategory = new();
        private UGList<ShopGoodsData> _goodsList;
        private UGList<ShopGoodsData> _goodsLinerList;
        private UGLoopList<ShopGoodsData> _goodsLoopList;
        private IFakePlayerDataService _playerDataService;
        private IConfigService _configService;

        protected override void OnInitialize()
        {
            _goodsList = UGListCreateHelper.Create<ShopGoodsData>(baseui.ScrollViewGoodsList);
            _goodsLinerList = UGListCreateHelper.Create<ShopGoodsData>(baseui.ScrollViewGoodsLinerList);
            _goodsLoopList = UGListCreateHelper.CreateLoop<ShopGoodsData>(baseui.ScrollViewGoodsLoopList);
            _playerDataService = AppServices.Require<IFakePlayerDataService>();
            _configService = AppServices.Require<IConfigService>();

            baseui.TogRecommend.onValueChanged.AddListener(OnTogRecommendChanged);
            baseui.TogItem.onValueChanged.AddListener(OnTogItemChanged);
            baseui.TogSkin.onValueChanged.AddListener(OnTogSkinChanged);
            baseui.TogPack.onValueChanged.AddListener(OnTogPackChanged);
            baseui.BtnClose.onClick.AddListener(OnBtnCloseClick);

            baseui.TogRecommend.isOn = true;
            RefreshCurrencyText();
            SwitchCategory(EShopCategory.TEST);
        }

        protected override async UniTask OnOpenAsync()
        {
            await UniTask.Delay(3000);
            RefreshCurrencyText();
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
            baseui.TextCurrency.SetLocalizationArgs(_playerDataService.Credit.ToStringNonAlloc());
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
            var datas = GetGoods(category);
            _goodsList.Data = datas.Take(16).ToList();
            _goodsLinerList.Data = datas;
            _goodsLoopList.Data = datas;
        }

        private List<ShopGoodsData> GetGoods(EShopCategory category)
        {
            if (_goodsByCategory.TryGetValue(category, out List<ShopGoodsData> goods))
            {
                return goods;
            }

            goods = BuildGoods(category);
            _goodsByCategory[category] = goods;
            return goods;
        }

        private List<ShopGoodsData> BuildGoods(EShopCategory category)
        {
            var configs = _configService.Shop.DataList;
            List<ShopGoodsData> goods = new(configs.Count);
            string tag = GetCategoryTag(category);
            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];
                if (config.Type != category || config.ItemId_Ref == null)
                {
                    continue;
                }

                goods.Add(new ShopGoodsData(config, tag, GetAccentColor(i)));
            }

            return goods;
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
                EShopCategory.MATERIAL => LocalizationKey.UI.SHOP_TAG_ITEM,
                EShopCategory.EQUIPMENT => LocalizationKey.UI.SHOP_TAG_SKIN,
                EShopCategory.CONSUMABLE => LocalizationKey.UI.SHOP_TAG_PACK,
                _ => LocalizationKey.UI.SHOP_TAG_RECOMMEND
            };
        }
    }
}
