using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>
/// kr_game_temp.bin 解码后的 JSON 文档（不可变）。
/// 结构：顶层对象的 resources 字段是一个「装着 JSON 数组的字符串」，数组每项
/// {"resId","bundleName","installDirPath"} 记录一个画质档位的游戏目录。
/// 改写采用外科式字节替换：只动目标档位 installDirPath 的值，其它字节保持不变。
/// </summary>
public sealed class KrGameTempDocument
{
    private const string DefaultResId = "10003";
    private const string UnicodeQuoteEscape = "\\" + "u0022"; // 外层把内层引号写成 反斜杠+u0022
    private const string BackslashQuoteEscape = "\\\""; // 或写成 反斜杠+引号

    private readonly byte[] _json;
    private readonly ResourcesLayout? _layout;

    private KrGameTempDocument(byte[] json, ResourcesLayout? layout, string? legacyPath, string? formatProblem)
    {
        _json = json;
        _layout = layout;
        LegacyInstallDirPath = legacyPath;
        FormatProblem = formatProblem;
        Bundles = layout?.Entries
            .Where(e => e.BundleName is not null && e.Path is not null)
            .Select(e => new BundleEntry(e.BundleName!, QualityTiers.Parse(e.BundleName), e.ResId, e.Path!))
            .ToArray() ?? Array.Empty<BundleEntry>();
    }

    /// <summary>resources 中解析出的条目（按文件中的顺序）。</summary>
    public IReadOnlyList<BundleEntry> Bundles { get; }

    /// <summary>顶层旧字段 installDirPath（只读）。</summary>
    public string? LegacyInstallDirPath { get; }

    /// <summary>resources 有问题时的说明；为 null 表示可写。</summary>
    public string? FormatProblem { get; }

    public bool CanWrite => FormatProblem is null;

    /// <summary>取某档位的游戏目录；未配置返回 null（同档位多条时取第一条）。</summary>
    public string? GetPath(QualityTier tier) => Bundles.FirstOrDefault(b => b.Tier == tier)?.InstallDirPath;

    public byte[] ToJsonBytes() => (byte[])_json.Clone();

