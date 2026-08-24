using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class RvChatItemViewHolder : ViewHolder<RvChatData>
    {
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI message;

        protected override void OnBind(RvChatData data, int index)
        {
            const float horizontalPad = 28f;
            const float verticalPad = 24f;
            const float maxBubbleWidth = 520f;

            if (message != null)
            {
                message.text = data.Text;
                message.alignment = data.IsSelf
                    ? TextAlignmentOptions.TopRight
                    : TextAlignmentOptions.TopLeft;
                message.enableWordWrapping = true;
                message.overflowMode = TextOverflowModes.Overflow;
                message.ForceMeshUpdate();
            }

            float textWidth = message != null ? Mathf.Min(message.preferredWidth, maxBubbleWidth - horizontalPad) : 0f;
            if (message != null)
            {
                message.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
                message.ForceMeshUpdate();
            }

            float textHeight = message != null ? Mathf.Max(message.preferredHeight, 28f) : 28f;
            float bubbleWidth = Mathf.Clamp(textWidth + horizontalPad, 120f, maxBubbleWidth);
            float bubbleHeight = textHeight + verticalPad;

            if (background != null)
            {
                RectTransform bubble = background.rectTransform;
                bubble.anchorMin = data.IsSelf ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);
                bubble.anchorMax = bubble.anchorMin;
                bubble.pivot = data.IsSelf ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);
                bubble.sizeDelta = new Vector2(bubbleWidth, bubbleHeight);
                bubble.anchoredPosition = data.IsSelf ? new Vector2(-16f, 0f) : new Vector2(16f, 0f);
                background.color = data.IsSelf
                    ? new Color(0.18f, 0.42f, 0.32f, 0.95f)
                    : new Color(0.16f, 0.22f, 0.32f, 0.95f);
            }

            if (message != null)
            {
                RectTransform textRect = message.rectTransform;
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.sizeDelta = new Vector2(textWidth, textHeight);
                textRect.anchoredPosition = Vector2.zero;
            }

            RectTransform root = transform as RectTransform;
            if (root != null)
            {
                root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bubbleHeight);
            }

            data.DeclaredLength = bubbleHeight;
            RequestResize();
        }
    }
}
