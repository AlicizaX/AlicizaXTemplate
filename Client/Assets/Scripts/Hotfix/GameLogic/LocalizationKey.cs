using AlicizaX;
using AlicizaX.Localization.Runtime;

/// <summary>
/// AutoGenerate
/// </summary>
public static class LocalizationKey
{
    private static ILocalizationService _localizationService;

    private static ILocalizationService LocalizationService
    {
        get
        {
            if (_localizationService == null)
            {
                _localizationService = AppServices.App.Require<ILocalizationService>();
            }

            return _localizationService;
        }
    }

    public static class Log
    {
        /// <summary>
        /// 确认购买：{0} x{1}，总价 {2}
        /// </summary>
        public static string BUY_CONFIRMED(string arg1, string arg2, string arg3)
        {
            return LocalizationService.GetString("Log.Buy.Confirmed", arg1, arg2, arg3);
        }
        public static string BUY_CONFIRMED_Raw => "Log.Buy.Confirmed";

        /// <summary>
        /// 信用点不足，无法购买：{0}x{1}
        /// </summary>
        public static string BUY_INSUFFICIENT(string arg1, string arg2)
        {
            return LocalizationService.GetString("Log.Buy.Insufficient", arg1, arg2);
        }
        public static string BUY_INSUFFICIENT_Raw => "Log.Buy.Insufficient";

        /// <summary>
        /// 点击背包按钮
        /// </summary>
        public static string HOME_BAGCLICK => LocalizationService.GetString("Log.Home.BagClick");
        public static string HOME_BAGCLICK_Raw => "Log.Home.BagClick";

        /// <summary>
        /// 点击退出按钮
        /// </summary>
        public static string HOME_EXITCLICK => LocalizationService.GetString("Log.Home.ExitClick");
        public static string HOME_EXITCLICK_Raw => "Log.Home.ExitClick";

        /// <summary>
        /// 点击角色属性按钮
        /// </summary>
        public static string HOME_ROLECLICK => LocalizationService.GetString("Log.Home.RoleClick");
        public static string HOME_ROLECLICK_Raw => "Log.Home.RoleClick";

        /// <summary>
        /// 点击测试提示按钮
        /// </summary>
        public static string HOME_TESTTIPSCLICK => LocalizationService.GetString("Log.Home.TestTipsClick");
        public static string HOME_TESTTIPSCLICK_Raw => "Log.Home.TestTipsClick";
    }

    public static class UI
    {
        /// <summary>
        /// 确认购买
        /// </summary>
        public static string BUY_BUTTONCONFIRM => LocalizationService.GetString("UI.Buy.ButtonConfirm");
        public static string BUY_BUTTONCONFIRM_Raw => "UI.Buy.ButtonConfirm";

        /// <summary>
        /// 输入数量或使用 +/- 调整购买数量
        /// </summary>
        public static string BUY_FOOTERHINT => LocalizationService.GetString("UI.Buy.FooterHint");
        public static string BUY_FOOTERHINT_Raw => "UI.Buy.FooterHint";

        /// <summary>
        /// 数量
        /// </summary>
        public static string BUY_QUANTITYPLACEHOLDER => LocalizationService.GetString("UI.Buy.QuantityPlaceholder");
        public static string BUY_QUANTITYPLACEHOLDER_Raw => "UI.Buy.QuantityPlaceholder";

        /// <summary>
        /// 购买确认
        /// </summary>
        public static string BUY_TITLE => LocalizationService.GetString("UI.Buy.Title");
        public static string BUY_TITLE_Raw => "UI.Buy.Title";

        /// <summary>
        /// 总价格：{0}
        /// </summary>
        public static string BUY_TOTALPRICE(string arg1)
        {
            return LocalizationService.GetString("UI.Buy.TotalPrice", arg1);
        }
        public static string BUY_TOTALPRICE_Raw => "UI.Buy.TotalPrice";

