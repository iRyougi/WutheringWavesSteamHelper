# WutheringWavesSteamHelper WinUI 3 重构计划 v2.0.0

## 📋 项目概述

### 原项目分析
- **技术栈**: .NET 8.0 + Windows Forms
- **版本**: v1.1.0
- **核心功能**:
  1. Steam 路径自动检测（注册表 + 常见路径）
  2. Steam 游戏库路径检测（支持多库）
  3. 从 SteamDB API 获取 BuildID 和 Manifest
  4. 生成 appmanifest_3513350.acf 配置文件
  5. 支持官方启动器和 WeGame 两种游戏源
  6. 生成 Steam 启动命令
  7. 设置持久化（JSON）

### 重构目标
- **UI框架**: 从 Windows Forms 迁移到 WinUI 3
- **版本号**: v2.0.0
- **功能范围**: 保持所有原有功能不变
- **UI改进**: 采用现代化设计，提升用户体验
- **架构**: 采用 MVVM 模式，提高代码可维护性

---

## 🎨 UI设计方案

### 设计风格
参考 Windows 11 Fluent Design 和现代应用设计：
- **Mica 材质背景**: 半透明毛玻璃效果
- **圆角卡片**: 内容区域使用卡片布局
- **流畅动画**: 页面切换和交互动画
- **自适应布局**: 支持窗口大小调整
- **深色/浅色主题**: 跟随系统主题

### 布局结构
```
┌─────────────────────────────────────────┐
│  [标题栏]                    [- □ ×]    │
├──────────┬──────────────────────────────┤
│          │                              │
│  导航栏  │        主内容区              │
│          │                              │
│  鸣潮    │   [配置卡片]                 │
│  敬请期待│   [操作按钮]                 │
│          │   [日志输出]                 │
│          │                              │
└──────────┴──────────────────────────────┘
```

### 页面组件
1. **NavigationView**: 左侧导航栏（游戏列表）
2. **配置区域**:
   - Steam 路径配置卡片
   - 游戏库路径配置卡片
   - Steam ID 输入卡片
   - BuildID/Manifest 配置卡片
3. **操作区域**:
   - 主操作按钮（生成配置）
   - 辅助按钮（获取信息、复制命令）
4. **日志区域**:
   - 可滚动的日志输出
   - 支持复制和清空

---

## 🏗️ 技术架构

### 项目结构
```
WetheringWavesSteamHelper_WinUI/
├── App.xaml                          # 应用程序入口
├── App.xaml.cs
├── Package.appxmanifest              # 应用清单
│
├── Views/                            # 视图层（XAML页面）
│   ├── MainWindow.xaml               # 主窗口
│   ├── Pages/
│   │   ├── WutheringWavesPage.xaml  # 鸣潮配置页面
│   │   └── PlaceholderPage.xaml     # 占位页面
│   └── Controls/                     # 自定义控件
│       ├── PathConfigCard.xaml      # 路径配置卡片
│       ├── SteamIdCard.xaml         # Steam ID卡片
│       └── LogViewer.xaml           # 日志查看器
│
├── ViewModels/                       # 视图模型层（MVVM）
│   ├── MainViewModel.cs              # 主窗口VM
│   ├── WutheringWavesViewModel.cs   # 鸣潮页面VM
│   └── Base/
│       └── ViewModelBase.cs         # VM基类
│
├── Services/                         # 服务层（业务逻辑）
│   ├── SteamService.cs              # Steam相关服务
│   ├── GameConfigService.cs         # 游戏配置服务
│   ├── SettingsService.cs           # 设置持久化服务
│   └── LogService.cs                # 日志服务
│
├── Models/                           # 数据模型
│   ├── AppSettings.cs               # 应用设置
│   ├── GameConfig.cs                # 游戏配置
│   └── SteamLibrary.cs              # Steam库信息
│
├── Helpers/                          # 辅助类
│   ├── RegistryHelper.cs            # 注册表操作
│   └── FileHelper.cs                # 文件操作
│
└── Assets/                           # 资源文件
    ├── Icons/                        # 图标
    └── Images/                       # 图片
```

