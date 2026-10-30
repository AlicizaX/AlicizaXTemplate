# Localization 模块

详细规则见多语言包的 `README.md`。本工程保留外部 `Config` 目录与普通 Luban 链。多语言源表位于 `Config/Excels/Localization`，导出规则合并在 `Config/config.ini`。运行 `Config/gen-all.bat`（Windows）或 `Config/gen-all.command`（macOS）一次生成普通配置与多语言；Unity 的 **AlicizaX / Config / Generate Config** 菜单调用同一脚本。

`Config/config.ini` 按目录和单表规则选择工作簿。`LocalizationConst_Update.xlsx` 的更新文案输出到 `Client/Assets/Resources/Localization/Update_{locale}.bytes`，启动流程在更新界面前加载；其访问器是 `UpdateLocalizationKey.cs`。其他常量表和普通映射表合并输出到 `Client/Assets/Bundles/Configs/bytes/localization/Game_{locale}.bytes`，热更资源可用后由 `HotfixEntry` 追加；常量访问器是 `LocalizationKey.cs`。语言列表、默认语言和编辑器预览所需的词库路径共用 Resources 下的 `RuntimeOptions.json`。

所有表共用全局 Key 唯一性校验。文案工作簿没有层级列；空单元格表示缺失，返回原始 Key，`""` 表示空字符串。普通 Luban 表的 `text` 引用也在生成时校验。包内编辑器工具自动查找 `RuntimeOptions.json` 并读取已生成的词库，无需解析 `config.ini`。业务 UI 的现有手写赋值仍由业务代码维护，新增 TMP 组件可用 `LocalizedText` 绑定 Key 并在编辑器预览。

普通 Luban `ConfigService` 生成代码通过两个回调设置翻译与语言变化订阅，在配置加载后立即刷新，服务销毁时退订。多语言包本身不依赖项目的 `Game.Config`。构建前运行 `Config/gen-all.bat --check`（Windows）或 `Config/gen-all.command --check`（macOS）；构建语言校验读取已生成的 `RuntimeOptions.json`，构建结束后清理临时语言文件。
