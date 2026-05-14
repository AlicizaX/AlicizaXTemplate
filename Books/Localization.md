# Localization 模块

Localization 模块负责维护当前语言、缓存本地化字符串，并配合配置生成工具把 Luban 的 `text` 字段转换成运行时可直接读取的文本值。当前方案是普通配置表只导出一份，多语言文本单独按语言导出；切换语言时只重载本地化表，并刷新普通配置中已经生成的 `text` 字段。

源码位置：

- `Client/Packages/com.alicizax.unity.framework/Runtime/Localization/Service`
- `Client/Assets/Scripts/Hotfix/GameProto/Config/LocalizationConst`
- `Client/Assets/Scripts/Hotfix/GameLogic/LocalizationKey.cs`

配置和工具位置：

- `Config/Excels/Localization/Localization.xlsx`
- `Config/Excels/Localization/LocalizationConst.xlsx`
- `Config/config.ini`
- `Config/CustomeTools/ConfigGenerate`
- `Config/CustomeTools/ConfigGenerateTool`
- `Config/CustomeTools/ClientTemplate`
- `Config/CustomeTools/ClientTemplateLazy`

## 使用前提

本地化服务需要先初始化当前语言，再由配置服务加载配置。配置加载完成后，`ConfigService` 会把 `TbLocalization` 和 `TbLocalizationConst` 的内容写入 `LocalizationService` 的字符串缓存。

普通配置里的 Luban `text` 字段应保存本地化 key，不直接保存具体语言文本。语言文本统一放在 `Localization.xlsx` 或 `LocalizationConst.xlsx` 中。

当前热更入口会注册 `IConfigService`，并保存到 `HotfixEntry.ConfigService`。业务代码也可以在服务注册完成后通过 `AppServices.App.Require<IConfigService>()` 获取配置服务。

## 配置表分工

`Localization.xlsx` 是普通 `text` 字段的文本映射表。普通配置表中填的是 key，生成后运行时会通过该表翻译成当前语言文本。

`LocalizationConst.xlsx` 是多语言常量表，用于导出：

- `tables_tblocalizationconst_{Language}.bytes`
- `Assets/Editor/Config/LocalizationConst.json`
- `Assets/Scripts/Hotfix/GameLogic/LocalizationKey.cs`

多语言常量表使用自定义表头，第一行直接是字段名：

```text
ID | key | ChineseSimplified | English | Japanese
```

不需要 Luban 的 `##var`、`type`、`group`、注释行。工具会读取 `Hotfix`、`Main` 两个 sheet；缺少其中某个 sheet 时会输出 warning 并跳过。

## 生成配置

生成入口在 `Config/gen-all.bat` 和 `Config/gen-all.sh`，内部通过 dotnet 调用：

```bash
dotnet ./CustomeTools/ConfigGenerate/ConfigGenerate.dll config.ini ClientTemplate
```

Windows 下也可以直接执行：

```bat
gen-all.bat ClientTemplate
gen-all.bat ClientTemplateLazy
```

Unity 编辑器里可以通过 `Gen-Config` 选择普通模板或 Lazy 模板生成配置；通过 `Open-LocalizationConfig` 可以打开 `Config/Excels/Localization` 目录。

`config.ini` 中和本地化相关的配置：

| 配置 | 说明 |
| --- | --- |
| `table_data_out` | 普通配置表二进制输出目录 |
| `l10n_text_out` | 多语言文本表输出目录 |
| `l10n_const_out` | 多语言常量表输出目录 |
| `l10n_text_xlsx` | `Localization.xlsx` 路径 |
| `l10n_const_xlsx` | `LocalizationConst.xlsx` 路径 |
| `l10n_editor_out` | 编辑器辅助 JSON 输出目录 |
| `l10n_key_code_out` | `LocalizationKey.cs` 输出路径 |
| `l10n_key_comment_language` | `LocalizationKey.cs` 注释使用的语言列 |
| `l10n_const_out_rule` | `LocalizationKey.cs` 生成前缀白名单，空数组表示全部导出 |

语言列表写在 `[languages]` 下，语言名必须和 Excel 中的语言列名一致。

生成后的多语言文件不按语言目录拆分，而是按文件名区分：

```text
Assets/Bundles/Configs/bytes/tables_tblocalization_ChineseSimplified.bytes
Assets/Bundles/Configs/bytes/tables_tblocalization_English.bytes
Assets/Bundles/Configs/bytes/tables_tblocalization_Japanese.bytes
Assets/Bundles/Configs/bytes/tables_tblocalizationconst_ChineseSimplified.bytes
Assets/Bundles/Configs/bytes/tables_tblocalizationconst_English.bytes
Assets/Bundles/Configs/bytes/tables_tblocalizationconst_Japanese.bytes
```

## LocalizationKey 规则

`LocalizationKey.cs` 根据 `LocalizationConst.xlsx` 生成。业务层推荐优先使用它，因为它返回的是当前语言文本，不是 key。

```csharp
baseui.TxtTitle.text = LocalizationKey.UI.SHOP_TITLE;
baseui.TxtPrice.text = LocalizationKey.UI.COMMON_CREDITPRICE(price);
```

如果 key 对应文本里包含 `{0}`、`{1}` 这类格式化参数，生成结果会是方法：

```csharp
string text = LocalizationKey.Log.BUY_CONFIRMED(itemName, count, totalPrice);
```

如果需要原始 key，可以使用生成的 `_Raw` 属性：

```csharp
string key = LocalizationKey.UI.SHOP_TITLE_Raw;
```

key 到 C# 成员名的规则：

