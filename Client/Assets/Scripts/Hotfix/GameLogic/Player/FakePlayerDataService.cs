using System.Collections.Generic;
using AlicizaX;
using Game.Config;
using Game.Config.Tables;
using GameLogic.UI;

namespace GameLogic.Player
{
    public interface IFakePlayerDataService : IService
    {
        int Credit { get; }
        IReadOnlyList<FakeBagItemData> BagItems { get; }
        bool TryBuy(ShopGoodsData goodsData, int quantity, out int totalPrice);
    }

    public sealed class FakePlayerDataService : ServiceBase, IFakePlayerDataService
    {
        private readonly List<FakeBagItemData> _bagItems = new(16);
        private IConfigService _configService;

        public int Credit { get; private set; }
        public IReadOnlyList<FakeBagItemData> BagItems => _bagItems;

        protected override void OnInitialize()
        {
            Credit = 12000;
            _configService = AppServices.Require<IConfigService>();
            _bagItems.Clear();
            AddInitialItem(1001, 3);
            AddInitialItem(2002, 5);
            AddInitialItem(3001, 2);
        }

        protected override void OnDestroyService()
        {
            _bagItems.Clear();
        }

        public bool TryBuy(ShopGoodsData goodsData, int quantity, out int totalPrice)
        {
            totalPrice = 0;
            if (goodsData == null || quantity <= 0)
            {
                return false;
            }

            int unitPrice = goodsData.Price;
            totalPrice = unitPrice * quantity;
            if (Credit < totalPrice)
            {
                return false;
            }

            Credit -= totalPrice;
            AddBagItem(goodsData.ItemId, goodsData.Name, quantity);
            var evt = new PlayerDataChangedEvent(Credit, goodsData.ItemId, goodsData.Name, quantity, totalPrice);
            EventBus.Publish(in evt);
            return true;
        }

        private void AddInitialItem(int itemId, int quantity)
        {
            ItemConfig itemConfig = _configService.TbItem.GetOrDefault(itemId);
            if (itemConfig != null)
            {
                AddBagItem(itemConfig.Id, itemConfig.Name, quantity);
            }
        }

        private void AddBagItem(int itemId, string itemName, int quantity)
        {
            for (int i = 0; i < _bagItems.Count; i++)
            {
                if (_bagItems[i].ItemId != itemId)
                {
                    continue;
                }

                FakeBagItemData item = _bagItems[i];
                item.Count += quantity;
                _bagItems[i] = item;
                return;
            }

            _bagItems.Add(new FakeBagItemData(itemId, itemName, quantity));
        }
    }

    public struct FakeBagItemData
    {
        public int ItemId;
        public string ItemName;
        public int Count;

        public FakeBagItemData(int itemId, string itemName, int count)
        {
            ItemId = itemId;
            ItemName = itemName;
            Count = count;
        }
    }

    [Prewarm(8)]
    public readonly struct PlayerDataChangedEvent : IPayloadEventArgs
    {
        public readonly int Credit;
        public readonly int ChangedItemId;
        public readonly string ChangedItemName;
        public readonly int ChangedItemCount;
        public readonly int Cost;

        public PlayerDataChangedEvent(int credit, int changedItemId, string changedItemName, int changedItemCount, int cost)
        {
            Credit = credit;
            ChangedItemId = changedItemId;
            ChangedItemName = changedItemName;
            ChangedItemCount = changedItemCount;
            Cost = cost;
        }
    }

    public static class PriceUtility
    {
        public static int ParseCreditPrice(string price)
        {
            if (string.IsNullOrEmpty(price))
            {
                return 0;
            }

            int value = 0;
            for (int i = 0; i < price.Length; i++)
            {
                char c = price[i];
                if (c >= '0' && c <= '9')
                {
                    value = value * 10 + c - '0';
                }
            }

            return value;
        }
    }
}
