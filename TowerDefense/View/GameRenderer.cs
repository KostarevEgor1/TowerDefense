using System;
using System.Drawing;
using TowerDefense.Model;

namespace TowerDefense.View
{
    public class GameRenderer
    {
        private readonly GameModel model;
        public GameRenderer(GameModel model) => this.model = model;

        public void Draw(Graphics g, Point mouseCell, TowerType selectedType = TowerType.Basic)
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
            
            // Зоны строительства
            using var buildZoneBrush = new SolidBrush(Color.FromArgb(40, 100, 150, 100));
            using var buildZonePen = new Pen(Color.FromArgb(120, 100, 200, 100), 2);
            foreach (var zone in field.BuildZones)
            {
                g.FillRectangle(buildZoneBrush, zone.X * field.CellSize, zone.Y * field.CellSize, 
                    field.CellSize, field.CellSize);
                g.DrawRectangle(buildZonePen, zone.X * field.CellSize + 2, zone.Y * field.CellSize + 2, 
                    field.CellSize - 4, field.CellSize - 4);
            }
            
            using var labelFont = new Font("Arial", 10, FontStyle.Bold);
            var startPoint = field.Path[0]; 
            var endPoint = field.Path[field.Path.Count - 1];
            g.FillRectangle(Brushes.LimeGreen, startPoint.X * field.CellSize, startPoint.Y * field.CellSize, field.CellSize, field.CellSize);
            g.DrawString("S", labelFont, Brushes.Black, startPoint.X * field.CellSize + 12, startPoint.Y * field.CellSize + 11);
            g.FillRectangle(Brushes.Crimson, endPoint.X * field.CellSize, endPoint.Y * field.CellSize, field.CellSize, field.CellSize);
            g.DrawString("E", labelFont, Brushes.White, endPoint.X * field.CellSize + 12, endPoint.Y * field.CellSize + 11);

            foreach (var tower in model.Towers)
            {
                int tx = tower.Col * field.CellSize;
                int ty = tower.Row * field.CellSize;
                var sprite = SpriteManager.GetTowerSprite(tower.Type);
                int offsetX = (field.CellSize - sprite.Width) / 2;
                int offsetY = (field.CellSize - sprite.Height) / 2;
                g.DrawImage(sprite, tx + offsetX, ty + offsetY);
                
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
                
                float hpRatio = (float)enemy.Health / enemy.MaxHealth;
                g.FillRectangle(Brushes.DarkGray, ex, ey - 6, sprite.Width, 4);
                g.FillRectangle(hpRatio > 0.5f ? Brushes.LimeGreen : Brushes.Red,
                    ex, ey - 6, sprite.Width * hpRatio, 4);
            }

            // Превью радиуса башни при наведении
            if (mouseCell.X >= 0 && mouseCell.X < field.Cols && mouseCell.Y >= 0 && mouseCell.Y < field.Rows)
            {
                if (model.CanPlaceTower(mouseCell.X, mouseCell.Y))
                {
                    float cx = mouseCell.X * field.CellSize + field.CellSize / 2f;
                    float cy = mouseCell.Y * field.CellSize + field.CellSize / 2f;
                    
                    // Получаем радиус для выбранного типа башни
                    float range = selectedType switch
                    {
                        TowerType.Sniper => 220f,
                        TowerType.Rapid => 80f,
                        _ => 120f
                    };
                    
                    // Полупрозрачный круг радиуса
                    using var previewBrush = new SolidBrush(Color.FromArgb(40, 100, 200, 100));
                    g.FillEllipse(previewBrush, cx - range, cy - range, range * 2, range * 2);
                    
                    // Обводка
                    using var previewPen = new Pen(Color.FromArgb(150, 100, 255, 100), 2);
                    g.DrawEllipse(previewPen, cx - range, cy - range, range * 2, range * 2);
                    
                    // Подсветка клетки
                    using var cellBrush = new SolidBrush(Color.FromArgb(60, 100, 255, 100));
                    g.FillRectangle(cellBrush, mouseCell.X * field.CellSize, mouseCell.Y * field.CellSize, 
                        field.CellSize, field.CellSize);
                }
                else
                {
                    // Красная подсветка если нельзя поставить
                    using var cellBrush = new SolidBrush(Color.FromArgb(60, 255, 50, 50));
                    g.FillRectangle(cellBrush, mouseCell.X * field.CellSize, mouseCell.Y * field.CellSize, 
                        field.CellSize, field.CellSize);
                }
            }
        }
    }
}
