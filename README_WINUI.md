# WutheringWavesSteamHelper v1.2.0 - WinUI 3

## 项目说明

本项目已完全迁移到 WinUI 3 框架，采用现代化的 Fluent Design 风格。

## 技术栈

- **.NET 8.0** + **Windows App SDK 1.5**
- **WinUI 3** - 现代化 UI 框架
- **MVVM 架构** - CommunityToolkit.Mvvm
- **最低系统要求**：Windows 10 1809 (Build 17763)

## 项目结构

```
WutheringWavesSteamHelper.WinUI/
├── Views/              # 页面视图
│   └── WutheringWavesPage.xaml
├── ViewModels/         # 视图模型
│   └── MainViewModel.cs
├── Models/             # 数据模型
│   ├── AppSettings.cs
│   └── GameConfig.cs
├── Services/           # 业务逻辑
│   └── SteamHelper.cs
└── Assets/             # 资源文件
```

## 功能特性

✅ Steam 路径自动检测
✅ SteamLibrary 路径自动检测
✅ 从 SteamDB 获取 BuildID 和 Manifest
✅ 生成 Steam 配置文件（ACF）
✅ 生成启动命令
✅ 支持官方启动器和 WeGame 版本
✅ 设置持久化
✅ 实时日志显示

## 在 Visual Studio 中打开

双击打开解决方案文件：
```
WutheringWavesSteamHelper.WinUI.sln
```

## 构建要求

- Visual Studio 2022 (17.8+)
- Windows 10 SDK (10.0.19041.0)
- Windows App SDK 1.5

## 首次构建

1. 打开解决方案
2. 还原 NuGet 包
3. 选择 x64 平台
4. 按 F5 运行

## 与 v1.1.0 的区别

| 特性 | v1.1.0 (WinForms) | v1.2.0 (WinUI 3) |
|------|-------------------|------------------|
| UI 框架 | WinForms | WinUI 3 |
| 架构模式 | 事件驱动 | MVVM |
| 样式 | 手动绘制 | Fluent Design |
| 性能 | GDI+ | DirectX |
| 现代化程度 | 中 | 高 |

## 已删除的旧文件

- MainForm.cs / MainForm.Designer.cs
- Program.cs (WinForms 版本)
- WutheringWavesSteamHelper.csproj (旧项目文件)
- 所有 WinForms 相关代码

所有功能已完整迁移到 WinUI 3 版本。
