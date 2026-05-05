using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TowerDefense.Model;

namespace TowerDefense.View
{
    public class GameRenderer
    {
        private readonly IGameScene scene;

        public GameRenderer(IGameScene scene)
        {
            this.scene = scene;
        }

        public void Draw(Graphics g, Point mouseCell, TowerType selectedType = TowerType.Basic)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            DrawBackground(g);
            DrawPaths(g);
            DrawBuildCells(g);
            DrawPathMarkers(g);
            DrawProjectiles(g);
            DrawEnemies(g);
            DrawTowers(g, mouseCell);
            DrawImpactEffects(g);
            DrawPlacementPreview(g, mouseCell, selectedType);
        }

        private void DrawBackground(Graphics g)
        {
            var field = scene.Field;
            int width = field.Cols * field.CellSize;
            int height = field.Rows * field.CellSize;
            Rectangle bounds = new(0, 0, width, height);

            using (var background = new LinearGradientBrush(bounds, VisualTheme.FieldTop, VisualTheme.FieldBottom, 90f))
            {
                g.FillRectangle(background, bounds);
            }

            DrawAmbientGlow(g, new RectangleF(40, 50, 250, 210), VisualTheme.AccentBlue, 28);
            DrawAmbientGlow(g, new RectangleF(width - 270, 30, 220, 220), VisualTheme.AccentAmber, 20);
            DrawAmbientGlow(g, new RectangleF(width / 2f - 140f, height - 200f, 280f, 190f), VisualTheme.AccentMint, 22);

            for (int c = 0; c < field.Cols; c++)
            {
                for (int r = 0; r < field.Rows; r++)
                {
                    int x = c * field.CellSize;
                    int y = r * field.CellSize;
                    Color tile = ((c + r) % 2 == 0)
                        ? Color.FromArgb(18, 255, 255, 255)
                        : Color.FromArgb(10, 0, 0, 0);
                    using var tileBrush = new SolidBrush(tile);
                    g.FillRectangle(tileBrush, x, y, field.CellSize, field.CellSize);

                    using var accentPen = new Pen(Color.FromArgb(18, 190, 225, 220), 1f);
                    g.DrawLine(accentPen, x + 8, y + 8, x + 14, y + 8);
                    g.DrawLine(accentPen, x + 8, y + 8, x + 8, y + 14);
                }
            }

            using var gridPen = new Pen(VisualTheme.FieldGrid, 1f);
            for (int c = 0; c <= field.Cols; c++)
            {
                g.DrawLine(gridPen, c * field.CellSize, 0, c * field.CellSize, height);
            }
            for (int r = 0; r <= field.Rows; r++)
            {
                g.DrawLine(gridPen, 0, r * field.CellSize, width, r * field.CellSize);
            }
        }

        private void DrawPaths(Graphics g)
        {
            var field = scene.Field;

            for (int c = 0; c < field.Cols; c++)
            {
                for (int r = 0; r < field.Rows; r++)
                {
                    int pathIndex = field.PathIndexForCell(c, r);
                    if (pathIndex < 0)
                    {
                        continue;
                    }

                    int x = c * field.CellSize;
                    int y = r * field.CellSize;
                    Rectangle laneRect = new(x + 3, y + 3, field.CellSize - 6, field.CellSize - 6);
                    Color accent = pathIndex == 0 ? VisualTheme.PathA : VisualTheme.PathB;
                    Color bottom = VisualTheme.Blend(accent, Color.Black, 0.42f);

                    using (var path = VisualTheme.CreateRoundedRect(laneRect, 9f))
                    using (var fill = new LinearGradientBrush(laneRect, accent, bottom, 90f))
                    using (var border = new Pen(VisualTheme.WithAlpha(accent, 220), 1.4f))
                    {
                        g.FillPath(fill, path);
                        g.DrawPath(border, path);
                    }

                }
            }
        }

        private void DrawBuildCells(Graphics g)
        {
            var field = scene.Field;

            for (int c = 0; c < field.Cols; c++)
            {
                for (int r = 0; r < field.Rows; r++)
                {
                    if (!field.IsInBuildZone(c, r) || field.IsOnAnyPath(c, r))
                    {
                        continue;
                    }

                    int x = c * field.CellSize;
                    int y = r * field.CellSize;
                    Rectangle pad = new(x + 6, y + 6, field.CellSize - 12, field.CellSize - 12);

                    DrawAmbientGlow(g, new RectangleF(x + 4, y + 4, field.CellSize - 8, field.CellSize - 8), VisualTheme.BuildPadGlow, 34);
                    using (var path = VisualTheme.CreateRoundedRect(pad, 10f))
                    using (var fill = new LinearGradientBrush(pad, Color.FromArgb(92, 26, 56, 60), Color.FromArgb(148, 18, 32, 44), 90f))
                    using (var border = new Pen(Color.FromArgb(150, 114, 225, 206), 1.4f))
                    {
                        g.FillPath(fill, path);
                        g.DrawPath(border, path);
                    }

                    using (var ring = new Pen(Color.FromArgb(140, 170, 255, 232), 1.4f))
                    {
                        g.DrawEllipse(ring, x + 12, y + 12, field.CellSize - 24, field.CellSize - 24);
                    }

                    using var dot = new SolidBrush(Color.FromArgb(165, 214, 245, 236));
                    g.FillEllipse(dot, x + field.CellSize / 2 - 2, y + field.CellSize / 2 - 2, 4, 4);
                }
            }
        }

