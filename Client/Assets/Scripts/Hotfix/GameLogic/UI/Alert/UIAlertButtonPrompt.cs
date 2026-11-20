using AlicizaX.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class UIAlertButtonPrompt : InputGlyphBehaviourBase
    {
        [SerializeField] private GameObject _desktopFace;
        [SerializeField] private Image _gamepadIcon;
        [SerializeField] private Sprite _emptyKeycap;
        [SerializeField] private TMP_Text _controlName;

        protected override void RefreshGlyph()
        {
            var action = ResolveAction();
            string path = UXInput.Watch.CurrentInputType == InputWatch.InputType.Gamepad
                ? Glyph.GetBindingControlPath(action, CompositePartName)
                : null;
            bool showGamepad = !string.IsNullOrEmpty(path);
            _desktopFace.SetActive(!showGamepad);
            _gamepadIcon.gameObject.SetActive(showGamepad);
            if (!showGamepad) return;

            bool hasSprite = Glyph.TryGetSprite(path, usePlaceholder: false, out var sprite);
            _gamepadIcon.sprite = hasSprite ? sprite : _emptyKeycap;
            _controlName.gameObject.SetActive(!hasSprite);
            if (!hasSprite) _controlName.SetText(Glyph.GetDisplayName(action, CompositePartName));
        }
    }
}
