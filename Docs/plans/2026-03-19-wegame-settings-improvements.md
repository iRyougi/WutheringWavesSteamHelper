# WeGame支持、配置优化、设置持久化 实现计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 支持WeGame版鸣潮路径识别、优化生成配置覆盖逻辑、持久化用户设置、新增官方启动器/WeGame切换RadioButton。

**Architecture:** 新增 `AppSettings.cs` 负责设置读写；`SteamHelper.cs` 扩展WeGame注册表检测和双路径结构支持；`MainForm.Designer.cs` 新增切换RadioButton区域；`MainForm.cs` 调整生成逻辑和设置加载/保存。

**Tech Stack:** C# .NET 8.0-windows, WinForms, System.Text.Json, Microsoft.Win32

---

### Task 1: 新增 AppSettings.cs — 设置持久化

**Files:**
- Create: `AppSettings.cs`

**Step 1: 创建 AppSettings.cs**

```csharp
using System.Text.Json;

namespace WutheringWavesSteamHelper
{
    public class AppSettings
    {
        public string SteamLibraryPath { get; set; } = "";
        public string SteamInstallPath { get; set; } = "";
        public string SteamId { get; set; } = "";
        public string BuildId { get; set; } = "";
        public string Manifest { get; set; } = "";
        // "official" 或 "wegame"
        public string CnGameSource { get; set; } = "official";

        private static readonly string SettingsDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "WutheringWavesSteamHelper");
        private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
    }
}
```

**Step 2: Commit**

```bash
git add AppSettings.cs
git commit -m "feat: add AppSettings for persistent user settings"
```

---

### Task 2: 扩展 SteamHelper.cs — WeGame注册表检测 + 双路径结构

**Files:**
- Modify: `SteamHelper.cs`

**Step 1: 在 `DetectCnWutheringWavesPaths()` 中增加WeGame注册表检测**

在现有注册表检测块（`registryKeys` 循环）之后、公共路径搜索之前，插入：

```csharp
// WeGame 注册表检测 (HKCU\SOFTWARE\Rail\WutheringWaves)
try
{
    using var wegameKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Rail\WutheringWaves");
    var wegameInstall = wegameKey?.GetValue("InstallPath") as string;
    if (!string.IsNullOrEmpty(wegameInstall))
    {
        // WeGame结构：{InstallPath}\Client\Binaries\Win64\Client-Win64-Shipping.exe
        var wegameExe = Path.Combine(wegameInstall, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
        if (File.Exists(wegameExe) && !results.Contains(wegameInstall, StringComparer.OrdinalIgnoreCase))
            results.Add(wegameInstall);
    }
}
catch { }
```

**Step 2: 修改 `GenerateLaunchCommand()` 支持双路径结构**

将现有方法替换为：

```csharp
public static string GenerateLaunchCommand(string installPath)
{
    // 优先检测WeGame结构（无"Wuthering Waves Game"层）
    var wegameExe = Path.Combine(installPath, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
    if (File.Exists(wegameExe))
        return $"\"{wegameExe}\" %command%";

    // 官方启动器结构
    var officialExe = Path.Combine(installPath, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
    return $"\"{officialExe}\" %command%";
}
```

**Step 3: 新增 `DetectWeGameInstallPath()` 供切换逻辑使用**

```csharp
public static string? DetectWeGameInstallPath()
{
    try
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Rail\WutheringWaves");
        var path = key?.GetValue("InstallPath") as string;
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            return path;
    }
    catch { }
    return null;
}
```

**Step 4: Commit**

```bash
git add SteamHelper.cs
git commit -m "feat: add WeGame registry detection and dual-path launch command support"
```

---

### Task 3: MainForm.Designer.cs — 新增切换RadioButton区域

**Files:**
- Modify: `MainForm.Designer.cs`

**Step 1: 在字段声明区（文件末尾 `#endregion` 之前）新增字段**

```csharp
private Panel pnlSourceSwitch;
private RadioButton rdoOfficial;
private RadioButton rdoWeGame;
```

**Step 2: 调整内容区布局，在 `btnLaunchCommand` 上方插入切换面板**

当前布局（y坐标）：
- btnGenerate: y=412, h=44
- btnLaunchCommand: y=464, h=44
- btnOpenLauncher: y=516, h=44
- cardLogPanel: y=572, h=128

