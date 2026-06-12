using AlicizaX;
using AlicizaX.UI.Runtime;
using Cysharp.Threading.Tasks;
using Game.UI;
using UnityEngine;

namespace GameLogic.UI
{
    [Window(UILayer.Tips, UIOcclusionMode.None, 3)]
    public sealed class UITipsWindow : UITabWindow<ui_UITipsWindow>, IUIAsyncInitialize
    {
        private UITextTipsWidget _textTip;
        private UIIconTipsWidget _iconTip;

        async UniTask IUIAsyncInitialize.OnInitializeAsync()
        {
            _textTip = await CreateWidgetAsync<UITextTipsWidget>(baseui.RectTextTipsRoot, false);
            _iconTip = await CreateWidgetAsync<UIIconTipsWidget>(baseui.RectIconTipsRoot, false);
        }

        public static void ShowText(string content, float duration = 1.6f)
        {
            ShowTextAsync(content, duration).Forget();
        }

        public static void ShowLocalizedText(string key, float duration = 1.6f)
        {
            ShowText(GameApp.Localization.GetString(key), duration);
        }

        public static void ShowIcon(string content, Sprite icon, float duration = 1.6f)
        {
            ShowIconAsync(content, icon, duration).Forget();
        }

        public static void ShowLocalizedIcon(string key, Sprite icon, float duration = 1.6f)
        {
            ShowIcon(GameApp.Localization.GetString(key), icon, duration);
        }

        private void ShowTextTip(string content, float duration)
        {
            _iconTip.Close();
            _textTip.Show(content, duration).Forget();
        }

        private void ShowIconTip(string content, Sprite icon, float duration)
        {
            _textTip.Close();
            _iconTip.Show(content, icon, duration).Forget();
        }

        private static async UniTaskVoid ShowTextAsync(string content, float duration)
        {
            UITipsWindow window = await GameApp.UI.ShowUI<UITipsWindow>();
            window?.ShowTextTip(content, duration);
        }

        private static async UniTaskVoid ShowIconAsync(string content, Sprite icon, float duration)
        {
            UITipsWindow window = await GameApp.UI.ShowUI<UITipsWindow>();
            window?.ShowIconTip(content, icon, duration);
        }
    }
}
