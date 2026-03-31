using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace TowerDefense.View
{
    public static class SpriteManager
    {
        private static Bitmap? towerSprite;
        private static Bitmap? enemySprite;
        private static Bitmap? projectileSprite;

        public static Bitmap GetTowerSprite()
        {
            if (towerSprite != null) return towerSprite;
            
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "tower_basic.png");
            if (File.Exists(path))
            {
                towerSprite = new Bitmap(path);
                return towerSprite;
            }
            
            // Fallback
            towerSprite = new Bitmap(32, 32);
            using var g = Graphics.FromImage(towerSprite);
            g.Clear(Color.Gray);
            return towerSprite;
        }

        public static Bitmap GetEnemySprite()
        {
            if (enemySprite != null) return enemySprite;
            
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "enemy_normal.png");
            if (File.Exists(path))
            {
                enemySprite = new Bitmap(path);
                return enemySprite;
            }
            
            // Fallback
            enemySprite = new Bitmap(28, 28);
            using var g = Graphics.FromImage(enemySprite);
            g.Clear(Color.Red);
            return enemySprite;
        }

        public static Bitmap GetProjectileSprite()
        {
            if (projectileSprite != null) return projectileSprite;
            
            projectileSprite = new Bitmap(8, 8);
            using var g = Graphics.FromImage(projectileSprite);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Снаряд
            g.FillEllipse(new SolidBrush(Color.FromArgb(255, 220, 0)), 0, 0, 8, 8);
            g.DrawEllipse(new Pen(Color.FromArgb(255, 150, 0), 1), 0, 0, 7, 7);
            g.FillEllipse(new SolidBrush(Color.FromArgb(150, 255, 255, 255)), 2, 2, 3, 3);
            
            return projectileSprite;
        }
    }
}