        /// <summary>
        /// 未知道具
        /// </summary>
        public static string BUY_UNKNOWNITEM => LocalizationService.GetString("UI.Buy.UnknownItem");
        public static string BUY_UNKNOWNITEM_Raw => "UI.Buy.UnknownItem";

        /// <summary>
        /// {0} 信用点
        /// </summary>
        public static string COMMON_CREDITPRICE(string arg1)
        {
            return LocalizationService.GetString("UI.Common.CreditPrice", arg1);
        }
        public static string COMMON_CREDITPRICE_Raw => "UI.Common.CreditPrice";

        /// <summary>
        /// 背包
        /// </summary>
        public static string HOME_BUTTONBAG => LocalizationService.GetString("UI.Home.ButtonBag");
        public static string HOME_BUTTONBAG_Raw => "UI.Home.ButtonBag";

        /// <summary>
        /// 退出
        /// </summary>
        public static string HOME_BUTTONEXIT => LocalizationService.GetString("UI.Home.ButtonExit");
        public static string HOME_BUTTONEXIT_Raw => "UI.Home.ButtonExit";

        /// <summary>
        /// 角色
        /// </summary>
        public static string HOME_BUTTONROLE => LocalizationService.GetString("UI.Home.ButtonRole");
        public static string HOME_BUTTONROLE_Raw => "UI.Home.ButtonRole";

        /// <summary>
        /// 商店
        /// </summary>
        public static string HOME_BUTTONSHOP => LocalizationService.GetString("UI.Home.ButtonShop");
        public static string HOME_BUTTONSHOP_Raw => "UI.Home.ButtonShop";

        /// <summary>
        /// 提示
        /// </summary>
        public static string HOME_BUTTONTIPS => LocalizationService.GetString("UI.Home.ButtonTips");
        public static string HOME_BUTTONTIPS_Raw => "UI.Home.ButtonTips";

        /// <summary>
        /// 点击按钮测试 UI 逻辑 / ESC 返回
        /// </summary>
        public static string HOME_FOOTERHINT => LocalizationService.GetString("UI.Home.FooterHint");
        public static string HOME_FOOTERHINT_Raw => "UI.Home.FooterHint";

        /// <summary>
        /// 欢迎进入测试主界面。右上角可以打开商店、背包、角色属性和测试提示。
        /// 
        /// 当前界面用于验证UI 框架、按钮绑定和默认过场动画。
        /// </summary>
        public static string HOME_NOTICECONTENT => LocalizationService.GetString("UI.Home.NoticeContent");
        public static string HOME_NOTICECONTENT_Raw => "UI.Home.NoticeContent";

        /// <summary>
        /// 任务公告
        /// </summary>
        public static string HOME_NOTICETITLE => LocalizationService.GetString("UI.Home.NoticeTitle");
        public static string HOME_NOTICETITLE_Raw => "UI.Home.NoticeTitle";

        /// <summary>
        /// 退出
        /// </summary>
        public static string SHOP_BUTTONCLOSE => LocalizationService.GetString("UI.Shop.ButtonClose");
        public static string SHOP_BUTTONCLOSE_Raw => "UI.Shop.ButtonClose";

        /// <summary>
        /// 来自 Shop 表 MATERIAL 分类的材料道具。列表数据由配置表动态读取。
        /// </summary>
        public static string SHOP_CATEGORY_ITEM_DESCRIPTION => LocalizationService.GetString("UI.Shop.Category.Item.Description");
        public static string SHOP_CATEGORY_ITEM_DESCRIPTION_Raw => "UI.Shop.Category.Item.Description";

        /// <summary>
        /// 材料商城
        /// </summary>
        public static string SHOP_CATEGORY_ITEM_TITLE => LocalizationService.GetString("UI.Shop.Category.Item.Title");
        public static string SHOP_CATEGORY_ITEM_TITLE_Raw => "UI.Shop.Category.Item.Title";

