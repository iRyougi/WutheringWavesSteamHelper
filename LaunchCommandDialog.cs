namespace WutheringWavesSteamHelper
{
    public class LaunchCommandDialog : Form
    {
        public LaunchCommandDialog(string launchCommand)
        {
            Text = "Steam 启动命令";
            Size = new Size(620, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Microsoft YaHei UI", 9F);
            BackColor = Color.FromArgb(245, 247, 250);

            using var iconStream = typeof(MainForm).Assembly.GetManifestResourceStream("WutheringWavesSteamHelper.ico");
            if (iconStream != null)
            {
                this.Icon = new Icon(iconStream);
            }

            var lblInstruction = new Label
            {
                Text = "请将以下启动命令复制到 Steam 中鸣潮的启动选项中：\n\n" +
                       "操作步骤：Steam 库 → 右键「Wuthering Waves」→ 属性 → 通用 → 启动选项 → 粘贴以下命令",
                Location = new Point(20, 20),
                Size = new Size(560, 60),
                ForeColor = Color.FromArgb(55, 65, 81)
            };

            var txtCommand = new TextBox
            {
                Text = launchCommand,
                Location = new Point(20, 90),
                Size = new Size(560, 60),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9.5F),
                ScrollBars = ScrollBars.Vertical
            };

            var btnCopy = new Button
            {
                Text = "复制命令",
                Location = new Point(20, 165),
                Size = new Size(120, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(launchCommand);
                btnCopy.Text = "已复制";
                btnCopy.BackColor = Color.FromArgb(34, 139, 34);
                var timer = new System.Windows.Forms.Timer { Interval = 2000 };
                timer.Tick += (_, _) =>
                {
                    btnCopy.Text = "复制命令";
                    btnCopy.BackColor = Color.FromArgb(59, 130, 246);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            };

            var lblNote = new Label
            {
                Text = "注意：此命令会让 Steam 直接启动国服鸣潮客户端，而非 Steam 版本的鸣潮。",
                Location = new Point(20, 215),
                Size = new Size(560, 20),
                ForeColor = Color.FromArgb(180, 83, 9),
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            var btnClose = new Button
            {
                Text = "关闭",
                DialogResult = DialogResult.OK,
                Location = new Point(490, 165),
                Size = new Size(90, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(55, 65, 81),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            AcceptButton = btnClose;
            Controls.AddRange(new Control[] { lblInstruction, txtCommand, btnCopy, lblNote, btnClose });
        }
    }
}
