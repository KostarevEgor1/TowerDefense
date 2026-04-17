using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using TowerDefense.Model;

namespace TowerDefense.View
{
    public class GameRenderer
    {
        private readonly GameModel model;
        public GameRenderer(GameModel model) => this.model = model;

        private static readonly Color[] PathColors =
        {
            Color.FromArgb(180, 200, 140),
            Color.FromArgb(200, 160, 120),
        };

        public void Draw(Graphics g, Point mouseCell, TowerType selectedType = TowerType.Basic)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(34, 85, 34));
            var field = model.Field;

            using var gridPen = new Pen(Color.FromArgb(20, 0, 0, 0));
            for (int c = 0; c <= field.Cols; c++)
                g.DrawLine(gridPen, c * field.CellSize, 0, c * field.CellSize, field.Rows * field.CellSize);
            for (int r = 0; r <= field.Rows; r++)
                g.DrawLine(gridPen, 0, r * field.CellSize, field.Cols * field.CellSize, r * field.CellSize);

            for (int c = 0; c < field.Cols; c++)
                for (int r = 0; r < field.Rows; r++)
                {
                    int idx = field.PathIndexForCell(c, r);
                    if (idx < 0) continue;
                    var color = PathColors[idx % PathColors.Length];
                    g.FillRectangle(new SolidBrush(color),
                        c * field.CellSize, r * field.CellSize, field.CellSize, field.CellSize);
                    using var edge = new Pen(Color.FromArgb(140, 110, 70), 1);
                    g.DrawRectangle(edge,
                        c * field.CellSize, r * field.CellSize, field.CellSize, field.CellSize);
                }

            // Зоны строительства (статические)
            using var buildZoneBrush = new SolidBrush(Color.FromArgb(40, 100, 150, 100));
            using var buildZonePen = new Pen(Color.FromArgb(120, 100, 200, 100), 2);
            foreach (var zone in field.BuildZones)
            {
                g.FillRectangle(buildZoneBrush, zone.X * field.CellSize, zone.Y * field.CellSize, 
                    field.CellSize, field.CellSize);
                g.DrawRectangle(buildZonePen, zone.X * field.CellSize + 2, zone.Y * field.CellSize + 2, 
                    field.CellSize - 4, field.CellSize - 4);
            }
            
            // Динамические зоны строительства (вплотную к путям)
            using var dynamicBuildZoneBrush = new SolidBrush(Color.FromArgb(50, 255, 200, 100));
            using var dynamicBuildZonePen = new Pen(Color.FromArgb(150, 255, 200, 100), 2);
            int path1Y = field.ActivePaths[0][0].Y;
            int[] dynamicCols = { 2, 7, 12, 17 };
            
            // Вплотную к пути 1
            foreach (int col in dynamicCols)
            {
                // Сверху от пути
                g.FillRectangle(dynamicBuildZoneBrush, col * field.CellSize, (path1Y - 1) * field.CellSize, 
                    field.CellSize, field.CellSize);
                g.DrawRectangle(dynamicBuildZonePen, col * field.CellSize + 2, (path1Y - 1) * field.CellSize + 2, 
                    field.CellSize - 4, field.CellSize - 4);
                // Снизу от пути
                g.FillRectangle(dynamicBuildZoneBrush, col * field.CellSize, (path1Y + 1) * field.CellSize, 
                    field.CellSize, field.CellSize);
                g.DrawRectangle(dynamicBuildZonePen, col * field.CellSize + 2, (path1Y + 1) * field.CellSize + 2, 
                    field.CellSize - 4, field.CellSize - 4);
            }
            
            // Вплотную к пути 2 (y=10)
            foreach (int col in dynamicCols)
            {
                // Сверху от пути (y=9)
                g.FillRectangle(dynamicBuildZoneBrush, col * field.CellSize, 9 * field.CellSize, 
                    field.CellSize, field.CellSize);
                g.DrawRectangle(dynamicBuildZonePen, col * field.CellSize + 2, 9 * field.CellSize + 2, 
                    field.CellSize - 4, field.CellSize - 4);
                // Снизу от пути (y=11)
                g.FillRectangle(dynamicBuildZoneBrush, col * field.CellSize, 11 * field.CellSize, 
                    field.CellSize, field.CellSize);
                g.DrawRectangle(dynamicBuildZonePen, col * field.CellSize + 2, 11 * field.CellSize + 2, 
                    field.CellSize - 4, field.CellSize - 4);
            }

            using var lf = new Font("Arial", 8, FontStyle.Bold);
            for (int i = 0; i < field.ActivePaths.Count; i++)
            {
                var path = field.ActivePaths[i];
                var sp = path[0];
                string label = i == 0 ? "S1" : "S2";
                g.FillRectangle(Brushes.LimeGreen,
                    sp.X * field.CellSize, sp.Y * field.CellSize, field.CellSize, field.CellSize);
                g.DrawString(label, lf, Brushes.Black,
                    sp.X * field.CellSize + 6, sp.Y * field.CellSize + 12);
                
                // Конец каждого пути - база
                var ep = path[path.Count - 1];
                string baseLabel = i == 0 ? "База 1" : "База 2";
                int baseHp = i == 0 ? model.Resources.Base1Hp : model.Resources.Base2Hp;
                
                // Рисуем базу
                g.FillRectangle(Brushes.Crimson,
                    ep.X * field.CellSize, ep.Y * field.CellSize, field.CellSize, field.CellSize);
                
                // Название базы
                using var baseFont = new Font("Arial", 7, FontStyle.Bold);
                g.DrawString(baseLabel, baseFont, Brushes.White,
                    ep.X * field.CellSize + 2, ep.Y * field.CellSize + 8);
                
                // HP базы
                g.DrawString($"{baseHp}HP", baseFont, Brushes.Yellow,
                    ep.X * field.CellSize + 4, ep.Y * field.CellSize + 22);
            }

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
                var sprite = SpriteManager.GetEnemySprite(enemy.Type);
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
