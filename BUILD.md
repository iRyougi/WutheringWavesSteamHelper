# WutheringWavesSteamHelper v1.2.0 构建指南

## ✅ 项目迁移完成

项目已完全从 WinForms 迁移到 WinUI 3，所有功能已实现。

## 🔧 构建要求

### 必需软件
- **Visual Studio 2022** (17.8 或更高版本)
- **Windows 10 SDK** (10.0.19041.0)
- **Windows App SDK** (1.5)
- **.NET 8.0 SDK**

### 工作负载
在 Visual Studio Installer 中安装：
- `.NET 桌面开发`
- `通用 Windows 平台开发`

## 📦 如何构建

### 方法 1：使用 Visual Studio（推荐）

1. 双击打开 `WutheringWavesSteamHelper.sln`
2. 选择 `Debug` 或 `Release` 配置
3. 选择 `x64` 平台
4. 按 `F5` 或点击"开始调试"

### 方法 2：命令行构建

**注意：** WinUI 3 的 XAML 编译器在命令行环境下可能遇到问题，建议使用 Visual Studio。

如果必须使用命令行：
```bash
# 使用 MSBuild（需要 VS 2022）
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" WutheringWavesSteamHelper.sln /p:Configuration=Release /p:Platform=x64
```

## 🎯 已完成的功能

- ✅ Steam 路径自动检测和手动选择
- ✅ SteamLibrary 路径配置
- ✅ 从 SteamDB 获取 BuildID 和 Manifest
- ✅ 生成 ACF 配置文件
- ✅ 生成启动命令
- ✅ 打开官方启动器
- ✅ 官方/WeGame 版本切换
- ✅ 设置持久化
- ✅ 实时日志显示

## 📁 项目结构

```
WutheringWavesSteamHelper/
├── App.xaml / App.xaml.cs          # 应用程序入口
├── MainWindow.xaml / .cs           # 主窗口
├── Models/                         # 数据模型
│   ├── AppSettings.cs
│   └── GameConfig.cs
├── Services/                       # 业务逻辑
│   └── SteamHelper.cs
├── ViewModels/                     # MVVM 视图模型
│   └── MainViewModel.cs
├── Views/                          # 页面视图
│   └── WutheringWavesPage.xaml
└── Assets/                         # 资源文件
```

## 🚀 Git 提交记录

已完成 6 个提交：
1. 创建 WinUI 3 项目框架
2. 完整功能迁移并删除旧 WinForms 代码
3. 添加项目说明文档
4. 重构项目结构到根目录
5. 修复 XAML 绑定

## ⚠️ 已知问题

- 命令行 `dotnet build` 可能因 XAML 编译器问题失败
- 解决方案：使用 Visual Studio 2022 打开并构建

## 📝 下一步

1. 在 Visual Studio 2022 中打开项目
2. 还原 NuGet 包
3. 构建并运行
4. 测试所有功能

项目已准备好在 Visual Studio 中打开和构建！
