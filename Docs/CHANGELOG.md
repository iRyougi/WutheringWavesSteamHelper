# 更新日志 / Changelog

本文件遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/) 格式。

---

## [1.2.0] - 2026-04-02

### 新增

- **WinUI 3 重构**：完全迁移到 WinUI 3，采用现代化 Fluent Design 风格
- **MVVM 架构**：使用 CommunityToolkit.Mvvm 实现数据绑定和命令模式
- **原生 Fluent Design**：深色主题、Acrylic 材质、流畅动画
- **更好的性能**：DirectX 渲染，更流畅的 UI 响应

### 改进

- 左侧导航栏采用 NavigationView 控件
- 卡片式布局使用原生 WinUI 3 控件
- 文件选择器使用 Windows.Storage.Pickers
- 更好的 DPI 适配和多显示器支持

### 技术

- 目标框架：.NET 8.0 + Windows App SDK 1.5
- 最低系统要求：Windows 10 1809 (Build 17763)
- 项目结构：Views、ViewModels、Models、Services 分层

---

## [1.1.0] - 2026-03-19

### 新增

- **WeGame 支持**：新增 WeGame 版鸣潮路径自动识别（注册表检测），支持生成 WeGame 结构的启动命令
- **版本切换**：界面新增"版本切换"卡片，可在官方启动器与 WeGame 之间切换，切换时自动验证对应版本是否已安装，未安装则自动拨回
- **启动时自动检测**：程序启动时根据本机安装情况自动预设版本选项
- **打开官方启动器**：新增按钮，自动搜索并启动国服鸣潮 `launcher.exe`
- **设置持久化**：路径、Steam ID、BuildID、Manifest、版本选择等设置自动保存至 `%AppData%\WutheringWavesSteamHelper\settings.json`，下次启动自动填入
- **禁止多开**：同一用户会话内只允许运行一个实例，重复启动时自动聚焦已有窗口
- **侧边栏导航**：重构为侧边栏 + 内容区布局，预留碧蓝档案、尘白禁区等游戏入口（敬请期待）

### 改进

- **UI 重构**：采用 Windows 11 风格卡片式布局，侧边栏深色导航 + 内容区白色卡片
- **自适应窗口**：窗口改为可拖动调整大小，内容区随窗口伸缩，日志区自动填充剩余空间
- **DPI 适配**：`AutoScaleMode` 改为 `Dpi`，配合 `PerMonitorV2`，200% 缩放下布局精确缩放不溢出
- **覆盖逻辑优化**：生成配置时，EXE 已存在静默跳过，仅 ACF 文件存在时才询问是否覆盖

### 技术

- 新增 `GameConfig.cs`：多游戏配置数据类，包含 `GameConfigs.All` 静态列表
- 新增 `AppSettings.cs`：基于 `System.Text.Json` 的设置读写
- `Program.cs`：新增 Mutex 单实例控制，`SetForegroundWindow` 聚焦已有窗口

---

## [1.0.0] - 2024-01-01

### 新增

- 首次发布
- 自动检测 Steam 安装路径
- 自动检测 Steam 游戏库路径（支持多个库，读取 `libraryfolders.vdf`）
- 从 SteamDB API 自动获取最新 BuildID 和 Manifest
- 自动生成游戏配置文件（`appmanifest_3513350.acf`）
- 一键生成 Steam 启动命令
- 实时运行日志显示

---

## 版本规划

遵循 [语义化版本](https://semver.org/lang/zh-CN/)：**主版本.次版本.修订号**

| 版本 | 状态 | 说明 |
|------|------|------|
| 1.0.0 | 已发布 | 首个正式版本 |
| 1.1.0 | 已发布 | UI 重构、WeGame 支持、设置持久化 |
| 1.2.0 | 计划中 | 多游戏支持（碧蓝档案、尘白禁区等） |