        /// <summary>
        /// 来自 Shop 表 CONSUMABLE 分类的消耗品。列表数据由配置表动态读取。
        /// </summary>
        public static string SHOP_CATEGORY_PACK_DESCRIPTION => LocalizationService.GetString("UI.Shop.Category.Pack.Description");
        public static string SHOP_CATEGORY_PACK_DESCRIPTION_Raw => "UI.Shop.Category.Pack.Description";

        /// <summary>
        /// 消耗品商城
        /// </summary>
        public static string SHOP_CATEGORY_PACK_TITLE => LocalizationService.GetString("UI.Shop.Category.Pack.Title");
        public static string SHOP_CATEGORY_PACK_TITLE_Raw => "UI.Shop.Category.Pack.Title";

        /// <summary>
        /// 来自 Shop 表 TEST 分类的测试道具。列表数据由配置表动态读取。
        /// </summary>
        public static string SHOP_CATEGORY_RECOMMEND_DESCRIPTION => LocalizationService.GetString("UI.Shop.Category.Recommend.Description");
        public static string SHOP_CATEGORY_RECOMMEND_DESCRIPTION_Raw => "UI.Shop.Category.Recommend.Description";

        /// <summary>
        /// 测试商城
        /// </summary>
        public static string SHOP_CATEGORY_RECOMMEND_TITLE => LocalizationService.GetString("UI.Shop.Category.Recommend.Title");
        public static string SHOP_CATEGORY_RECOMMEND_TITLE_Raw => "UI.Shop.Category.Recommend.Title";

        /// <summary>
        /// 来自 Shop 表 EQUIPMENT 分类的装备道具。列表数据由配置表动态读取。
        /// </summary>
        public static string SHOP_CATEGORY_SKIN_DESCRIPTION => LocalizationService.GetString("UI.Shop.Category.Skin.Description");
        public static string SHOP_CATEGORY_SKIN_DESCRIPTION_Raw => "UI.Shop.Category.Skin.Description";

        /// <summary>
        /// 装备商城
        /// </summary>
        public static string SHOP_CATEGORY_SKIN_TITLE => LocalizationService.GetString("UI.Shop.Category.Skin.Title");
        public static string SHOP_CATEGORY_SKIN_TITLE_Raw => "UI.Shop.Category.Skin.Title";

        /// <summary>
        /// 信用点：{0}
        /// </summary>
        public static string SHOP_CURRENCY(string arg1)
        {
            return LocalizationService.GetString("UI.Shop.Currency", arg1);
        }
        public static string SHOP_CURRENCY_Raw => "UI.Shop.Currency";

        /// <summary>
        /// 点击商品记录测试购买日志/ ESC 返回
        /// </summary>
        public static string SHOP_FOOTERHINT => LocalizationService.GetString("UI.Shop.FooterHint");
        public static string SHOP_FOOTERHINT_Raw => "UI.Shop.FooterHint";

        /// <summary>
        /// 用于锁定区域的临时授权卡。
        /// </summary>
        public static string SHOP_GOODS_ITEM_ACCESSKEY_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Item.AccessKey.Description");
        public static string SHOP_GOODS_ITEM_ACCESSKEY_DESCRIPTION_Raw => "UI.Shop.Goods.Item.AccessKey.Description";

        /// <summary>
        /// 通行密钥
        /// </summary>
        public static string SHOP_GOODS_ITEM_ACCESSKEY_NAME => LocalizationService.GetString("UI.Shop.Goods.Item.AccessKey.Name");
        public static string SHOP_GOODS_ITEM_ACCESSKEY_NAME_Raw => "UI.Shop.Goods.Item.AccessKey.Name";

        /// <summary>
        /// 原型设备可复用的备用电池。
        /// </summary>
        public static string SHOP_GOODS_ITEM_ENERGYBATTERY_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Item.EnergyBattery.Description");
        public static string SHOP_GOODS_ITEM_ENERGYBATTERY_DESCRIPTION_Raw => "UI.Shop.Goods.Item.EnergyBattery.Description";

