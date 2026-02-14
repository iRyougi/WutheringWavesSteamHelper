# WutheringWavesSteamHelper

鸣潮 Steam 助手 - 用于在 Steam 库中添加鸣潮游戏，实现 Steam 启动器管理。

## 简介

WutheringWavesSteamHelper 是一个通过生成必要的配置文件，让玩家可以通过 Steam 客户端启动和管理国服鸣潮

## 主要功能

- 自动检测 Steam 安装路径
- 自动检测 Steam 游戏库路径（支持多个库）
- 自动从 SteamDB 获取最新的 BuildID 和 Manifest
- 自动生成游戏配置文件（appmanifest_3513350.acf）
- 一键从Steam端启动国服鸣潮

## 系统要求

- Windows 操作系统（Windows 10/11）基于x64
- .NET 8.0 运行时
- 已安装 Steam 客户端
- Steam游戏库内已入库鸣潮游戏

## 使用说明

### 1. 启动程序

运行 `WutheringWavesSteamHelper.exe`，程序会自动尝试检测您的 Steam 安装路径和游戏库路径。

### 2. 配置路径

#### Steam 安装路径
- **自动检测**：程序启动时会自动检测，通常为 `C:\Program Files (x86)\Steam`
- **手动选择**：如果自动检测失败，点击"浏览"按钮手动选择 Steam 安装目录（包含 steam.exe 的文件夹）
- **重新检测**：点击"自动检测"按钮重新检测路径

#### SteamLibrary 路径
- **自动检测**：程序会读取 Steam 的 libraryfolders.vdf 文件，自动检测所有游戏库
- **多库选择**：如果检测到多个游戏库，会弹出选择对话框让您选择要使用的库
- **手动选择**：点击"浏览"按钮手动选择游戏库目录（包含 steamapps 文件夹的目录）

### 3. 输入 Steam ID

在 "Steam ID (SteamID64)" 文本框中输入您的 Steam ID。

**如何获取 Steam ID？**
- 访问 [SteamID.io](https://steamid.io/) 或 [SteamDB Calculator](https://steamdb.info/calculator/)
- 输入您的 Steam 个人资料链接
- 复制 steamID64（纯数字格式）

### 4. 获取 BuildID 和 Manifest

点击"从 SteamDB 获取 BuildID 和 Manifest"按钮，程序会自动从 SteamDB API 获取最新的游戏版本信息。

**如果自动获取失败：**
1. 访问 [鸣潮 SteamDB 页面](https://steamdb.info/app/3513350/depots/)
2. 找到 public 分支的 BuildID
3. 找到 Depot 3513351 的 ManifestID
4. 手动填入对应的文本框

### 5. 生成配置

点击"生成配置并创建快捷方式"按钮，程序会自动：
1. 创建 `steamapps` 目录（如果不存在）
2. 生成 `appmanifest_3513350.acf` 配置文件
3. 创建 `steamapps/common/Wuthering Waves` 目录
4. 创建空的 `Wuthering Waves.exe` 文件

### 6. 在 Steam 中填入启动命令

1. 重启 Steam 客户端
2. 在游戏库中找到"Wuthering Waves"
3. 点击工具中的蓝色按钮获取命令
4. 在你的启动选项处填入你刚刚复制的命令
5. 点击"更新"或直接启动游戏

## 注意事项

**重要提示**

1. **首次使用**：首次添加后，Steam 会显示游戏需要更新，这是正常现象
3. **Steam ID**：请确保输入正确的 Steam ID，否则可能导致游戏无法正常识别
4. **网络连接**：自动获取 BuildID 和 Manifest 需要网络连接
5. **Steam 运行**：操作前建议关闭 Steam 客户端，完成后再启动

## 常见问题

### Q: 生成配置后 Steam 库中看不到游戏？
A: 请完全退出 Steam（包括托盘图标），然后重新启动 Steam 客户端。

### Q: 提示"未找到 steam.exe"？
A: 请确保选择的是 Steam 的安装目录，而不是游戏目录。正确的路径应该包含 steam.exe 文件。

### Q: 自动获取 BuildID 失败怎么办？
A: 可以手动访问 https://steamdb.info/app/3513350/depots/ 查看并手动填写相关信息。

### Q: 可以选择任意游戏库吗？
A: 可以，程序支持 Steam 的所有游戏库。建议选择空间充足的磁盘。

## 技术说明

- **框架**：.NET 8.0 Windows Forms
- **开发**：iRyougi
- **游戏 AppID**：3513350
- **主要 Depot**：3513351

## 关于

本工具由社区开发，旨在为玩家提供便利的 Steam 集成体验。

- 帮助文档：[https://www.iryougi.com/index.php/wutheringwavessteamhelper/](https://www.iryougi.com/index.php/wutheringwavessteamhelper/)
- GitHub 仓库：[https://github.com/iRyougi/WutheringWavesSteamHelper](https://github.com/iRyougi/WutheringWavesSteamHelper)

## 免责声明

本工具仅用于学习和研究目的。使用本工具产生的任何问题，开发者不承担责任。请支持正版游戏。

## 许可证

本项目遵循开源协议，具体请查看 LICENSE 文件。
