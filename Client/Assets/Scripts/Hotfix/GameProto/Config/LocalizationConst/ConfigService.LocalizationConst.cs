using System.Collections.Generic;
using AlicizaX;
using Cysharp.Threading.Tasks;
using Game.Config.Tables;

namespace Game.Config
{
    public partial interface IConfigService
    {
        TbLocalization TbLocalization { get; }

        TbLocalizationConst TbLocalizationConst { get; }

        UniTask SwitchLanguageAsync();
    }

    public partial class ConfigService
    {
        public TbLocalization TbLocalization { private set; get; }

        public TbLocalizationConst TbLocalizationConst { private set; get; }

        partial void PostLoadAsync(ref List<UniTask> loadTasks)
        {
            TbLocalization = new TbLocalization(_loader);
            loadTasks.Add(TbLocalization.LoadAsync());

            TbLocalizationConst = new TbLocalizationConst(_loader);
            loadTasks.Add(TbLocalizationConst.LoadAsync());
        }

        public async UniTask SwitchLanguageAsync()
        {
            await ReloadLocalizedAsync();
        }

        partial void PostReloadLocalizedAsync(ref List<UniTask> loadTasks)
        {
            TbLocalization.Reset();
            loadTasks.Add(TbLocalization.LoadAsync());

            TbLocalizationConst.Reset();
            loadTasks.Add(TbLocalizationConst.LoadAsync());
        }

        partial void PostResolveRef()
        {
            TbLocalization.ResolveRef(this);
            TbLocalizationConst.ResolveRef(this);
        }

        partial void PostLocalizedTablesLoaded()
        {
            RefreshLocalizationService();
        }

        partial void PostTranslateText(string key, ref string value)
        {
            if (TbLocalization != null)
            {
                value = TbLocalization.GetOrDefault(key)?.Text;
            }
        }

        public void RefreshLocalizationService()
        {
            GameApp.Localization.ReplaceRawStrings(EnumerateLocalizationStrings());
        }

        private IEnumerable<KeyValuePair<string, string>> EnumerateLocalizationStrings()
        {
            // 非常量表不要加 预留可以用 要是你项目实在特殊需要你在加 或者私信我有什么想扩展的
            // if (TbLocalization != null)
            // {
            //     foreach (var item in TbLocalization.DataList)
            //     {
            //         yield return new KeyValuePair<string, string>(item.Key, item.Text);
            //     }
            // }

            if (TbLocalizationConst != null)
            {
                foreach (var item in TbLocalizationConst.DataList)
                {
                    yield return new KeyValuePair<string, string>(item.Key, item.Text);
                }
            }
        }
    }
}
