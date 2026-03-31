using System;
using System.Collections.Generic;

namespace TowerDefense.Model
{
    public class Tower
    {
        public int Col { get; }
        public int Row { get; }
        public float Range { get; } = 120f;
        public int Damage { get; } = 1;
        public int FireRate { get; } = 40;
        private int cooldown;

        public Tower(int col, int row) { Col = col; Row = row; }

        public Enemy? FindTarget(List<Enemy> enemies, int cellSize)
        {
            float cx = Col * cellSize + cellSize / 2f;
            float cy = Row * cellSize + cellSize / 2f;
            Enemy? best = null; float bestDist = float.MaxValue;
            foreach (var e in enemies)
            {
                float d = MathF.Sqrt((e.X - cx) * (e.X - cx) + (e.Y - cy) * (e.Y - cy));
                if (d <= Range && d < bestDist) { best = e; bestDist = d; }
            }
            return best;
        }

        public bool TryShoot(List<Enemy> enemies, int cellSize, out Enemy? target, out Projectile? projectile)
        {
            target = null;
            projectile = null;
            if (cooldown > 0) { cooldown--; return false; }
            target = FindTarget(enemies, cellSize);
            if (target == null) return false;
            
            float cx = Col * cellSize + cellSize / 2f;
            float cy = Row * cellSize + cellSize / 2f;
            projectile = new Projectile(cx, cy, target, Damage);
            cooldown = FireRate;
            return true;
        }
    }
}
