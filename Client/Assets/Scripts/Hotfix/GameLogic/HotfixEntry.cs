using System.Reflection;
using System.Collections.Generic;
using AlicizaX;
using AlicizaX.Localization.Runtime;
using AlicizaX.Input;
using Cysharp.Threading.Tasks;
using Game.Config;
using GameLogic.Player;
using GameLogic.UI;
using Luban;
using UnityEngine;

namespace GameLogic
{
    public static class HotfixEntry
    {
        private static List<Assembly> _hotfixAssembly;
        public static List<Assembly> HotfixAssembly => _hotfixAssembly;

        public static IConfigService ConfigService { get; set; }

        public static void Entrance(object[] objects)
        {
            EntranceAsync(objects).Forget();
        }

        private static async UniTaskVoid EntranceAsync(object[] objects)
        {
            Log.Info("HotFix Logic Entry!");
            _hotfixAssembly = (List<Assembly>)objects[0];

            await LocalizationKit.AddTableAsync("CommonTable", new AssetBundleTableSource("CommonTable_{locale}"));

            ConfigService = AppServices.Register<IConfigService>(new ConfigService());
            await ConfigService.Initialize(new ConfigBytesLoader());
            await ConfigService.SetLanguageAsync(LocalizationKit.CurrentLanguage);
            ConfigService.SetLanguageBinding(LocalizationKit.RegisterLanguageLoader(ConfigService.SetLanguageAsync));


            if (!AppServices.TryGet<IFakePlayerDataService>(out _))
            {
                AppServices.Register<IFakePlayerDataService>(new FakePlayerDataService());
            }

            UXInput.Feedback.Profile = Resources.Load<InputFeedbackProfile>("UX/InputFeedbackProfile");
            GameAudioSettings.ApplyMasterVolume(GameAudioSettings.MasterVolume);
            await GameApp.UI.ShowUI<UIHomeWindow>();
        }

        private sealed class ConfigBytesLoader : IConfigBytesLoader
        {
            public UniTask<ByteBuf> LoadLocalizationAsync(string locale) => LoadCommonAsync("Game_" + locale);

            public async UniTask<ByteBuf> LoadCommonAsync(string file)
            {
                string location = $"{file}";
                using var asset = await GameApp.Resource.LoadLeaseAsync<TextAsset>(location);
                if (!asset.IsValid) throw new System.InvalidOperationException($"Missing config asset: {location}");
                return ByteBuf.Wrap(asset.Asset.bytes);
            }
        }

    }
}
