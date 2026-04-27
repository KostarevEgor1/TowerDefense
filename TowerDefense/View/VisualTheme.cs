using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using TowerDefense.Model;

namespace TowerDefense.View
{
    public static class VisualTheme
    {
        public static readonly Color ShellTop = Color.FromArgb(12, 20, 30);
        public static readonly Color ShellBottom = Color.FromArgb(5, 11, 18);
        public static readonly Color ShellGlow = Color.FromArgb(42, 66, 120, 132);
        public static readonly Color FieldTop = Color.FromArgb(21, 36, 48);
        public static readonly Color FieldBottom = Color.FromArgb(8, 18, 27);
        public static readonly Color FieldGrid = Color.FromArgb(22, 190, 225, 220);
        public static readonly Color FieldShadow = Color.FromArgb(20, 0, 0, 0);
        public static readonly Color PanelTop = Color.FromArgb(228, 17, 28, 40);
        public static readonly Color PanelBottom = Color.FromArgb(236, 10, 16, 26);
        public static readonly Color PanelBorder = Color.FromArgb(150, 112, 201, 191);
        public static readonly Color PanelHighlight = Color.FromArgb(80, 210, 240, 236);
        public static readonly Color PathA = Color.FromArgb(84, 120, 164);
        public static readonly Color PathB = Color.FromArgb(181, 126, 82);
        public static readonly Color BuildPad = Color.FromArgb(76, 62, 136, 128);
        public static readonly Color BuildPadGlow = Color.FromArgb(90, 120, 240, 214);
        public static readonly Color TextPrimary = Color.FromArgb(238, 246, 247);
        public static readonly Color TextSecondary = Color.FromArgb(172, 191, 202);
        public static readonly Color AccentMint = Color.FromArgb(120, 230, 204);
        public static readonly Color AccentBlue = Color.FromArgb(122, 164, 255);
        public static readonly Color AccentAmber = Color.FromArgb(242, 188, 102);
        public static readonly Color AccentCoral = Color.FromArgb(244, 112, 102);
        public static readonly Color AccentGold = Color.FromArgb(249, 214, 120);

        public static GraphicsPath CreateRoundedRect(RectangleF rect, float radius)
        {
            float diameter = Math.Max(1f, radius * 2f);
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void DrawRoundedPanel(
            Graphics g,
            Rectangle rect,
            float radius,
            Color top,
            Color bottom,
            Color border,
            Color highlight,
            int shadowAlpha = 86)
        {
            var shadowRect = rect;
            shadowRect.Offset(0, 6);
            using (var shadowPath = CreateRoundedRect(shadowRect, radius))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(shadowAlpha, 0, 0, 0)))
            {
                g.FillPath(shadowBrush, shadowPath);
            }

            using var panelPath = CreateRoundedRect(rect, radius);
            using var fill = new LinearGradientBrush(rect, top, bottom, 90f);
            g.FillPath(fill, panelPath);

            Rectangle sheenRect = new(rect.X + 1, rect.Y + 1, rect.Width - 2, Math.Max(18, rect.Height / 3));
            using (var sheenPath = CreateRoundedRect(sheenRect, Math.Max(8f, radius - 8f)))
            using (var sheenBrush = new LinearGradientBrush(
                sheenRect,
                Color.FromArgb(44, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                90f))
            {
                g.FillPath(sheenBrush, sheenPath);
            }

            using (var borderPen = new Pen(border, 1.35f))
            {
                g.DrawPath(borderPen, panelPath);
            }

            using var clip = CreateRoundedRect(rect, radius);
            var state = g.Save();
            g.SetClip(clip);
            using var highlightPen = new Pen(highlight, 1.6f);
            g.DrawLine(highlightPen, rect.Left + 18, rect.Top + 16, rect.Right - 18, rect.Top + 16);
            g.Restore(state);
        }

        public static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(Math.Clamp(alpha, 0, 255), color.R, color.G, color.B);
        }

        public static Color Blend(Color from, Color to, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            int r = (int)Math.Round(from.R + (to.R - from.R) * amount);
            int g = (int)Math.Round(from.G + (to.G - from.G) * amount);
            int b = (int)Math.Round(from.B + (to.B - from.B) * amount);
            int a = (int)Math.Round(from.A + (to.A - from.A) * amount);
            return Color.FromArgb(a, r, g, b);
        }

        public static Color TowerAccent(TowerType type)
        {
            return type switch
            {
                TowerType.Sniper => AccentBlue,
                TowerType.Rapid => AccentAmber,
                _ => AccentMint
            };
        }

        public static Color EnemyAccent(EnemyType type)
        {
            return type switch
            {
                EnemyType.Fast => Color.FromArgb(108, 215, 255),
                EnemyType.Tank => Color.FromArgb(220, 112, 94),
                _ => AccentCoral
            };
        }
    }
}
