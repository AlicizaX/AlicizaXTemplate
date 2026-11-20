using System;
using AlicizaX;
using UnityEngine;

namespace GameLogic.UI
{
    public sealed class AlertData : MemoryObject
    {
        public string Title;
        public string Content;
        public Action OnConfirm;
        public Action OnCancel;
        public string ConfirmLabel;
        public bool ShowCancel = true;
        internal RenderTexture Backdrop;

        public override void Clear()
        {
            Title = Content = ConfirmLabel = null;
            OnConfirm = OnCancel = null;
            ShowCancel = true;
            if (Backdrop != null) RenderTexture.ReleaseTemporary(Backdrop);
            Backdrop = null;
        }
    }
}