### MVVM架构设计

#### 数据流向
```
View (XAML) ←→ ViewModel ←→ Service ←→ Model
     ↑              ↑           ↑
  数据绑定      命令绑定    业务逻辑
```

#### 核心组件职责
- **View**: 纯UI展示，通过数据绑定与ViewModel交互
- **ViewModel**: 处理UI逻辑，暴露属性和命令给View
- **Service**: 封装业务逻辑（Steam检测、文件生成等）
- **Model**: 数据结构定义

---

## 📦 依赖包

### NuGet包列表
```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260317003" />
<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.28000.1721" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
<PackageReference Include="Microsoft.Win32.Registry" Version="5.0.0" />
```

### 包说明
- **WindowsAppSDK**: WinUI 3核心框架
- **CommunityToolkit.Mvvm**: MVVM工具包（简化ViewModel开发）
- **Microsoft.Win32.Registry**: 注册表访问

---

## 🔄 代码迁移映射

### 核心功能迁移

| 原Windows Forms | 新WinUI 3 | 说明 |
|----------------|-----------|------|
| `SteamHelper.cs` | `Services/SteamService.cs` | 保持逻辑不变，重构为服务 |
| `AppSettings.cs` | `Services/SettingsService.cs` + `Models/AppSettings.cs` | 分离数据和逻辑 |
| `GameConfig.cs` | `Models/GameConfig.cs` + `Services/GameConfigService.cs` | 分离配置和服务 |
| `MainForm.cs` | `ViewModels/WutheringWavesViewModel.cs` | UI逻辑迁移到ViewModel |
| `MainForm.Designer.cs` | `Views/Pages/WutheringWavesPage.xaml` | 控件布局用XAML重写 |
| `ToggleSwitch.cs` | WinUI 3内置 `ToggleSwitch` | 使用原生控件 |
| `LaunchCommandDialog.cs` | `ContentDialog` | 使用WinUI 3对话框 |
| `LibrarySelectionDialog.cs` | `ContentDialog` | 使用WinUI 3对话框 |

---

## 🎯 实施阶段

### Phase 1: 项目初始化（1-2天）
**目标**: 创建WinUI 3项目骨架

**任务清单**:
- [ ] 创建WinUI 3空白应用项目
- [ ] 配置项目属性（版本号v2.0.0、图标等）
- [ ] 添加NuGet依赖包
- [ ] 创建基础目录结构
- [ ] 配置MVVM基础设施（ViewModelBase）
- [ ] 迁移Assets资源文件

**验收标准**:
- 项目可编译运行
- 显示空白主窗口
- 目录结构完整

### Phase 2: 服务层迁移（2-3天）
**目标**: 迁移核心业务逻辑

**任务清单**:
- [ ] 迁移 `SteamHelper.cs` → `SteamService.cs`
  - Steam路径检测
  - 游戏库检测
  - SteamDB API调用
  - ACF文件生成
- [ ] 创建 `SettingsService.cs`
  - JSON序列化/反序列化
  - 配置读写
- [ ] 创建 `GameConfigService.cs`
  - 游戏配置管理
  - 启动命令生成
- [ ] 创建 `LogService.cs`
  - 日志记录和管理
- [ ] 单元测试（可选）

**验收标准**:
- 所有服务方法可独立调用
- 功能与原版一致
- 无UI依赖

### Phase 3: UI框架搭建（2-3天）
**目标**: 构建主窗口和导航结构

**任务清单**:
- [ ] 设计主窗口布局（MainWindow.xaml）
  - 配置NavigationView
  - 设置Mica背景
  - 自定义标题栏
- [ ] 创建鸣潮配置页面（WutheringWavesPage.xaml）
  - 基础布局框架
  - 卡片容器
- [ ] 创建占位页面（PlaceholderPage.xaml）
- [ ] 实现页面导航逻辑
- [ ] 配置主题资源（颜色、字体、圆角等）

**验收标准**:
- 导航栏可切换页面
- UI符合设计稿
- 支持深色/浅色主题

### Phase 4: ViewModel和数据绑定（2-3天）
**目标**: 实现MVVM数据绑定