        /// <summary>
        /// 能量电池
        /// </summary>
        public static string SHOP_GOODS_ITEM_ENERGYBATTERY_NAME => LocalizationService.GetString("UI.Shop.Goods.Item.EnergyBattery.Name");
        public static string SHOP_GOODS_ITEM_ENERGYBATTERY_NAME_Raw => "UI.Shop.Goods.Item.EnergyBattery.Name";

        /// <summary>
        /// 用于实地测试的即时恢复补给。
        /// </summary>
        public static string SHOP_GOODS_ITEM_MEDKIT_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Item.Medkit.Description");
        public static string SHOP_GOODS_ITEM_MEDKIT_DESCRIPTION_Raw => "UI.Shop.Goods.Item.Medkit.Description";

        /// <summary>
        /// 急救包
        /// </summary>
        public static string SHOP_GOODS_ITEM_MEDKIT_NAME => LocalizationService.GetString("UI.Shop.Goods.Item.Medkit.Name");
        public static string SHOP_GOODS_ITEM_MEDKIT_NAME_Raw => "UI.Shop.Goods.Item.Medkit.Name";

        /// <summary>
        /// 为导航测试记录一个临时位置点。
        /// </summary>
        public static string SHOP_GOODS_ITEM_SIGNALMARKER_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Item.SignalMarker.Description");
        public static string SHOP_GOODS_ITEM_SIGNALMARKER_DESCRIPTION_Raw => "UI.Shop.Goods.Item.SignalMarker.Description";

        /// <summary>
        /// 信号标记
        /// </summary>
        public static string SHOP_GOODS_ITEM_SIGNALMARKER_NAME => LocalizationService.GetString("UI.Shop.Goods.Item.SignalMarker.Name");
        public static string SHOP_GOODS_ITEM_SIGNALMARKER_NAME_Raw => "UI.Shop.Goods.Item.SignalMarker.Name";

        /// <summary>
        /// 探索循环所需的消耗品与钥匙。
        /// </summary>
        public static string SHOP_GOODS_PACK_EXPLORATIONPACK_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Pack.ExplorationPack.Description");
        public static string SHOP_GOODS_PACK_EXPLORATIONPACK_DESCRIPTION_Raw => "UI.Shop.Goods.Pack.ExplorationPack.Description";

        /// <summary>
        /// 探索礼包
        /// </summary>
        public static string SHOP_GOODS_PACK_EXPLORATIONPACK_NAME => LocalizationService.GetString("UI.Shop.Goods.Pack.ExplorationPack.Name");
        public static string SHOP_GOODS_PACK_EXPLORATIONPACK_NAME_Raw => "UI.Shop.Goods.Pack.ExplorationPack.Name";

        /// <summary>
        /// 三件外观组合的折扣套装。
        /// </summary>
        public static string SHOP_GOODS_PACK_OUTFITPACK_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Pack.OutfitPack.Description");
        public static string SHOP_GOODS_PACK_OUTFITPACK_DESCRIPTION_Raw => "UI.Shop.Goods.Pack.OutfitPack.Description";

        /// <summary>
        /// 外观礼包
        /// </summary>
        public static string SHOP_GOODS_PACK_OUTFITPACK_NAME => LocalizationService.GetString("UI.Shop.Goods.Pack.OutfitPack.Name");
        public static string SHOP_GOODS_PACK_OUTFITPACK_NAME_Raw => "UI.Shop.Goods.Pack.OutfitPack.Name";

        /// <summary>
        /// 用于首次流程验证的基础道具包。
        /// </summary>
        public static string SHOP_GOODS_PACK_STARTERPACK_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Pack.StarterPack.Description");
        public static string SHOP_GOODS_PACK_STARTERPACK_DESCRIPTION_Raw => "UI.Shop.Goods.Pack.StarterPack.Description";

