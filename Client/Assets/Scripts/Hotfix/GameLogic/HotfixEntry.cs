using System.Reflection;
using System.Collections.Generic;
using AlicizaX;
using AlicizaX.Localization.Runtime;
using AlicizaX.UI.UXFeedback;
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

            await LocalizationKit.AddCatalogAsync(new ResourceCatalogSource("Game_{locale}"));

            ConfigService = AppServices.App.Register<IConfigService>(new ConfigService());
            await ConfigService.Initialize(new ConfigBytesLoader());
            ConfigService.SetLocalizationChange(LocalizationKit.Get, LocalizationKit.SubscribeChanged);


            if (!AppServices.App.TryGet<IFakePlayerDataService>(out _))
            {
                AppServices.App.Register<IFakePlayerDataService>(new FakePlayerDataService());
            }

            UXUiAudioRouter.Bind(Resources.Load<UXUiAudioProfile>("UX/UXUiAudioProfile"));
            await GameApp.UI.ShowUI<UIHomeWindow>();
        }

        private sealed class ConfigBytesLoader : IConfigBytesLoader
        {
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