1. key 必须至少包含一个 `.`，第一个段作为嵌套类名。
2. 第一个段保持原大小写，例如 `UI.Shop.Title` 生成 `LocalizationKey.UI`。
3. 后续段合并成成员名，`.` 替换为 `_`。
4. 空白会被移除。
5. 小写字母会转为大写，例如 `UI.Shop.ButtonClose` 生成 `SHOP_BUTTONCLOSE`。
6. 出现不能作为 C# 类名或变量名的字符时，工具会输出 warning，包含 sheet、行、列和具体 key，并跳过该行。

`l10n_const_out_rule` 只过滤 `LocalizationKey.cs` 的生成范围，不影响 `tables_tblocalizationconst_{Language}.bytes` 导出。示例：

```ini
l10n_const_out_rule=[Log.,System.]
```

这表示只生成 key 以 `Log.` 或 `System.` 开头的常量代码。`UI.Shop.Title` 仍会导出到 bytes 和编辑器 JSON，但不会出现在 `LocalizationKey.cs` 中。

如果生成期间没有任何可导出的有效 key，工具不会写出空的 `LocalizationKey.cs`。

## 运行时读取

直接按 key 读取：

```csharp
string title = GameApp.Localization.GetString("UI.Shop.Title");
string price = GameApp.Localization.GetString("UI.Common.CreditPrice", value);
```

推荐使用生成常量：

```csharp
baseui.TxtTitle.text = LocalizationKey.UI.SHOP_TITLE;
baseui.TxtPrice.text = LocalizationKey.UI.COMMON_CREDITPRICE(value);
```

读取原始字符串时可以使用：

```csharp
if (GameApp.Localization.TryGetRawString("UI.Shop.Title", out string value))
{
    baseui.TxtTitle.text = value;
}
```

`GetString` 找不到 key 时会直接返回 key 本身，便于在界面上暴露缺失配置。

## 切换语言

切换语言需要按顺序完成三件事：

```csharp
GameApp.Localization.SwitchLanguage(language);
await HotfixEntry.ConfigService.SwitchLanguageAsync();
GameApp.Localization.ApplyLanguage();
```

`SwitchLanguage` 只负责设置当前语言并保存语言偏好，不会自己重载配置表。

`ConfigService.SwitchLanguageAsync()` 会触发本地化表重载，只重载 `TbLocalization` 和 `TbLocalizationConst`。普通配置表不会重新读取 bytes，也不会整套重载。

本地化表重载完成后，`ConfigService` 会调用 `RefreshLocalizationService()`，把 `TbLocalization` 和 `TbLocalizationConst` 合并写入 `LocalizationService` 的缓存。随后普通配置中由 Luban `text` 生成的字段会通过 `PostTranslateText` 刷新为当前语言文本。业务再次读取普通配置字段时拿到的就是刷新后的值，不需要每次都手动查语言表。

`ApplyLanguage` 只负责发布语言变化事件，应该放在语言表和配置字段刷新完成之后调用，让 UI 或业务在事件回调里重新绑定显示内容。

## 普通配置 text 字段

普通配置表中使用 Luban `text` 类型时，表内填写本地化 key。例如某个商品名字段填：

```text
UI.Shop.Goods.Item.Medkit.Name
```

配置生成时普通配置仍然只导出一份。运行时加载普通配置后，生成代码会通过 `PostTranslateText` 把 key 翻译成当前语言文本。切换语言时也只刷新这些本地化字段，不会重新加载所有普通配置表。

这类字段的业务用法就是直接读取配置字段：

```csharp
baseui.TxtName.text = goodsConfig.Name;
baseui.TxtDesc.text = goodsConfig.Desc;
```

不要在业务层对这些已经翻译过的字段再次调用 `GetString`。

## API 速查

| API | 说明 |
| --- | --- |
| `GameApp.Localization.Language` | 当前语言 |
| `GameApp.Localization.Initialize(language)` | 初始化语言并保存偏好 |
| `GameApp.Localization.SwitchLanguage(language)` | 修改当前语言并保存偏好，不重载表 |
| `HotfixEntry.ConfigService.SwitchLanguageAsync()` | 重载本地化表并刷新普通配置的 `text` 字段 |
| `GameApp.Localization.ApplyLanguage()` | 发布语言变化事件 |
| `GameApp.Localization.GetString(key)` | 获取当前语言文本 |
| `GameApp.Localization.GetString(key, args...)` | 获取并格式化当前语言文本 |
| `GameApp.Localization.TryGetRawString(key, out value)` | 尝试获取未格式化文本 |
| `GameApp.Localization.ReplaceRawStrings(strings)` | 替换当前语言字符串缓存 |
| `HotfixEntry.ConfigService.RefreshLocalizationService()` | 将本地化表内容写入本地化服务缓存 |

## 注意事项

1. 普通配置表和多语言表是分离的；切语言只重载 `TbLocalization` 和 `TbLocalizationConst`。
2. `SwitchLanguage` 不会自动触发 `SwitchLanguageAsync`，业务流程必须显式调用配置服务的切语言接口。
3. 语言变化事件应在本地化表和普通配置字段刷新后再发布。
4. `LocalizationKey` 属性和方法返回当前语言文本，`_Raw` 才返回 key。
5. 普通配置中的 `text` 字段被刷新后应直接使用，不要重复调用 `GetString`。
6. `LocalizationConst.xlsx` 的 key 需要兼容 C# 变量名生成规则；非法 key 会被跳过。
7. 修改 Excel、`config.ini`、模板或生成工具后，需要重新执行 `gen-all`。
