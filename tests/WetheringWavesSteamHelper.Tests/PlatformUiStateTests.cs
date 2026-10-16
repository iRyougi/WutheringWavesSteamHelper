using System.Text;
using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;
using Xunit;

namespace WetheringWavesSteamHelper.Tests;

public sealed class PlatformUiStateTests
{
    [Fact]
    public void ActiveGameScope_AsObject()
    {
        var json = Encoding.UTF8.GetBytes("""{"activeGameScope":{"gameId":"G152","bundleName":"UHD"}}""");
        Assert.Equal(QualityTier.UHD, PlatformUiState.TryReadActiveBundle(json));
    }

    [Fact]
    public void ActiveGameScope_AsJsonString()
    {
        var json = Encoding.UTF8.GetBytes("""{"activeGameScope":"{\u0022bundleName\u0022:\u0022SD\u0022}"}""");
        Assert.Equal(QualityTier.SD, PlatformUiState.TryReadActiveBundle(json));
    }

    [Theory]
    [InlineData("""{"other":1}""")]
    [InlineData("""{"activeGameScope":{"bundleName":5}}""")]
    [InlineData("not json")]
    public void Missing_Or_Invalid_ReturnsNull(string text)
    {
        Assert.Null(PlatformUiState.TryReadActiveBundle(Encoding.UTF8.GetBytes(text)));
    }
}
