namespace WutheringWavesSteamHelper.WinUI.Models;

public class GameConfig
{
    public string Name { get; set; } = "";
    public int SteamAppId { get; set; }
    public int SteamDepotId { get; set; }
    public string GameFolderName { get; set; } = "";
    public string ExeRelativePath { get; set; } = "";
    public string? CnLauncherExeName { get; set; }
    public string? CnGameRelativePath { get; set; }
    public bool IsPlaceholder { get; set; }
}

public static class GameConfigs
{
    public static readonly List<GameConfig> All =
    [
        new GameConfig
        {
            Name = "鸣潮",
            SteamAppId = 3513350,
            SteamDepotId = 3513351,
            GameFolderName = "Wuthering Waves",
            ExeRelativePath = "Wuthering Waves.exe",
            CnLauncherExeName = "launcher.exe",
            CnGameRelativePath = Path.Combine("Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe"),
            IsPlaceholder = false
        },
        new GameConfig
        {
            Name = "敬请期待",
            IsPlaceholder = true
        },
    ];
}
