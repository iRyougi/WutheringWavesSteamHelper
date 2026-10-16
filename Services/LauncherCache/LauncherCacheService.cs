namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>
/// 启动器缓存门面：读取状态、改写各档位游戏目录、备份与恢复。
/// 公开方法不抛异常，统一以 <see cref="LauncherStatus"/> / <see cref="WriteResult"/> 返回。
/// </summary>
public sealed class LauncherCacheService
{
    public const int MaxBackups = 30;
    private const string BackupPrefix = "kr_game_temp.";
    private const string TempSuffix = ".wwsh-tmp";

    private readonly Func<string, bool> _isLauncherRunning;
    private readonly string _backupDir;
    private readonly Func<DateTime> _clock;
    private readonly Func<byte[], bool>? _verifyOverride;

    public LauncherCacheService()
        : this(LauncherProcessGuard.IsRunning, DefaultBackupDir, () => DateTime.Now)
    {
    }

    /// <param name="isLauncherRunning">启动器运行检测（参数为启动器目录）。</param>
    /// <param name="backupDir">备份目录。</param>
    /// <param name="clock">时间源（用于备份文件名）。</param>
    /// <param name="verifyOverride">仅测试用：返回 false 模拟回读校验失败。</param>
    public LauncherCacheService(
        Func<string, bool> isLauncherRunning,
        string backupDir,
        Func<DateTime> clock,
        Func<byte[], bool>? verifyOverride = null)
    {
        _isLauncherRunning = isLauncherRunning;
        _backupDir = backupDir;
        _clock = clock;
        _verifyOverride = verifyOverride;
    }

