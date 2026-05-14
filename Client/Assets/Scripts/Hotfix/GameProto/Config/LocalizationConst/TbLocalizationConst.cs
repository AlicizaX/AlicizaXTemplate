using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Luban;

namespace Game.Config.Tables
{
    public sealed class TbLocalizationConst
    {
        private const string TableFileName = "tables_tblocalizationconst";

        private readonly IConfigBytesLoader _loader;
        private readonly Dictionary<int, LocalizationConstConfig> _dataMap = new();
        private readonly List<LocalizationConstConfig> _dataList = new();

        public TbLocalizationConst(IConfigBytesLoader loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        internal bool IsLoaded { get; private set; }
        internal bool IsResolved { get; private set; }

        public IReadOnlyDictionary<int, LocalizationConstConfig> DataMap => _dataMap;
        public IReadOnlyList<LocalizationConstConfig> DataList => _dataList;

        public LocalizationConstConfig GetOrDefault(int key) => _dataMap.TryGetValue(key, out var value) ? value : null;
        public LocalizationConstConfig Get(int key) => _dataMap[key];
        public LocalizationConstConfig this[int key] => _dataMap[key];

        internal async UniTask LoadAsync()
        {
            Reset();
            ByteBuf buf = await _loader.LoadLocalizedAsync(TableFileName);
            AppendBuffer(buf);
            IsLoaded = true;
        }

        internal void ResolveRef(ConfigService tables)
        {
            foreach (var item in _dataList)
            {
                item.ResolveRef(tables);
            }

            IsResolved = true;
        }

        internal void Reset()
        {
            _dataMap.Clear();
            _dataList.Clear();
            IsLoaded = false;
            IsResolved = false;
        }

        internal void TranslateText(Func<string, string> translator)
        {
        }

        private void AppendBuffer(ByteBuf buf)
        {
            int count = buf.ReadSize();
            for (int i = 0; i < count; i++)
            {
                var item = new LocalizationConstConfig(buf);
                if (_dataMap.ContainsKey(item.Id))
                {
                    throw new InvalidOperationException($"Duplicate LocalizationConst id: {item.Id}");
                }

                _dataMap.Add(item.Id, item);
                _dataList.Add(item);
            }
        }
    }
}
