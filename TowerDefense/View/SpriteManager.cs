using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using TowerDefense.Model;

namespace TowerDefense.View
{
    public static class SpriteManager
    {
        private static readonly Dictionary<TowerType, Bitmap> towerSprites = new();
        private static readonly Dictionary<EnemyType, Bitmap> enemySprites = new();
        private static Bitmap? projectileSprite;

        public static Bitmap GetTowerSprite(TowerType type = TowerType.Basic)
        {
            if (!towerSprites.TryGetValue(type, out var sprite))
            {
                sprite = CreateTowerSprite(type);
                towerSprites[type] = sprite;
            }

            return sprite;
        }

        public static Bitmap GetEnemySprite(EnemyType type = EnemyType.Normal)
        {
            if (!enemySprites.TryGetValue(type, out var sprite))
            {
                sprite = CreateEnemySprite(type);
                enemySprites[type] = sprite;
            }

            return sprite;
        }

        public static Bitmap GetProjectileSprite()
        {
            if (projectileSprite != null)
            {
                return projectileSprite;
            }

            projectileSprite = new Bitmap(18, 18);
            using var g = Graphics.FromImage(projectileSprite);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawGlow(g, new RectangleF(1, 1, 16, 16), VisualTheme.AccentAmber, 116);
            using (var shell = new SolidBrush(Color.FromArgb(210, 255, 245, 200)))
            {
                g.FillEllipse(shell, 4, 4, 10, 10);
            }

            using (var core = new SolidBrush(Color.FromArgb(255, 255, 255, 255)))
            {
                g.FillEllipse(core, 6, 6, 6, 6);
            }

            PointF[] flare =
            {
                new PointF(3f, 9f),
                new PointF(7f, 6f),
                new PointF(7f, 12f)
            };
            using var flareBrush = new SolidBrush(Color.FromArgb(160, 255, 198, 110));
            g.FillPolygon(flareBrush, flare);

            return projectileSprite;
        }

        private static Bitmap CreateTowerSprite(TowerType type)
        {
            var bmp = new Bitmap(72, 72);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color accent = VisualTheme.TowerAccent(type);
            Color shell = Color.FromArgb(58, 70, 90);
            Color shellDeep = Color.FromArgb(32, 40, 56);

            DrawShadow(g, new RectangleF(15, 52, 42, 10), 72);
            DrawGlow(g, new RectangleF(18, 18, 36, 36), accent, 78);

            using (var ring = new Pen(VisualTheme.WithAlpha(accent, 146), 2.6f))
            {
                g.DrawEllipse(ring, 17, 38, 38, 18);
            }

            FillRoundedRect(g, new RectangleF(18, 36, 36, 16), 8f, shell, shellDeep, Color.FromArgb(90, 108, 132));

            switch (type)
            {
                case TowerType.Sniper:
                    DrawSniperTower(g, accent, shell, shellDeep);
                    break;

                case TowerType.Rapid:
                    DrawRapidTower(g, accent, shell, shellDeep);
                    break;

                default:
                    DrawBasicTower(g, accent, shell, shellDeep);
                    break;
            }

            return bmp;
        }

        private static Bitmap CreateEnemySprite(EnemyType type)
        {
            var bmp = new Bitmap(70, 70);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color accent = VisualTheme.EnemyAccent(type);
            Color shell = Color.FromArgb(70, 42, 48);
            Color shellDeep = Color.FromArgb(36, 22, 28);

            DrawShadow(g, new RectangleF(16, 52, 38, 10), 76);

            switch (type)
            {
                case EnemyType.Fast:
                    DrawFastEnemy(g, accent, shell, shellDeep);
                    break;

                case EnemyType.Tank:
                    DrawTankEnemy(g, accent, shell, shellDeep);
                    break;

                default:
                    DrawNormalEnemy(g, accent, shell, shellDeep);
                    break;
            }

            return bmp;
        }