新增切换面板（h=36）插入在 btnLaunchCommand 之前，所有后续元素下移 44px：

切换面板本身：y=464, h=36
- btnLaunchCommand: y=508, h=44
- btnOpenLauncher: y=560, h=44
- cardLogPanel: y=616, h=128

cardLogPanel底部 = 616+128 = 744，加底部间距16 → 内容总高760。
窗口高度从 772 调整为 816（56头部 + 760内容）。

**Step 3: 在 `InitializeComponent()` 中，`btnLaunchCommand` 定义之前插入切换面板代码**

将现有 `btnLaunchCommand` 的 `Location` 从 `new Point(16, 464)` 改为 `new Point(16, 508)`。
将现有 `btnOpenLauncher` 的 `Location` 从 `new Point(16, 516)` 改为 `new Point(16, 560)`。
将现有 `cardLogPanel` 的 `CreateCard` 调用从 `new Point(16, 572)` 改为 `new Point(16, 616)`。

在 `btnGenerate` 定义之后、`btnLaunchCommand` 定义之前，插入：

```csharp
// --- 国服版本切换面板 (y=464, h=36) ---
pnlSourceSwitch = new Panel();
pnlSourceSwitch.Location = new Point(16, 464);
pnlSourceSwitch.Size = new Size(648, 36);
pnlSourceSwitch.BackColor = Color.FromArgb(241, 245, 249);
// 圆角效果用边框模拟
var switchBorder = new Panel();
switchBorder.Location = new Point(0, 0);
switchBorder.Size = new Size(648, 36);
switchBorder.BackColor = Color.Transparent;

var lblSwitchHint = new Label();
lblSwitchHint.Text = "国服版本：";
lblSwitchHint.Font = new Font("Microsoft YaHei UI", 9F);
lblSwitchHint.ForeColor = Color.FromArgb(55, 65, 81);
lblSwitchHint.Location = new Point(12, 9);
lblSwitchHint.AutoSize = true;

rdoOfficial = new RadioButton();
rdoOfficial.Text = "官方启动器";
rdoOfficial.Font = new Font("Microsoft YaHei UI", 9F);
rdoOfficial.ForeColor = Color.FromArgb(30, 41, 59);
rdoOfficial.Location = new Point(88, 8);
rdoOfficial.AutoSize = true;
rdoOfficial.Checked = true;
rdoOfficial.Cursor = Cursors.Hand;

rdoWeGame = new RadioButton();
rdoWeGame.Text = "WeGame";
rdoWeGame.Font = new Font("Microsoft YaHei UI", 9F);
rdoWeGame.ForeColor = Color.FromArgb(30, 41, 59);
rdoWeGame.Location = new Point(196, 8);
rdoWeGame.AutoSize = true;
rdoWeGame.Cursor = Cursors.Hand;

pnlSourceSwitch.Controls.Add(lblSwitchHint);
pnlSourceSwitch.Controls.Add(rdoOfficial);
pnlSourceSwitch.Controls.Add(rdoWeGame);
pnlContent.Controls.Add(pnlSourceSwitch);
```

同时将 `pnlSidebar` 和相关面板的高度从 772 更新为 816，`ClientSize` 从 `new Size(900, 772)` 改为 `new Size(900, 816)`，`pnlContent.Size` 从 `new Size(680, 716)` 改为 `new Size(680, 760)`，`lblVersion.Location` 的 y 从 748 改为 792。

**Step 4: Commit**

```bash
git add MainForm.Designer.cs
git commit -m "feat: add official/WeGame source switch radio buttons to UI"
```

---

### Task 4: MainForm.cs — 逻辑调整（覆盖逻辑、设置持久化、切换联动）

**Files:**
- Modify: `MainForm.cs`

**Step 1: 新增字段和构造函数调整**

在类顶部字段区新增：
```csharp
private AppSettings _settings = new();
```

将构造函数改为：
```csharp
public MainForm()
{
    InitializeComponent();
    BuildSidebarNavButtons();
    _settings = AppSettings.Load();
    LoadSettingsToUI();
    AutoDetectPaths();
    rdoOfficial.CheckedChanged += OnSourceChanged;
    rdoWeGame.CheckedChanged += OnSourceChanged;
    // 根据已保存的设置恢复选择
    if (_settings.CnGameSource == "wegame")
        rdoWeGame.Checked = true;
}
```

