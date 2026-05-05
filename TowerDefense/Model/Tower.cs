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
        public int Level { get; private set; } = 1;
        public const int MaxLevel = 3;
        public float Range { get; private set; }
        public int Damage { get; private set; }
        public int FireRate { get; private set; }
        public int Cost { get; }
        public int TotalInvested { get; private set; }
        public bool CanUpgrade => Level < MaxLevel;
        private int cooldown;

        public Tower(int col, int row, TowerType type = TowerType.Basic)
        {
            Col = col;
            Row = row;
            Type = type;
            Cost = type switch
            {
                TowerType.Sniper => 100,
                TowerType.Rapid => 75,
                _ => 50
            };
            TotalInvested = Cost;
            ApplyLevelStats();
        }

        public int GetUpgradeCost()
        {
            if (!CanUpgrade)
            {
                return 0;
            }

            return Type switch
            {
                TowerType.Basic => Level == 1 ? 40 : 70,
                TowerType.Sniper => Level == 1 ? 75 : 115,
                TowerType.Rapid => Level == 1 ? 60 : 95,
                _ => 0
            };
        }

        public int GetSellValue()
        {
            return (int)MathF.Round(TotalInvested * 0.7f);
        }

        public bool TryUpgrade()
        {
            if (!CanUpgrade)
            {
                return false;
            }

            TotalInvested += GetUpgradeCost();
            Level++;
            ApplyLevelStats();
            return true;
        }

        private void ApplyLevelStats()
        {
            switch (Type)
            {
                case TowerType.Sniper:
                    Range = Level switch { 2 => 235f, 3 => 250f, _ => 220f };
                    Damage = Level switch { 2 => 5, 3 => 7, _ => 3 };
                    FireRate = Level switch { 2 => 74, 3 => 68, _ => 80 };
                    break;

                case TowerType.Rapid:
                    Range = Level switch { 2 => 88f, 3 => 96f, _ => 80f };
                    Damage = Level switch { 2 => 2, 3 => 2, _ => 1 };
                    FireRate = Level switch { 2 => 13, 3 => 10, _ => 15 };
                    break;

                default:
                    Range = Level switch { 2 => 130f, 3 => 145f, _ => 120f };
                    Damage = Level switch { 2 => 2, 3 => 3, _ => 1 };
                    FireRate = Level switch { 2 => 35, 3 => 30, _ => 40 };
                    break;
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
