namespace WutheringWavesSteamHelper
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            // 设计尺寸：边栏220 + 内容区680 = 900宽；头部56 + 内容区716 = 772高
            this.ClientSize = new System.Drawing.Size(900, 772);
            this.Text = "Steam 游戏启动助手";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei UI", 9F);
            this.BackColor = Color.FromArgb(243, 243, 243);

            var iconStream = typeof(MainForm).Assembly.GetManifestResourceStream("WutheringWavesSteamHelper.ico");
            if (iconStream != null)
            {
                this.Icon = new Icon(iconStream);
                iconStream.Dispose();
            }

            // ===== Sidebar (宽220) =====
            pnlSidebar = new Panel();
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Size = new Size(220, 772);
            pnlSidebar.BackColor = Color.FromArgb(30, 42, 58);

            // 标题用 AutoSize，不限制宽高
            var lblAppTitle = new Label();
            lblAppTitle.Text = "Steam 游戏\n启动助手";
            lblAppTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
            lblAppTitle.ForeColor = Color.White;
            lblAppTitle.Location = new Point(16, 18);
            lblAppTitle.AutoSize = true;
            pnlSidebar.Controls.Add(lblAppTitle);

            var divider = new Panel();
            divider.Location = new Point(16, 76);
            divider.Size = new Size(188, 1);
            divider.BackColor = Color.FromArgb(55, 70, 90);
            pnlSidebar.Controls.Add(divider);

            pnlGameButtons = new Panel();
            pnlGameButtons.Location = new Point(0, 86);
            pnlGameButtons.Size = new Size(220, 600);
            pnlGameButtons.BackColor = Color.Transparent;
            pnlSidebar.Controls.Add(pnlGameButtons);

            lblVersion = new Label();
            lblVersion.Text = "v1.1.0";
            lblVersion.Font = new Font("Microsoft YaHei UI", 8.5F);
            lblVersion.ForeColor = Color.FromArgb(100, 120, 145);
            lblVersion.Location = new Point(16, 748);
            lblVersion.AutoSize = true;
            pnlSidebar.Controls.Add(lblVersion);

            // ===== Header (x=220, 宽680, 高56) =====
            pnlHeader = new Panel();
            pnlHeader.Location = new Point(220, 0);
            pnlHeader.Size = new Size(680, 56);
            pnlHeader.BackColor = Color.FromArgb(248, 249, 252);

            lblGameTitle = new Label();
            lblGameTitle.Text = "鸣潮";
            lblGameTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
            lblGameTitle.ForeColor = Color.FromArgb(30, 41, 59);
            lblGameTitle.Location = new Point(20, 14);
            lblGameTitle.AutoSize = true;
            pnlHeader.Controls.Add(lblGameTitle);

            btnHelp = new Button();
            btnHelp.Text = "帮助";
            btnHelp.Location = new Point(592, 12);
            btnHelp.Size = new Size(72, 32);
            btnHelp.Font = new Font("Microsoft YaHei UI", 9F);
            btnHelp.BackColor = Color.FromArgb(241, 245, 249);
            btnHelp.ForeColor = Color.FromArgb(55, 65, 81);
            btnHelp.FlatStyle = FlatStyle.Flat;
            btnHelp.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnHelp.FlatAppearance.BorderSize = 1;
            btnHelp.Cursor = Cursors.Hand;
            btnHelp.Click += BtnHelp_Click;
            pnlHeader.Controls.Add(btnHelp);

            var headerDivider = new Panel();
            headerDivider.Location = new Point(0, 55);
            headerDivider.Size = new Size(680, 1);
            headerDivider.BackColor = Color.FromArgb(218, 222, 230);
            pnlHeader.Controls.Add(headerDivider);

            // ===== Content Area (x=220, y=56, 宽680, 高716) =====
            pnlContent = new Panel();
            pnlContent.Location = new Point(220, 56);
            pnlContent.Size = new Size(680, 716);
            pnlContent.BackColor = Color.FromArgb(243, 243, 243);
            pnlContent.AutoScroll = true;

            // 内容区内边距16，卡片宽 680-32=648
            // --- 路径设置卡片 (y=16, 高172) ---
            // 内容：标题14px，标签行42px，输入行62px，标签行100px，输入行120px，底部间距16px → 120+26+16=162，取172
            var cardPaths = CreateCard(new Point(16, 16), new Size(648, 172));
            cardPaths.Controls.Add(CreateSectionTitle("路径设置", new Point(16, 14)));

            lblSteamLibrary = CreateLabel("SteamLibrary 路径：", new Point(16, 44));
            cardPaths.Controls.Add(lblSteamLibrary);

            // 文本框宽：648-16-16-8-80-8-88=432，但保持原有比例，右侧留给两个按钮
            // 两按钮：浏览80 + 间距8 + 自动识别100 = 188，加左右间距16+8+8=32 → 文本框宽 648-32-188=428
            // txtBox实际高30，btnBrowse实际高24，按钮y偏移+3使两者垂直居中对齐
            txtSteamLibraryPath = CreateTextBox(new Point(16, 64), 390, true);
            cardPaths.Controls.Add(txtSteamLibraryPath);

            btnBrowseLibrary = CreateSecondaryButton("浏览...", new Point(414, 64), new Size(72, 30));
            btnBrowseLibrary.Click += BtnBrowseLibrary_Click;
            cardPaths.Controls.Add(btnBrowseLibrary);

            btnAutoDetectLibrary = CreatePrimaryButton("自动识别", new Point(494, 64), new Size(138, 30));
            btnAutoDetectLibrary.Click += BtnAutoDetectLibrary_Click;
            cardPaths.Controls.Add(btnAutoDetectLibrary);

            lblSteamPath = CreateLabel("Steam 安装路径：", new Point(16, 104));
            cardPaths.Controls.Add(lblSteamPath);

            txtSteamInstallPath = CreateTextBox(new Point(16, 124), 390, true);
            cardPaths.Controls.Add(txtSteamInstallPath);

            btnBrowseSteam = CreateSecondaryButton("浏览...", new Point(414, 124), new Size(72, 30));
            btnBrowseSteam.Click += BtnBrowseSteam_Click;
            cardPaths.Controls.Add(btnBrowseSteam);

            btnAutoDetectSteam = CreatePrimaryButton("自动识别", new Point(494, 124), new Size(138, 30));
            btnAutoDetectSteam.Click += BtnAutoDetectSteam_Click;
            cardPaths.Controls.Add(btnAutoDetectSteam);

            pnlContent.Controls.Add(cardPaths);

            // --- 账号与版本信息卡片 (y=200, 高200) ---
            // 内容：标题14，SteamID标签42，SteamID输入62，BuildID标签102，输入122，获取按钮158，底部16 → 158+28+14=200
            var cardInfo = CreateCard(new Point(16, 200), new Size(648, 200));
            cardInfo.Controls.Add(CreateSectionTitle("账号与版本信息", new Point(16, 14)));

            lblSteamId = CreateLabel("Steam ID（SteamID64，例如 76561198422904257）：", new Point(16, 44));
            cardInfo.Controls.Add(lblSteamId);

            // SteamID 输入框占满宽度（减去左右内边距）
            txtSteamId = CreateTextBox(new Point(16, 64), 616, false);
            cardInfo.Controls.Add(txtSteamId);

            lblBuildId = CreateLabel("Build ID：", new Point(16, 104));
            cardInfo.Controls.Add(lblBuildId);

            // BuildID 和 Manifest 各占一半，中间留间距
            // 总可用宽 616，各占约 300，间距 16
            txtBuildId = CreateTextBox(new Point(16, 124), 296, true);
            cardInfo.Controls.Add(txtBuildId);

            lblManifest = CreateLabel("Manifest ID：", new Point(328, 104));
            cardInfo.Controls.Add(lblManifest);

            txtManifest = CreateTextBox(new Point(328, 124), 304, true);
            cardInfo.Controls.Add(txtManifest);

            // 获取按钮占满整行，彻底避免文字截断
            btnFetchSteamDb = CreateSecondaryButton("从 SteamDB 获取 BuildID 和 Manifest", new Point(16, 162), new Size(616, 30));
            btnFetchSteamDb.Click += BtnFetchSteamDb_Click;
            cardInfo.Controls.Add(btnFetchSteamDb);

            pnlContent.Controls.Add(cardInfo);

            // --- 操作按钮区 ---
            // cardInfo 底部 y = 200+200 = 400，间距12
            btnGenerate = new Button();
            btnGenerate.Text = "生成配置并启用 Steam 启动";
            btnGenerate.Location = new Point(16, 412);
            btnGenerate.Size = new Size(648, 44);
            btnGenerate.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
            btnGenerate.BackColor = Color.FromArgb(34, 139, 34);
            btnGenerate.ForeColor = Color.White;
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Cursor = Cursors.Hand;
            btnGenerate.Click += BtnGenerate_Click;
            pnlContent.Controls.Add(btnGenerate);

            btnLaunchCommand = new Button();
            btnLaunchCommand.Text = "搜索国服鸣潮并生成启动命令";
            btnLaunchCommand.Location = new Point(16, 464);
            btnLaunchCommand.Size = new Size(648, 44);
            btnLaunchCommand.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
            btnLaunchCommand.BackColor = Color.FromArgb(59, 130, 246);
            btnLaunchCommand.ForeColor = Color.White;
            btnLaunchCommand.FlatStyle = FlatStyle.Flat;
            btnLaunchCommand.FlatAppearance.BorderSize = 0;
            btnLaunchCommand.Cursor = Cursors.Hand;
            btnLaunchCommand.Click += BtnLaunchCommand_Click;
            pnlContent.Controls.Add(btnLaunchCommand);

            btnOpenLauncher = new Button();
            btnOpenLauncher.Text = "打开官方启动器";
            btnOpenLauncher.Location = new Point(16, 516);
            btnOpenLauncher.Size = new Size(648, 44);
            btnOpenLauncher.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
            btnOpenLauncher.BackColor = Color.FromArgb(100, 116, 139);
            btnOpenLauncher.ForeColor = Color.White;
            btnOpenLauncher.FlatStyle = FlatStyle.Flat;
            btnOpenLauncher.FlatAppearance.BorderSize = 0;
            btnOpenLauncher.Cursor = Cursors.Hand;
            btnOpenLauncher.Click += BtnOpenLauncher_Click;
            pnlContent.Controls.Add(btnOpenLauncher);

            // --- 日志卡片 (y=572, 高128) ---
            // btnOpenLauncher 底部 = 516+44 = 560，间距12 → y=572
            // 内容：标题14，文本框38，高72 → 38+72+18=128
            var cardLogPanel = CreateCard(new Point(16, 572), new Size(648, 128));
            cardLogPanel.Controls.Add(CreateSectionTitle("运行日志", new Point(16, 14)));

            lblLog = new Label();
            lblLog.Visible = false;

            txtLog = new TextBox();
            txtLog.Location = new Point(16, 38);
            txtLog.Size = new Size(616, 72);
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.BackColor = Color.FromArgb(248, 250, 252);
            txtLog.BorderStyle = BorderStyle.FixedSingle;
            txtLog.Font = new Font("Consolas", 8.5F);
            cardLogPanel.Controls.Add(txtLog);
            cardLogPanel.Controls.Add(lblLog);

            pnlContent.Controls.Add(cardLogPanel);
            // cardLogPanel 底部 = 572+128 = 700，加底部间距16 → 内容总高716，与 pnlContent 高度一致 ✓

            // ===== Assemble =====
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlContent);
        }

        private Panel CreateCard(Point location, Size size)
        {
            var card = new Panel();
            card.Location = location;
            card.Size = size;
            card.BackColor = Color.White;
            return card;
        }

        private Label CreateSectionTitle(string text, Point location)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(30, 41, 59);
            lbl.Location = location;
            lbl.AutoSize = true;
            return lbl;
        }

        private Label CreateLabel(string text, Point location)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Microsoft YaHei UI", 9F);
            lbl.ForeColor = Color.FromArgb(55, 65, 81);
            lbl.Location = location;
            lbl.AutoSize = true;
            return lbl;
        }

        private TextBox CreateTextBox(Point location, int width, bool readOnly)
        {
            var txt = new TextBox();
            txt.Location = location;
            txt.Size = new Size(width, 28);
            txt.ReadOnly = readOnly;
            txt.BackColor = readOnly ? Color.FromArgb(249, 250, 251) : Color.White;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = new Font("Microsoft YaHei UI", 9F);
            return txt;
        }

        private Button CreatePrimaryButton(string text, Point location, Size size)
        {
            var btn = new Button();
            btn.Text = text;
            btn.Location = location;
            btn.Size = size;
            btn.Font = new Font("Microsoft YaHei UI", 9F);
            btn.BackColor = Color.FromArgb(59, 130, 246);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private Button CreateSecondaryButton(string text, Point location, Size size)
        {
            var btn = new Button();
            btn.Text = text;
            btn.Location = location;
            btn.Size = size;
            btn.Font = new Font("Microsoft YaHei UI", 8);
            btn.BackColor = Color.FromArgb(241, 245, 249);
            btn.ForeColor = Color.FromArgb(55, 65, 81);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btn.FlatAppearance.BorderSize = 1;
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        #endregion

        private Panel pnlSidebar;
        private Panel pnlGameButtons;
        private Panel pnlHeader;
        private Panel pnlContent;
        private Label lblGameTitle;
        private Label lblVersion;

        private Label lblSteamLibrary;
        private TextBox txtSteamLibraryPath;
        private Button btnBrowseLibrary;
        private Button btnAutoDetectLibrary;

        private Label lblSteamPath;
        private TextBox txtSteamInstallPath;
        private Button btnBrowseSteam;
        private Button btnAutoDetectSteam;

        private Label lblSteamId;
        private TextBox txtSteamId;

        private Label lblBuildId;
        private TextBox txtBuildId;
        private Label lblManifest;
        private TextBox txtManifest;
        private Button btnFetchSteamDb;

        private Button btnGenerate;
        private Button btnHelp;
        private Button btnLaunchCommand;
        private Button btnOpenLauncher;

        private Label lblLog;
        private TextBox txtLog;
    }
}