        private static void DrawBasicTower(Graphics g, Color accent, Color shell, Color shellDeep)
        {
            PointF[] body =
            {
                new PointF(22f, 31f),
                new PointF(28f, 18f),
                new PointF(43f, 18f),
                new PointF(50f, 31f),
                new PointF(43f, 40f),
                new PointF(28f, 40f)
            };

            using (var fill = new LinearGradientBrush(new Rectangle(22, 18, 28, 22), accent, VisualTheme.Blend(accent, Color.Black, 0.32f), 90f))
            using (var border = new Pen(Color.FromArgb(38, 48, 64), 2.2f))
            {
                g.FillPolygon(fill, body);
                g.DrawPolygon(border, body);
            }

            using (var core = new SolidBrush(Color.FromArgb(230, 234, 251, 245)))
            using (var coreGlow = new Pen(VisualTheme.WithAlpha(accent, 190), 2f))
            {
                g.FillEllipse(core, 29, 23, 14, 14);
                g.DrawEllipse(coreGlow, 27, 21, 18, 18);
            }

            using (var barrelBack = new Pen(Color.FromArgb(28, 34, 46), 6f))
            using (var barrel = new Pen(VisualTheme.Blend(accent, Color.White, 0.28f), 3.2f))
            {
                barrelBack.StartCap = LineCap.Round;
                barrelBack.EndCap = LineCap.Round;
                barrel.StartCap = LineCap.Round;
                barrel.EndCap = LineCap.Round;
                g.DrawLine(barrelBack, 35f, 29f, 56f, 24f);
                g.DrawLine(barrel, 35f, 29f, 56f, 24f);
            }
        }

        private static void DrawSniperTower(Graphics g, Color accent, Color shell, Color shellDeep)
        {
            FillRoundedRect(g, new RectangleF(22, 23, 23, 14), 6f, accent, VisualTheme.Blend(accent, Color.Black, 0.28f), Color.FromArgb(34, 44, 58));

            using (var scopeGlow = new SolidBrush(Color.FromArgb(220, 245, 252, 255)))
            using (var scopeRing = new Pen(VisualTheme.WithAlpha(accent, 210), 2f))
            {
                g.FillEllipse(scopeGlow, 25, 18, 10, 10);
                g.DrawEllipse(scopeRing, 24, 17, 12, 12);
            }

            using (var bodyPlate = new SolidBrush(shell))
            {
                g.FillRectangle(bodyPlate, 26, 34, 14, 7);
            }

            using (var barrelBack = new Pen(Color.FromArgb(24, 32, 44), 7f))
            using (var barrel = new Pen(VisualTheme.Blend(accent, Color.White, 0.16f), 3.2f))
            {
                barrelBack.StartCap = LineCap.Round;
                barrelBack.EndCap = LineCap.Round;
                barrel.StartCap = LineCap.Round;
                barrel.EndCap = LineCap.Round;
                g.DrawLine(barrelBack, 33f, 30f, 59f, 22f);
                g.DrawLine(barrel, 33f, 30f, 59f, 22f);
            }

            using var stabilizer = new Pen(VisualTheme.WithAlpha(accent, 156), 2.2f);
            g.DrawLine(stabilizer, 28f, 38f, 47f, 38f);
        }

        private static void DrawRapidTower(Graphics g, Color accent, Color shell, Color shellDeep)
        {
            FillRoundedRect(g, new RectangleF(20, 24, 28, 18), 7f, accent, VisualTheme.Blend(accent, Color.Black, 0.28f), Color.FromArgb(46, 42, 26));

            using (var vent = new SolidBrush(Color.FromArgb(210, 48, 56, 70)))
            {
                g.FillRectangle(vent, 25, 28, 3, 10);
                g.FillRectangle(vent, 31, 28, 3, 10);
                g.FillRectangle(vent, 37, 28, 3, 10);
            }

            using (var barrelBack = new Pen(Color.FromArgb(34, 36, 42), 5.5f))
            using (var barrel = new Pen(Color.FromArgb(240, 255, 238, 196), 2.4f))
            {
                barrelBack.StartCap = LineCap.Round;
                barrelBack.EndCap = LineCap.Round;
                barrel.StartCap = LineCap.Round;
                barrel.EndCap = LineCap.Round;

                g.DrawLine(barrelBack, 42f, 28f, 60f, 24f);
                g.DrawLine(barrelBack, 42f, 33f, 61f, 31f);
                g.DrawLine(barrelBack, 42f, 38f, 60f, 38f);

                g.DrawLine(barrel, 42f, 28f, 60f, 24f);
                g.DrawLine(barrel, 42f, 33f, 61f, 31f);
                g.DrawLine(barrel, 42f, 38f, 60f, 38f);
            }
        }

