using System.Text;

namespace WetheringWavesSteamHelper.Tests;

/// <summary>按 docs/format.md 描述的启动器写法构造测试用 kr_game_temp JSON。</summary>
internal static class Samples
{
    public const string RhiOptionCache = """{\u0022dx11\u0022:\u00220\u0022}""";

    /// <summary>JSON 字符串转义（启动器内层写法：\ → \\，" → \"）。</summary>
    public static string JsonEscape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>内层 resources 数组 JSON 文本。</summary>
    public static string Inner(params (string ResId, string Bundle, string Path)[] entries) =>
        "[" + string.Join(",", entries.Select(e =>
            $$"""{"resId":"{{e.ResId}}","bundleName":"{{e.Bundle}}","installDirPath":"{{JsonEscape(e.Path)}}"}""")) + "]";

    /// <summary>把内层 JSON 塞进外层字符串：\ → \\，" → \u0022（或 \"）。</summary>
    public static string OuterEscape(string inner, bool unicodeQuotes = true) =>
        inner.Replace("\\", "\\\\").Replace("\"", unicodeQuotes ? "\\u0022" : "\\\"");

    public static string Outer(string inner, bool unicodeQuotes = true, string legacy = @"D:\Games\WuWa\Wuthering Waves Game") =>
        "{\"exit\":\"true\",\"installDirPath\":\"" + JsonEscape(legacy) +
        "\",\"rhiOptionCache\":\"" + RhiOptionCache +
        "\",\"speedLimit\":\"0\",\"resources\":\"" + OuterEscape(inner, unicodeQuotes) + "\"}";

    public static byte[] Bytes(string json) => Encoding.UTF8.GetBytes(json);

    public static byte[] ThreeTiers(string uhd, string hd, string sd, bool unicodeQuotes = true) =>
        Bytes(Outer(Inner(("10003", "UHD", uhd), ("10003", "HD", hd), ("10003", "SD", sd)), unicodeQuotes));

    /// <summary>a 与 b 不同的字节区间 [start, endA)/[start, endB)：去掉公共前后缀后剩下的部分。</summary>
    public static (int Start, int EndA, int EndB) DiffRange(byte[] a, byte[] b)
    {
        var start = 0;
        while (start < a.Length && start < b.Length && a[start] == b[start]) start++;
        int ea = a.Length, eb = b.Length;
        while (ea > start && eb > start && a[ea - 1] == b[eb - 1]) { ea--; eb--; }
        return (start, ea, eb);
    }
}
