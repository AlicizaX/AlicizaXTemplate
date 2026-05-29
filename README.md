<div align="center">

![AlicizaX Logo](Books/src/AlicizaXLogo.png)

**模块化 Unity 框架模板，覆盖启动流程、热更新、资源管理与 UI 工作流。**

[![Unity Version](https://img.shields.io/badge/Unity-2022.3.20%2B-blue.svg?style=flat-square)](https://unity.com/)
[![License](https://img.shields.io/github/license/AlicizaX/AlicizaXTemplate?style=flat-square)](https://github.com/AlicizaX/AlicizaXTemplate)
[![Last Commit](https://img.shields.io/github/last-commit/AlicizaX/AlicizaXTemplate?style=flat-square)](https://github.com/AlicizaX/AlicizaXTemplate)
[![Issues](https://img.shields.io/github/issues/AlicizaX/AlicizaXTemplate?style=flat-square)](https://github.com/AlicizaX/AlicizaXTemplate/issues)
[![Top Language](https://img.shields.io/github/languages/top/AlicizaX/AlicizaXTemplate?style=flat-square)](https://github.com/AlicizaX/AlicizaXTemplate)

</div>

## 简介

AlicizaX 是一套面向 Unity 项目的框架模板，围绕启动流程、资源管理、UI 开发、热更新、对象池、事件、计时器等常用模块做了封装。项目目标是提供一套结构清晰、易于接入、方便裁剪的基础工程，而不是把所有业务形态都固化进框架。
- Unity | 2022.3.x 或更高版本

## 主要能力

- **0 GC 高频事件系统**：每种事件类型拥有独立静态容器，使用 `struct` 事件参数、`in` 参数派发、版本化 `EventRuntimeHandle`、空闲槽复用和 packed callback 数组。发布阶段走紧凑数组遍历、swap-remove 退订和循环展开，适合下载进度、战斗刷新、状态同步等高频通知，并能和 UI 生命周期自动解绑机制无缝结合。
- **四级时间轮 Timer**：内置 4 层 x 256 桶的 hierarchical timing wheel，同时维护 scaled / unscaled 两套时间队列。计时器使用分页 slot 分配和版本化 `ulong` 句柄，添加、暂停、恢复、删除都不需要每帧全量扫描，适合技能冷却、心跳、倒计时、超时控制和大量 UI 延迟任务。
- **重构版 MemoryPool**：使用分页 slab、slot generation 校验、free / empty / evict 队列、tombstone 清理和 native metadata 管理对象生命周期。运行时通过 EWMA 速率、水位预测、快慢峰值和分阶段增长/回收预算动态调整池容量，在低分配和内存回收之间取得平衡。
- **生命周期 ObjectPool**：面向带目标对象的 `ObjectBase` 包装器，内部使用分页槽位、空闲页栈、unused 双向链表、名称索引和引用目标索引管理对象。支持 Register、Spawn、Unspawn、锁定、多次 Spawn、过期释放、低内存释放和按帧释放预算，适合资源对象、音频对象、子弹、敌人、特效等运行时复用。
- **高性能 UI 架构**：窗口、层级、缓存、Widget、Tab、异步打开和同步打开统一封装，Holder 自动生成后天然实现逻辑与视图分离。窗口逻辑只处理业务，视图引用由生成代码维护，同一份视图可以在窗口、分页、列表项、弹窗和子 Widget 中自由复用。
- **RecyclerView 虚拟列表**：列表组件基于可见区间增量刷新、ViewHolder 对象池、模板复用和布局管理器工作，支持普通列表、混合模板、循环列表、分组列表、网格、分页、圆形布局、吸附和惯性滚动。异步绑定可用 binding version 防止旧回调污染新数据。
- **强封装 UI 扩展能力**：提供 `UXButton`、`UXToggle`、`UXImage`、`UXTextMeshPro`、拖拽、导航、输入图标等组件，覆盖复杂交互、多端输入和运行时按键图标显示，减少业务层重复处理 UI 边界问题。
- **模块化服务体系**：通过 `AppServices` 和 `GameApp` 统一注册、获取和驱动运行时服务，让资源、UI、音频、场景、本地化、事件、计时器和对象池保持清晰边界。
- **标准化启动与资源流程**：使用 `Procedure` 串联版本检查、资源初始化、资源下载、热更程序集加载和入口跳转，并基于 YooAsset 封装资源包初始化、加载、实例化、下载器和回收。
- **HybridCLR 与工程化模板**：集成 AOT + Hotfix 分层示例、热更程序集加载、入口反射调用、构建脚本、资源构建入口和 Luban 配置表工程结构，方便在模板基础上继续扩展业务框架。

## 新工程安装

在一个新的 Unity 工程中，先通过 Unity Package Manager 的 Git URL 方式安装安装器包：

```text
https://github.com/AlicizaX/FramworkInstaller.git
```

安装完成后，点击 Unity 菜单 `AlicizaX/Installer` 打开安装器。安装器会先自动检查并补齐所需的 OpenUPM scoped registry 与 scopes，然后点击 **Install Core** 安装 `com.alicizax.unity.framework`。Core 安装完成后，安装器会解锁 Normal / Hybrid 模板导入入口，再根据项目需要安装对应模板。

## 快速开始

建议从框架快速入门开始阅读：

- [QuickStart 快速入门](Books/QuickStart.md)
- [部分UI、多语言、使用案例已更新 更新到本地查看具体效果](https://github.com/AlicizaX/AlicizaXTemplate/tree/main/Client/Assets/Scripts/Hotfix/GameLogic)

如果只想了解启动场景需要挂哪些组件，可以先看：

- [Service 基础服务](Books/Service.md)
- [Procedure 流程模块](Books/Procedure.md)
- [Resources 资源模块](Books/Resources.md)
- [UI 模块](Books/UI.md)
- [UI Extension 扩展包](Books/Extension/README.md)

## 文档导航

### 入门与基础

| 文档 | 内容 |
| --- | --- |
| [QuickStart](Books/QuickStart.md) | 启动链路、场景组件、资源初始化、热更入口和常用模块调用 |
| [Service](Books/Service.md) | 服务容器、服务作用域、自定义服务、Tick 驱动 |
| [Procedure](Books/Procedure.md) | 流程状态机、启动流程、异步流程写法 |
| [Debugger](Books/Debugger.md) | 运行时调试面板、内置调试窗口、自定义调试页 |

### 资源与对象管理

| 文档 | 内容 |
| --- | --- |
| [Resources](Books/Resources.md) | YooAsset 初始化、资源加载、下载器、资源回收 |
| [ObjectPool](Books/ObjectPool.md) | 普通对象池、对象生命周期、释放策略 |
| [GameObjectPool](Books/GameObjectPool.md) | GameObject 实例池、预制体加载、实例回收 |
| [MemoryPool](Books/MemoryPool.md) | 引用对象内存池、严格检查、容量管理 |

### 业务常用模块

| 文档 | 内容 |
| --- | --- |
| [UI](Books/UI.md) | UI 窗口、Holder 生成、Widget、Tab、UI 事件管理 |
| [Audio](Books/Audio.md) | 音效、音乐、3D 声音、音量分组 |
| [Scene](Books/Scene.md) | 场景加载、挂起、激活、卸载 |
| [Localization](Books/Localization.md) | 本地化表、语言切换、变更事件 |
| [Timer](Books/Timer.md) | 延迟执行、循环计时器、暂停恢复、容量预热 |
| [Event](Books/Event.md) | 事件总线、订阅发布、事件句柄释放 |

### UI 扩展包

| 文档 | 内容 |
| --- | --- |
| [Extension](Books/Extension/README.md) | `com.alicizax.unity.ui.extension` 包总览、模块划分、编辑器入口和接入前提 |
| [UXComponent](Books/Extension/UXComponent.md) | `UXButton`、`UXToggle`、`UXImage`、`UXTextMeshPro`、`UXController`、快捷键和拖拽组件 |
| [RecyclerView](Books/Extension/RecyclerView.md) | 虚拟列表、`ViewHolder`、`ItemRender`、普通列表、混合模板列表、循环列表和分组列表 |
| [InputGlyph](Books/Extension/InputGlyph.md) | Input System 按键图标、设备识别、输入读取和运行时按键重绑定 |

## 项目结构

```text
Aliciza/
├── Books/                         # 框架文档和图片资源
│   └── Extension/                 # UI 扩展包文档
├── Client/                        # Unity 客户端工程
│   ├── Assets/
│   │   ├── Art/                   # 美术资源
│   │   ├── Bundles/               # 热更资源目录
│   │   │   ├── Audios/            # 音频资源
│   │   │   ├── Configs/           # 配置和本地化资源
│   │   │   ├── DLL/               # 热更程序集资源
│   │   │   ├── Scenes/            # 资源场景
│   │   │   ├── UI/                # UI 预制体
│   │   │   └── UIRaw/             # UI 原始图片资源
│   │   ├── Editor/                # 项目编辑器脚本
│   │   ├── HybridCLRGenerate/     # HybridCLR 生成内容
│   │   ├── Scenes/                # 启动场景
│   │   ├── Scripts/
│   │   │   ├── Startup/           # AOT 启动程序集
│   │   │   └── Hotfix/            # 热更程序集
│   │   │       ├── GameBase/
│   │   │       ├── GameLib/
│   │   │       ├── GameLogic/
│   │   │       └── GameProto/
│   │   └── YooAsset/              # YooAsset 配置
│   └── Packages/
│       ├── com.alicizax.unity.framework/
│       ├── com.alicizax.unity.ui.extension/
│       └── ...
└── Config/                        # 配置表工程
```

## 推荐阅读顺序

1. [QuickStart](Books/QuickStart.md)
2. [Service](Books/Service.md)
3. [Resources](Books/Resources.md)
4. [Procedure](Books/Procedure.md)
5. [UI](Books/UI.md)
6. [UI Extension](Books/Extension/README.md)
7. 按业务需要继续阅读 Audio、Scene、Localization、Timer、Event、ObjectPool 等模块文档。

## 🌟 开源项目推荐

| 项目            | 描述                                         | 链接 |
|---------------|--------------------------------------------|------|
| **TEngine**   | 本框架源于此鼻祖框架 性能强大、易用、设计优秀 上手使用很方便 有多款已上线商业项目验证 ⭐⭐⭐⭐⭐     | [GitHub](https://github.com/Alex-Rachel/TEngine) |
| **YooAsset**  | 商业级经历百万 DAU 游戏验证的资源管理系统                    | [GitHub](https://github.com/tuyoogame/YooAsset) |
| **HybridCLR** | 特性完整、零成本、高性能、低内存的近乎完美的 Unity 全平台原生 C# 热更方案 | [GitHub](https://github.com/focus-creative-games/hybridclr) |
| **Luban**     | 最佳游戏配置解决方案                                 | [GitHub](https://github.com/focus-creative-games/luban) |
| **Fantasy**   | 源于 ETServer 但极为简洁，更好上手的商业级服务器框架            | [GitHub](https://github.com/qq362946/Fantasy) |



## 贡献

欢迎提交 Issue 或 Pull Request。提交前建议先说明问题背景、复现步骤或改动范围，方便讨论和 review。

## 致谢

感谢所有参与 AlicizaX 的开发者和反馈者。
[![Contributors](https://contrib.rocks/image?repo=AlicizaX/AlicizaXTemplate)](https://github.com/AlicizaX/AlicizaXTemplate/graphs/contributors)