        private void DrawPathMarkers(Graphics g)
        {
            var field = scene.Field;

            for (int i = 0; i < field.ActivePaths.Count; i++)
            {
                var path = field.ActivePaths[i];
                var end = path[path.Count - 1];
                DrawMarkerCell(g, end.X, end.Y, VisualTheme.AccentCoral, string.Empty, null, null, null);
            }
        }

        private void DrawMarkerCell(Graphics g, int col, int row, Color accent, string title, Font? titleFont, string? footer, Font? footerFont)
        {
            var field = scene.Field;
            int x = col * field.CellSize + 4;
            int y = row * field.CellSize + 4;
            Rectangle rect = new(x, y, field.CellSize - 8, field.CellSize - 8);

            using (var path = VisualTheme.CreateRoundedRect(rect, 9f))
            using (var fill = new LinearGradientBrush(rect, VisualTheme.Blend(accent, Color.White, 0.14f), VisualTheme.Blend(accent, Color.Black, 0.45f), 90f))
            using (var border = new Pen(Color.FromArgb(215, accent), 1.5f))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            if (!string.IsNullOrWhiteSpace(title) && titleFont != null)
            {
                TextRenderer.DrawText(g, title, titleFont, new Rectangle(x, y + 4, rect.Width, 14), Color.WhiteSmoke,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            if (!string.IsNullOrWhiteSpace(footer) && footerFont != null)
            {
                TextRenderer.DrawText(g, footer, footerFont, new Rectangle(x, y + 18, rect.Width, 12), VisualTheme.AccentGold,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawTowers(Graphics g, Point mouseCell)
        {
            var field = scene.Field;
            Tower? hoveredTower = null;
            foreach (var tower in scene.Towers)
            {
                if (tower.Col == mouseCell.X && tower.Row == mouseCell.Y)
                {
                    hoveredTower = tower;
                    break;
                }
            }
            using var badgeFont = new Font("Bahnschrift SemiBold", 7.5f, FontStyle.Bold);

            foreach (var tower in scene.Towers)
            {
                int tx = tower.Col * field.CellSize;
                int ty = tower.Row * field.CellSize;
                Color accent = VisualTheme.TowerAccent(tower.Type);

                if (hoveredTower == tower)
                {
                    DrawRange(g, tx, ty, tower.Range, accent);
                }

                DrawAmbientGlow(g, new RectangleF(tx + 4, ty + 5, field.CellSize - 8, field.CellSize - 6), accent, hoveredTower == tower ? 48 : 24);
                using (var baseShadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                {
                    g.FillEllipse(baseShadow, tx + 8, ty + field.CellSize - 12, field.CellSize - 16, 8);
                }

                using (var ring = new Pen(Color.FromArgb(hoveredTower == tower ? 180 : 120, accent), hoveredTower == tower ? 2f : 1.2f))
                {
                    g.DrawEllipse(ring, tx + 8, ty + field.CellSize - 18, field.CellSize - 16, 10);
                }

                var sprite = SpriteManager.GetTowerSprite(tower.Type);
                var targetRect = new Rectangle(tx + 2, ty + 1, field.CellSize - 4, field.CellSize - 2);
                g.DrawImage(sprite, targetRect);

                if (tower.Level > 1)
                {
                    Rectangle badgeRect = new(tx + field.CellSize - 18, ty + 3, 14, 14);
                    VisualTheme.DrawRoundedPanel(
                        g,
                        badgeRect,
                        7f,
                        Color.FromArgb(235, 18, 28, 40),
                        Color.FromArgb(240, 8, 13, 20),
                        Color.FromArgb(180, accent),
                        Color.FromArgb(60, 255, 255, 255),
                        shadowAlpha: 34);

                    TextRenderer.DrawText(g, tower.Level.ToString(), badgeFont, badgeRect, VisualTheme.TextPrimary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }

        private void DrawProjectiles(Graphics g)
        {
            var sprite = SpriteManager.GetProjectileSprite();
            foreach (var projectile in scene.Projectiles)
            {
                using (var trailPen = new Pen(Color.FromArgb(120, 255, 188, 112), 2.4f))
                {
                    trailPen.StartCap = LineCap.Round;
                    trailPen.EndCap = LineCap.Round;
                    g.DrawLine(trailPen, projectile.PreviousX, projectile.PreviousY, projectile.X, projectile.Y);
                }

                g.DrawImage(sprite, projectile.X - sprite.Width / 2f, projectile.Y - sprite.Height / 2f);
            }
        }

        private void DrawEnemies(Graphics g)
        {
            foreach (var enemy in scene.Enemies)
            {
                var sprite = SpriteManager.GetEnemySprite(enemy.Type);
                float drawSize = enemy.Type == EnemyType.Tank ? 36f : 32f;
                float ex = enemy.X - drawSize / 2f;
                float ey = enemy.Y - drawSize / 2f;
                Color accent = VisualTheme.EnemyAccent(enemy.Type);

                using (var shadow = new SolidBrush(Color.FromArgb(78, 0, 0, 0)))
                {
                    g.FillEllipse(shadow, ex + 5, ey + drawSize - 6, drawSize - 10, 7);
                }

                g.DrawImage(sprite, ex, ey - 1, drawSize, drawSize);

                float hpRatio = Math.Max(0f, (float)enemy.Health / enemy.MaxHealth);
                RectangleF hpRect = new(ex, ey - 9, drawSize, 6);
                using (var back = new SolidBrush(Color.FromArgb(145, 10, 14, 18)))
                using (var border = new Pen(Color.FromArgb(120, 255, 255, 255), 1f))
                using (var fill = new SolidBrush(Color.FromArgb(220, hpRatio > 0.45f ? accent : VisualTheme.AccentCoral)))
                {
                    g.FillRectangle(back, hpRect);
                    g.FillRectangle(fill, hpRect.X + 1, hpRect.Y + 1, Math.Max(0, (drawSize - 2) * hpRatio), hpRect.Height - 2);
                    g.DrawRectangle(border, hpRect.X, hpRect.Y, hpRect.Width, hpRect.Height);
                }
            }
        }

        private void DrawImpactEffects(Graphics g)
        {
            foreach (var effect in scene.ImpactEffects)
            {
                float lifeRatio = effect.MaxLifetime <= 0 ? 0f : (float)effect.Lifetime / effect.MaxLifetime;
                float progress = 1f - lifeRatio;
                bool burst = effect.MaxLifetime >= 14;
                float baseRadius = burst ? 10f : 5f;
                float radiusGrowth = burst ? 28f : 18f;
                float radius = baseRadius + progress * radiusGrowth;
                int alpha = (int)((burst ? 180 : 150) * lifeRatio);
                Color tone = burst ? VisualTheme.AccentCoral : VisualTheme.AccentAmber;

                DrawAmbientGlow(g, new RectangleF(effect.X - radius, effect.Y - radius, radius * 2, radius * 2), tone, alpha / 3);
                using var ring = new Pen(Color.FromArgb(alpha, tone), 2f);
                g.DrawEllipse(ring, effect.X - radius, effect.Y - radius, radius * 2, radius * 2);
            }
        }

        private void DrawPlacementPreview(Graphics g, Point mouseCell, TowerType selectedType)
        {
            var field = scene.Field;
            if (mouseCell.X < 0 || mouseCell.X >= field.Cols || mouseCell.Y < 0 || mouseCell.Y >= field.Rows)
            {
                return;
            }
            bool inBuildZone = field.IsInBuildZone(mouseCell.X, mouseCell.Y);
            bool occupied = false;
            foreach (var tower in scene.Towers)
            {
                if (tower.Col == mouseCell.X && tower.Row == mouseCell.Y)
                {
                    occupied = true;
                    break;
                }
            }

            Color accent = VisualTheme.TowerAccent(selectedType);
            if (!inBuildZone || occupied)
            {
                accent = VisualTheme.AccentCoral;
            }

            Rectangle padRect = new(mouseCell.X * field.CellSize + 4, mouseCell.Y * field.CellSize + 4, field.CellSize - 8, field.CellSize - 8);
            using (var path = VisualTheme.CreateRoundedRect(padRect, 11f))
            using (var fill = new SolidBrush(Color.FromArgb(54, accent)))
            using (var border = new Pen(Color.FromArgb(170, accent), 1.8f))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            var sprite = SpriteManager.GetTowerSprite(selectedType);
            DrawImageWithOpacity(g, sprite, new Rectangle(mouseCell.X * field.CellSize + 2, mouseCell.Y * field.CellSize + 1, field.CellSize - 4, field.CellSize - 2), 0.52f);
        }

        private static void DrawRange(Graphics g, int tx, int ty, float range, Color accent)
        {
            float cx = tx + 20f;
            float cy = ty + 20f;
            using var fill = new SolidBrush(Color.FromArgb(20, accent));
            using var pen = new Pen(Color.FromArgb(135, accent), 1.5f);
            pen.DashStyle = DashStyle.Dash;
            g.FillEllipse(fill, cx - range, cy - range, range * 2, range * 2);
            g.DrawEllipse(pen, cx - range, cy - range, range * 2, range * 2);
        }

        private static void DrawAmbientGlow(Graphics g, RectangleF rect, Color color, int alpha)
        {
            using var path = new GraphicsPath();
            path.AddEllipse(rect);
            using var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(Math.Max(0, Math.Min(255, alpha)), color),
                SurroundColors = new[] { Color.FromArgb(0, color) }
            };
            g.FillPath(brush, path);
        }

        private static void DrawImageWithOpacity(Graphics g, Image image, Rectangle bounds, float opacity)
        {
            using var attributes = new ImageAttributes();
            var matrix = new ColorMatrix
            {
                Matrix33 = Math.Max(0f, Math.Min(1f, opacity))
            };
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            g.DrawImage(image, bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
        }
    }
}
