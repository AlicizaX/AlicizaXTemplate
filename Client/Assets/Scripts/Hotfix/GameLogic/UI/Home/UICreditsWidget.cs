using System;
using AlicizaX.UI.Runtime;
using Game.UI;
using PrimeTween;
using UnityEngine;

namespace GameLogic.UI
{
    public sealed class UICreditsWidget : UIWidget<ui_UICreditsWidget>
    {
        private const float ScrollSpeed = 52f;
        private const float StartOffset = -136f;

        private RectTransform _roll;
        private RectTransform _viewport;
        private Tween _scrollTween;

        public event Action Closed;

        protected override void OnInitialize()
        {
            _roll = baseui.TextRoll.rectTransform;
            _viewport = (RectTransform)_roll.parent;
            baseui.BtnBack.onClick.AddListener(Close);
        }

        protected override void OnOpen()
        {
            _roll.anchoredPosition = new Vector2(0f, StartOffset);
            baseui.TextRoll.RegisterDirtyLayoutCallback(Scroll);
            Scroll();
        }

        private void Scroll()
        {
            _scrollTween.Stop();
            float startY = _roll.anchoredPosition.y;
            float endY = baseui.TextRoll.preferredHeight;
            if (startY >= endY)
            {
                startY = -_viewport.rect.height;
            }

            _scrollTween = Tween.UIAnchoredPositionY(_roll, startY, endY, (endY - startY) / ScrollSpeed,
                Ease.Linear, useUnscaledTime: true).OnComplete(this, static widget => widget.Scroll());
        }

        protected override void OnClose()
        {
            baseui.TextRoll.UnregisterDirtyLayoutCallback(Scroll);
            _scrollTween.Stop();
            Closed?.Invoke();
        }
    }
}