        /// <summary>
        /// 新手礼包
        /// </summary>
        public static string SHOP_GOODS_PACK_STARTERPACK_NAME => LocalizationService.GetString("UI.Shop.Goods.Pack.StarterPack.Name");
        public static string SHOP_GOODS_PACK_STARTERPACK_NAME_Raw => "UI.Shop.Goods.Pack.StarterPack.Name";

        /// <summary>
        /// 按商城周期刷新的限时礼包。
        /// </summary>
        public static string SHOP_GOODS_PACK_WEEKLYSUPPLY_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Pack.WeeklySupply.Description");
        public static string SHOP_GOODS_PACK_WEEKLYSUPPLY_DESCRIPTION_Raw => "UI.Shop.Goods.Pack.WeeklySupply.Description";

        /// <summary>
        /// 每周补给
        /// </summary>
        public static string SHOP_GOODS_PACK_WEEKLYSUPPLY_NAME => LocalizationService.GetString("UI.Shop.Goods.Pack.WeeklySupply.Name");
        public static string SHOP_GOODS_PACK_WEEKLYSUPPLY_NAME_Raw => "UI.Shop.Goods.Pack.WeeklySupply.Name";

        /// <summary>
        /// 用于商城列表测试的模拟货币包。
        /// </summary>
        public static string SHOP_GOODS_RECOMMEND_CREDITCRATE_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Recommend.CreditCrate.Description");
        public static string SHOP_GOODS_RECOMMEND_CREDITCRATE_DESCRIPTION_Raw => "UI.Shop.Goods.Recommend.CreditCrate.Description";

        /// <summary>
        /// 信用点箱
        /// </summary>
        public static string SHOP_GOODS_RECOMMEND_CREDITCRATE_NAME => LocalizationService.GetString("UI.Shop.Goods.Recommend.CreditCrate.Name");
        public static string SHOP_GOODS_RECOMMEND_CREDITCRATE_NAME_Raw => "UI.Shop.Goods.Recommend.CreditCrate.Name";

        /// <summary>
        /// 核心消耗品的每日折扣组合。
        /// </summary>
        public static string SHOP_GOODS_RECOMMEND_DAILYSUPPLY_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Recommend.DailySupply.Description");
        public static string SHOP_GOODS_RECOMMEND_DAILYSUPPLY_DESCRIPTION_Raw => "UI.Shop.Goods.Recommend.DailySupply.Description";

        /// <summary>
        /// 每日补给
        /// </summary>
        public static string SHOP_GOODS_RECOMMEND_DAILYSUPPLY_NAME => LocalizationService.GetString("UI.Shop.Goods.Recommend.DailySupply.Name");
        public static string SHOP_GOODS_RECOMMEND_DAILYSUPPLY_NAME_Raw => "UI.Shop.Goods.Recommend.DailySupply.Name";

        /// <summary>
        /// 适配暗色菜单场景的高对比外观。
        /// </summary>
        public static string SHOP_GOODS_RECOMMEND_NIGHTOUTFIT_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Recommend.NightOutfit.Description");
        public static string SHOP_GOODS_RECOMMEND_NIGHTOUTFIT_DESCRIPTION_Raw => "UI.Shop.Goods.Recommend.NightOutfit.Description";

        /// <summary>
        /// 夜行外观
        /// </summary>
        public static string SHOP_GOODS_RECOMMEND_NIGHTOUTFIT_NAME => LocalizationService.GetString("UI.Shop.Goods.Recommend.NightOutfit.Name");
        public static string SHOP_GOODS_RECOMMEND_NIGHTOUTFIT_NAME_Raw => "UI.Shop.Goods.Recommend.NightOutfit.Name";

        /// <summary>
        /// 用于验证购买流程的可复用模块。
        /// </summary>
        public static string SHOP_GOODS_RECOMMEND_TACTICALMODULE_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Recommend.TacticalModule.Description");
        public static string SHOP_GOODS_RECOMMEND_TACTICALMODULE_DESCRIPTION_Raw => "UI.Shop.Goods.Recommend.TacticalModule.Description";

