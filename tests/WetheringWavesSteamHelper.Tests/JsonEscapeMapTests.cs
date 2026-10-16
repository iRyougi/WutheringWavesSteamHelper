using System.Text;
using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;
using Xunit;

namespace WetheringWavesSteamHelper.Tests;

public sealed class JsonEscapeMapTests
{
    [Fact]
    public void Unescape_MapsEachOutputByteToItsEscapeSequence()
    {
        // 原文：a\\b\u0022c\/\n
        var raw = Encoding.ASCII.GetBytes(@"a\\b\u0022c\/\n");
        var u = JsonEscapeMap.Unescape(raw);

        Assert.Equal("a\\b\"c/\n", Encoding.UTF8.GetString(u.Bytes));
        Assert.Equal(new[] { 0, 1, 3, 4, 10, 11, 13 }, u.SrcStart);
        Assert.Equal(new[] { 1, 3, 4, 10, 11, 13, 15 }, u.SrcEnd);
    }

    [Fact]
    public void Unescape_UnicodeEscape_AllUtf8BytesShareOneSpan()
    {
        var raw = Encoding.ASCII.GetBytes(@"\u9e23x"); // 鸣
        var u = JsonEscapeMap.Unescape(raw);

        Assert.Equal("鸣x", Encoding.UTF8.GetString(u.Bytes));
        Assert.Equal(new[] { 0, 0, 0, 6 }, u.SrcStart);
        Assert.Equal(new[] { 6, 6, 6, 7 }, u.SrcEnd);
    }

    [Fact]
    public void Unescape_SurrogatePair_MergedIntoOneSpan()
    {
        var raw = Encoding.ASCII.GetBytes(@"\ud83d\ude00"); // 😀
        var u = JsonEscapeMap.Unescape(raw);

        Assert.Equal("😀", Encoding.UTF8.GetString(u.Bytes));
        Assert.All(u.SrcStart, s => Assert.Equal(0, s));
        Assert.All(u.SrcEnd, e => Assert.Equal(12, e));
    }

    [Fact]
    public void Unescape_LiteralUtf8_PassesThrough()
    {
        var raw = Encoding.UTF8.GetBytes("鸣");
        var u = JsonEscapeMap.Unescape(raw);
        Assert.Equal(raw, u.Bytes);
        Assert.Equal(new[] { 0, 1, 2 }, u.SrcStart);
    }
}
