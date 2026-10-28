using System.Collections.Generic;
using AlicizaX;
using AlicizaX.Localization.Runtime;
using AlicizaX.UI.Runtime;
using Cysharp.Threading.Tasks;
using Game.UI;
using TMPro;
using UnityEngine;

namespace GameLogic.UI
{
    [Window(UILayer.UI, 30)]
    public class UIHomeWindow : UIWindow<ui_UIHomeWindow>
    {
        private string currentLanguage;
        private UITestAWidget _aWidget;

        protected override void OnInitialize()
        {
            baseui.BtnShop.onClick.AddListener(OnBtnShopClick);
            baseui.BtnBag.onClick.AddListener(OnBtnBagClick);
            baseui.BtnRole.onClick.AddListener(OnBtnRoleClick);
            baseui.BtnTestTips.onClick.AddListener(OnBtnTestTipsClick);
            baseui.BtnExit.onClick.AddListener(OnBtnExitClick);
            baseui.DropdownLanguages.onValueChanged.AddListener(OnDropdownLanguageChange);
            List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>();

            _aWidget=CreateWidgetSync<UITestAWidget>(baseui.RectTransform,false);

            currentLanguage = LocalizationKit.CurrentLanguage;

            foreach (var language in LocalizationKit.SupportedLanguages)
            {
                optionDatas.Add(new TMP_Dropdown.OptionData(language));
            }

            baseui.DropdownLanguages.options = optionDatas;
            baseui.DropdownLanguages.SetValueWithoutNotify(Mathf.Max(0, optionDatas.FindIndex(option => option.text == currentLanguage)));
        }

        private void OnDropdownLanguageChange(int arg0)
        {
            var selectedLanguage = baseui.DropdownLanguages.options[arg0].text;
            if (currentLanguage == selectedLanguage) return;
            SwitchLanguageAsync(selectedLanguage).Forget();
        }

        private async UniTask SwitchLanguageAsync(string selectedLanguage)
        {
            baseui.DropdownLanguages.interactable = false;
            try
            {
                if (await LocalizationKit.SetLanguageAsync(selectedLanguage))
                {
                    currentLanguage = selectedLanguage;
                }
            }
            catch (System.Exception error)
            {
                Debug.LogException(error);
            }
            finally
            {
                currentLanguage = LocalizationKit.CurrentLanguage;
                if (baseui.DropdownLanguages != null)
                {
                    baseui.DropdownLanguages.SetValueWithoutNotify(Mathf.Max(0, baseui.DropdownLanguages.options.FindIndex(option => option.text == currentLanguage)));
                    baseui.DropdownLanguages.interactable = true;
                }
            }
        }

        private void OnBtnShopClick()
        {
            GameApp.UI.ShowUI<UIShopWindow>();
        }

        private void OnBtnBagClick()
        {
            GameApp.UI.ShowUI<UIRecyclerViewTestHubWindow>();
        }

        private void OnBtnRoleClick()
        {
            Log.Info("点击角色属性按钮");
            _aWidget.Open();
        }

        private void OnBtnTestTipsClick()
        {
            Log.Info("点击测试提示按钮");
            _aWidget.Close();
        }

        private void OnBtnExitClick()
        {
            Log.Info("Quit Game");
        }

        protected override void OnOpen()
        {
            Debug.Log("HomeOpen");
        }

        protected override void OnRefresh()
        {
            Debug.Log("OnRefresh");
        }

        protected override void OnClose()
        {
            Debug.Log("HomeOnClose");
        }

        protected override void OnDestroy()
        {
            Debug.Log("HomeOnDestroy");
        }
    }
}
