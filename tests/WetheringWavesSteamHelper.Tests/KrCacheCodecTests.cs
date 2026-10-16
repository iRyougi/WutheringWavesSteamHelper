using System.Text;
using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;
using Xunit;

namespace WetheringWavesSteamHelper.Tests;

public sealed class KrCacheCodecTests
{
    [Fact]
    public void Decode_KnownCipherText_XorsWith0x63()
    {
        // "{}" = 0x7B 0x7D → XOR 0x63 → 0x18 0x1E → Base64 "GB4="
        var decoded = KrCacheCodec.Decode(Encoding.ASCII.GetBytes("GB4="));
        Assert.Equal("{}", Encoding.UTF8.GetString(decoded));
    }

    [Fact]
    public void Encode_ThenDecode_RoundTrips()
    {
        var json = Samples.ThreeTiers(@"D:\鸣潮\Wuthering Waves Game", @"D:\a", @"D:\b");
        var file = KrCacheCodec.Encode(json);
        Assert.Equal(json, KrCacheCodec.Decode(file));
        Assert.True(KrCacheCodec.IsRoundTripSafe(file));
    }

    [Fact]
    public void Decode_InvalidBase64_ThrowsFormatException()
    {
        Assert.Throws<KrCacheFormatException>(() => KrCacheCodec.Decode(Encoding.ASCII.GetBytes("@@not base64@@")));
    }

    [Fact]
    public void TrailingNewline_DecodesButIsNotRoundTripSafe()
    {
        var file = KrCacheCodec.Encode(Encoding.UTF8.GetBytes("{}")).Concat("\r\n"u8.ToArray()).ToArray();
        Assert.Equal("{}", Encoding.UTF8.GetString(KrCacheCodec.Decode(file)));
        Assert.False(KrCacheCodec.IsRoundTripSafe(file));
    }
}
