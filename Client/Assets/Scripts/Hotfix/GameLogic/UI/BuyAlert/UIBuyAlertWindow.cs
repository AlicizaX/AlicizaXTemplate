using AlicizaX;
using AlicizaX.Localization.Runtime;
using AlicizaX.Resource.Runtime;
using AlicizaX.UI.Runtime;
using Game.UI;
using GameLogic.Player;
using UnityEngine;

namespace GameLogic.UI
{
    [Window(UILayer.Popup, -1)]
    public sealed class UIBuyAlertWindow : UIWindow<ui_UIBuyAlertWindow>
    {
        private IFakePlayerDataService _player;
        private ShopGoodsData _item;
        private int _quantity;

        protected override void OnInitialize()
        {
            _player = AppServices.Require<IFakePlayerDataService>();
            baseui.BtnClose.onClick.AddListener(() => CloseSelf());
            baseui.BtnConfirm.onClick.AddListener(Buy);
            baseui.BtnMinus.onClick.AddListener(() => SetQuantity(_quantity - 1));
            baseui.BtnPlus.onClick.AddListener(() => SetQuantity(_quantity + 1));
            baseui.InputQuantity.onValueChanged.AddListener(OnQuantityEdited);
            baseui.InputQuantity.onEndEdit.AddListener(_ => SetQuantity(_quantity));
            EventProxy.AddUIEvent<PlayerDataChangedEvent>(OnPlayerDataChanged);
        }

        protected override void OnOpen()
        {
            _item = UserData as ShopGoodsData;
            baseui.ImgItemIcon.enabled = _item != null;
            if (_item != null) baseui.ImgItemIcon.SetSprite(Res, _item.Icon);
            LocalizationKit.Changed += OnLanguageChanged;
            _quantity = 1;
            baseui.InputQuantity.SetTextWithoutNotify("1");
            OnLanguageChanged(null);
        }

        protected override void OnClose()
        {
            LocalizationKit.Changed -= OnLanguageChanged;
            _item = null;
        }

        private void OnLanguageChanged(string language)
        {
            baseui.TextTitle.SetText(LocalizationKey.UI.BUY_TITLE);
            baseui.TextItemName.SetText(_item?.Name ?? LocalizationKey.UI.BUY_UNKNOWNITEM);
            RefreshPrice();
        }

        private void OnQuantityEdited(string value)
        {
            if (int.TryParse(value, out int quantity)) SetQuantity(quantity);
        }

        private void SetQuantity(int quantity)
        {
            _quantity = Mathf.Clamp(quantity, 1, 999);
            baseui.InputQuantity.SetTextWithoutNotify(_quantity.ToString());
            RefreshPrice();
        }

        private void RefreshPrice()
        {
            int total = (_item?.Price ?? 0) * _quantity;
            bool affordable = total <= _player.Credit;
            baseui.BtnMinus.interactable = _quantity > 1;
            baseui.BtnPlus.interactable = _quantity < 999;
            baseui.TextTotalPrice.SetText(LocalizationKey.UI.BUY_TOTALPRICE(LocalizationKey.UI.COMMON_CREDITPRICE(total)));
            baseui.TextConfirmLabel.SetText(affordable ? LocalizationKey.UI.BUY_BUTTONCONFIRM : LocalizationKey.UI.BUY_SHORTFUNDS);
            baseui.BtnConfirm.interactable = _item != null && affordable;
        }

        private void OnPlayerDataChanged(in PlayerDataChangedEvent evt)
        {
            if (_item != null) RefreshPrice();
        }

        private void Buy()
        {
            if (_player.TryBuy(_item, _quantity, out int total))
            {
                Log.Info(LocalizationKey.Log.BUY_CONFIRMED(_item.Name, _quantity, total));
                CloseSelf();
            }
            else RefreshPrice();
        }
    }
}
