namespace WutheringWavesSteamHelper
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // PerMonitorV2: each monitor uses its own DPI; WinForms re-scales on monitor change.
            // Must be set before any UI is created, and works in tandem with the PerMonitorV2
            // declaration in app.manifest.
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
