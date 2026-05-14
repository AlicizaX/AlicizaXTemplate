using System.Collections.Generic;
using AlicizaX;
using AlicizaX.Localization;
using AlicizaX.Resource.Runtime;
using AlicizaX.UI.Runtime;
using Cysharp.Threading.Tasks;
using Game.Config;
using Game.UI;
using TMPro;
using UnityEngine;

namespace GameLogic.UI
{
    [Window(UILayer.UI, false, 3)]
    public class UIHomeWindow : UITabWindow<ui_UIHomeWindow>
    {
        private string currentLanguage;

        static readonly IReadOnlyList<string> Languages = new List<string>
        {
            "ChineseSimplified",
            "English",
            "Japanese",
        };

        protected override void OnInitialize()
        {
            baseui.BtnShop.onClick.AddListener(OnBtnShopClick);
            baseui.BtnBag.onClick.AddListener(OnBtnBagClick);
            baseui.BtnRole.onClick.AddListener(OnBtnRoleClick);
            baseui.BtnTestTips.onClick.AddListener(OnBtnTestTipsClick);
            baseui.BtnExit.onClick.AddListener(OnBtnExitClick);
            baseui.DropdownLanguages.onValueChanged.AddListener(OnDropdownLanguageChange);
            List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>();

            currentLanguage = GameApp.Localization.Language;

            foreach (var language in LanguageTypes.Languages)
            {
                optionDatas.Add(new TMP_Dropdown.OptionData(language));
            }

            baseui.DropdownLanguages.options = optionDatas;
            baseui.DropdownLanguages.SetValueWithoutNotify(LanguageTypes.StringToIndex(currentLanguage));
        }

        private void OnDropdownLanguageChange(int arg0)
        {
            var selectedLanguage = baseui.DropdownLanguages.options[arg0].text;
            //example临时这么判断看有没有语言资源 自己做的话 别整奥
            var existState = GameApp.Resource.HasAsset($"tables_tblocalization_{selectedLanguage}");
            if (existState == HasAssetResult.NotExist)
            {
                Debug.Log("没有这个语言...吊");
                return;
            }

            if (currentLanguage == selectedLanguage) return;
            currentLanguage = selectedLanguage;
            GameApp.Localization.SwitchLanguage(currentLanguage);
            SwitchLanguage().Forget();
        }

        private async UniTask SwitchLanguage()
        {
            Debug.Log("切换语言"+GameApp.Localization.Language);
            await AppServices.Require<IConfigService>().SwitchLanguageAsync();
            GameApp.Localization.ApplyLanguage();
            LocalizationChangeEvent.Publisher(GameApp.Localization.Language);
        }

        private void OnBtnShopClick()
        {
            GameApp.UI.ShowUISync<UIShopWindow>();
        }

        private void OnBtnBagClick()
        {
            Log.Info("点击背包按钮");
        }

        private void OnBtnRoleClick()
        {
            Log.Info("点击角色属性按钮");
        }

        private void OnBtnTestTipsClick()
        {
            Log.Info("点击测试提示按钮");
        }

        private void OnBtnExitClick()
        {
            Utility.Platform.Quit();
        }
    }
}