        /// <summary>
        /// 战术模块
        /// </summary>
        public static string SHOP_GOODS_RECOMMEND_TACTICALMODULE_NAME => LocalizationService.GetString("UI.Shop.Goods.Recommend.TacticalModule.Name");
        public static string SHOP_GOODS_RECOMMEND_TACTICALMODULE_NAME_Raw => "UI.Shop.Goods.Recommend.TacticalModule.Name";

        /// <summary>
        /// 适合城市关卡的哑光战术头盔。
        /// </summary>
        public static string SHOP_GOODS_SKIN_ASHHELMET_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Skin.AshHelmet.Description");
        public static string SHOP_GOODS_SKIN_ASHHELMET_DESCRIPTION_Raw => "UI.Shop.Goods.Skin.AshHelmet.Description";

        /// <summary>
        /// 灰烬头盔
        /// </summary>
        public static string SHOP_GOODS_SKIN_ASHHELMET_NAME => LocalizationService.GetString("UI.Shop.Goods.Skin.AshHelmet.Name");
        public static string SHOP_GOODS_SKIN_ASHHELMET_NAME_Raw => "UI.Shop.Goods.Skin.AshHelmet.Name";

        /// <summary>
        /// 低对比度标记的极简面具。
        /// </summary>
        public static string SHOP_GOODS_SKIN_GHOSTMASK_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Skin.GhostMask.Description");
        public static string SHOP_GOODS_SKIN_GHOSTMASK_DESCRIPTION_Raw => "UI.Shop.Goods.Skin.GhostMask.Description";

        /// <summary>
        /// 幽灵面具
        /// </summary>
        public static string SHOP_GOODS_SKIN_GHOSTMASK_NAME => LocalizationService.GetString("UI.Shop.Goods.Skin.GhostMask.Name");
        public static string SHOP_GOODS_SKIN_GHOSTMASK_NAME_Raw => "UI.Shop.Goods.Skin.GhostMask.Name";

        /// <summary>
        /// 带有青色细光带的暗色外套。
        /// </summary>
        public static string SHOP_GOODS_SKIN_NEONCOAT_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Skin.NeonCoat.Description");
        public static string SHOP_GOODS_SKIN_NEONCOAT_DESCRIPTION_Raw => "UI.Shop.Goods.Skin.NeonCoat.Description";

        /// <summary>
        /// 霓虹外套
        /// </summary>
        public static string SHOP_GOODS_SKIN_NEONCOAT_NAME => LocalizationService.GetString("UI.Shop.Goods.Skin.NeonCoat.Name");
        public static string SHOP_GOODS_SKIN_NEONCOAT_NAME_Raw => "UI.Shop.Goods.Skin.NeonCoat.Name";

        /// <summary>
        /// 来自地下标识风格的小型徽章。
        /// </summary>
        public static string SHOP_GOODS_SKIN_WANDERERBADGE_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Skin.WandererBadge.Description");
        public static string SHOP_GOODS_SKIN_WANDERERBADGE_DESCRIPTION_Raw => "UI.Shop.Goods.Skin.WandererBadge.Description";

        /// <summary>
        /// 流浪徽章
        /// </summary>
        public static string SHOP_GOODS_SKIN_WANDERERBADGE_NAME => LocalizationService.GetString("UI.Shop.Goods.Skin.WandererBadge.Name");
        public static string SHOP_GOODS_SKIN_WANDERERBADGE_NAME_Raw => "UI.Shop.Goods.Skin.WandererBadge.Name";

        /// <summary>
        /// 用于滚动测试的 RecyclerView 复用条目。
        /// </summary>
        public static string SHOP_GOODS_TEST_DESCRIPTION => LocalizationService.GetString("UI.Shop.Goods.Test.Description");
        public static string SHOP_GOODS_TEST_DESCRIPTION_Raw => "UI.Shop.Goods.Test.Description";

