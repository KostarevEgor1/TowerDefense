using System;
using System.Collections.Generic;

namespace TowerDefense.Model
{
    public enum TowerType { Basic, Sniper, Rapid }

    public class Tower
    {
        public int Col { get; }
        public int Row { get; }
        public TowerType Type { get; }
        public float Range { get; }
        public int Damage { get; }
        public int FireRate { get; }
        public int Cost { get; }
        private int cooldown;

        public Tower(int col, int row, TowerType type = TowerType.Basic)
        {
            Col = col; Row = row; Type = type;
            switch (type)
            {
                case TowerType.Basic:  Range = 120; Damage = 1; FireRate = 40; Cost = 50;  break;
                case TowerType.Sniper: Range = 220; Damage = 3; FireRate = 80; Cost = 100; break;
                case TowerType.Rapid:  Range = 80;  Damage = 1; FireRate = 15; Cost = 75;  break;
            }
        }

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
