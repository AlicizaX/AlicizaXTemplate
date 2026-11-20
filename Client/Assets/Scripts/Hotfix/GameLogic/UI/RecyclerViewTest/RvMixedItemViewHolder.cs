using AlicizaX.UI;
using TMPro;
using UnityEngine;

namespace GameLogic.UI
{
    public sealed class RvMixedItemViewHolder : ViewHolder<RvMixedData>
    {
        [SerializeField] private TextMeshProUGUI title;

        protected override void OnBind(RvMixedData data, int index)
        {
            if (title != null)
            {
                title.text = data.Text;
            }
        }
    }
}
