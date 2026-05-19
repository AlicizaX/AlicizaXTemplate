using System.Reflection;
using System.Collections.Generic;
using AlicizaX;
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
            ConfigService = AppServices.App.Register<IConfigService>(new ConfigService());
            await ConfigService.Initialize(new ConfigBytesLoader());


            if (!AppServices.App.TryGet<IFakePlayerDataService>(out _))
            {
                AppServices.App.Register<IFakePlayerDataService>(new FakePlayerDataService());
            }

            GameApp.UI.ShowUI<UIHomeWindow>();
        }

        private sealed class ConfigBytesLoader : IConfigBytesLoader
        {
            public UniTask<ByteBuf> LoadCommonAsync(string file)
            {
                return LoadBufferAsync($"{file}");
            }

            public UniTask<ByteBuf> LoadLocalizedAsync(string file)
            {
                return LoadBufferAsync($"{file}_{GameApp.Localization.Language}");
            }

            private static async UniTask<ByteBuf> LoadBufferAsync(string location)
            {
                var textAsset = await GameApp.Resource.LoadAssetAsync<TextAsset>(location);
                byte[] bytes = textAsset.bytes;
                GameApp.Resource.UnloadAsset(textAsset);
                return ByteBuf.Wrap(bytes);
            }
        }
    }
}