        private static void DrawNormalEnemy(Graphics g, Color accent, Color shell, Color shellDeep)
        {
            DrawGlow(g, new RectangleF(18, 16, 34, 34), accent, 84);

            PointF[] body =
            {
                new PointF(35f, 14f),
                new PointF(50f, 26f),
                new PointF(47f, 44f),
                new PointF(35f, 52f),
                new PointF(23f, 44f),
                new PointF(20f, 26f)
            };

            using (var fill = new LinearGradientBrush(new Rectangle(20, 14, 30, 38), accent, VisualTheme.Blend(accent, Color.Black, 0.28f), 90f))
            using (var border = new Pen(Color.FromArgb(110, 38, 28, 30), 2.4f))
            {
                g.FillPolygon(fill, body);
                g.DrawPolygon(border, body);
            }

            using (var core = new SolidBrush(Color.FromArgb(214, 255, 222, 214)))
            using (var rim = new Pen(Color.FromArgb(180, 255, 244, 234), 1.4f))
            {
                g.FillEllipse(core, 29, 25, 12, 12);
                g.DrawEllipse(rim, 27, 23, 16, 16);
            }

            using var slit = new Pen(Color.FromArgb(160, 62, 24, 24), 2.4f);
            g.DrawArc(slit, 28, 32, 14, 8, 20, 140);
        }

        private static void DrawTankEnemy(Graphics g, Color accent, Color shell, Color shellDeep)
        {
            DrawGlow(g, new RectangleF(16, 16, 38, 38), accent, 72);

            PointF[] hull =
            {
                new PointF(22f, 24f),
                new PointF(30f, 16f),
                new PointF(42f, 16f),
                new PointF(50f, 24f),
                new PointF(50f, 42f),
                new PointF(42f, 50f),
                new PointF(30f, 50f),
                new PointF(22f, 42f)
            };

            using (var fill = new LinearGradientBrush(new Rectangle(22, 16, 28, 34), accent, VisualTheme.Blend(accent, Color.Black, 0.36f), 90f))
            using (var border = new Pen(Color.FromArgb(115, 48, 24, 22), 3f))
            {
                g.FillPolygon(fill, hull);
                g.DrawPolygon(border, hull);
            }

            FillRoundedRect(g, new RectangleF(27, 24, 18, 18), 6f, shell, shellDeep, Color.FromArgb(160, 255, 220, 214));

            using var frame = new Pen(Color.FromArgb(140, 255, 190, 178), 1.8f);
            g.DrawRectangle(frame, 30, 27, 12, 12);
        }

        private static void DrawFastEnemy(Graphics g, Color accent, Color shell, Color shellDeep)
        {
            DrawGlow(g, new RectangleF(16, 18, 40, 30), accent, 78);

            PointF[] body =
            {
                new PointF(18f, 34f),
                new PointF(34f, 19f),
                new PointF(56f, 24f),
                new PointF(43f, 39f),
                new PointF(28f, 45f)
            };

            using (var fill = new LinearGradientBrush(new Rectangle(18, 19, 38, 26), accent, VisualTheme.Blend(accent, Color.Black, 0.24f), 90f))
            using (var border = new Pen(Color.FromArgb(96, 20, 56, 64), 2.4f))
            {
                g.FillPolygon(fill, body);
                g.DrawPolygon(border, body);
            }

            using (var streak = new Pen(Color.FromArgb(160, 225, 252, 255), 2.8f))
            {
                streak.StartCap = LineCap.Round;
                streak.EndCap = LineCap.Round;
                g.DrawLine(streak, 10f, 26f, 22f, 26f);
                g.DrawLine(streak, 6f, 34f, 20f, 34f);
                g.DrawLine(streak, 12f, 42f, 23f, 42f);
            }

            using var core = new SolidBrush(Color.FromArgb(220, 246, 255, 255));
            g.FillEllipse(core, 30, 27, 10, 10);
        }

        private static void FillRoundedRect(Graphics g, RectangleF rect, float radius, Color top, Color bottom, Color border)
        {
            using var path = VisualTheme.CreateRoundedRect(rect, radius);
            using (var fill = new LinearGradientBrush(Rectangle.Round(rect), top, bottom, 90f))
            {
                g.FillPath(fill, path);
            }

            using var borderPen = new Pen(border, 2f);
            g.DrawPath(borderPen, path);
        }

        private static void DrawGlow(Graphics g, RectangleF rect, Color color, int alpha)
        {
            using var path = new GraphicsPath();
            path.AddEllipse(rect);
            using var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(alpha, color),
                SurroundColors = new[] { Color.FromArgb(0, color) }
            };
            g.FillPath(brush, path);
        }

        private static void DrawShadow(Graphics g, RectangleF rect, int alpha)
        {
            using var brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0));
            g.FillEllipse(brush, rect);
        }
    }
}
