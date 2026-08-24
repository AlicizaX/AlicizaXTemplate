using AlicizaX.UI;
using UnityEngine;

namespace GameLogic.UI
{
    public sealed class RvTextData : ISimpleViewData
    {
        public string Title;
        public Color Color;
    }

    public sealed class RvChatData : IMeasuredViewData
    {
        public string Text;
        public bool IsSelf;
        public float DeclaredLength;

        public bool TryGetItemLength(out float length)
        {
            length = DeclaredLength;
            return DeclaredLength > 0f;
        }
    }

    public sealed class RvMixedData : IMixedViewData
    {
        public int TemplateId { get; set; }
        public string Text;
    }

    public sealed class RvGroupData : IGroupViewData
    {
        public int TemplateId { get; set; }
        public bool Expanded { get; set; }
        public int Type { get; set; }
        public string Text;
    }
}
