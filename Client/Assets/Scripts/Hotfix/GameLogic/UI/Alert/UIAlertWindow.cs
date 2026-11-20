using AlicizaX;
using AlicizaX.UI.Runtime;
using Game.UI;

namespace GameLogic.UI
{
    [Window(UILayer.Popup, 30)]
    public sealed class UIAlertWindow : UIWindow<ui_UIAlertWindow>
    {
        private AlertData _data;

        protected override void OnInitialize()
        {
            baseui.BtnConfirm.onClick.AddListener(() => Submit(true));
            baseui.BtnCancel.onClick.AddListener(() => Submit(false));
        }

        protected override void OnOpen() => OnRefresh();

        protected override void OnRefresh()
        {
            var data = (AlertData)UserData;
            if (_data != null && _data != data)
            {
                // 刷新文案时沿用打开窗口前的背景。
                if (data.Backdrop == null)
                {
                    data.Backdrop = _data.Backdrop;
                    _data.Backdrop = null;
                }
                MemoryPool.Release(_data);
            }
            _data = data;
            baseui.RImgBackdrop.texture = data.Backdrop;
            baseui.RImgBackdrop.enabled = data.Backdrop != null;
            baseui.TextTitle.SetText(data.Title);
            baseui.TextDescription.SetText(data.Content);
            baseui.TextDescription.gameObject.SetActive(!string.IsNullOrEmpty(data.Content));
            baseui.TextConfirmLabel.SetText(data.ConfirmLabel ?? LocalizationKey.UI.ALERT_CONFIRM);
            baseui.TextCancelLabel.SetText(LocalizationKey.UI.ALERT_CANCEL);
            baseui.BtnCancel.gameObject.SetActive(data.ShowCancel);
        }

        private void Submit(bool confirm)
        {
            var callback = confirm ? _data.OnConfirm : _data.OnCancel;
            CloseSelf();
            callback?.Invoke();
        }

        protected override void OnClose()
        {
            baseui.RImgBackdrop.texture = null;
            MemoryPool.Release(_data);
            _data = null;
        }
    }
}
