using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;
using Xunit;

namespace WetheringWavesSteamHelper.Tests;

public sealed class LauncherCacheFilesTests
{
    [Theory]
    [InlineData(@"D:\Wuthering Waves\launcher_main.exe", @"D:\Wuthering Waves", true)]
    [InlineData(@"D:\Wuthering Waves\2.1.0\launcher_main.exe", @"D:\Wuthering Waves\", true)]
    [InlineData(@"D:\Wuthering Waves Other\launcher_main.exe", @"D:\Wuthering Waves", false)]
    [InlineData(@"d:\wuthering waves", @"D:\Wuthering Waves", true)]
    public void IsUnderDirectory(string path, string dir, bool expected)
    {
        Assert.Equal(expected, LauncherCacheFiles.IsUnderDirectory(path, dir));
    }

    [Fact]
    public void GetInstallState_NotConfigured_WhenNull()
    {
        Assert.Equal(BundleInstallState.NotConfigured, LauncherCacheFiles.GetInstallState(QualityTier.HD, null));
        Assert.Equal(BundleInstallState.DirectoryMissing, LauncherCacheFiles.GetInstallState(QualityTier.HD, @"Z:\definitely\missing"));
    }
}