**Step 2: 新增 `LoadSettingsToUI()` 方法**

```csharp
private void LoadSettingsToUI()
{
    if (!string.IsNullOrEmpty(_settings.SteamLibraryPath))
        txtSteamLibraryPath.Text = _settings.SteamLibraryPath;
    if (!string.IsNullOrEmpty(_settings.SteamInstallPath))
        txtSteamInstallPath.Text = _settings.SteamInstallPath;
    if (!string.IsNullOrEmpty(_settings.SteamId))
        txtSteamId.Text = _settings.SteamId;
    if (!string.IsNullOrEmpty(_settings.BuildId))
    {
        txtBuildId.Text = _settings.BuildId;
        txtBuildId.ReadOnly = false;
    }
    if (!string.IsNullOrEmpty(_settings.Manifest))
    {
        txtManifest.Text = _settings.Manifest;
        txtManifest.ReadOnly = false;
    }
}
```

**Step 3: 新增 `SaveSettingsFromUI()` 方法**

```csharp
private void SaveSettingsFromUI()
{
    _settings.SteamLibraryPath = txtSteamLibraryPath.Text;
    _settings.SteamInstallPath = txtSteamInstallPath.Text;
    _settings.SteamId = txtSteamId.Text;
    _settings.BuildId = txtBuildId.Text;
    _settings.Manifest = txtManifest.Text;
    _settings.CnGameSource = rdoWeGame.Checked ? "wegame" : "official";
    _settings.Save();
}
```

**Step 4: 新增 `OnSourceChanged()` 事件处理**

```csharp
private void OnSourceChanged(object? sender, EventArgs e)
{
    if (!rdoWeGame.Checked) return;
    // WeGame 选中时尝试自动检测路径提示
    var wegamePath = SteamHelper.DetectWeGameInstallPath();
    if (wegamePath != null)
        AppendLog($"已检测到 WeGame 鸣潮路径：{wegamePath}");
    else
        AppendLog("未检测到 WeGame 鸣潮安装，请确认已安装 WeGame 版鸣潮");
}
```

**Step 5: 修改 `BtnGenerate_Click` — 调整覆盖逻辑**

将现有的覆盖检测逻辑（`if (acfExists || exeExists)` 块）替换为：

```csharp
// exe 已存在则静默跳过，不提示不覆盖
// acf 已存在才询问是否覆盖
if (acfExists)
{
    var result = MessageBox.Show(
        $"检测到配置文件已存在：\nappmanifest_3513350.acf\n\n是否要覆盖该文件？",
        "文件已存在", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    if (result != DialogResult.Yes)
    {
        AppendLog("用户取消覆盖 ACF 文件，操作已跳过。");
        return;
    }
    AppendLog("用户选择覆盖 ACF 文件。");
}
```

同时在 `BtnGenerate_Click` 末尾（`MessageBox.Show` 成功提示之前）调用 `SaveSettingsFromUI()`。

将 exe 创建逻辑改为只在不存在时创建：
```csharp
if (!exeExists)
{
    File.Create(exePath).Dispose();
    AppendLog($"已创建占位 EXE：{exePath}");
}
else
{
    AppendLog($"EXE 文件已存在，跳过：{exePath}");
}
```

**Step 6: 修改 `BtnLaunchCommand_Click` — 支持WeGame路径和切换**

将方法开头的路径检测逻辑改为根据 `rdoWeGame.Checked` 分支：