**任务清单**:
- [ ] 创建 `WutheringWavesViewModel.cs`
  - 属性定义（路径、ID、BuildID等）
  - 命令定义（检测、生成、获取等）
  - 日志集合
- [ ] 实现数据绑定
  - TextBox双向绑定
  - Button命令绑定
  - ListView日志绑定
- [ ] 实现命令逻辑
  - 调用Service层方法
  - 更新UI状态
  - 错误处理

**验收标准**:
- 所有UI控件正确绑定
- 命令可正常执行
- 数据流转正确

### Phase 5: UI组件实现（3-4天）
**目标**: 实现所有UI组件和交互

**任务清单**:
- [ ] 路径配置卡片
  - 自动检测按钮
  - 手动浏览按钮
  - 路径显示和验证
- [ ] Steam ID配置卡片
  - 输入框
  - 验证提示
- [ ] BuildID/Manifest配置卡片
  - 自动获取按钮
  - 手动输入框
  - 加载状态
- [ ] 游戏源选择（官方/WeGame）
  - RadioButtons
  - 动态切换逻辑
- [ ] 操作按钮区
  - 生成配置按钮
  - 复制启动命令按钮
  - 状态反馈
- [ ] 日志查看器
  - 滚动列表
  - 清空按钮
  - 复制功能

**验收标准**:
- 所有组件可交互
- UI响应流畅
- 错误提示友好

### Phase 6: 功能测试和优化（2-3天）
**目标**: 确保功能完整性和用户体验

**任务清单**:
- [ ] 功能测试
  - Steam路径检测（多种场景）
  - 游戏库检测（单库/多库）
  - SteamDB API获取
  - ACF文件生成
  - 启动命令生成
  - 设置保存/加载
- [ ] 边界情况处理
  - 网络异常
  - 路径不存在
  - 权限不足
  - 无效输入
- [ ] UI优化
  - 动画效果
  - 加载状态
  - 响应式布局
- [ ] 性能优化
  - 异步操作
  - UI线程优化

**验收标准**:
- 所有功能与原版一致
- 无崩溃和卡顿
- 用户体验流畅

### Phase 7: 打包和发布（1天）
**目标**: 准备发布版本

**任务清单**:
- [ ] 配置MSIX打包
- [ ] 设置应用图标和启动画面
- [ ] 编写更新日志
- [ ] 生成发布版本
- [ ] 测试安装包

**验收标准**:
- 可正常安装和卸载
- 版本号为v2.0.0
- 所有功能正常

---

## 📊 时间估算

| 阶段 | 预计时间 | 说明 |
|------|---------|------|
| Phase 1: 项目初始化 | 1-2天 | 搭建基础框架 |
| Phase 2: 服务层迁移 | 2-3天 | 核心逻辑迁移 |
| Phase 3: UI框架搭建 | 2-3天 | 主窗口和导航 |
| Phase 4: ViewModel绑定 | 2-3天 | MVVM实现 |
| Phase 5: UI组件实现 | 3-4天 | 详细UI开发 |
| Phase 6: 测试优化 | 2-3天 | 功能测试 |
| Phase 7: 打包发布 | 1天 | 发布准备 |
| **总计** | **13-19天** | 约2-3周 |

---

## 🎨 UI设计参考

### 推荐的WinUI 3模板和资源

1. **WinUI Gallery** (官方示例)
   - GitHub: https://github.com/microsoft/WinUI-Gallery
   - 包含所有WinUI 3控件示例
   - 可参考布局和样式

2. **Template Studio for WinUI**
   - 快速生成MVVM项目模板
   - 内置导航和主题支持

3. **设计参考**
   - Windows 11设置应用
   - Microsoft Store应用
   - Windows Terminal设置界面

### 配色方案

**浅色主题**:
- 主色调: `#0078D4` (Windows Blue)
- 背景: `#F3F3F3`
- 卡片: `#FFFFFF`
- 文字: `#000000`

**深色主题**:
- 主色调: `#60CDFF` (Light Blue)
- 背景: `#202020`
- 卡片: `#2D2D2D`
- 文字: `#FFFFFF`

