using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

namespace TowerDefense.View
{
    public class AccentButton : Button
    {
        private bool hovered;
        private bool pressed;
        private bool squareStyle;
        private Color baseColor = VisualTheme.AccentMint;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BaseColor
        {
            get => baseColor;
            set
            {
                baseColor = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GlowColor { get; set; } = VisualTheme.AccentMint;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selected { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SquareStyle
        {
            get => squareStyle;
            set
            {
                if (squareStyle == value)
                {
                    return;
                }

                squareStyle = value;
                UpdateButtonRegion();
                Invalidate();
            }
        }

        public AccentButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);

            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = VisualTheme.TextPrimary;
            Font = new Font("Bahnschrift SemiBold", 10.5f, FontStyle.Bold);
            UpdateButtonRegion();
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            UpdateButtonRegion();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        private void UpdateButtonRegion()
        {
            if (Width <= 1 || Height <= 1)
            {
                return;
            }

            if (SquareStyle)
            {
                Region = null;
                return;
            }

            using var path = VisualTheme.CreateRoundedRect(new RectangleF(0, 0, Width - 1, Height - 1), 14f);
            Region = new Region(path);
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            hovered = false;
            pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Left)
            {
                pressed = true;
                Invalidate();
            }

            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SquareStyle ? SmoothingMode.None : SmoothingMode.AntiAlias;

            if (SquareStyle)
            {
                Rectangle squareRect = new(0, 0, System.Math.Max(1, Width - 1), System.Math.Max(1, Height - 1));
                Color fillColor = VisualTheme.Blend(baseColor, Color.White, pressed ? 0.05f : hovered ? 0.1f : 0.08f);
                using var fill = new SolidBrush(fillColor);
                e.Graphics.FillRectangle(fill, squareRect);

                Color borderColor = Selected ? GlowColor : VisualTheme.Blend(baseColor, Color.Black, 0.34f);
                using var border = new Pen(borderColor, Selected ? 2f : 1f);
                e.Graphics.DrawRectangle(border, squareRect);

                Rectangle squareTextRect = new(squareRect.Left + 1, squareRect.Top + 1, System.Math.Max(1, squareRect.Width - 2), System.Math.Max(1, squareRect.Height - 2));
                TextRenderer.DrawText(
                    e.Graphics,
                    Text,
                    Font,
                    squareTextRect,
                    ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                return;
            }

            Rectangle rect = new(1, 1, System.Math.Max(1, Width - 3), System.Math.Max(1, Height - 3));
            Rectangle shadowRect = rect;
            shadowRect.Offset(0, 3);

            using (var shadowPath = VisualTheme.CreateRoundedRect(shadowRect, 14f))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
            }

            Color glow = Selected
                ? GlowColor
                : hovered
                    ? VisualTheme.Blend(GlowColor, Color.White, 0.2f)
                    : VisualTheme.WithAlpha(GlowColor, 180);

            Color top = VisualTheme.Blend(baseColor, Color.White, pressed ? 0.08f : hovered ? 0.18f : 0.12f);
            Color bottom = VisualTheme.Blend(baseColor, Color.Black, pressed ? 0.34f : 0.2f);

            using var path = VisualTheme.CreateRoundedRect(rect, 14f);
            using (var fill = new LinearGradientBrush(rect, top, bottom, 90f))
            {
                e.Graphics.FillPath(fill, path);
            }

            using (var glossPen = new Pen(Color.FromArgb(hovered ? 110 : 72, 255, 255, 255), 1.3f))
            {
                e.Graphics.DrawLine(glossPen, rect.Left + 14, rect.Top + 10, rect.Right - 14, rect.Top + 10);
            }

            using (var borderPen = new Pen(Selected ? glow : VisualTheme.WithAlpha(glow, hovered ? 210 : 150), Selected ? 2f : 1.25f))
            {
                e.Graphics.DrawPath(borderPen, path);
            }

            Rectangle textRect = new(rect.Left, rect.Top - 1, rect.Width, rect.Height);
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                textRect,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    public class GlassPanel : Panel
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CornerRadius { get; set; } = 28;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color FillTop { get; set; } = VisualTheme.PanelTop;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color FillBottom { get; set; } = VisualTheme.PanelBottom;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = VisualTheme.PanelBorder;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color HighlightColor { get; set; } = VisualTheme.PanelHighlight;

        public GlassPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            BackColor = Color.Transparent;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new(0, 0, Width - 1, Height - 1);
            VisualTheme.DrawRoundedPanel(
                e.Graphics,
                rect,
                CornerRadius,
                FillTop,
                FillBottom,
                BorderColor,
                HighlightColor,
                shadowAlpha: 98);
        }

        protected override void OnResize(System.EventArgs eventargs)
        {
            base.OnResize(eventargs);
            using var path = VisualTheme.CreateRoundedRect(new RectangleF(0, 0, Width - 1, Height - 1), CornerRadius);
            Region = new Region(path);
        }
    }
}