```csharp
private void BtnLaunchCommand_Click(object? sender, EventArgs e)
{
    AppendLog("正在搜索国服鸣潮安装路径...");

    string? selectedPath = null;

    if (rdoWeGame.Checked)
    {
        // WeGame 模式：优先读注册表
        selectedPath = SteamHelper.DetectWeGameInstallPath();
        if (selectedPath == null)
        {
            AppendLog("未检测到 WeGame 鸣潮，请手动选择");
            using var dialog = new FolderBrowserDialog
            {
                Description = "请选择 WeGame 鸣潮安装文件夹（含 Client 子文件夹）",
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var exePath = Path.Combine(dialog.SelectedPath, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
                if (!File.Exists(exePath))
                {
                    MessageBox.Show(
                        $"所选路径中未找到客户端文件：\n{exePath}\n\n请确认选择了 WeGame 鸣潮的安装根目录（含 Client 文件夹）。",
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                selectedPath = dialog.SelectedPath;
            }
            else return;
        }
    }
    else
    {
        // 官方启动器模式：原有逻辑
        var paths = SteamHelper.DetectCnWutheringWavesPaths()
            .Where(p => File.Exists(Path.Combine(p, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe")))
            .ToList();

        if (paths.Count == 0)
        {
            AppendLog("未找到官方启动器鸣潮路径，请手动选择");
            using var dialog = new FolderBrowserDialog
            {
                Description = "请选择国服鸣潮的安装文件夹（含 \"Wuthering Waves Game\" 子文件夹）",
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var exePath = Path.Combine(dialog.SelectedPath, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
                if (!File.Exists(exePath))
                {
                    MessageBox.Show(
                        $"所选路径中未找到客户端文件：\n{exePath}\n\n请确认选择了国服鸣潮的安装根目录。",
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                selectedPath = dialog.SelectedPath;
            }
            else return;
        }
        else if (paths.Count == 1)
        {
            selectedPath = paths[0];
        }
        else
        {
            using var selectionDialog = new LibrarySelectionDialog(paths);
            selectionDialog.Text = "选择国服鸣潮安装路径";
            selectedPath = selectionDialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(selectionDialog.SelectedPath)
                ? selectionDialog.SelectedPath : paths[0];
        }
    }

    AppendLog($"已找到鸣潮路径：{selectedPath}");
    var launchCommand = SteamHelper.GenerateLaunchCommand(selectedPath);
    using var launchCommandDialog = new LaunchCommandDialog(launchCommand);
    launchCommandDialog.ShowDialog(this);
}
```

**Step 7: 修改 `BtnOpenLauncher_Click` — 支持WeGame模式**

WeGame 版没有 `launcher.exe`，选择 WeGame 时提示用户直接通过 WeGame 客户端启动，或禁用该按钮。

在方法开头加判断：
```csharp
if (rdoWeGame.Checked)
{
    MessageBox.Show(
        "WeGame 版鸣潮请通过 WeGame 客户端启动游戏。\n\n此按钮仅适用于官方启动器版本。",
        "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    return;
}
```

**Step 8: 在 `AutoDetectPaths()` 末尾调用 `SaveSettingsFromUI()` 时机说明**

`AutoDetectPaths()` 只在构造函数中调用，且在 `LoadSettingsToUI()` 之后。若已有保存的路径则不覆盖（`AutoDetectPaths` 中加判断）：

```csharp
private void AutoDetectPaths()
{
    if (string.IsNullOrEmpty(txtSteamInstallPath.Text))
    {
        var steamPath = SteamHelper.DetectSteamInstallPath();
        if (!string.IsNullOrEmpty(steamPath))
        {
            txtSteamInstallPath.Text = steamPath;
            AppendLog($"已自动识别 Steam 安装路径：{steamPath}");
        }
    }

    if (string.IsNullOrEmpty(txtSteamLibraryPath.Text))
    {
        var libraryPaths = SteamHelper.DetectSteamLibraryPaths();
        if (libraryPaths.Count > 0)
        {
            txtSteamLibraryPath.Text = libraryPaths[0];
            AppendLog($"已自动识别 SteamLibrary 路径：{libraryPaths[0]}");
        }
    }
}
```

**Step 9: Commit**

```bash
git add MainForm.cs AppSettings.cs
git commit -m "feat: settings persistence, WeGame switch logic, fix overwrite behavior"
```

---

### Task 5: 验证与收尾

**Step 1: 构建项目**

```bash
dotnet build WutheringWavesSteamHelper.csproj
```
预期：Build succeeded，0 errors。

**Step 2: 手动验证清单**
- [ ] 启动程序，已保存的 SteamID/路径自动填入
- [ ] 切换到 WeGame，日志显示检测结果
- [ ] 生成配置时，若 exe 已存在不提示，若 acf 已存在才提示
- [ ] 生成配置成功后，设置已保存到 `%AppData%\WutheringWavesSteamHelper\settings.json`
- [ ] WeGame 模式下"生成启动命令"使用正确路径结构
- [ ] 点击"打开官方启动器"在 WeGame 模式下显示提示

**Step 3: Commit**

```bash
git add .
git commit -m "chore: final verification and cleanup"
```
