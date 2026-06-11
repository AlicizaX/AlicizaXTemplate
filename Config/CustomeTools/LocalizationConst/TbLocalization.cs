using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Luban;

namespace Game.Config.Tables
{
    public sealed class TbLocalization
    {
        private const string TableFileName = "tables_tblocalization";

        private readonly IConfigBytesLoader _loader;
        private readonly Dictionary<string, LocalizationConfig> _dataMap = new();
        private readonly List<LocalizationConfig> _dataList = new();

        public TbLocalization(IConfigBytesLoader loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        internal bool IsLoaded { get; private set; }
        internal bool IsResolved { get; private set; }

        public IReadOnlyDictionary<string, LocalizationConfig> DataMap => _dataMap;
        public IReadOnlyList<LocalizationConfig> DataList => _dataList;

        public LocalizationConfig GetOrDefault(string key) => _dataMap.TryGetValue(key, out var value) ? value : null;
        public LocalizationConfig Get(string key) => _dataMap[key];
        public LocalizationConfig this[string key] => _dataMap[key];

        internal async UniTask LoadAsync()
        {
            Reset();
            ByteBuf buf = await _loader.LoadLocalizedAsync(TableFileName);
            AppendBuffer(buf);
            IsLoaded = true;
        }

        internal void ResolveRef(ConfigService tables)
        {
            IsResolved = true;
        }

        internal void TranslateText(Func<string, string> translator)
        {
        }

        internal void Reset()
        {
            _dataMap.Clear();
            _dataList.Clear();
            IsLoaded = false;
            IsResolved = false;
        }

        private void AppendBuffer(ByteBuf buf)
        {
            int count = buf.ReadSize();
            for (int i = 0; i < count; i++)
            {
                var item = new LocalizationConfig(buf);
                if (_dataMap.ContainsKey(item.Key))
                {
                    throw new InvalidOperationException($"Duplicate Localization key: {item.Key}");
                }

                _dataMap.Add(item.Key, item);
                _dataList.Add(item);
            }
        }
    }
}
