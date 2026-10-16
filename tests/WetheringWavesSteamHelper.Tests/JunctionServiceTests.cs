using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;
using Xunit;

namespace WetheringWavesSteamHelper.Tests;

public sealed class JunctionServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "wwsh-jn-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch (IOException) { }
    }

    private string MakeGame(string name)
    {
        var dir = Path.Combine(_root, name);
        Directory.CreateDirectory(Path.Combine(dir, "Client", "Binaries", "Win64"));
        File.WriteAllBytes(Path.Combine(dir, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe"), Array.Empty<byte>());
        return dir;
    }

    [Fact]
    public void GetLinkPath_IsSiblingNamedWutheringWavesGame()
    {
        Assert.Equal(@"D:\rail_apps\Wuthering Waves Game", JunctionService.GetLinkPath(@"D:\rail_apps\Wuthering Waves(2002137)\"));
        Assert.Null(JunctionService.GetLinkPath(@"D:\"));
    }

    [Fact]
    public void Validate_RejectsNonGameDir_AlreadyNamed_AndExistingLink()
    {
        Assert.NotNull(JunctionService.Validate(Directory.CreateDirectory(Path.Combine(_root, "x")).FullName));
        Assert.NotNull(JunctionService.Validate(MakeGame("Wuthering Waves Game")));

        var wegame = MakeGame("Wuthering Waves(2002137)");
        Assert.NotNull(JunctionService.Validate(wegame)); // 同级已有 Wuthering Waves Game
    }

    [Fact]
    public void Create_LiteralEnvironmentVariableInPath_IsNotExpanded()
    {
        var game = MakeGame("%USERNAME% & WuWa!");
        var link = JunctionService.GetLinkPath(game)!;
        try
        {
            var result = JunctionService.Create(game);

            Assert.True(result.Success, result.Message);
            Assert.True(File.Exists(LauncherCacheFiles.ClientExePath(link)));
            Assert.Equal(game, Directory.ResolveLinkTarget(link, returnFinalTarget: true)!.FullName);
        }
        finally
        {
            // A failed command may still have created a dangling junction.
            if (new DirectoryInfo(link).LinkTarget is not null)
                Directory.Delete(link);
        }
        Assert.True(File.Exists(LauncherCacheFiles.ClientExePath(game)));
    }

    [Fact]
    public void Create_MakesJunctionToTarget()
    {
        var wegame = MakeGame("Wuthering Waves(2002137)");
        Assert.Null(JunctionService.Validate(wegame));

        var result = JunctionService.Create(wegame);

        Assert.True(result.Success, result.Message);
        Assert.True(File.Exists(Path.Combine(result.LinkPath, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe")));
        Directory.Delete(result.LinkPath); // 删除联接本身，不删目标
        Assert.True(Directory.Exists(wegame));
    }
}