---

## ⚠️ 风险和注意事项

### 技术风险
1. **WinUI 3学习曲线**
   - 团队需要熟悉XAML和MVVM
   - 建议先学习官方文档和示例

2. **兼容性问题**
   - WinUI 3需要Windows 10 1809+
   - 原版支持更低版本Windows

3. **打包复杂度**
   - MSIX打包比传统exe复杂
   - 需要配置证书

### 功能风险
1. **注册表访问**
   - 确保MSIX应用有足够权限
   - 可能需要在manifest中声明

2. **文件系统访问**
   - WinUI 3应用默认有沙箱限制
   - 需要配置broadFileSystemAccess权限

### 缓解措施
- 提前进行技术验证
- 保留原Windows Forms版本作为备份
- 分阶段发布（先内测再公开）

---

## 📝 开发规范

### 命名规范
- **文件名**: PascalCase (如 `SteamService.cs`)
- **类名**: PascalCase (如 `SteamService`)
- **方法名**: PascalCase (如 `DetectSteamPath`)
- **属性名**: PascalCase (如 `SteamInstallPath`)
- **私有字段**: _camelCase (如 `_steamService`)
- **XAML控件**: camelCase (如 `btnGenerate`)

### 代码规范
- 使用 `async/await` 处理异步操作
- 使用 `CommunityToolkit.Mvvm` 简化ViewModel
- 所有UI操作必须在UI线程
- 异常必须捕获并友好提示
- 日志记录关键操作

### XAML规范
- 使用 `x:Bind` 而非 `Binding`（性能更好）
- 合理使用资源字典
- 控件间距使用8的倍数
- 圆角统一使用4或8

---

## 🔍 功能对比检查清单

### 必须保留的功能
- [ ] Steam安装路径自动检测
- [ ] Steam安装路径手动选择
- [ ] Steam游戏库路径自动检测（支持多库）
- [ ] Steam游戏库路径手动选择
- [ ] Steam ID输入和验证
- [ ] 从SteamDB API自动获取BuildID和Manifest
- [ ] BuildID和Manifest手动输入
- [ ] 官方启动器游戏路径检测
- [ ] WeGame游戏路径检测
- [ ] 官方/WeGame模式切换
- [ ] 生成appmanifest_3513350.acf文件
- [ ] 生成Steam启动命令
- [ ] 复制启动命令到剪贴板
- [ ] 设置持久化（保存/加载）
- [ ] 日志输出
- [ ] 错误提示

### UI改进点
- [ ] 现代化卡片布局
- [ ] 流畅的动画效果
- [ ] 更好的视觉反馈
- [ ] 响应式布局
- [ ] 深色/浅色主题支持
- [ ] 更友好的错误提示
- [ ] 加载状态指示

---

## 📚 参考资源

### 官方文档
- [WinUI 3 官方文档](https://learn.microsoft.com/windows/apps/winui/winui3/)
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/)
- [MVVM Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)

### 示例项目
- [WinUI Gallery](https://github.com/microsoft/WinUI-Gallery)
- [Template Studio](https://github.com/microsoft/TemplateStudio)

### 设计指南
- [Fluent Design System](https://www.microsoft.com/design/fluent/)
- [Windows 11 Design Principles](https://learn.microsoft.com/windows/apps/design/)

---

## ✅ 下一步行动

1. **确认计划**: 审核本重构计划，确认技术方案和时间安排
2. **环境准备**: 安装Visual Studio 2022 + Windows App SDK
3. **启动Phase 1**: 创建WinUI 3项目骨架
4. **定期同步**: 每完成一个Phase进行功能验证

---

## 📌 备注

- 本计划仅涉及UI重构，不涉及功能更新
- 版本号从v1.1.0升级到v2.0.0
- 保持与原版100%功能一致
- UI设计可根据实际情况灵活调整
- 建议使用Git进行版本控制，方便回滚

---

**计划制定日期**: 2026-04-02
**目标完成日期**: 2026-04-20 (约3周)
**计划版本**: v1.0
