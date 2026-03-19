using System.Runtime.InteropServices;

namespace WutheringWavesSteamHelper
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        [STAThread]
        static void Main()
        {
            // 禁止多开：同一用户会话内只允许一个实例
            using var mutex = new System.Threading.Mutex(true, "WutheringWavesSteamHelper_SingleInstance", out bool createdNew);
            if (!createdNew)
            {
                // 找到已有实例窗口并聚焦
                var current = System.Diagnostics.Process.GetCurrentProcess();
                foreach (var proc in System.Diagnostics.Process.GetProcessesByName(current.ProcessName))
                {
                    if (proc.Id != current.Id && proc.MainWindowHandle != IntPtr.Zero)
                    {
                        ShowWindow(proc.MainWindowHandle, SW_RESTORE);
                        SetForegroundWindow(proc.MainWindowHandle);
                        break;
                    }
                }
                return;
            }

            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
