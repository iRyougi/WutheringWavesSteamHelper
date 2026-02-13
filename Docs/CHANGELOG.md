# 更新日志 / Changelog

## [1.0.0] - 2024-01-XX

### 新增功能 / Added
- ? 首次发布
- ?? 自动检测 Steam 安装路径功能
- ?? 自动检测 Steam 游戏库路径（支持多个游戏库）
- ?? 集成 SteamDB API，自动获取最新 BuildID 和 Manifest
- ?? 友好的图形用户界面（Windows Forms）
- ?? 自动生成 Steam 游戏配置文件（appmanifest_3513350.acf）
- ?? 一键添加鸣潮到 Steam 库
- ?? 实时操作日志显示
- ?? 内置帮助文档链接

### 技术特性 / Technical Features
- 基于 .NET 8.0 框架开发
- 支持 Windows 10/11 操作系统
- 使用 Windows Registry API 读取 Steam 配置
- 支持解析 Steam libraryfolders.vdf 文件
- 异步网络请求，避免界面卡顿
- 完善的错误处理和用户提示

### 支持的功能 / Supported Features
- ? Steam 路径自动检测与手动选择
- ? 多游戏库支持与选择
- ? SteamDB 自动获取版本信息
- ? 手动输入版本信息（备用方案）
- ? Steam ID (SteamID64) 配置
- ? 自动创建游戏目录结构
- ? 生成标准 ACF 配置文件

---

## 版本说明 / Version Notes

### 版本编号规则
本项目采用 [语义化版本控制](https://semver.org/lang/zh-CN/) (Semantic Versioning)：

- **主版本号（Major）**：不兼容的 API 修改
- **次版本号（Minor）**：向下兼容的功能性新增
- **修订号（Patch）**：向下兼容的问题修正

### 当前版本特性
- **版本**：1.0.0
- **发布日期**：待定
- **开发商**：KAMITSUBAKI STUDIO
- **目标框架**：.NET 8.0 Windows
- **支持的游戏**：鸣潮 (Wuthering Waves)
- **游戏 AppID**：3513350
- **主要 Depot ID**：3513351

---

## 未来计划 / Roadmap

### 计划中的功能 / Planned Features
- ?? 自动更新检测
- ?? 多语言支持（英语、日语等）
- ?? 配置保存与加载
- ?? 界面主题切换
- ?? 游戏文件大小显示
- ?? 操作完成通知
- ?? 便携版支持

### 考虑中的改进 / Considerations
- 支持其他游戏的 Steam 集成
- 添加游戏启动器选项配置
- 集成游戏文件完整性验证
- 提供命令行接口（CLI）

---

## 技术栈 / Tech Stack

- **开发语言**：C# 12
- **框架**：.NET 8.0
- **UI 框架**：Windows Forms
- **依赖包**：
  - Microsoft.Win32.Registry 5.0.0

---

## 贡献 / Contributing

欢迎提交 Issue 和 Pull Request！

在提交代码前，请确保：
1. 代码遵循项目的编码规范
2. 测试通过所有功能
3. 更新相关文档

---

## 支持 / Support

如果您在使用过程中遇到问题，可以：
1. 查看 [使用说明](README.md)
2. 访问 [帮助文档](https://www.iryougi.com/index.php/wutheringwavessteamhelper/)
3. 在 GitHub 提交 [Issue](https://github.com/iRyougi/WutheringWavesSteamHelper/issues)

---

## 许可证 / License

本项目遵循开源协议，具体请查看 LICENSE 文件。

---

**注意**：本更新日志采用 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/) 格式。
