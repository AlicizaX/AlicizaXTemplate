using System;
using AlicizaX;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameLogic.UI
{
    public static class TipUtils
    {
        public static void ShowMessageBox(string title, string content = null, Action onConfirm = null,
            Action onCancel = null, string confirmLabel = null)
        {
            ShowAsync(title, content, onConfirm, onCancel, confirmLabel, true).Forget();
        }

        public static void ShowMessageBoxOnlyConfirm(string title, string content = null, Action onConfirm = null)
        {
            ShowAsync(title, content, onConfirm, null, null, false).Forget();
        }

        private static async UniTask ShowAsync(string title, string content, Action onConfirm, Action onCancel,
            string confirmLabel, bool showCancel)
        {
            var data = MemoryPool.Acquire<AlertData>();
            data.Title = title;
            data.Content = content;
            data.OnConfirm = onConfirm;
            data.OnCancel = onCancel;
            data.ConfirmLabel = confirmLabel;
            data.ShowCancel = showCancel;

            if (!GameApp.UI.IsOpen<UIAlertWindow>())
            {
                // ScreenCapture 需要在底层界面渲染完成后取图。
                await UniTask.WaitForEndOfFrame(GameApp.Base);
                var descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGB32, 0)
                {
                    useMipMap = true,
                    autoGenerateMips = false
                };
                data.Backdrop = RenderTexture.GetTemporary(descriptor);
                data.Backdrop.filterMode = FilterMode.Trilinear;
                data.Backdrop.wrapMode = TextureWrapMode.Clamp;
                ScreenCapture.CaptureScreenshotIntoRenderTexture(data.Backdrop);
                data.Backdrop.GenerateMips();
            }

            if (await GameApp.UI.ShowUI<UIAlertWindow>(data) == null) MemoryPool.Release(data);
        }
    }
}
