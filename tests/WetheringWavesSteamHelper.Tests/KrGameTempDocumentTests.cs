using System.Text;
using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;
using Xunit;

namespace WetheringWavesSteamHelper.Tests;

public sealed class KrGameTempDocumentParseTests
{
    [Fact]
    public void Parse_DocExample_SingleHdEntry()
    {
        var json = Samples.Bytes(Samples.Outer(Samples.Inner(("10003", "HD", @"D:\Games\WuWa\Wuthering Waves Game"))));
        var doc = KrGameTempDocument.Parse(json);

        Assert.True(doc.CanWrite);
        var entry = Assert.Single(doc.Bundles);
        Assert.Equal(new BundleEntry("HD", QualityTier.HD, "10003", @"D:\Games\WuWa\Wuthering Waves Game"), entry);
        Assert.Equal(@"D:\Games\WuWa\Wuthering Waves Game", doc.LegacyInstallDirPath);
        Assert.Null(doc.GetPath(QualityTier.UHD));
    }

    [Fact]
    public void Parse_ThreeTiers_BackslashQuoteStyle()
    {
        var doc = KrGameTempDocument.Parse(Samples.ThreeTiers(@"E:\U", @"D:\H", @"C:\S", unicodeQuotes: false));
        Assert.Equal(@"E:\U", doc.GetPath(QualityTier.UHD));
        Assert.Equal(@"D:\H", doc.GetPath(QualityTier.HD));
        Assert.Equal(@"C:\S", doc.GetPath(QualityTier.SD));
    }

    [Fact]
    public void Parse_FieldOrderShuffled()
    {
        var inner = """[{"installDirPath":"D:\\X","bundleName":"UHD","resId":"10003"}]""";
        var doc = KrGameTempDocument.Parse(Samples.Bytes(Samples.Outer(inner)));
        Assert.Equal(@"D:\X", doc.GetPath(QualityTier.UHD));
    }

    [Fact]
    public void Parse_ChinesePath_LiteralAndUnicodeEscaped()
    {
        var literal = KrGameTempDocument.Parse(Samples.ThreeTiers(@"D:\鸣潮\UHD", @"D:\H", @"D:\S"));
        Assert.Equal(@"D:\鸣潮\UHD", literal.GetPath(QualityTier.UHD));

        var escapedInner = """[{"resId":"10003","bundleName":"UHD","installDirPath":"D:\\\u9e23\u6f6e"}]""";
        var escaped = KrGameTempDocument.Parse(Samples.Bytes(Samples.Outer(escapedInner)));
        Assert.Equal(@"D:\鸣潮", escaped.GetPath(QualityTier.UHD));
    }

    [Fact]
    public void Parse_ResourcesMissing_IsReadOnly()
    {
        var doc = KrGameTempDocument.Parse(Samples.Bytes("""{"exit":"true","installDirPath":"D:\\G"}"""));
        Assert.False(doc.CanWrite);
        Assert.Empty(doc.Bundles);
        Assert.NotNull(doc.FormatProblem);
    }

    [Fact]
    public void Parse_ResourcesEmptyArray_IsReadOnly()
    {
        var doc = KrGameTempDocument.Parse(Samples.Bytes(Samples.Outer("[]")));
        Assert.False(doc.CanWrite);
    }

    [Fact]
    public void Parse_NotJsonObject_Throws()
    {
        Assert.Throws<KrCacheFormatException>(() => KrGameTempDocument.Parse(Samples.Bytes("[1,2]")));
        Assert.Throws<KrCacheFormatException>(() => KrGameTempDocument.Parse(Samples.Bytes("{broken")));
    }
}

public sealed class KrGameTempDocumentWriteTests
{
    [Fact]
    public void WithBundlePath_OnlyTargetValueBytesChange()
    {
        var before = Samples.ThreeTiers(@"D:\Old\UHD", @"D:\Old\HD", @"D:\Old\SD");
        var after = KrGameTempDocument.Parse(before).WithBundlePath(QualityTier.HD, @"E:\New Dir\Wuthering Waves Game");

        var doc = KrGameTempDocument.Parse(after);
        Assert.Equal(@"D:\Old\UHD", doc.GetPath(QualityTier.UHD));
        Assert.Equal(@"E:\New Dir\Wuthering Waves Game", doc.GetPath(QualityTier.HD));
        Assert.Equal(@"D:\Old\SD", doc.GetPath(QualityTier.SD));

        var (start, endA, endB) = Samples.DiffRange(before, after);
        var changedOld = Encoding.UTF8.GetString(before, start, endA - start);
        var changedNew = Encoding.UTF8.GetString(after, start, endB - start);
        Assert.DoesNotContain("UHD", changedOld + changedNew);
        Assert.DoesNotContain("u0022", changedOld + changedNew);
        // 新值在原文中以 4 个反斜杠表示路径分隔符
        Assert.Contains(@"E:\\\\New Dir\\\\Wuthering Waves Game", Encoding.UTF8.GetString(after));
    }