    public static string DefaultBackupDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WutheringWavesSteamHelper",
        "launcher-backup");

    public string BackupDir => _backupDir;

    /// <summary>定位启动器并读取状态。preferredDir 若在候选中则优先使用。</summary>
    public LauncherStatus GetStatus(string? savedLauncherDir, IEnumerable<string> extraInstallRoots, string? preferredDir = null)
    {
        IReadOnlyList<string> candidates;
        try
        {
            candidates = LauncherLocator.FindCandidates(savedLauncherDir, extraInstallRoots);
        }
        catch (Exception ex)
        {
            return LauncherStatus.NotFound(Array.Empty<string>()) with { Error = $"检测启动器失败：{ex.Message}" };
        }

        if (candidates.Count == 0)
            return LauncherStatus.NotFound(candidates);

        var dir = candidates.FirstOrDefault(c => string.Equals(c, preferredDir, StringComparison.OrdinalIgnoreCase))
                  ?? candidates[0];
        return ReadStatus(dir, candidates);
    }

    /// <summary>读取指定启动器目录的状态（不扫描注册表）。</summary>
    public LauncherStatus ReadStatus(string launcherDir, IReadOnlyList<string> candidates)
    {
        var running = SafeIsRunning(launcherDir);
        try
        {
            var fileBytes = File.ReadAllBytes(LauncherCacheFiles.GameTempPath(launcherDir));
            var doc = KrGameTempDocument.Parse(KrCacheCodec.Decode(fileBytes));
            var active = ReadActiveBundle(launcherDir);

            var bundles = QualityTiers.All
                .Select(tier =>
                {
                    var path = doc.GetPath(tier);
                    return new BundleStatus(tier, path, LauncherCacheFiles.GetInstallState(tier, path), active == tier);
                })
                .ToArray();

            string? error = null;
            if (!doc.CanWrite)
                error = doc.FormatProblem;
            else if (!KrCacheCodec.IsRoundTripSafe(fileBytes))
                error = "缓存文件编码与预期不完全一致，已切换为只读模式";

            return new LauncherStatus(candidates, launcherDir, bundles, active, error is null, running, error);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or KrCacheFormatException)
        {
            return new LauncherStatus(candidates, launcherDir, Array.Empty<BundleStatus>(), null, false, running,
                $"读取启动器缓存失败：{ex.Message}");
        }
    }

    public WriteResult SetBundlePath(string launcherDir, QualityTier tier, string newPath, bool allowMissingExe) =>
        Write(launcherDir, newPath, allowMissingExe, doc => doc.WithBundlePath(tier, newPath),
            $"{tier.BundleName()} 游戏目录");

    public WriteResult SetAllBundlePaths(string launcherDir, string newPath, bool allowMissingExe) =>
        Write(launcherDir, newPath, allowMissingExe, doc =>
        {
            var json = doc.ToJsonBytes();
            foreach (var tier in QualityTiers.All)
                json = KrGameTempDocument.Parse(json).WithBundlePath(tier, newPath);
            return json;
        }, "UHD/HD/SD 三档游戏目录");

    public WriteResult Backup(string launcherDir)
    {
        try
        {
            var path = CreateBackup(LauncherCacheFiles.GameTempPath(launcherDir), "bin");
            return new WriteResult(true, path, new[] { $"已备份到：{path}" });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Fail($"备份失败：{ex.Message}");
        }
    }

    public IReadOnlyList<BackupInfo> ListBackups()
    {
        try
        {
            if (!Directory.Exists(_backupDir)) return Array.Empty<BackupInfo>();
            return Directory.GetFiles(_backupDir, BackupPrefix + "*.bin")
                .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
                .Select(f => new FileInfo(f))
                .Select(f => new BackupInfo(f.FullName, f.LastWriteTime, f.Length))
                .ToArray();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Array.Empty<BackupInfo>();
        }
    }

    public WriteResult Restore(string launcherDir, string backupFile)
    {
        var target = LauncherCacheFiles.GameTempPath(launcherDir);
        if (SafeIsRunning(launcherDir))
            return Fail("启动器正在运行，请先完全退出启动器（包括右下角托盘图标）后再操作");

        try
        {
            var backupBytes = File.ReadAllBytes(backupFile);
            KrGameTempDocument.Parse(KrCacheCodec.Decode(backupBytes)); // 备份本身必须可解析

            var before = CreateBackup(target, "restore-before.bin");
            File.Copy(backupFile, target, overwrite: true);

            if (!File.ReadAllBytes(target).AsSpan().SequenceEqual(backupBytes))
            {
                File.Copy(before, target, overwrite: true);
                return Fail("恢复后回读校验失败，已自动回滚");
            }
            return new WriteResult(true, before, new[] { $"已从备份恢复：{backupFile}", $"恢复前的文件已另存：{before}" });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or KrCacheFormatException)
        {
            return Fail($"恢复失败：{ex.Message}");
        }
    }

    private WriteResult Write(string launcherDir, string newPath, bool allowMissingExe,
        Func<KrGameTempDocument, byte[]> transform, string what)
    {
        var target = LauncherCacheFiles.GameTempPath(launcherDir);

        if (string.IsNullOrWhiteSpace(newPath) || !Path.IsPathFullyQualified(newPath))
            return Fail("新路径必须是完整的绝对路径");
        if (!Directory.Exists(newPath))
            return Fail($"目录不存在：{newPath}");
        if (!allowMissingExe && !File.Exists(LauncherCacheFiles.ClientExePath(newPath)))
            return Fail($"目录中未找到游戏客户端：{LauncherCacheFiles.ClientExePath(newPath)}");
        if (SafeIsRunning(launcherDir))
            return Fail("启动器正在运行，请先完全退出启动器（包括右下角托盘图标）后再操作");

        byte[] original;
        byte[] expected;
        try
        {
            original = File.ReadAllBytes(target);
            if (!KrCacheCodec.IsRoundTripSafe(original))
                return Fail("缓存文件编码与预期不完全一致，为安全起见拒绝写入");
            var doc = KrGameTempDocument.Parse(KrCacheCodec.Decode(original));
            expected = KrCacheCodec.Encode(transform(doc));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or KrCacheFormatException or ArgumentException)
        {
            return Fail($"读取或改写缓存失败：{ex.Message}");
        }

        string backupPath;
        try
        {
            backupPath = CreateBackup(target, "bin");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Fail($"写入前备份失败，已取消写入：{ex.Message}");
        }

        var messages = new List<string> { $"已备份到：{backupPath}" };
        try
        {
            ReplaceFile(target, expected);
            var readBack = File.ReadAllBytes(target);
            var ok = readBack.AsSpan().SequenceEqual(expected) && (_verifyOverride?.Invoke(readBack) ?? true);
            if (!ok)
                throw new IOException("写入后回读校验不一致");

            messages.Add($"已写入{what}：{newPath}");
            messages.Add("请完全退出并重新打开启动器后生效");
            return new WriteResult(true, backupPath, messages);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            try
            {
                File.Copy(backupPath, target, overwrite: true);
                messages.Add($"写入失败（{ex.Message}），已自动回滚到写入前的状态");
            }
            catch (Exception rollbackEx) when (rollbackEx is IOException or UnauthorizedAccessException)
            {
                messages.Add($"写入失败（{ex.Message}），自动回滚也失败（{rollbackEx.Message}），请用备份手动恢复：{backupPath}");
            }
            return new WriteResult(false, backupPath, messages);
        }
    }

    private static void ReplaceFile(string target, byte[] content)
    {
        var tmp = target + TempSuffix;
        File.WriteAllBytes(tmp, content);
        try
        {
            File.Replace(tmp, target, null);
        }
        catch (Exception ex) when (ex is IOException or PlatformNotSupportedException)
        {
            File.Copy(tmp, target, overwrite: true);
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
    }

    private string CreateBackup(string source, string suffix)
    {
        Directory.CreateDirectory(_backupDir);
        var name = $"{BackupPrefix}{_clock():yyyyMMdd-HHmmss-fff}.{suffix}";
        var path = Path.Combine(_backupDir, name);
        File.Copy(source, path, overwrite: false);
        PruneBackups();
        return path;
    }

    private void PruneBackups()
    {
        var old = Directory.GetFiles(_backupDir, BackupPrefix + "*.bin")
            .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
            .Skip(MaxBackups);
        foreach (var file in old)
            File.Delete(file);
    }

    private bool SafeIsRunning(string launcherDir)
    {
        try
        {
            return _isLauncherRunning(launcherDir);
        }
        catch (Exception)
        {
            return true; // 检测失败按运行中处理，宁可拒写
        }
    }

    private static QualityTier? ReadActiveBundle(string launcherDir)
    {
        try
        {
            var path = LauncherCacheFiles.UiStatePath(launcherDir);
            return File.Exists(path)
                ? PlatformUiState.TryReadActiveBundle(KrCacheCodec.Decode(File.ReadAllBytes(path)))
                : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or KrCacheFormatException)
        {
            return null;
        }
    }

    private static WriteResult Fail(string message) => new(false, null, new[] { message });
}
