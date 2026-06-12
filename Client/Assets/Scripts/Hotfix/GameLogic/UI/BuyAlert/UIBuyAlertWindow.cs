using AlicizaX;
using AlicizaX.UI.Runtime;
using Game.UI;
using GameLogic.Player;
using TMPro;
using UnityEngine;

namespace GameLogic.UI
{
    [Window(UILayer.Popup, UIOcclusionMode.None, 3)]
    public class UIBuyAlertWindow : UITabWindow<ui_UIBuyAlertWindow>, IUISyncInitialize
    {
        private ShopGoodsData _goodsData;
        private IFakePlayerDataService _playerDataService;
        private int _unitPrice;
        private int _quantity = 1;

        void IUISyncInitialize.OnInitialize()
        {
            baseui.BtnClose.onClick.AddListener(OnBtnCloseClick);
            baseui.BtnConfirm.onClick.AddListener(OnBtnConfirmClick);
            baseui.BtnMinus.onClick.AddListener(OnBtnMinusClick);
            baseui.BtnPlus.onClick.AddListener(OnBtnPlusClick);
            baseui.InputQuantity.onValueChanged.AddListener(OnQuantityInputChanged);
            _playerDataService = AppServices.Require<IFakePlayerDataService>();
        }

        protected override void OnOpen()
        {
            _goodsData = UserData as ShopGoodsData;
            _unitPrice = _goodsData?.Price ?? 0;
            _quantity = 1;
            RefreshView();
        }

        private void OnBtnCloseClick()
        {
            CloseSelf();
        }

        private void OnBtnConfirmClick()
        {
            if (_goodsData == null)
            {
                CloseSelf();
                return;
            }

            if (_playerDataService.TryBuy(_goodsData, _quantity, out int totalPrice))
            {
                Log.Info(LocalizationKey.Log.BUY_CONFIRMED(_goodsData.Name, _quantity.ToString(), totalPrice.ToString()));
                CloseSelf();
                return;
            }

            Log.Info(LocalizationKey.Log.BUY_INSUFFICIENT(_goodsData.Name, _quantity.ToString()));
        }

        private void OnBtnMinusClick()
        {
            SetQuantity(_quantity - 1);
        }

        private void OnBtnPlusClick()
        {
            SetQuantity(_quantity + 1);
        }

        private void OnQuantityInputChanged(string value)
        {
            if (!int.TryParse(value, out int quantity))
            {
                return;
            }

            SetQuantity(quantity, false);
        }

        private void SetQuantity(int quantity, bool syncInput = true)
        {
            int clamped = Mathf.Clamp(quantity, 1, 999);
            if (_quantity == clamped && (!syncInput || baseui.InputQuantity.text == clamped.ToString()))
            {
                return;
            }

            _quantity = clamped;
            RefreshTotalPrice();

            if (syncInput)
            {
                baseui.InputQuantity.SetTextWithoutNotify(_quantity.ToString());
            }
        }

        private void RefreshView()
        {
            baseui.TextTitle.text = LocalizationKey.UI.BUY_TITLE;
            baseui.TextItemName.text = _goodsData != null ? _goodsData.Name : LocalizationKey.UI.BUY_UNKNOWNITEM;
            if (_goodsData != null)
            {
                baseui.ImgItemIcon.SetSprite(_goodsData.Icon);
                baseui.ImgItemIcon.color = _goodsData.AccentColor;
            }
            else
            {
                baseui.ImgItemIcon.color = Color.white;
            }
            baseui.InputQuantity.contentType = TMP_InputField.ContentType.IntegerNumber;
            baseui.InputQuantity.SetTextWithoutNotify(_quantity.ToString());
            RefreshTotalPrice();
        }

        private void RefreshTotalPrice()
        {
            string totalPrice = LocalizationKey.UI.COMMON_CREDITPRICE((_unitPrice * _quantity).ToStringNonAlloc());
            baseui.TextTotalPrice.text = LocalizationKey.UI.BUY_TOTALPRICE(totalPrice);
        }
    }
}
