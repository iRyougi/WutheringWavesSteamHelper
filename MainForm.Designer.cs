namespace WutheringWavesSteamHelper
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 620);
            this.Text = "\u9e23\u6f6e Steam \u542f\u52a8\u52a9\u624b";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei UI", 9F);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // Set application icon
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "WutheringWavesSteamHelper.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }

            // --- Color Palette ---
            var panelBg = Color.White;
            var borderColor = Color.FromArgb(218, 222, 230);
            var accentColor = Color.FromArgb(59, 130, 246);
            var accentHover = Color.FromArgb(37, 99, 235);
            var successColor = Color.FromArgb(34, 139, 34);
            var labelColor = Color.FromArgb(55, 65, 81);
            var sectionTitleColor = Color.FromArgb(30, 41, 59);
            var inputBg = Color.FromArgb(249, 250, 251);
            var btnSecondaryBg = Color.FromArgb(241, 245, 249);
            var btnSecondaryBorder = Color.FromArgb(203, 213, 225);

            // === Title Header Panel ===
            var pnlHeader = new Panel();
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Size = new Size(720, 56);
            pnlHeader.BackColor = Color.FromArgb(30, 41, 59);

            var lblTitle = new Label();
            lblTitle.Text = "\u9e23\u6f6e Steam \u542f\u52a8\u52a9\u624b";
            lblTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 13);
            lblTitle.AutoSize = true;
            pnlHeader.Controls.Add(lblTitle);

            btnHelp = new Button();
            btnHelp.Text = "\u5e2e\u52a9";
            btnHelp.Location = new Point(628, 12);
            btnHelp.Size = new Size(72, 32);
            btnHelp.Font = new Font("Microsoft YaHei UI", 9F);
            btnHelp.BackColor = Color.FromArgb(51, 65, 85);
            btnHelp.ForeColor = Color.FromArgb(203, 213, 225);
            btnHelp.FlatStyle = FlatStyle.Flat;
            btnHelp.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
            btnHelp.FlatAppearance.BorderSize = 1;
            btnHelp.Cursor = Cursors.Hand;
            btnHelp.Click += BtnHelp_Click;
            pnlHeader.Controls.Add(btnHelp);

            // ==================== Path Settings Group ====================
            var grpPaths = new GroupBox();
            grpPaths.Text = "  \u8def\u5f84\u8bbe\u7f6e  ";
            grpPaths.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            grpPaths.ForeColor = sectionTitleColor;
            grpPaths.Location = new Point(16, 70);
            grpPaths.Size = new Size(688, 180);
            grpPaths.BackColor = panelBg;

            // --- SteamLibrary Path ---
            lblSteamLibrary = new Label();
            lblSteamLibrary.Text = "SteamLibrary \u8def\u5f84\uff1a";
            lblSteamLibrary.Font = new Font("Microsoft YaHei UI", 9F);
            lblSteamLibrary.ForeColor = labelColor;
            lblSteamLibrary.Location = new Point(18, 28);
            lblSteamLibrary.AutoSize = true;

            txtSteamLibraryPath = new TextBox();
            txtSteamLibraryPath.Location = new Point(18, 50);
            txtSteamLibraryPath.Size = new Size(440, 25);
            txtSteamLibraryPath.ReadOnly = true;
            txtSteamLibraryPath.BackColor = inputBg;
            txtSteamLibraryPath.BorderStyle = BorderStyle.FixedSingle;
            txtSteamLibraryPath.Font = new Font("Microsoft YaHei UI", 9F);

            btnBrowseLibrary = new Button();
            btnBrowseLibrary.Text = "\u6d4f\u89c8...";
            btnBrowseLibrary.Location = new Point(470, 48);
            btnBrowseLibrary.Size = new Size(90, 30);
            btnBrowseLibrary.FlatStyle = FlatStyle.Flat;
            btnBrowseLibrary.BackColor = btnSecondaryBg;
            btnBrowseLibrary.ForeColor = labelColor;
            btnBrowseLibrary.FlatAppearance.BorderColor = btnSecondaryBorder;
            btnBrowseLibrary.Font = new Font("Microsoft YaHei UI", 9F);
            btnBrowseLibrary.Cursor = Cursors.Hand;
            btnBrowseLibrary.Click += BtnBrowseLibrary_Click;

            btnAutoDetectLibrary = new Button();
            btnAutoDetectLibrary.Text = "\u81ea\u52a8\u8bc6\u522b";
            btnAutoDetectLibrary.Location = new Point(572, 48);
            btnAutoDetectLibrary.Size = new Size(96, 30);
            btnAutoDetectLibrary.FlatStyle = FlatStyle.Flat;
            btnAutoDetectLibrary.BackColor = accentColor;
            btnAutoDetectLibrary.ForeColor = Color.White;
            btnAutoDetectLibrary.FlatAppearance.BorderSize = 0;
            btnAutoDetectLibrary.Font = new Font("Microsoft YaHei UI", 9F);
            btnAutoDetectLibrary.Cursor = Cursors.Hand;
            btnAutoDetectLibrary.Click += BtnAutoDetectLibrary_Click;

            // --- Steam Install Path ---
            lblSteamPath = new Label();
            lblSteamPath.Text = "Steam \u5b89\u88c5\u8def\u5f84\uff1a";
            lblSteamPath.Font = new Font("Microsoft YaHei UI", 9F);
            lblSteamPath.ForeColor = labelColor;
            lblSteamPath.Location = new Point(18, 96);
            lblSteamPath.AutoSize = true;

            txtSteamInstallPath = new TextBox();
            txtSteamInstallPath.Location = new Point(18, 118);
            txtSteamInstallPath.Size = new Size(440, 25);
            txtSteamInstallPath.ReadOnly = true;
            txtSteamInstallPath.BackColor = inputBg;
            txtSteamInstallPath.BorderStyle = BorderStyle.FixedSingle;
            txtSteamInstallPath.Font = new Font("Microsoft YaHei UI", 9F);

            btnBrowseSteam = new Button();
            btnBrowseSteam.Text = "\u6d4f\u89c8...";
            btnBrowseSteam.Location = new Point(470, 116);
            btnBrowseSteam.Size = new Size(90, 30);
            btnBrowseSteam.FlatStyle = FlatStyle.Flat;
            btnBrowseSteam.BackColor = btnSecondaryBg;
            btnBrowseSteam.ForeColor = labelColor;
            btnBrowseSteam.FlatAppearance.BorderColor = btnSecondaryBorder;
            btnBrowseSteam.Font = new Font("Microsoft YaHei UI", 9F);
            btnBrowseSteam.Cursor = Cursors.Hand;
            btnBrowseSteam.Click += BtnBrowseSteam_Click;

            btnAutoDetectSteam = new Button();
            btnAutoDetectSteam.Text = "\u81ea\u52a8\u8bc6\u522b";
            btnAutoDetectSteam.Location = new Point(572, 116);
            btnAutoDetectSteam.Size = new Size(96, 30);
            btnAutoDetectSteam.FlatStyle = FlatStyle.Flat;
            btnAutoDetectSteam.BackColor = accentColor;
            btnAutoDetectSteam.ForeColor = Color.White;
            btnAutoDetectSteam.FlatAppearance.BorderSize = 0;
            btnAutoDetectSteam.Font = new Font("Microsoft YaHei UI", 9F);
            btnAutoDetectSteam.Cursor = Cursors.Hand;
            btnAutoDetectSteam.Click += BtnAutoDetectSteam_Click;

            grpPaths.Controls.AddRange(new Control[]
            {
                lblSteamLibrary, txtSteamLibraryPath, btnBrowseLibrary, btnAutoDetectLibrary,
                lblSteamPath, txtSteamInstallPath, btnBrowseSteam, btnAutoDetectSteam
            });

            // ==================== Account & Build Info Group ====================
            var grpInfo = new GroupBox();
            grpInfo.Text = "  \u8d26\u53f7\u4e0e\u7248\u672c\u4fe1\u606f  ";
            grpInfo.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            grpInfo.ForeColor = sectionTitleColor;
            grpInfo.Location = new Point(16, 260);
            grpInfo.Size = new Size(688, 178);
            grpInfo.BackColor = panelBg;

            // --- Steam ID ---
            lblSteamId = new Label();
            lblSteamId.Text = "Steam ID\uff08SteamID64\uff0c\u4f8b\u5982 76561198422904257\uff09\uff1a";
            lblSteamId.Font = new Font("Microsoft YaHei UI", 9F);
            lblSteamId.ForeColor = labelColor;
            lblSteamId.Location = new Point(18, 28);
            lblSteamId.AutoSize = true;

            txtSteamId = new TextBox();
            txtSteamId.Location = new Point(18, 50);
            txtSteamId.Size = new Size(650, 25);
            txtSteamId.BorderStyle = BorderStyle.FixedSingle;
            txtSteamId.Font = new Font("Microsoft YaHei UI", 9F);

            // --- Build ID & Manifest ---
            lblBuildId = new Label();
            lblBuildId.Text = "Build ID\uff1a";
            lblBuildId.Font = new Font("Microsoft YaHei UI", 9F);
            lblBuildId.ForeColor = labelColor;
            lblBuildId.Location = new Point(18, 90);
            lblBuildId.AutoSize = true;

            txtBuildId = new TextBox();
            txtBuildId.Location = new Point(18, 112);
            txtBuildId.Size = new Size(290, 25);
            txtBuildId.ReadOnly = true;
            txtBuildId.BackColor = inputBg;
            txtBuildId.BorderStyle = BorderStyle.FixedSingle;
            txtBuildId.Font = new Font("Microsoft YaHei UI", 9F);

            lblManifest = new Label();
            lblManifest.Text = "Manifest ID\uff1a";
            lblManifest.Font = new Font("Microsoft YaHei UI", 9F);
            lblManifest.ForeColor = labelColor;
            lblManifest.Location = new Point(340, 90);
            lblManifest.AutoSize = true;

            txtManifest = new TextBox();
            txtManifest.Location = new Point(340, 112);
            txtManifest.Size = new Size(328, 25);
            txtManifest.ReadOnly = true;
            txtManifest.BackColor = inputBg;
            txtManifest.BorderStyle = BorderStyle.FixedSingle;
            txtManifest.Font = new Font("Microsoft YaHei UI", 9F);

            btnFetchSteamDb = new Button();
            btnFetchSteamDb.Text = "\u4ece SteamDB \u83b7\u53d6 BuildID \u548c Manifest";
            btnFetchSteamDb.Location = new Point(18, 144);
            btnFetchSteamDb.Size = new Size(290, 28);
            btnFetchSteamDb.FlatStyle = FlatStyle.Flat;
            btnFetchSteamDb.BackColor = btnSecondaryBg;
            btnFetchSteamDb.ForeColor = labelColor;
            btnFetchSteamDb.FlatAppearance.BorderColor = btnSecondaryBorder;
            btnFetchSteamDb.Font = new Font("Microsoft YaHei UI", 9F);
            btnFetchSteamDb.Cursor = Cursors.Hand;
            btnFetchSteamDb.Click += BtnFetchSteamDb_Click;

            grpInfo.Controls.AddRange(new Control[]
            {
                lblSteamId, txtSteamId,
                lblBuildId, txtBuildId, lblManifest, txtManifest, btnFetchSteamDb
            });

            // ==================== Generate Button ====================
            btnGenerate = new Button();
            btnGenerate.Text = "\u751f\u6210\u914d\u7f6e\u5e76\u542f\u7528 Steam \u542f\u52a8";
            btnGenerate.Location = new Point(16, 450);
            btnGenerate.Size = new Size(688, 48);
            btnGenerate.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            btnGenerate.BackColor = successColor;
            btnGenerate.ForeColor = Color.White;
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Cursor = Cursors.Hand;
            btnGenerate.Click += BtnGenerate_Click;

            // ==================== Log Group ====================
            var grpLog = new GroupBox();
            grpLog.Text = "  \u8fd0\u884c\u65e5\u5fd7  ";
            grpLog.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            grpLog.ForeColor = sectionTitleColor;
            grpLog.Location = new Point(16, 510);
            grpLog.Size = new Size(688, 98);
            grpLog.BackColor = panelBg;

            lblLog = new Label();
            lblLog.Visible = false;

            txtLog = new TextBox();
            txtLog.Location = new Point(18, 24);
            txtLog.Size = new Size(650, 62);
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.BackColor = Color.FromArgb(248, 250, 252);
            txtLog.BorderStyle = BorderStyle.FixedSingle;
            txtLog.Font = new Font("Consolas", 8.5F);

            grpLog.Controls.AddRange(new Control[] { txtLog });

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                pnlHeader,
                grpPaths,
                grpInfo,
                btnGenerate,
                grpLog,
                lblLog
            });
        }

        #endregion

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

        private Label lblLog;
        private TextBox txtLog;
    }
}
