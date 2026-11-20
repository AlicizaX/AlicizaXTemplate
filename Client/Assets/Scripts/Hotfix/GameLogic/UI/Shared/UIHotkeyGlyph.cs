using AlicizaX.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class UIHotkeyGlyph : InputGlyphBehaviourBase
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _fallbackLabel;

        protected override void RefreshGlyph()
        {
            if (_image == null)
                return;

            var action = ResolveAction();
            string path = Glyph.GetBindingControlPath(action, CompositePartName);
            bool hasSprite = Glyph.TryGetSprite(path, usePlaceholder: false, out var sprite);
            if (_fallbackLabel != null)
            {
                _fallbackLabel.enabled = !hasSprite;
                if (!hasSprite) _fallbackLabel.SetText(Glyph.GetDisplayName(action, CompositePartName));
            }
            else if (!hasSprite)
            {
                Debug.LogWarning($"[UIHotkeyGlyph] No sprite for action '{action?.name ?? "<unassigned>"}', binding '{path}', profile '{CurrentProfileId}'.", this);
            }

            _image.sprite = sprite;
            _image.enabled = hasSprite;
        }
    }
}
