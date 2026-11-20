using System.Collections.Generic;
using System.Linq;
using AlicizaX;
using AlicizaX.Input;
using AlicizaX.Localization.Runtime;
using AlicizaX.Resource.Runtime;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Cysharp.Threading.Tasks;
using Game.Config;
using Game.UI;
using GameLogic.Player;

namespace GameLogic.UI
{
    [Window(UILayer.UI, 3)]
    public sealed class UIShopWindow : UIWindow<ui_UIShopWindow>
    {
        private List<ShopGoodsData> _catalog;
        private UGList<ShopGoodsData> _list;
        private RecyclerViewNavigationController _navigation;
        private IFakePlayerDataService _player;
        private bool _oldKeyboardSelection;

        private ShopGoodsData Selected => _list.HasChoice ? _list.GetItem(_list.ChoiceIndex) : null;

        protected override void OnInitialize()
        {
            _player = AppServices.Require<IFakePlayerDataService>();
            _catalog = BuildCatalog(AppServices.Require<IConfigService>());
            _list = UGListCreateHelper.Create<ShopGoodsData>(baseui.ScrollViewGoodsList);
            _navigation = baseui.ScrollViewGoodsList.GetComponent<RecyclerViewNavigationController>();
            _list.OnChoiceIndexChanged += ShowItem;
            baseui.TogAll.onValueChanged.AddListener(on => { if (on) ShowCategory(null); });
            baseui.TogEquipment.onValueChanged.AddListener(on => { if (on) ShowCategory(EShopCategory.EQUIPMENT); });
            baseui.TogMaterials.onValueChanged.AddListener(on => { if (on) ShowCategory(EShopCategory.MATERIAL); });
            baseui.TogConsumables.onValueChanged.AddListener(on => { if (on) ShowCategory(EShopCategory.CONSUMABLE); });
            baseui.BtnPreviousTab.onClick.AddListener(baseui.TogAll.group.Previous);
            baseui.BtnNextTab.onClick.AddListener(baseui.TogAll.group.Next);
            baseui.BtnPurchase.onClick.AddListener(() => GameApp.UI.ShowUI<UIBuyAlertWindow>(Selected).Forget());
            baseui.BtnClose.onClick.AddListener(() => CloseSelf());
            EventProxy.AddUIEvent<PlayerDataChangedEvent>(OnPlayerDataChanged);
        }

        protected override void OnOpen()
        {
            LocalizationKit.Changed += OnLanguageChanged;
            _oldKeyboardSelection = UXInput.Navigation.KeyboardRequireSelection;
            UXInput.Navigation.KeyboardRequireSelection = true;
            baseui.TogAll.SetIsOnWithoutNotify(true);
            ShowCategory(null);
            RefreshBalance();
        }

        protected override void OnClose()
        {
            LocalizationKit.Changed -= OnLanguageChanged;
            UXInput.Navigation.KeyboardRequireSelection = _oldKeyboardSelection;
        }

        private static List<ShopGoodsData> BuildCatalog(IConfigService config)
        {
            var seen = new HashSet<(EShopCategory, int)>();
            var goods = new List<ShopGoodsData>();
            foreach (var offer in config.Shop.DataList)
            {
                if (offer.Type != EShopCategory.TEST && offer.ItemId_Ref != null && seen.Add((offer.Type, offer.ItemId)))
                    goods.Add(new ShopGoodsData(offer));
            }
            return goods.OrderByDescending(item => item.Price).ThenBy(item => item.ItemId).ToList();
        }

        private void ShowCategory(EShopCategory? category)
        {
            var goods = category.HasValue ? _catalog.FindAll(item => item.Category == category.Value) : _catalog;
            _list.SetData(goods, ListScrollPolicy.ResetScroll);
            baseui.TextCount.SetText(LocalizationKey.UI.SHOP_COUNT(goods.Count));
            bool hasItems = goods.Count > 0;
            baseui.RectDetails.gameObject.SetActive(hasItems);
            baseui.TextEmpty.gameObject.SetActive(!hasItems);
            if (!hasItems) return;

            if (_list.ChoiceIndex == 0) ShowItem(0);
            else _list.ChoiceIndex = 0;
            _navigation.RestoreFocus(0);
        }

        private void ShowItem(int index)
        {
            if (index < 0 || index >= _list.DataCount) return;
            var item = _list.GetItem(index);
            baseui.ImgPreview.SetSprite(Res, item.Icon);
            baseui.TextItemName.SetText(item.Name);
            baseui.TextDescription.SetText(item.Description);
            baseui.TextPrice.SetText(LocalizationKey.UI.COMMON_CREDITPRICE(item.Price));
            baseui.TextCategory.SetText(item.Category switch
            {
                EShopCategory.EQUIPMENT => LocalizationKey.UI.SHOP_TABSKIN,
                EShopCategory.MATERIAL => LocalizationKey.UI.SHOP_TABITEM,
                _ => LocalizationKey.UI.SHOP_TABPACK
            });
            RefreshOwned(item.ItemId);
        }

        private void RefreshOwned(int itemId)
        {
            var bag = _player.BagItems;
            int count = 0;
            for (int i = 0; i < bag.Count; i++)
            {
                if (bag[i].ItemId != itemId) continue;
                count = bag[i].Count;
                break;
            }
            baseui.TextOwned.SetText(LocalizationKey.UI.SHOP_OWNED(count));
        }

        private void RefreshBalance() => baseui.TextCurrency.SetText(_player.Credit.ToString("N0"));

        private void OnPlayerDataChanged(in PlayerDataChangedEvent evt)
        {
            RefreshBalance();
            if (Selected?.ItemId == evt.ChangedItemId) RefreshOwned(evt.ChangedItemId);
        }

        private void OnLanguageChanged(string language)
        {
            baseui.TextCount.SetText(LocalizationKey.UI.SHOP_COUNT(_list.DataCount));
            if (_list.TryGetVisibleRange(out int first, out int last))
            {
                for (int i = first; i <= last; i++) _list.RefreshItem(i);
            }
            ShowItem(_list.ChoiceIndex);
        }
    }
}
