using AlicizaX;
using AlicizaX.UI.Runtime;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Unity.Startup.Procedure
{
    [Window(UILayer.Top)]
    public sealed class UILaunchWindow : UIWindow<ui_UILaunchWindow>
    {
        private const double BytesPerMegabyte = 1024 * 1024;
        private const float PromptPulsePeriod = 2.2f;
        private const float MinPromptAlpha = 0.35f;
        private TMP_Text _startPrompt;
        private float _promptShownAt;
        private bool _startPressed;

        protected override void OnInitialize()
        {
            _startPrompt = baseui.ObjStartPrompt.GetComponent<TMP_Text>();
            EventProxy.AddUIEvent<AssetDownloadProgressUpdateEventArgs>(OnDownloadProgress);
            EventProxy.AddUIEvent<LaunchStateChangedEvent>(OnLaunchStateChanged);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_startPressed)
            {
                _startPressed = false;
                CloseSelf();
                EventBus.Publish<LaunchStartRequestedEvent>();
                return;
            }

            if (!_startPrompt.isActiveAndEnabled) return;

            float phase = (Time.unscaledTime - _promptShownAt) * (2f * Mathf.PI / PromptPulsePeriod);
            float brightness = (Mathf.Cos(phase) + 1f) * 0.5f;
            _startPrompt.alpha = Mathf.Lerp(MinPromptAlpha, 1f, brightness);
        }

        protected override void OnClose() => InputSystem.onEvent -= OnInputEvent;

        protected override void OnDestroy() => InputSystem.onEvent -= OnInputEvent;

        private void SetStatus(string status) => baseui.TextStatus.text = status;

        private void OnLaunchStateChanged(in LaunchStateChangedEvent update)
        {
            switch (update.State)
            {
                case LaunchState.Checking:
                    SetStatus(UpdateLocalizationKey.Bootstrap.UPDATE_CHECKING);
                    break;
                case LaunchState.NetworkUnavailable:
                    SetStatus(UpdateLocalizationKey.Bootstrap.UPDATE_NETWORKERROR);
                    break;
                case LaunchState.Ready:
                    baseui.ObjUpdate.SetActive(false);
                    _promptShownAt = Time.unscaledTime;
                    _startPrompt.alpha = 1f;
                    baseui.ObjStartPrompt.SetActive(true);
                    InputSystem.onEvent += OnInputEvent;
                    break;
            }
        }

        private void OnInputEvent(InputEventPtr inputEvent, InputDevice device)
        {
            if (device is not (Mouse or Keyboard or Gamepad) ||
                (inputEvent.type != StateEvent.Type && inputEvent.type != DeltaStateEvent.Type)) return;

            foreach (var control in inputEvent.EnumerateChangedControls(device))
            {
                if (control is not ButtonControl button || button.isPressed ||
                    !button.ReadValueFromEvent(inputEvent, out float value) || !button.IsValueConsideredPressed(value)) continue;

                InputSystem.onEvent -= OnInputEvent;
                _startPressed = true;
                return;
            }
        }

        private void OnDownloadProgress(in AssetDownloadProgressUpdateEventArgs update)
        {
            float progress = update.TotalDownloadSizeBytes > 0
                ? (float)((double)update.CurrentDownloadSizeBytes / update.TotalDownloadSizeBytes)
                : update.CurrentDownloadCount / (float)Mathf.Max(1, update.TotalDownloadCount);

            progress = Mathf.Clamp01(progress);
            SetProgress(progress);
            SetStatus(UpdateLocalizationKey.Bootstrap.UPDATE_DOWNLOADING(
                update.CurrentDownloadCount, update.TotalDownloadCount, progress * 100f));
            if (update.TotalDownloadSizeBytes > 0)
            {
                baseui.TextDetail.text = UpdateLocalizationKey.Bootstrap.UPDATE_SIZE(
                    (update.CurrentDownloadSizeBytes / BytesPerMegabyte).ToString("0.0"),
                    (update.TotalDownloadSizeBytes / BytesPerMegabyte).ToString("0.0"));
            }
        }

        private void SetProgress(float progress)
        {
            baseui.ImgFill.fillAmount = progress;
        }
    }
}
