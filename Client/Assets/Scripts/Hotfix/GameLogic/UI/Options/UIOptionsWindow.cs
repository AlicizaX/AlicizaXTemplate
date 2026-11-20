using System;
using System.Collections.Generic;
using AlicizaX.Input;
using AlicizaX.Localization.Runtime;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Cysharp.Threading.Tasks;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.UI
{
    [Window(UILayer.UI, UICacheTime.Forever)]
    public sealed class UIOptionsWindow : UIWindow<ui_UIOptionsWindow>
    {
        private static readonly string[] Toggle = { "Off", "On" };

        private UGList<OptionsSetting> _list;
        private RecyclerViewNavigationController _listNavigation;
        private List<OptionsSetting>[] _categories;
        private OptionsSetting _language;
        private OptionsSetting _masterVolume;
        private OptionsSetting _inspected;
        private int _category;
        private bool _oldKeyboardSelection;
        private bool _changingLanguage;

        protected override void OnInitialize()
        {
            _list = UGListCreateHelper.Create<OptionsSetting>(baseui.ScrollViewSettings);
            _listNavigation = baseui.ScrollViewSettings.GetComponent<RecyclerViewNavigationController>();
            var tabs = new[] { baseui.TogGeneral, baseui.TogAccessibility, baseui.TogDisplay, baseui.TogAudio, baseui.TogControls };
            for (int i = 0; i < tabs.Length; i++)
            {
                int category = i;
                tabs[i].onValueChanged.AddListener(isOn =>
                {
                    if (isOn) SwitchCategory(category);
                });
            }

            baseui.BtnBack.onClick.AddListener(() => CloseSelf());
            baseui.BtnReset.onClick.AddListener(() => TipUtils.ShowMessageBox(
                LocalizationKey.UI.SETTINGS_RESETQUESTION, LocalizationKey.UI.SETTINGS_RESETDESCRIPTION,
                ResetOptions));
            baseui.BtnPreviousTab.onClick.AddListener(baseui.TogGeneral.group.Previous);
            baseui.BtnNextTab.onClick.AddListener(baseui.TogGeneral.group.Next);
            CreateSettings();
        }

        protected override void OnOpen()
        {
            LocalizationKit.Changed += OnLanguageChanged;
            SyncLanguage();
            _masterVolume.Value = Mathf.RoundToInt(GameAudioSettings.MasterVolume * 100f);
            _oldKeyboardSelection = UXInput.Navigation.KeyboardRequireSelection;
            UXInput.Navigation.KeyboardRequireSelection = true;
            baseui.TogGeneral.SetIsOnWithoutNotify(true);
            SwitchCategory(0);
        }

        protected override void OnClose()
        {
            LocalizationKit.Changed -= OnLanguageChanged;
            UXInput.Navigation.KeyboardRequireSelection = _oldKeyboardSelection;
            PlayerPrefs.Save();
        }

        private void CreateSettings()
        {
            string[] languages = LocalizationKit.Settings.Locales;
            _language = new OptionsSetting("Language", Array.IndexOf(languages, LocalizationKit.Settings.DefaultLocale), languages);
            _masterVolume = new OptionsSetting("MasterVolume", 100);
            _categories = new[]
            {
                new List<OptionsSetting>
                {
                    _language, new("PauseFocus", 1, Toggle), new("Hints", 1, Toggle),
                    new("Difficulty", 1, "Relaxed", "Standard", "Hard"), new("Markers", 1, Toggle),
                    new("Prompts", 1, Toggle), new("SaveNotice", 1, Toggle), new("SkipIntro", 0, Toggle),
                },
                new List<OptionsSetting>
                {
                    new("Subtitles", 1, Toggle), new("TextSize", 1, "Small", "Medium", "Large"),
                    new("SubtitleBackdrop", 1, Toggle), new("ReducedMotion", 0, Toggle),
                },
                new List<OptionsSetting>
                {
                    new("Brightness", 50), new("Quality", 2, "Low", "Medium", "High"),
                    new("ScreenMode", 1, "Windowed", "Borderless", "Fullscreen"),
                    new("FrameLimit", 1, "30", "60", "120", "Unlimited"),
                },
                new List<OptionsSetting> { _masterVolume, new("DynamicRange", 1, "Night", "Standard", "Wide") },
                new List<OptionsSetting> { new("Sensitivity", 50), new("InvertY", 0, Toggle), new("Vibration", 1, Toggle) },
            };

            foreach (var category in _categories)
            foreach (var option in category)
            {
                option.Change = ChangeOption;
                option.Inspect = ShowDetails;
            }
        }

        private void SwitchCategory(int category)
        {
            _category = category;
            var rows = _categories[category];
            _list.SetData(rows, ListScrollPolicy.ResetScroll);
            _list.Adapter.SetChoiceIndex(0);
            ShowDetails(rows[0]);
            _listNavigation.RestoreFocus(0);
        }

        private void ShowDetails(OptionsSetting option)
        {
            _inspected = option;
            baseui.TextDetailTitle.SetText(option.Title);
            baseui.TextDetailBody.SetText(option.Help);
            baseui.TextDetailStatus.SetText(OptionsSetting.Translate(option.IsLive ? "Applied" : "Preview"));
            baseui.TextDetailNote.SetText(option.IsLive
                ? (_changingLanguage && option == _language ? OptionsSetting.Translate("ChangingLanguage") : string.Empty)
                : OptionsSetting.Translate("PreviewHint"));
        }

        private void ChangeOption(OptionsSetting option, int value)
        {
            int index = _categories[_category].IndexOf(option);
            if (_listNavigation.FocusedDataIndex != index) _listNavigation.SetFocus(index, scroll: false);

            if (option == _language)
            {
                ChangeLanguageAsync(_language.Choices[value]).Forget();
                return;
            }

            option.Value = value;
            if (option == _masterVolume) GameAudioSettings.SetMasterVolume(value / 100f);
        }

        private async UniTask ChangeLanguageAsync(string locale)
        {
            if (_changingLanguage || locale == LocalizationKit.CurrentLanguage) return;
            _changingLanguage = true;
            ShowDetails(_inspected);
            try
            {
                await LocalizationKit.SetLanguageAsync(locale);
            }
            finally
            {
                _changingLanguage = false;
                if (State is UIState.Opened or UIState.Opening) ShowDetails(_inspected);
            }
        }

        private void SyncLanguage() => _language.Value = Array.IndexOf(_language.Choices, LocalizationKit.CurrentLanguage);

        private void OnLanguageChanged(string language)
        {
            SyncLanguage();
            RefreshDisplayedOptions();
        }

        private void RefreshDisplayedOptions()
        {
            var inspected = _inspected;
            var rows = _categories[_category];
            _list.SetData(rows);
            _list.Adapter.SetChoiceIndex(rows.IndexOf(inspected));
            ShowDetails(inspected);
        }

        private void ResetOptions()
        {
            if (_changingLanguage) return;
            foreach (var category in _categories)
            foreach (var option in category)
            {
                if (option != _language) option.Value = option.DefaultValue;
            }

            GameAudioSettings.SetMasterVolume(_masterVolume.DefaultValue / 100f);
            RefreshDisplayedOptions();
            ChangeLanguageAsync(LocalizationKit.Settings.DefaultLocale).Forget();
        }
    }
}
