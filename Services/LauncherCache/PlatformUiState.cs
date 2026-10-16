using System.Text.Json;
using System.Text.Json.Nodes;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>读取 platform_ui_state.bin（解码后）中启动器界面当前选中的档位。只读。</summary>
public static class PlatformUiState
{
    public static QualityTier? TryReadActiveBundle(byte[] json)
    {
        try
        {
            var scope = JsonNode.Parse(json)?["activeGameScope"];
            if (scope is JsonValue value && value.TryGetValue<string>(out var nested))
                scope = JsonNode.Parse(nested);

            var name = scope?["bundleName"]?.GetValue<string>();
            return QualityTiers.Parse(name);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            return null;
        }
    }
}
