using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class RvMixedItemViewHolder : ViewHolder<RvMixedData>
    {
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI title;

        protected override void OnBind(RvMixedData data, int index)
        {
            if (title != null)
            {
                title.text = data.Text;
            }

            if (background != null)
            {
                background.color = data.TemplateId switch
                {
                    1 => new Color(0.18f, 0.32f, 0.22f, 0.95f),
                    2 => new Color(0.28f, 0.22f, 0.12f, 0.95f),
                    _ => new Color(0.16f, 0.2f, 0.34f, 0.95f)
                };
            }
        }
    }
}