    [Fact]
    public void WithBundlePath_SamePathOnAllTiers_ChangesOnlyTarget()
    {
        var p = @"D:\WuWa\Wuthering Waves Game";
        var after = KrGameTempDocument.Parse(Samples.ThreeTiers(p, p, p)).WithBundlePath(QualityTier.SD, @"F:\SD");
        var doc = KrGameTempDocument.Parse(after);
        Assert.Equal(p, doc.GetPath(QualityTier.UHD));
        Assert.Equal(p, doc.GetPath(QualityTier.HD));
        Assert.Equal(@"F:\SD", doc.GetPath(QualityTier.SD));
    }

    [Theory]
    [InlineData(@"D:\鸣潮\极致 包体")]
    [InlineData(@"D:\a{b},c [d]")]
    [InlineData(@"\\NAS\share\WuWa")]
    public void WithBundlePath_UnusualPaths_RoundTrip(string newPath)
    {
        var after = KrGameTempDocument.Parse(Samples.ThreeTiers(@"D:\U", @"D:\H", @"D:\S")).WithBundlePath(QualityTier.UHD, newPath);
        Assert.Equal(newPath, KrGameTempDocument.Parse(after).GetPath(QualityTier.UHD));
    }

    [Fact]
    public void WithBundlePath_BackslashQuoteStyle_IsPreserved()
    {
        var before = Samples.ThreeTiers(@"D:\U", @"D:\H", @"D:\S", unicodeQuotes: false);
        var after = KrGameTempDocument.Parse(before).WithBundlePath(QualityTier.UHD, @"E:\X");
        Assert.DoesNotContain("u0022", Encoding.UTF8.GetString(after).Split("resources")[1]);
        Assert.Equal(@"E:\X", KrGameTempDocument.Parse(after).GetPath(QualityTier.UHD));
    }

    [Fact]
    public void WithBundlePath_EmptyOriginalValue()
    {
        var before = Samples.Bytes(Samples.Outer(Samples.Inner(("10003", "HD", ""))));
        var after = KrGameTempDocument.Parse(before).WithBundlePath(QualityTier.HD, @"D:\G");
        Assert.Equal(@"D:\G", KrGameTempDocument.Parse(after).GetPath(QualityTier.HD));
    }

    [Fact]
    public void WithBundlePath_MissingTier_AppendsEntryWithExistingResId()
    {
        var before = Samples.Bytes(Samples.Outer(Samples.Inner(("10007", "HD", @"D:\H"))));
        var after = KrGameTempDocument.Parse(before).WithBundlePath(QualityTier.UHD, @"E:\U");

        var doc = KrGameTempDocument.Parse(after);
        Assert.Equal(2, doc.Bundles.Count);
        Assert.Equal(new BundleEntry("HD", QualityTier.HD, "10007", @"D:\H"), doc.Bundles[0]);
        Assert.Equal(new BundleEntry("UHD", QualityTier.UHD, "10007", @"E:\U"), doc.Bundles[1]);
    }

    [Fact]
    public void WithBundlePath_KeepsOtherTopLevelFieldsByteIdentical()
    {
        var before = Samples.ThreeTiers(@"D:\U", @"D:\H", @"D:\S");
        var after = KrGameTempDocument.Parse(before).WithBundlePath(QualityTier.HD, @"Z:\Z");
        var text = Encoding.UTF8.GetString(after);
        Assert.Contains("\"rhiOptionCache\":\"" + Samples.RhiOptionCache + "\"", text);
        Assert.StartsWith("{\"exit\":\"true\",\"installDirPath\":\"D:\\\\Games\\\\WuWa\\\\Wuthering Waves Game\"", text);
    }

    [Fact]
    public void WithBundlePath_ReadOnlyDocument_Throws()
    {
        var doc = KrGameTempDocument.Parse(Samples.Bytes(Samples.Outer("[]")));
        Assert.Throws<KrCacheFormatException>(() => doc.WithBundlePath(QualityTier.HD, @"D:\G"));
    }

    [Fact]
    public void WithBundlePath_EmptyPath_Throws()
    {
        var doc = KrGameTempDocument.Parse(Samples.ThreeTiers(@"D:\U", @"D:\H", @"D:\S"));
        Assert.Throws<ArgumentException>(() => doc.WithBundlePath(QualityTier.HD, " "));
    }
}
