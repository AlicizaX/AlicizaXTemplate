using System;
using AlicizaX;
using AlicizaX.Input;
using AlicizaX.Localization.Runtime;
using AlicizaX.UI.Runtime;
using Cysharp.Threading.Tasks;
using Game.UI;
using UnityEngine;
using UnityEngine.Video;

namespace GameLogic.UI
{
    /// <summary>
    /// 主界面窗口。按参考稿重建：动态海雾背景 + 标题 Logo + 纵向菜单 + 右上状态区。
    /// </summary>
    [Window(UILayer.UI, 30)]
    public class UIHomeWindow : UIWindow<ui_UIHomeWindow>
    {
        /// <summary>构建号，正式版本请改为打包时注入的真实构建号。</summary>
        private const int BuildNumber = 34791;

        private VideoPlayer _backgroundVideo;
        private Texture _backgroundTexture;
        private Material _backgroundMaterial;
        private UICreditsWidget _credits;

        /// <summary>右上角显示的用户名，业务层可在打开窗口前覆盖。</summary>
        public string ActiveUserName { get; set; } = Environment.UserName;

        protected override void OnInitialize()
        {
            var background = baseui.RImgAtmosphere;
            _backgroundVideo = background.GetComponent<VideoPlayer>();
            _backgroundTexture = background.texture;
            _backgroundMaterial = background.material;

            baseui.BtnStartNewGame.onClick.AddListener(() => GameApp.UI.ShowUI<UIRecyclerViewTestHubWindow>().Forget());
            baseui.BtnQuit.onClick.AddListener(() => Log.Info("[UIHomeWindow] menu selected: Quit"));
            baseui.BtnShop.onClick.AddListener(() => GameApp.UI.ShowUI<UIShopWindow>().Forget());
            baseui.BtnCredits.onClick.AddListener(OpenCredits);
            baseui.BtnOptions.onClick.AddListener(OpenOptions);
        }

        protected override void OnOpen()
        {
            LocalizationKit.Changed += OnLanguageChanged;
            RefreshStatus();

            if (_backgroundVideo != null)
            {
                _backgroundVideo.frameReady += OnBackgroundFirstFrame;
                _backgroundVideo.sendFrameReadyEvents = true;
                _backgroundVideo.Play();
            }
        }

        protected override void OnClose()
        {
            LocalizationKit.Changed -= OnLanguageChanged;
            _credits?.Close();

            if (_backgroundVideo != null)
            {
                _backgroundVideo.frameReady -= OnBackgroundFirstFrame;
                _backgroundVideo.sendFrameReadyEvents = false;
                baseui.RImgAtmosphere.texture = _backgroundTexture;
                baseui.RImgAtmosphere.material = _backgroundMaterial;
                _backgroundVideo.Stop();
            }
        }

        private void OnBackgroundFirstFrame(VideoPlayer player, long frame)
        {
            player.frameReady -= OnBackgroundFirstFrame;
            player.sendFrameReadyEvents = false;
            baseui.RImgAtmosphere.texture = player.texture;
            baseui.RImgAtmosphere.material = null;
        }

        protected override void OnDestroy()
        {
            LocalizationKit.Changed -= OnLanguageChanged;
        }

        private void OpenCredits()
        {
            if (_credits == null)
            {
                _credits = CreateWidgetSync<UICreditsWidget>(baseui.transform, visible: false);
                if (_credits == null) return;
                _credits.Closed += RestoreHomeContent;
            }

            baseui.RectContent.gameObject.SetActive(false);
            _credits.Open();
        }

        private void OpenOptions() => GameApp.UI.ShowUI<UIOptionsWindow>().Forget();

        private void RestoreHomeContent()
        {
            baseui.RectContent.gameObject.SetActive(true);
            if (State == UIState.Opened && UXInput.Watch?.IsNavigationInput == true)
            {
                baseui.BtnCredits.Select();
            }
        }

        // ------------------------------------------------------------------ status
        private void OnLanguageChanged(string language) => RefreshStatus();

        private void RefreshStatus()
        {
            baseui.TextVersion.SetText(LocalizationKey.UI.HOME_STATUS_VERSION(Application.version, BuildNumber));
            baseui.TextActiveUser.SetText(LocalizationKey.UI.HOME_STATUS_ACTIVEUSER(ActiveUserName));
        }

    }
}
