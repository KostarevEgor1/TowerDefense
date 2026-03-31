using System;
using System.Drawing;
using TowerDefense.Model;

namespace TowerDefense.View
{
    public class GameRenderer
    {
        private readonly GameModel model;
        public GameRenderer(GameModel model) => this.model = model;

        public void Draw(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(34, 85, 34));
            var field = model.Field;

            using var gridPen = new Pen(Color.FromArgb(25, 0, 0, 0));
            for (int c = 0; c <= field.Cols; c++)
                g.DrawLine(gridPen, c * field.CellSize, 0, c * field.CellSize, field.Rows * field.CellSize);
            for (int r = 0; r <= field.Rows; r++)
                g.DrawLine(gridPen, 0, r * field.CellSize, field.Cols * field.CellSize, r * field.CellSize);

            using var pathBrush = new SolidBrush(Color.FromArgb(200, 160, 120));
            for (int c = 0; c < field.Cols; c++)
                for (int r = 0; r < field.Rows; r++)
                    if (field.IsOnPath(c, r))
                        g.FillRectangle(pathBrush, c * field.CellSize, r * field.CellSize, field.CellSize, field.CellSize);
            foreach (var tower in model.Towers)
            {
                int tx = tower.Col * field.CellSize;
                int ty = tower.Row * field.CellSize;
                var sprite = SpriteManager.GetTowerSprite();
                int offsetX = (field.CellSize - sprite.Width) / 2;
                int offsetY = (field.CellSize - sprite.Height) / 2;
                g.DrawImage(sprite, tx + offsetX, ty + offsetY);
                
                // Радиус атаки (полупрозрачный)
                float cx = tower.Col * field.CellSize + field.CellSize / 2f;
                float cy = tower.Row * field.CellSize + field.CellSize / 2f;
                using var rangePen = new Pen(Color.FromArgb(30, 100, 180, 255), 1);
                g.DrawEllipse(rangePen, cx - tower.Range, cy - tower.Range, tower.Range * 2, tower.Range * 2);
            }
            foreach (var projectile in model.Projectiles)
            {
                var sprite = SpriteManager.GetProjectileSprite();
                g.DrawImage(sprite, projectile.X - sprite.Width / 2, projectile.Y - sprite.Height / 2);
            }
            foreach (var enemy in model.Enemies)
            {
                var sprite = SpriteManager.GetEnemySprite();
                float ex = enemy.X - sprite.Width / 2;
                float ey = enemy.Y - sprite.Height / 2;
                g.DrawImage(sprite, ex, ey);
                
                // HP бар
                float hpRatio = (float)enemy.Health / enemy.MaxHealth;
                g.FillRectangle(Brushes.DarkGray, ex, ey - 6, sprite.Width, 4);
                g.FillRectangle(hpRatio > 0.5f ? Brushes.LimeGreen : Brushes.Red,
                    ex, ey - 6, sprite.Width * hpRatio, 4);
            }
            using var lf = new Font("Arial", 10, FontStyle.Bold);
            var sp = field.Path[0]; var ep = field.Path[field.Path.Count - 1];
            g.FillRectangle(Brushes.LimeGreen, sp.X * field.CellSize, sp.Y * field.CellSize, field.CellSize, field.CellSize);
            g.DrawString("S", lf, Brushes.Black, sp.X * field.CellSize + 12, sp.Y * field.CellSize + 11);
            g.FillRectangle(Brushes.Crimson,   ep.X * field.CellSize, ep.Y * field.CellSize, field.CellSize, field.CellSize);
            g.DrawString("E", lf, Brushes.White, ep.X * field.CellSize + 12, ep.Y * field.CellSize + 11);
        }
    }
}
