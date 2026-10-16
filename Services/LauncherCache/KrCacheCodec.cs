using System;
using System.Text;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>
/// 鸣潮启动器缓存（kr_game_cache\*.bin）编解码：文件 = Base64( 每字节 XOR 0x63 ( UTF-8 JSON ) )。
/// 格式事实来源：B1ngFeng/wuwa-launcher-path docs/format.md（本实现独立编写，未复制其代码）。
/// </summary>
public static class KrCacheCodec
{
    public const byte XorKey = 0x63;

    public static byte[] Decode(byte[] fileBytes)
    {
        ArgumentNullException.ThrowIfNull(fileBytes);

        var span = fileBytes.AsSpan();
        if (span.Length >= 3 && span[0] == 0xEF && span[1] == 0xBB && span[2] == 0xBF)
            span = span[3..];

        var text = Encoding.ASCII.GetString(span).Trim();
        byte[] raw;
        try
        {
            raw = Convert.FromBase64String(text);
        }
        catch (FormatException ex)
        {
            throw new KrCacheFormatException("缓存文件不是有效的 Base64 文本", ex);
        }

        return Xor(raw);
    }

    public static byte[] Encode(byte[] json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return Encoding.ASCII.GetBytes(Convert.ToBase64String(Xor(json)));
    }

    /// <summary>重新编码后与原文件逐字节一致时才允许写回（防止格式漂移时写坏文件）。</summary>
    public static bool IsRoundTripSafe(byte[] fileBytes)
    {
        try
        {
            return Encode(Decode(fileBytes)).AsSpan().SequenceEqual(fileBytes);
        }
        catch (KrCacheFormatException)
        {
            return false;
        }
    }

    private static byte[] Xor(byte[] input)
    {
        var output = new byte[input.Length];
        for (var i = 0; i < input.Length; i++)
            output[i] = (byte)(input[i] ^ XorKey);
        return output;
    }
}
