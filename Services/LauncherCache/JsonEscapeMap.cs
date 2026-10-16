using System;
using System.Collections.Generic;
using System.Text;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>反转义后的 JSON 字符串内容，以及每个输出字节对应原始转义文本中的区间。</summary>
public sealed class UnescapedString
{
    public UnescapedString(byte[] bytes, int[] srcStart, int[] srcEnd)
    {
        Bytes = bytes;
        SrcStart = srcStart;
        SrcEnd = srcEnd;
    }

    /// <summary>反转义后的 UTF-8 字节。</summary>
    public byte[] Bytes { get; }

    /// <summary>Bytes[i] 来源的转义序列在原始文本中的起始偏移（含）。</summary>
    public int[] SrcStart { get; }

    /// <summary>Bytes[i] 来源的转义序列在原始文本中的结束偏移（不含）。</summary>
    public int[] SrcEnd { get; }
}

/// <summary>JSON 字符串字面量（不含两端引号）的反转义，带位置映射，用于外科式替换。</summary>
public static class JsonEscapeMap
{
    public static UnescapedString Unescape(ReadOnlySpan<byte> raw)
    {
        var bytes = new List<byte>(raw.Length);
        var starts = new List<int>(raw.Length);
        var ends = new List<int>(raw.Length);

        void Emit(byte b, int start, int end)
        {
            bytes.Add(b);
            starts.Add(start);
            ends.Add(end);
        }

        var i = 0;
        while (i < raw.Length)
        {
            var b = raw[i];
            if (b != (byte)'\\')
            {
                Emit(b, i, i + 1);
                i++;
                continue;
            }

            if (i + 1 >= raw.Length)
                throw new KrCacheFormatException("JSON 字符串以不完整的转义结尾");

            var e = raw[i + 1];
            switch (e)
            {
                case (byte)'"': Emit((byte)'"', i, i + 2); i += 2; break;
                case (byte)'\\': Emit((byte)'\\', i, i + 2); i += 2; break;
                case (byte)'/': Emit((byte)'/', i, i + 2); i += 2; break;
                case (byte)'b': Emit(0x08, i, i + 2); i += 2; break;
                case (byte)'f': Emit(0x0C, i, i + 2); i += 2; break;
                case (byte)'n': Emit(0x0A, i, i + 2); i += 2; break;
                case (byte)'r': Emit(0x0D, i, i + 2); i += 2; break;
                case (byte)'t': Emit(0x09, i, i + 2); i += 2; break;
                case (byte)'u':
                {
                    var codePoint = ParseHex4(raw, i + 2);
                    var end = i + 6;
                    if (codePoint is >= 0xD800 and <= 0xDBFF
                        && end + 6 <= raw.Length && raw[end] == (byte)'\\' && raw[end + 1] == (byte)'u')
                    {
                        var low = ParseHex4(raw, end + 2);
                        if (low is >= 0xDC00 and <= 0xDFFF)
                        {
                            codePoint = 0x10000 + ((codePoint - 0xD800) << 10) + (low - 0xDC00);
                            end += 6;
                        }
                    }

                    if (!Rune.IsValid(codePoint))
                        throw new KrCacheFormatException($"JSON 字符串含无效的 Unicode 转义（偏移 {i}）");

                    Span<byte> utf8 = stackalloc byte[4];
                    var written = new Rune(codePoint).EncodeToUtf8(utf8);
                    for (var k = 0; k < written; k++)
                        Emit(utf8[k], i, end);
                    i = end;
                    break;
                }
                default:
                    throw new KrCacheFormatException($"JSON 字符串含未知转义 \\{(char)e}（偏移 {i}）");
            }
        }

        return new UnescapedString(bytes.ToArray(), starts.ToArray(), ends.ToArray());
    }

    private static int ParseHex4(ReadOnlySpan<byte> raw, int start)
    {
        if (start + 4 > raw.Length)
            throw new KrCacheFormatException("JSON 字符串含不完整的 \\u 转义");

        var value = 0;
        for (var k = 0; k < 4; k++)
        {
            var c = raw[start + k];
            int digit = c switch
            {
                >= (byte)'0' and <= (byte)'9' => c - '0',
                >= (byte)'a' and <= (byte)'f' => c - 'a' + 10,
                >= (byte)'A' and <= (byte)'F' => c - 'A' + 10,
                _ => throw new KrCacheFormatException("JSON 字符串含非法的十六进制转义"),
            };
            value = (value << 4) | digit;
        }
        return value;
    }
}
