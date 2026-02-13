namespace WutheringWavesSteamHelper
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            AutoDetectPaths();
        }

        private void AutoDetectPaths()
        {
            var steamPath = SteamHelper.DetectSteamInstallPath();
            if (!string.IsNullOrEmpty(steamPath))
            {
                txtSteamInstallPath.Text = steamPath;
                AppendLog($"已自动识别 Steam 安装路径：{steamPath}");
            }

            var libraryPaths = SteamHelper.DetectSteamLibraryPaths();
            if (libraryPaths.Count > 0)
            {
                txtSteamLibraryPath.Text = libraryPaths[0];
                AppendLog($"已自动识别 SteamLibrary 路径：{libraryPaths[0]}");
            }
        }

        private void BtnBrowseLibrary_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "请选择 SteamLibrary 文件夹（包含 steamapps 的文件夹）",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var steamappsPath = Path.Combine(dialog.SelectedPath, "steamapps");
                if (!Directory.Exists(steamappsPath))
                {
                    var result = MessageBox.Show(
                        $"所选文件夹下未找到 steamapps 目录，是否仍然使用该路径？\n{dialog.SelectedPath}",
                        "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) return;
                }
                txtSteamLibraryPath.Text = dialog.SelectedPath;
                AppendLog($"已手动选择 SteamLibrary 路径：{dialog.SelectedPath}");
            }
        }

        private void BtnAutoDetectLibrary_Click(object? sender, EventArgs e)
        {
            var paths = SteamHelper.DetectSteamLibraryPaths();
            if (paths.Count == 0)
            {
                MessageBox.Show("未能自动识别 SteamLibrary 路径，请手动选择。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (paths.Count == 1)
            {
                txtSteamLibraryPath.Text = paths[0];
                AppendLog($"已自动识别 SteamLibrary 路径：{paths[0]}");
                return;
            }

            // Multiple library paths found, let user choose
            using var selectionDialog = new LibrarySelectionDialog(paths);
            if (selectionDialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(selectionDialog.SelectedPath))
            {
                txtSteamLibraryPath.Text = selectionDialog.SelectedPath;
                AppendLog($"已选择 SteamLibrary 路径：{selectionDialog.SelectedPath}");
            }
            else
            {
                txtSteamLibraryPath.Text = paths[0];
                AppendLog($"已自动识别 SteamLibrary 路径：{paths[0]}");
            }
        }

        private void BtnBrowseSteam_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "请选择 Steam 安装文件夹（包含 steam.exe）",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var steamExe = Path.Combine(dialog.SelectedPath, "steam.exe");
                if (!File.Exists(steamExe))
                {
                    var result = MessageBox.Show(
                        $"所选文件夹下未找到 steam.exe，是否仍然使用该路径？\n{dialog.SelectedPath}",
                        "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) return;
                }
                txtSteamInstallPath.Text = dialog.SelectedPath;
                AppendLog($"已手动选择 Steam 安装路径：{dialog.SelectedPath}");
            }
        }

        private void BtnAutoDetectSteam_Click(object? sender, EventArgs e)
        {
            var path = SteamHelper.DetectSteamInstallPath();
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("未能自动识别 Steam 安装路径，请手动选择。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            txtSteamInstallPath.Text = path;
            AppendLog($"已自动识别 Steam 安装路径：{path}");
        }

        private async void BtnFetchSteamDb_Click(object? sender, EventArgs e)
        {
            btnFetchSteamDb.Enabled = false;
            btnFetchSteamDb.Text = "正在获取...";
            AppendLog("正在从 SteamDB 获取 BuildID 和 Manifest...");

            try
            {
                var result = await SteamHelper.FetchSteamDbInfoAsync();
                if (result.HasValue)
                {
                    txtBuildId.Text = result.Value.buildId;
                    txtManifest.Text = result.Value.manifest;
                    AppendLog($"获取成功！BuildID: {result.Value.buildId}, Manifest: {result.Value.manifest}");
                }
                else
                {
                    AppendLog("获取失败，请检查网络连接或手动填写。");
                    MessageBox.Show(
                        "无法从 SteamDB 获取信息，请检查网络连接。\n\n你也可以手动访问 https://steamdb.info/app/3513350/depots/ 获取信息后填写。",
                        "获取失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtBuildId.ReadOnly = false;
                    txtManifest.ReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"获取异常：{ex.Message}");
                txtBuildId.ReadOnly = false;
                txtManifest.ReadOnly = false;
            }
            finally
            {
                btnFetchSteamDb.Enabled = true;
                btnFetchSteamDb.Text = "从 SteamDB 获取 BuildID 和 Manifest";
            }
        }

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtSteamLibraryPath.Text))
            {
                MessageBox.Show("请先选择或自动识别 SteamLibrary 路径。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSteamInstallPath.Text))
            {
                MessageBox.Show("请先选择或自动识别 Steam 安装路径。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSteamId.Text))
            {
                MessageBox.Show("请输入 Steam ID（SteamID64）。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBuildId.Text))
            {
                MessageBox.Show("请先获取 Build ID。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtManifest.Text))
            {
                MessageBox.Show("请先获取 Manifest ID。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var libraryPath = txtSteamLibraryPath.Text;
                var steamappsPath = Path.Combine(libraryPath, "steamapps");
                var commonPath = Path.Combine(steamappsPath, "common");
                var gamePath = Path.Combine(commonPath, "Wuthering Waves");
                var launcherPath = Path.Combine(txtSteamInstallPath.Text, "steam.exe");

                var acfPath = Path.Combine(steamappsPath, "appmanifest_3513350.acf");
                var exePath = Path.Combine(gamePath, "Wuthering Waves.exe");

                // Check if files already exist
                bool acfExists = File.Exists(acfPath);
                bool exeExists = File.Exists(exePath);

                if (acfExists || exeExists)
                {
                    var existingFiles = new List<string>();
                    if (acfExists) existingFiles.Add("appmanifest_3513350.acf");
                    if (exeExists) existingFiles.Add("Wuthering Waves.exe");

                    var message = $"检测到以下文件已存在：\n\n{string.Join("\n", existingFiles)}\n\n是否要覆盖这些文件？";
                    var result = MessageBox.Show(message, "文件已存在", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                    {
                        AppendLog("用户取消操作，未覆盖现有文件。");
                        return;
                    }

                    AppendLog("用户选择覆盖现有文件。");
                }

                // 1. Create steamapps directory if needed
                if (!Directory.Exists(steamappsPath))
                {
                    Directory.CreateDirectory(steamappsPath);
                    AppendLog($"已创建目录：{steamappsPath}");
                }

                // 2. Generate appmanifest_3513350.acf
                var acfContent = SteamHelper.GenerateAcfContent(
                    launcherPath,
                    txtBuildId.Text.Trim(),
                    txtSteamId.Text.Trim(),
                    txtManifest.Text.Trim());
                File.WriteAllText(acfPath, acfContent);
                AppendLog($"已生成：{acfPath}");

                // 3. Create common/Wuthering Waves directory
                if (!Directory.Exists(gamePath))
                {
                    Directory.CreateDirectory(gamePath);
                    AppendLog($"已创建目录：{gamePath}");
                }

                // 4. Create empty Wuthering Waves.exe
                if (!exeExists)
                {
                    File.Create(exePath).Dispose();
                    AppendLog($"已创建空 EXE：{exePath}");
                }
                else
                {
                    File.WriteAllBytes(exePath, Array.Empty<byte>());
                    AppendLog($"已覆盖 EXE 文件：{exePath}");
                }

                AppendLog("[完成] 全部配置已生成完毕！请重启 Steam 。");
                MessageBox.Show(
                    "配置生成成功！\n\n请重启 Steam 客户端，然后在库中找到「Wuthering Waves」配置。",
                    "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog($"[错误] 生成失败：{ex.Message}");
                MessageBox.Show($"生成过程中出错：\n{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLaunchCommand_Click(object? sender, EventArgs e)
        {
            AppendLog("正在搜索国服鸣潮安装路径...");
            var paths = SteamHelper.DetectCnWutheringWavesPaths();

            if (paths.Count == 0)
            {
                AppendLog("未找到国服鸣潮安装路径，请手动选择。");
                using var dialog = new FolderBrowserDialog
                {
                    Description = "请选择国服鸣潮的安装文件夹（包含 \"Wuthering Waves Game\" 的文件夹）",
                    UseDescriptionForTitle = true
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var exePath = Path.Combine(dialog.SelectedPath,
                        "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
                    if (!File.Exists(exePath))
                    {
                        MessageBox.Show(
                            $"所选路径下未找到客户端文件：\n{exePath}\n\n请确认选择的是鸣潮的安装根目录。",
                            "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var command = SteamHelper.GenerateLaunchCommand(dialog.SelectedPath);
                    AppendLog($"已手动选择国服鸣潮路径：{dialog.SelectedPath}");
                    using var cmdDialog = new LaunchCommandDialog(command);
                    cmdDialog.ShowDialog(this);
                }
                return;
            }

            string selectedPath;
            if (paths.Count == 1)
            {
                selectedPath = paths[0];
            }
            else
            {
                using var selectionDialog = new LibrarySelectionDialog(paths);
                selectionDialog.Text = "选择国服鸣潮安装路径";
                if (selectionDialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(selectionDialog.SelectedPath))
                    selectedPath = selectionDialog.SelectedPath;
                else
                    selectedPath = paths[0];
            }

            AppendLog($"已找到国服鸣潮路径：{selectedPath}");
            var launchCommand = SteamHelper.GenerateLaunchCommand(selectedPath);
            using var launchCommandDialog = new LaunchCommandDialog(launchCommand);
            launchCommandDialog.ShowDialog(this);
        }

        private void BtnHelp_Click(object? sender, EventArgs e)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.iryougi.com/index.php/wutheringwavessteamhelper/",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
                AppendLog("已打开帮助页面");
            }
            catch (Exception ex)
            {
                AppendLog($"打开帮助页面失败：{ex.Message}");
                MessageBox.Show($"无法打开帮助页面\n{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AppendLog(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }
    }
}
