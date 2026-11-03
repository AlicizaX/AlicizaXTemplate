<div align="center">

📖 **在线文档：https://b4lie.github.io/AlicizaXWiki/**

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="https://b4lie.github.io/AlicizaXWiki/assets/logo-light.png">
  <img alt="AlicizaX" src="https://b4lie.github.io/AlicizaXWiki/assets/logo.png" width="360">
</picture>

# AlicizaX

**AlicizaX** 是一套为商业化项目打造的 **Unity 游戏框架**。

服务容器、资源管线、UI 管理、事件与计时、对象池、热更，收敛成一套装好即用的骨架。

</div>

## 安装

### 1. 添加 Installer

Unity **Package Manager** → `+` → **Add package from git URL**：

```text
https://github.com/AlicizaX/FramworkInstaller.git
```

### 2. 打开安装器

菜单栏 **`AlicizaX / Installer`**。安装器会自动检测并补齐所需的 OpenUPM Scoped Registry 和 Scopes。

### 3. 安装核心库

点 **`Install Core`** 安装核心包 `com.alicizax.unity.framework`，装好后模板入口才会解锁。

### 4. 选择模板

| 模板 | 适用场景 |
| :--- | :--- |
| **Normal** | 标准项目开发 |
| **Hybrid** | 混合开发模式（含热更工程） |

## 可选扩展包

| 包 | 说明 |
| :--- | :--- |
| `com.alicizax.unity.framework` | 核心框架：服务容器、资源、UI、事件、计时、对象池（必装） |
| `com.alicizax.unity.ui.extension` | UI 扩展组件：`UXButton`、`UXImage`、`RecyclerView` 等 |
| `com.alicizax.unity.editor.extension` | 编辑器扩展工具 |
| `com.alicizax.unity.input` | 输入扩展：设备监听、快捷键、UI 导航、按键图标（依赖 Input System） |
| `com.alicizax.unity.debugger` | 运行时调试面板 |
| `com.alicizax.unity.localization` | 基于 Luban 的多语言支持 |
| `com.alicizax.unity.installer` | 安装器：装核心包、导入模板 |

详细的依赖关系与安装顺序见 **[安装与可选扩展包](https://b4lie.github.io/AlicizaXWiki/install/)**。

## 文档

完整文档在 **[https://b4lie.github.io/AlicizaXWiki/](https://b4lie.github.io/AlicizaXWiki/)**：

| 入口 | 内容 |
| :--- | :--- |
| [快速入门](https://b4lie.github.io/AlicizaXWiki/quickstart/) | 启动链路、场景根节点、资源初始化、第一个 UI |
| [概览](https://b4lie.github.io/AlicizaXWiki/overview/) | 模块能力一览与上手路径 |
| [安装与扩展包](https://b4lie.github.io/AlicizaXWiki/install/) | 安装步骤、可选扩展包与依赖 |
| [UI 模块](https://b4lie.github.io/AlicizaXWiki/modules/ui/) | 窗口、元数据与生命周期、路由转场、Widget |
| [Resources](https://b4lie.github.io/AlicizaXWiki/modules/resource/) | 租约、作用域、资源包与回收 |
| [UI Extension](https://b4lie.github.io/AlicizaXWiki/extension/) | UX 组件、RecyclerView、输入与导航 |
