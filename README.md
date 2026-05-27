# WutheringWavesSteamHelper

鸣潮 Steam 助手 - 用于在 Steam 库中添加鸣潮游戏，实现 Steam 启动器管理。
此处说明不一定会及时更新，详细文档请访问[鸣潮Steam助手帮助文档](https://www.iryougi.com/index.php/wutheringwavessteamhelper/)

## 简介

WutheringWavesSteamHelper 是一个通过生成必要的配置文件，让玩家可以通过 Steam 客户端启动和管理国服鸣潮。

自 v2.2.0 起，应用同时支持「自定义 Manifest」页面，允许用户为任意 Steam 游戏自定义 AppID / DepotID / BuildID / Manifest 等字段，生成对应 ACF 配置并复制启动命令。

## 主要功能

- **鸣潮专属页面**：一键生成鸣潮 ACF 配置 / 复制国服客户端启动命令 / 打开官方启动器
- **自定义 Manifest 页（v2.2.0 新增）**：自由填写任意游戏的 AppID/DepotID/BuildID/Manifest，生成 `appmanifest_<AppID>.acf`
- **Steam 全局配置**：Steam 安装路径、SteamLibrary 路径、SteamID 在「设置」中统一管理，全应用共享

## 系统要求

- Windows 操作系统（Windows 10/11）基于x64
- .NET 8.0 运行时
- Visual Studio Community 2022, 17.14.29 (March 2026)
- 已安装 Steam 客户端
- Steam游戏库内已入库鸣潮游戏

## 技术说明

- **框架**：.NET 8 WinUI 3（Windows App SDK）
- **开发**：iRyougi
- **游戏 AppID**：3513350
- **主要 Depot**：3513351

## 免责声明

本工具仅用于学习和研究目的。使用本工具产生的任何问题，开发者不承担责任。请支持正版游戏。

## 许可证

本项目遵循开源协议，具体请查看 LICENSE 文件。
