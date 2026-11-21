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
    /// <summary>
    /// 启动界面：只订阅启动事件做展示，玩家按任意键后发布 <see cref="LaunchStartRequestedEvent"/>。
    /// </summary>
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
            EventProxy.AddUIEvent<LaunchStateChangedEvent>(OnLaunchStateChanged);
            EventProxy.AddUIEvent<LaunchDownloadProgressEvent>(OnDownloadProgress);
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

        private void OnLaunchStateChanged(in LaunchStateChangedEvent e)
        {
            switch (e.State)
            {
                case LaunchState.Checking:
                    SetStatus(UpdateLocalizationKey.Bootstrap.UPDATE_CHECKING);
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

        private void OnDownloadProgress(in LaunchDownloadProgressEvent e)
        {
            float progress = e.Progress;
            baseui.ImgFill.fillAmount = progress;
            SetStatus(UpdateLocalizationKey.Bootstrap.UPDATE_DOWNLOADING(e.CurrentCount, e.TotalCount, progress * 100f));
            if (e.TotalBytes > 0)
            {
                baseui.TextDetail.text = UpdateLocalizationKey.Bootstrap.UPDATE_SIZE(
                    (e.CurrentBytes / BytesPerMegabyte).ToString("0.0"),
                    (e.TotalBytes / BytesPerMegabyte).ToString("0.0"));
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
    }
}