    /// <summary>解析解码后的 JSON 字节。顶层不是 JSON 对象时抛 <see cref="KrCacheFormatException"/>。</summary>
    public static KrGameTempDocument Parse(byte[] json)
    {
        ArgumentNullException.ThrowIfNull(json);

        int rawStart = -1, rawEnd = -1;
        string? legacyPath = null;
        try
        {
            var reader = new Utf8JsonReader(json);
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                throw new KrCacheFormatException("缓存内容不是 JSON 对象");

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName || reader.CurrentDepth != 1)
                    continue;

                if (reader.ValueTextEquals("resources"))
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        rawStart = (int)reader.TokenStartIndex + 1;
                        rawEnd = rawStart + reader.ValueSpan.Length;
                    }
                }
                else if (reader.ValueTextEquals("installDirPath"))
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.String)
                        legacyPath = reader.GetString();
                }
            }
        }
        catch (JsonException ex)
        {
            throw new KrCacheFormatException("缓存 JSON 解析失败", ex);
        }

        if (rawStart < 0)
            return new KrGameTempDocument(json, null, legacyPath, "缓存中没有 resources 字段（请先用启动器下载或定位一次游戏）");

        try
        {
            var layout = ParseResources(json, rawStart, rawEnd);
            var problem = layout.Entries.Count == 0 ? "resources 为空（请先用启动器下载或定位一次游戏）" : null;
            return new KrGameTempDocument(json, layout, legacyPath, problem);
        }
        catch (Exception ex) when (ex is JsonException or KrCacheFormatException)
        {
            return new KrGameTempDocument(json, null, legacyPath, $"resources 内容无法解析：{ex.Message}");
        }
    }

    /// <summary>返回把指定档位游戏目录改为 newPath 后的新 JSON 字节；档位不存在时追加一条。</summary>
    public byte[] WithBundlePath(QualityTier tier, string newPath)
    {
        if (string.IsNullOrWhiteSpace(newPath))
            throw new ArgumentException("新路径不能为空", nameof(newPath));
        if (!CanWrite || _layout is null)
            throw new KrCacheFormatException(FormatProblem ?? "缓存不可写");

        var layout = _layout;
        var raw = _json.AsSpan(layout.RawStart, layout.RawEnd - layout.RawStart);
        var quote = DetectQuoteStyle(raw);
        var inner = layout.Inner;

        var sameTier = layout.Entries.Where(e => QualityTiers.Parse(e.BundleName) == tier).ToList();
        var target = sameTier.FirstOrDefault();
        if (target is not null && target.Path is null)
            throw new KrCacheFormatException($"{tier.BundleName()} 条目缺少 installDirPath 字段");

        byte[] newRaw;
        if (target is not null)
        {
            int start, end;
            if (target.ValueEnd > target.ValueStart)
            {
                start = inner.SrcStart[target.ValueStart];
                end = inner.SrcEnd[target.ValueEnd - 1];
            }
            else
            {
                // 原值为空字符串：插入点 = 左引号转义序列的结束位置
                start = end = inner.SrcEnd[target.ValueStart - 1];
            }
            newRaw = Splice(raw, start, end, EncodeForResources(newPath, quote));
        }
        else
        {
            var resId = layout.Entries.Select(e => e.ResId).FirstOrDefault(r => !string.IsNullOrEmpty(r)) ?? DefaultResId;
            var q = quote == QuoteStyle.Unicode ? UnicodeQuoteEscape : BackslashQuoteEscape;
            var entry = new StringBuilder()
                .Append(",{")
                .Append(q).Append("resId").Append(q).Append(':').Append(q).Append(Encoding.UTF8.GetString(EncodeForResources(resId, quote))).Append(q).Append(',')
                .Append(q).Append("bundleName").Append(q).Append(':').Append(q).Append(tier.BundleName()).Append(q).Append(',')
                .Append(q).Append("installDirPath").Append(q).Append(':').Append(q).Append(Encoding.UTF8.GetString(EncodeForResources(newPath, quote))).Append(q)
                .Append('}')
                .ToString();
            var insertAt = inner.SrcStart[layout.ArrayCloseIndex];
            newRaw = Splice(raw, insertAt, insertAt, Encoding.UTF8.GetBytes(entry));
        }

        var result = new byte[layout.RawStart + newRaw.Length + (_json.Length - layout.RawEnd)];
        _json.AsSpan(0, layout.RawStart).CopyTo(result);
        newRaw.CopyTo(result, layout.RawStart);
        _json.AsSpan(layout.RawEnd).CopyTo(result.AsSpan(layout.RawStart + newRaw.Length));

        SelfCheck(result, tier, newPath, appended: target is null);
        return result;
    }

    private void SelfCheck(byte[] result, QualityTier tier, string newPath, bool appended)
    {
        var after = Parse(result);
        if (!after.CanWrite)
            throw new KrCacheFormatException($"改写自检失败：{after.FormatProblem}");

        var expectedCount = Bundles.Count + (appended ? 1 : 0);
        if (after.Bundles.Count != expectedCount)
            throw new KrCacheFormatException("改写自检失败：条目数量不符");

        var targetIndex = appended
            ? Bundles.Count
            : Bundles.ToList().FindIndex(b => b.Tier == tier);

        for (var i = 0; i < after.Bundles.Count; i++)
        {
            var b = after.Bundles[i];
            if (i == targetIndex)
            {
                if (b.Tier != tier || b.InstallDirPath != newPath)
                    throw new KrCacheFormatException("改写自检失败：目标档位路径不符");
            }
            else if (b != Bundles[i])
            {
                throw new KrCacheFormatException("改写自检失败：其它档位被意外修改");
            }
        }
    }

    private static ResourcesLayout ParseResources(byte[] json, int rawStart, int rawEnd)
    {
        var inner = JsonEscapeMap.Unescape(json.AsSpan(rawStart, rawEnd - rawStart));
        var reader = new Utf8JsonReader(inner.Bytes);
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            throw new KrCacheFormatException("resources 不是 JSON 数组");

        var entries = new List<EntrySpan>();
        var arrayClose = -1;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray && reader.CurrentDepth == 0)
            {
                arrayClose = (int)reader.TokenStartIndex;
                break;
            }

            if (reader.TokenType == JsonTokenType.StartObject && reader.CurrentDepth == 1)
            {
                entries.Add(ReadEntry(ref reader));
                continue;
            }

            if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                reader.Skip();
        }

        if (arrayClose < 0)
            throw new KrCacheFormatException("resources 数组未闭合");

        return new ResourcesLayout(rawStart, rawEnd, inner, entries, arrayClose);
    }

    private static EntrySpan ReadEntry(ref Utf8JsonReader reader)
    {
        var entry = new EntrySpan();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == 1)
                break;
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            var name = reader.GetString();
            reader.Read();

            if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
            {
                reader.Skip();
                continue;
            }

            switch (name)
            {
                case "bundleName" when reader.TokenType == JsonTokenType.String:
                    entry.BundleName = reader.GetString();
                    break;
                case "resId" when reader.TokenType == JsonTokenType.String:
                    entry.ResId = reader.GetString();
                    break;
                case "resId" when reader.TokenType == JsonTokenType.Number:
                    entry.ResId = Encoding.UTF8.GetString(reader.ValueSpan);
                    break;
                case "installDirPath" when reader.TokenType == JsonTokenType.String:
                    entry.Path = reader.GetString();
                    entry.ValueStart = (int)reader.TokenStartIndex + 1;
                    entry.ValueEnd = entry.ValueStart + reader.ValueSpan.Length;
                    break;
            }
        }
        return entry;
    }

    private static QuoteStyle DetectQuoteStyle(ReadOnlySpan<byte> raw)
    {
        if (raw.IndexOf(Encoding.ASCII.GetBytes(UnicodeQuoteEscape)) >= 0) return QuoteStyle.Unicode;
        // 字符串 token 内不可能出现未转义的引号，所以出现 " 字节即是 \" 风格
        if (raw.IndexOf((byte)'"') >= 0) return QuoteStyle.Backslash;
        return QuoteStyle.Unicode;
    }

    /// <summary>把一个字符串值编码成「内层 JSON 转义 + 外层 JSON 转义」后的形式（不含引号）。</summary>
    private static byte[] EncodeForResources(string value, QuoteStyle quote)
    {
        var outerQuote = quote == QuoteStyle.Unicode ? UnicodeQuoteEscape : BackslashQuoteEscape;
        var sb = new StringBuilder(value.Length * 2);
        foreach (var c in value)
        {
            switch (c)
            {
                case '\\':
                    sb.Append(@"\\\\");                 // 内层 \\，外层再翻倍
                    break;
                case '"':
                    sb.Append(@"\\").Append(outerQuote); // 内层 \"，外层 \\ + 引号
                    break;
                case < ' ':
                    sb.Append(@"\\u").Append(((int)c).ToString("x4")); // 内层 \u00XX，外层 \\u00XX
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static byte[] Splice(ReadOnlySpan<byte> source, int start, int end, byte[] replacement)
    {
        var result = new byte[source.Length - (end - start) + replacement.Length];
        source[..start].CopyTo(result);
        replacement.CopyTo(result, start);
        source[end..].CopyTo(result.AsSpan(start + replacement.Length));
        return result;
    }

    private enum QuoteStyle
    {
        Unicode,
        Backslash,
    }

    private sealed class EntrySpan
    {
        public string? BundleName { get; set; }
        public string? ResId { get; set; }
        public string? Path { get; set; }
        public int ValueStart { get; set; }
        public int ValueEnd { get; set; }
    }

    private sealed record ResourcesLayout(
        int RawStart,
        int RawEnd,
        UnescapedString Inner,
        IReadOnlyList<EntrySpan> Entries,
        int ArrayCloseIndex);
}
