using WetheringWavesSteamHelper_WinUI.Services;
using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;
using Xunit;

namespace WetheringWavesSteamHelper.Tests;

public sealed class QualityTierTests
{
    [Theory]
    [InlineData("uhd", QualityTier.UHD)]
    [InlineData("UHD", QualityTier.UHD)]
    [InlineData("极致", QualityTier.UHD)]
    [InlineData(" HD ", QualityTier.HD)]
    [InlineData("高清", QualityTier.HD)]
    [InlineData("sd", QualityTier.SD)]
    [InlineData("流畅", QualityTier.SD)]
    public void Parse_AcceptsCodesAndChineseNames(string input, QualityTier expected)
    {
        Assert.Equal(expected, QualityTiers.Parse(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("4K")]
    public void Parse_Unknown_ReturnsNull(string? input)
    {
        Assert.Null(QualityTiers.Parse(input));
    }

    [Fact]
    public void KrqlvValue_IsLowercaseCode()
    {
        Assert.Equal(new[] { "uhd", "hd", "sd" }, QualityTiers.All.Select(t => t.KrqlvValue()));
    }
}

public sealed class LaunchCommandBuilderTests
{
    [Theory]
    [InlineData(QualityTier.UHD, "uhd")]
    [InlineData(QualityTier.HD, "hd")]
    [InlineData(QualityTier.SD, "sd")]
    public void Build_PutsQualityArgBeforeCommand(QualityTier tier, string code)
    {
        var exe = @"G:\Wuthering Waves\Wuthering Waves Game\Client\Binaries\Win64\Client-Win64-Shipping.exe";
        Assert.Equal($"\"{exe}\" -krqlv={code} %command%", LaunchCommandBuilder.Build(exe, tier));
    }
}
