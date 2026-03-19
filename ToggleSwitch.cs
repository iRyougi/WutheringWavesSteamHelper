using System.Drawing.Drawing2D;

namespace WutheringWavesSteamHelper
{
    /// <summary>
    /// 左右拨动式 Toggle 开关，左侧显示"官方启动器"，右侧显示"WeGame"。
    /// Checked=false → 官方（滑块在左），Checked=true → WeGame（滑块在右）。
    /// </summary>
    public class ToggleSwitch : Control
    {
        private bool _checked = false;

        public event EventHandler? CheckedChanged;

        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Size = new Size(220, 36);
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var w = Width;
            var h = Height;
            int radius = h / 2;

            // 轨道颜色
            var trackColor = _checked
                ? Color.FromArgb(59, 130, 246)   // 蓝色 WeGame
                : Color.FromArgb(148, 163, 184);  // 灰蓝 官方

            // 画轨道（圆角矩形）
            using (var trackBrush = new SolidBrush(trackColor))
            {
                DrawRoundRect(g, trackBrush, 0, 0, w, h, radius);
            }

            // 滑块尺寸和位置
            int knobSize = h - 6;
            int knobY = 3;
            int knobX = _checked ? (w - knobSize - 3) : 3;

            // 画滑块
            using (var knobBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(knobBrush, knobX, knobY, knobSize, knobSize);
            }

            // 文字
            var font = new Font("Microsoft YaHei UI", 8.5F);
            var leftText = "官方启动器";
            var rightText = "WeGame";

            // 左侧文字（官方）
            var leftColor = _checked
                ? Color.FromArgb(200, 220, 240)
                : Color.White;
            // 右侧文字（WeGame）
            var rightColor = _checked
                ? Color.White
                : Color.FromArgb(200, 220, 240);

            var leftRect = new RectangleF(knobSize + 8, 0, w / 2f - knobSize - 8, h);
            var rightRect = new RectangleF(w / 2f, 0, w / 2f - knobSize / 2f - 4, h);

            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var leftBrush = new SolidBrush(leftColor))
                g.DrawString(leftText, font, leftBrush, leftRect, sf);

            using (var rightBrush = new SolidBrush(rightColor))
                g.DrawString(rightText, font, rightBrush, rightRect, sf);

            font.Dispose();
        }

        private static void DrawRoundRect(Graphics g, Brush brush, int x, int y, int w, int h, int r)
        {
            using var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }
    }
}
