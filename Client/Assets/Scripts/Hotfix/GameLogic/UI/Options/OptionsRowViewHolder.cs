using System;
using AlicizaX.Localization.Runtime;
using AlicizaX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public sealed class OptionsSetting : ISimpleViewData
    {
        private const string KeyPrefix = "UI.Settings.";

        internal OptionsSetting(string name, int defaultValue, params string[] choices)
        {
            Name = name;
            DefaultValue = defaultValue;
            Value = defaultValue;
            Choices = choices;
        }

        internal string Name { get; }
        internal int DefaultValue { get; }
        internal int Value { get; set; }
        internal string[] Choices { get; }
        internal bool IsSlider => Choices.Length == 0;
        internal bool IsLive => Name is "Language" or "MasterVolume";
        internal string Title => Translate(Name);
        internal string Help => Translate(Name + "Help");
        internal Action<OptionsSetting, int> Change { get; set; }
        internal Action<OptionsSetting> Inspect { get; set; }

        internal string DisplayValue => Name == "Language"
            ? LocalizationKit.Settings.DisplayName(Choices[Value])
            : IsSlider ? $"{Value}%"
            : int.TryParse(Choices[Value], out _) ? Choices[Value] : Translate(Choices[Value]);

        internal static string Translate(string key) => LocalizationKit.Get(KeyPrefix + key);
    }

    public sealed class OptionsRowViewHolder : NavigationViewHolder<OptionsSetting>
    {
        private static readonly Color FocusColor = new Color32(144, 192, 202, 52);
        private static readonly Color IdleValue = new Color32(208, 223, 228, 255);
        private static readonly Color FocusValue = new Color32(242, 251, 252, 255);

        [SerializeField] private Image _background;
        [SerializeField] private Image _accent;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private TMP_Text _percentage;
        [SerializeField] private UXButton _previous;
        [SerializeField] private UXButton _next;
        [SerializeField] private Slider _slider;

        private void Awake()
        {
            _previous.onClick.AddListener(() => Step(-1));
            _next.onClick.AddListener(() => Step(1));
            _slider.onValueChanged.AddListener(value => SetValue(Mathf.RoundToInt(value)));
        }

        protected override void OnBind(OptionsSetting data, int index)
        {
            _title.SetText(data.Title);
            _previous.gameObject.SetActive(!data.IsSlider);
            _next.gameObject.SetActive(!data.IsSlider);
            _value.gameObject.SetActive(!data.IsSlider);
            _slider.gameObject.SetActive(data.IsSlider);
            _percentage.gameObject.SetActive(data.IsSlider);
            RefreshValue(data);
            Paint(IsSelected);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (eventData.button == PointerEventData.InputButton.Left && eventData.eligibleForClick && !eventData.dragging)
                Inspect();
        }

        protected override void OnNavigationFocused(bool focused)
        {
            if (focused) Inspect();
        }

        protected override void OnSelectionChange(bool selected) => Paint(selected);

        public override bool HandleNavigationMove(AxisEventData eventData)
        {
            if (eventData.moveDir == MoveDirection.Left) Step(-1);
            else if (eventData.moveDir == MoveDirection.Right) Step(1);
            else return false;
            return true;
        }

        public override bool HandleNavigationSubmit()
        {
            Step(1);
            return true;
        }

        private void Inspect()
        {
            if (CurrentData == null) return;
            SetSelect();
            CurrentData.Inspect(CurrentData);
        }

        private void Step(int direction)
        {
            var data = CurrentData;
            if (data == null) return;
            int next = data.IsSlider
                ? Mathf.Clamp(data.Value + 5 * direction, 0, 100)
                : (data.Value + direction + data.Choices.Length) % data.Choices.Length;
            SetValue(next);
        }

        private void SetValue(int value)
        {
            var data = CurrentData;
            if (data == null) return;
            Inspect();
            data.Change(data, value);
            RefreshValue(data);
        }

        private void RefreshValue(OptionsSetting data)
        {
            if (data.IsSlider)
            {
                _slider.SetValueWithoutNotify(data.Value);
                _percentage.SetText(data.DisplayValue);
            }
            else _value.SetText(data.DisplayValue);
        }

        private void Paint(bool selected)
        {
            _background.color = selected ? FocusColor : Color.clear;
            _accent.enabled = selected;
            _value.color = _percentage.color = selected ? FocusValue : IdleValue;
        }
    }
}
