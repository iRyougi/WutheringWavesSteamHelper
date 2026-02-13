namespace WutheringWavesSteamHelper
{
    public class LibrarySelectionDialog : Form
    {
        private ListBox listBox;
        private Button btnOk;
        private Button btnCancel;

        public string? SelectedPath { get; private set; }

        public LibrarySelectionDialog(List<string> paths)
        {
            Text = "\u9009\u62e9 Steam \u5e93\u8def\u5f84";
            Size = new Size(520, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Microsoft YaHei UI", 9F);
            BackColor = Color.FromArgb(245, 247, 250);

            // Set application icon
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "WutheringWavesSteamHelper.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }

            var label = new Label
            {
                Text = "\u68c0\u6d4b\u5230\u591a\u4e2a Steam \u5e93\u8def\u5f84\uff0c\u8bf7\u9009\u62e9\u4e00\u4e2a\uff1a",
                Location = new Point(15, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };

            listBox = new ListBox
            {
                Location = new Point(15, 45),
                Size = new Size(472, 160),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            foreach (var p in paths)
                listBox.Items.Add(p);
            if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;

            btnOk = new Button
            {
                Text = "\u786e\u5b9a",
                DialogResult = DialogResult.OK,
                Location = new Point(300, 225),
                Size = new Size(88, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                SelectedPath = listBox.SelectedItem?.ToString();
                Close();
            };

            btnCancel = new Button
            {
                Text = "\u53d6\u6d88",
                DialogResult = DialogResult.Cancel,
                Location = new Point(400, 225),
                Size = new Size(88, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(55, 65, 81),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] { label, listBox, btnOk, btnCancel });
        }
    }
}
