using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;
using Xunit;

namespace WetheringWavesSteamHelper.Tests;

/// <summary>在临时目录中搭建「启动器目录 + 游戏目录」进行文件级测试。</summary>
public sealed class LauncherCacheServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "wwsh-tests-" + Guid.NewGuid().ToString("N"));
    private readonly string _launcherDir;
    private readonly string _backupDir;
    private readonly string _gameUhd;
    private readonly string _gameHd;
    private DateTime _now = new(2026, 10, 16, 12, 0, 0);

    public LauncherCacheServiceTests()
    {
        _launcherDir = Path.Combine(_root, "Wuthering Waves");
        _backupDir = Path.Combine(_root, "backup");
        _gameUhd = MakeGameDir(Path.Combine(_root, "U", "Wuthering Waves Game"), "UHD");
        _gameHd = MakeGameDir(Path.Combine(_launcherDir, "Wuthering Waves Game"), "HD");

        Directory.CreateDirectory(Path.Combine(_launcherDir, "kr_game_cache"));
        File.WriteAllBytes(GameTemp, KrCacheCodec.Encode(Samples.ThreeTiers(_gameUhd, _gameHd, _gameHd)));
    }

    private string GameTemp => LauncherCacheFiles.GameTempPath(_launcherDir);

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch (IOException) { }
    }

    private static string MakeGameDir(string dir, string tierFolder)
    {
        Directory.CreateDirectory(Path.Combine(dir, "Client", "Binaries", "Win64"));
        File.WriteAllBytes(Path.Combine(dir, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe"), Array.Empty<byte>());
        Directory.CreateDirectory(Path.Combine(dir, "Client", "Content", tierFolder));
        return dir;
    }

    private LauncherCacheService Service(bool running = false, Func<byte[], bool>? verify = null) =>
        new(_ => running, _backupDir, () => _now = _now.AddSeconds(1), verify);

    [Fact]
    public void ReadStatus_ReportsInstallStatePerTier()
    {
        var status = Service().ReadStatus(_launcherDir, new[] { _launcherDir });

        Assert.Null(status.Error);
        Assert.True(status.CanWrite);
        Assert.Equal(BundleInstallState.Installed, status.Bundles.Single(b => b.Tier == QualityTier.UHD).State);
        Assert.Equal(BundleInstallState.Installed, status.Bundles.Single(b => b.Tier == QualityTier.HD).State);
        Assert.Equal(BundleInstallState.ResourcesMissing, status.Bundles.Single(b => b.Tier == QualityTier.SD).State);
    }

    [Fact]
    public void ReadStatus_ReadsActiveBundleFromUiState()
    {
        File.WriteAllBytes(LauncherCacheFiles.UiStatePath(_launcherDir),
            KrCacheCodec.Encode(Samples.Bytes("""{"activeGameScope":{"bundleName":"UHD"}}""")));

        var status = Service().ReadStatus(_launcherDir, new[] { _launcherDir });

        Assert.Equal(QualityTier.UHD, status.ActiveBundle);
        Assert.True(status.Bundles.Single(b => b.Tier == QualityTier.UHD).IsActiveInLauncher);
    }

    [Fact]
    public void ReadStatus_CorruptFile_ReturnsError()
    {
        File.WriteAllText(GameTemp, "@@@");
        var status = Service().ReadStatus(_launcherDir, new[] { _launcherDir });
        Assert.False(status.CanWrite);
        Assert.NotNull(status.Error);
    }

    [Fact]
    public void SetBundlePath_Success_WritesBackupAndNewContent()
    {
        var original = File.ReadAllBytes(GameTemp);
        var result = Service().SetBundlePath(_launcherDir, QualityTier.HD, _gameUhd, allowMissingExe: false);

        Assert.True(result.Success, string.Join("\n", result.Messages));
        Assert.Equal(original, File.ReadAllBytes(result.BackupPath!));
        var doc = KrGameTempDocument.Parse(KrCacheCodec.Decode(File.ReadAllBytes(GameTemp)));
        Assert.Equal(_gameUhd, doc.GetPath(QualityTier.HD));
        Assert.False(File.Exists(GameTemp + ".wwsh-tmp"));
    }

    [Fact]
    public void SetAllBundlePaths_PointsAllTiersToSameDir()
    {
        var result = Service().SetAllBundlePaths(_launcherDir, _gameUhd, allowMissingExe: false);

        Assert.True(result.Success, string.Join("\n", result.Messages));
        var doc = KrGameTempDocument.Parse(KrCacheCodec.Decode(File.ReadAllBytes(GameTemp)));
        Assert.All(QualityTiers.All, t => Assert.Equal(_gameUhd, doc.GetPath(t)));
    }

    [Fact]
    public void SetBundlePath_LauncherRunning_Refuses()
    {
        var original = File.ReadAllBytes(GameTemp);
        var result = Service(running: true).SetBundlePath(_launcherDir, QualityTier.HD, _gameUhd, false);

        Assert.False(result.Success);
        Assert.Contains("启动器正在运行", result.Messages[0]);
        Assert.Equal(original, File.ReadAllBytes(GameTemp));
    }

    [Fact]
    public void SetBundlePath_MissingExe_RefusedUnlessAllowed()
    {
        var empty = Directory.CreateDirectory(Path.Combine(_root, "empty")).FullName;

        Assert.False(Service().SetBundlePath(_launcherDir, QualityTier.SD, empty, allowMissingExe: false).Success);
        Assert.True(Service().SetBundlePath(_launcherDir, QualityTier.SD, empty, allowMissingExe: true).Success);
    }

    [Fact]
    public void SetBundlePath_RelativeOrMissingDir_Refused()
    {
        Assert.False(Service().SetBundlePath(_launcherDir, QualityTier.SD, @"relative\dir", true).Success);
        Assert.False(Service().SetBundlePath(_launcherDir, QualityTier.SD, Path.Combine(_root, "nope"), true).Success);
    }

    [Fact]
    public void SetBundlePath_VerifyFails_RollsBack()
    {
        var original = File.ReadAllBytes(GameTemp);
        var result = Service(verify: _ => false).SetBundlePath(_launcherDir, QualityTier.HD, _gameUhd, false);

        Assert.False(result.Success);
        Assert.Contains(result.Messages, m => m.Contains("已自动回滚"));
        Assert.Equal(original, File.ReadAllBytes(GameTemp));
    }

    [Fact]
    public void SetBundlePath_NotRoundTripSafe_Refuses()
    {
        var withNewline = File.ReadAllBytes(GameTemp).Concat("\r\n"u8.ToArray()).ToArray();
        File.WriteAllBytes(GameTemp, withNewline);

        var result = Service().SetBundlePath(_launcherDir, QualityTier.HD, _gameUhd, false);

        Assert.False(result.Success);
        Assert.Equal(withNewline, File.ReadAllBytes(GameTemp));
    }

    [Fact]
    public void Restore_PutsBackupContentBack()
    {
        var service = Service();
        var original = File.ReadAllBytes(GameTemp);
        var write = service.SetBundlePath(_launcherDir, QualityTier.HD, _gameUhd, false);
        Assert.True(write.Success);

        var restore = service.Restore(_launcherDir, write.BackupPath!);

        Assert.True(restore.Success, string.Join("\n", restore.Messages));
        Assert.Equal(original, File.ReadAllBytes(GameTemp));
        Assert.Contains(service.ListBackups(), b => b.FilePath.EndsWith(".restore-before.bin"));
    }

    [Fact]
    public void Restore_InvalidBackup_Refuses()
    {
        var bad = Path.Combine(_root, "bad.bin");
        File.WriteAllText(bad, "@@@");
        var original = File.ReadAllBytes(GameTemp);

        Assert.False(Service().Restore(_launcherDir, bad).Success);
        Assert.Equal(original, File.ReadAllBytes(GameTemp));
    }

    [Fact]
    public void Backups_ArePrunedToMax()
    {
        var service = Service();
        for (var i = 0; i < LauncherCacheService.MaxBackups + 5; i++)
            Assert.True(service.Backup(_launcherDir).Success);

        Assert.Equal(LauncherCacheService.MaxBackups, service.ListBackups().Count);
    }
}
