using System;
using System.Collections.Generic;

namespace TowerDefense.Model
{
    public class GameModel
    {
        private readonly DifficultySettings difficulty;

        public GameField Field { get; } = new GameField();
        public List<Enemy> Enemies { get; } = new();
        public List<Tower> Towers { get; } = new();
        public List<Projectile> Projectiles { get; } = new();
        public List<ImpactEffect> ImpactEffects { get; } = new();
        public WaveManager Waves { get; }
        public ResourceManager Resources { get; } = new ResourceManager();
        public int Score { get; private set; }
        public DifficultyLevel DifficultyLevel => difficulty.Level;
        public string DifficultyName => difficulty.DisplayName;
        public bool IsGameOver => Resources.IsGameOver();

        public GameModel(DifficultyLevel difficultyLevel = DifficultyLevel.Normal)
        {
            difficulty = DifficultyCatalog.For(difficultyLevel);
            Waves = new WaveManager(difficulty);
        }

        public bool CanPlaceTower(int col, int row, TowerType type = TowerType.Basic)
        {
            int cost = new Tower(0, 0, type).Cost;
            return Field.IsInBuildZone(col, row) &&
                   !Towers.Exists(t => t.Col == col && t.Row == row) &&
                   Resources.Gold >= cost;
        }

        public void PlaceTower(int col, int row, TowerType type = TowerType.Basic)
        {
            if (!CanPlaceTower(col, row, type))
            {
                return;
            }

            var tower = new Tower(col, row, type);
            Towers.Add(tower);
            Resources.EarnGold(-tower.Cost);
        }

        public Tower? FindTower(int col, int row)
        {
            return Towers.Find(t => t.Col == col && t.Row == row);
        }

        public bool CanUpgradeTower(int col, int row)
        {
            var tower = FindTower(col, row);
            return tower != null && tower.CanUpgrade && Resources.Gold >= tower.GetUpgradeCost();
        }

        public bool UpgradeTower(int col, int row)
        {
            var tower = FindTower(col, row);
            if (tower == null || !tower.CanUpgrade)
            {
                return false;
            }

            int upgradeCost = tower.GetUpgradeCost();
            if (Resources.Gold < upgradeCost)
            {
                return false;
            }

            if (!tower.TryUpgrade())
            {
                return false;
            }

            Resources.EarnGold(-upgradeCost);
            return true;
        }

        public bool SellTower(int col, int row)
        {
            var tower = FindTower(col, row);
            if (tower == null)
            {
                return false;
            }

            Resources.EarnGold(tower.GetSellValue());
            Towers.Remove(tower);
            return true;
        }

        public void StartWave()
        {
            if (IsGameOver || Waves.WaveInProgress)
            {
                return;
            }

            Waves.StartNextWave();
        }

        public void Update()
        {
            if (IsGameOver)
            {
                return;
            }

            if (Waves.ShouldSpawn(out WaveSpawn spawn))
            {
                float waveHpMultiplier = 1f + Math.Max(0, Waves.CurrentWave - 1) * difficulty.EnemyHpWaveGrowth;
                int hp = Math.Max(1, (int)MathF.Round(spawn.BaseHealth * difficulty.EnemyHpMultiplier * waveHpMultiplier));
                int reward = ComputeEnemyReward(spawn.Type);
                var path = Field.ActivePaths[spawn.PathIndex];
                Enemies.Add(new Enemy(
                    path,
                    Field.CellSize,
                    hp,
                    spawn.Type,
                    spawn.PathIndex,
                    speedMultiplier: difficulty.EnemySpeedMultiplier,
                    goldReward: reward));
            }

            foreach (var tower in Towers)
            {
                if (tower.TryShoot(Enemies, Field.CellSize, out _, out var projectile) && projectile != null)
                {
                    Projectiles.Add(projectile);
                }
            }

            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                Projectiles[i].Update();
                if (Projectiles[i].HasHit)
                {
                    ImpactEffects.Add(new ImpactEffect(Projectiles[i].X, Projectiles[i].Y, lifetime: 10));
                    Projectiles.RemoveAt(i);
                }
                else if (Projectiles[i].Target.IsDead)
                {
                    Projectiles.RemoveAt(i);
                }
            }

            for (int i = ImpactEffects.Count - 1; i >= 0; i--)
            {
                ImpactEffects[i].Update();
                if (ImpactEffects[i].IsExpired)
                {
                    ImpactEffects.RemoveAt(i);
                }
            }

            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                var enemy = Enemies[i];
                enemy.Update();
                if (enemy.IsDead)
                {
                    ImpactEffects.Add(new ImpactEffect(enemy.X, enemy.Y, lifetime: 14));
                    Score += 10 + Waves.CurrentWave;
                    Resources.EarnGold(enemy.GoldReward);
                    Enemies.RemoveAt(i);
                }
                else if (enemy.ReachedEnd)
                {
                    if (enemy.PathIndex == 0)
                    {
                        Resources.LoseBase1Hp(1);
                    }
                    else
                    {
                        Resources.LoseBase2Hp(1);
                    }

                    Enemies.RemoveAt(i);
                }
            }

            if (Waves.IsWaveComplete(Enemies))
            {
                Resources.EarnGold(ComputeWaveClearReward());
                Waves.CompleteWave();
                bool pathShifted = Field.ShiftPathForWave(Waves.CurrentWave + 1);
                if (pathShifted)
                {
                    AutoSellTowersNowOnPath();
                }
            }
        }

        private void AutoSellTowersNowOnPath()
        {
            for (int i = Towers.Count - 1; i >= 0; i--)
            {
                var tower = Towers[i];
                if (!Field.IsOnAnyPath(tower.Col, tower.Row))
                {
                    continue;
                }

                int autoSellValue = (int)MathF.Round(tower.TotalInvested * 0.5f);
                Resources.EarnGold(autoSellValue);
                Towers.RemoveAt(i);
            }
        }

        private int ComputeEnemyReward(EnemyType type)
        {
            int baseReward = type switch
            {
                EnemyType.Fast => 11,
                EnemyType.Tank => 17,
                _ => 13
            };
            int wavePart = Math.Max(1, (Waves.CurrentWave + 1) / 2);
            return Math.Max(4, (int)MathF.Round((baseReward + wavePart) * difficulty.RewardMultiplier));
        }

        private int ComputeWaveClearReward()
        {
            int baseReward = 36 + Waves.CurrentWave * 4;
            if (Waves.IsSpikeWave)
            {
                baseReward += 18;
            }

            return Math.Max(12, (int)MathF.Round(baseReward * difficulty.RewardMultiplier));
        }
    }
}