        /// <summary>
        /// 测试商品 {0}
        /// </summary>
        public static string SHOP_GOODS_TEST_NAME(string arg1)
        {
            return LocalizationService.GetString("UI.Shop.Goods.Test.Name", arg1);
        }
        public static string SHOP_GOODS_TEST_NAME_Raw => "UI.Shop.Goods.Test.Name";

        /// <summary>
        /// 补给终端
        /// </summary>
        public static string SHOP_SUBTITLE => LocalizationService.GetString("UI.Shop.SubTitle");
        public static string SHOP_SUBTITLE_Raw => "UI.Shop.SubTitle";

        /// <summary>
        /// 材料
        /// </summary>
        public static string SHOP_TABITEM => LocalizationService.GetString("UI.Shop.TabItem");
        public static string SHOP_TABITEM_Raw => "UI.Shop.TabItem";

        /// <summary>
        /// 消耗品
        /// </summary>
        public static string SHOP_TABPACK => LocalizationService.GetString("UI.Shop.TabPack");
        public static string SHOP_TABPACK_Raw => "UI.Shop.TabPack";

        /// <summary>
        /// 测试
        /// </summary>
        public static string SHOP_TABRECOMMEND => LocalizationService.GetString("UI.Shop.TabRecommend");
        public static string SHOP_TABRECOMMEND_Raw => "UI.Shop.TabRecommend";

        /// <summary>
        /// 装备
        /// </summary>
        public static string SHOP_TABSKIN => LocalizationService.GetString("UI.Shop.TabSkin");
        public static string SHOP_TABSKIN_Raw => "UI.Shop.TabSkin";

        /// <summary>
        /// 折扣
        /// </summary>
        public static string SHOP_TAG_DISCOUNT => LocalizationService.GetString("UI.Shop.Tag.Discount");
        public static string SHOP_TAG_DISCOUNT_Raw => "UI.Shop.Tag.Discount";

        /// <summary>
        /// 热门
        /// </summary>
        public static string SHOP_TAG_HOT => LocalizationService.GetString("UI.Shop.Tag.Hot");
        public static string SHOP_TAG_HOT_Raw => "UI.Shop.Tag.Hot";

        /// <summary>
        /// 材料
        /// </summary>
        public static string SHOP_TAG_ITEM => LocalizationService.GetString("UI.Shop.Tag.Item");
        public static string SHOP_TAG_ITEM_Raw => "UI.Shop.Tag.Item";

        /// <summary>
        /// 新品
        /// </summary>
        public static string SHOP_TAG_NEW => LocalizationService.GetString("UI.Shop.Tag.New");
        public static string SHOP_TAG_NEW_Raw => "UI.Shop.Tag.New";

        /// <summary>
        /// 消耗品
        /// </summary>
        public static string SHOP_TAG_PACK => LocalizationService.GetString("UI.Shop.Tag.Pack");
        public static string SHOP_TAG_PACK_Raw => "UI.Shop.Tag.Pack";

        /// <summary>
        /// 测试
        /// </summary>
        public static string SHOP_TAG_RECOMMEND => LocalizationService.GetString("UI.Shop.Tag.Recommend");
        public static string SHOP_TAG_RECOMMEND_Raw => "UI.Shop.Tag.Recommend";

        /// <summary>
        /// 装备
        /// </summary>
        public static string SHOP_TAG_SKIN => LocalizationService.GetString("UI.Shop.Tag.Skin");
        public static string SHOP_TAG_SKIN_Raw => "UI.Shop.Tag.Skin";

        /// <summary>
        /// 商店
        /// </summary>
        public static string SHOP_TITLE => LocalizationService.GetString("UI.Shop.Title");
        public static string SHOP_TITLE_Raw => "UI.Shop.Title";
    }

    public static class Test
    {
        /// <summary>
        /// 测试
        /// </summary>
        public static string TEST => LocalizationService.GetString("Test.Test");
        public static string TEST_Raw => "Test.Test";
    }
}
