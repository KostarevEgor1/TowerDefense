using System;
using System.Collections.Generic;

namespace TowerDefense.Model
{
    public enum WavePattern
    {
        Standard,
        FastRush,
        Heavy,
        Mixed,
        Spike
    }

    public readonly struct WaveSpawn
    {
        public EnemyType Type { get; }
        public int PathIndex { get; }
        public int BaseHealth { get; }
        public int SpawnDelay { get; }

        public WaveSpawn(EnemyType type, int pathIndex, int baseHealth, int spawnDelay)
        {
            Type = type;
            PathIndex = pathIndex;
            BaseHealth = baseHealth;
            SpawnDelay = spawnDelay;
        }
    }

    public class WaveManager
    {
        private const int SpikePeriod = 5;
        private readonly DifficultySettings difficulty;
        private int spawnTimer;
        private int spawnCursor;
        private List<WaveSpawn> currentWavePlan = new();

        public WaveManager()
            : this(DifficultyCatalog.For(DifficultyLevel.Normal))
        {
        }

        public WaveManager(DifficultySettings difficulty)
        {
            this.difficulty = difficulty;
        }

        public int CurrentWave { get; private set; }
        public bool WaveInProgress { get; private set; }
        public int SpawnedInWave => spawnCursor;
        public int EnemiesPerWave => currentWavePlan.Count;
        public WavePattern CurrentPattern { get; private set; } = WavePattern.Standard;
        public bool IsSpikeWave =>
            difficulty.WaveRuleset == WaveRuleset.Modern &&
            CurrentWave > 0 &&
            CurrentWave % SpikePeriod == 0;

        public bool ShouldSpawn(out WaveSpawn spawn)
        {
            spawn = default;
            if (!WaveInProgress || spawnCursor >= currentWavePlan.Count)
            {
                return false;
            }

            spawnTimer++;
            var next = currentWavePlan[spawnCursor];
            if (spawnTimer < next.SpawnDelay)
            {
                return false;
            }

            spawnTimer = 0;
            spawn = next;
            spawnCursor++;
            return true;
        }

        public bool IsWaveComplete(List<Enemy> enemies)
        {
            return WaveInProgress && spawnCursor >= currentWavePlan.Count && enemies.Count == 0;
        }

        public void StartNextWave()
        {
            if (WaveInProgress)
            {
                return;
            }

            CurrentWave++;
            CurrentPattern = difficulty.WaveRuleset == WaveRuleset.Legacy
                ? WavePattern.Standard
                : ResolvePattern(CurrentWave);
            currentWavePlan = BuildWavePlan(CurrentWave, CurrentPattern);
            spawnTimer = 0;
            spawnCursor = 0;
            WaveInProgress = true;
        }

        public void CompleteWave()
        {
            WaveInProgress = false;
            spawnTimer = 0;
        }

        private static WavePattern ResolvePattern(int wave)
        {
            if (wave % SpikePeriod == 0)
            {
                return WavePattern.Spike;
            }

            int selector = wave % 4;
            return selector switch
            {
                1 => WavePattern.Standard,
                2 => WavePattern.FastRush,
                3 => WavePattern.Heavy,
                _ => WavePattern.Mixed
            };
        }

        private List<WaveSpawn> BuildWavePlan(int wave, WavePattern pattern)
        {
            return difficulty.WaveRuleset == WaveRuleset.Legacy
                ? BuildLegacyWavePlan(wave)
                : BuildModernWavePlan(wave, pattern);
        }

        private static List<WaveSpawn> BuildLegacyWavePlan(int wave)
        {
            int enemiesPerWave = 5 + wave * 3;
            var plan = new List<WaveSpawn>(enemiesPerWave);

            for (int i = 0; i < enemiesPerWave; i++)
            {
                bool isStrong = i % 3 == 2;
                int pathIndex = isStrong ? 1 : 0;
                int health = 1 + wave + (isStrong ? 2 : 0);
                var type = pathIndex == 1
                    ? (health > 4 ? EnemyType.Tank : EnemyType.Normal)
                    : EnemyType.Fast;

                plan.Add(new WaveSpawn(type, pathIndex, health, spawnDelay: 55));
            }

            return plan;
        }

        private List<WaveSpawn> BuildModernWavePlan(int wave, WavePattern pattern)
        {
            int baseline = 6 + wave * 2;

            return pattern switch
            {
                WavePattern.FastRush => BuildFastRushWave(wave, ScaleCount(baseline + 1)),
                WavePattern.Heavy => BuildHeavyWave(wave, ScaleCount(baseline - 1)),
                WavePattern.Mixed => BuildMixedWave(wave, ScaleCount(baseline)),
                WavePattern.Spike => BuildSpikeWave(wave, ScaleCount(baseline + 5)),
                _ => BuildStandardWave(wave, ScaleCount(baseline))
            };
        }

        private List<WaveSpawn> BuildStandardWave(int wave, int totalEnemies)
        {
            var plan = new List<WaveSpawn>(totalEnemies);

            for (int i = 0; i < totalEnemies; i++)
            {
                bool heavy = i % 5 == 4;
                var type = heavy ? EnemyType.Tank : EnemyType.Normal;
                int hp = heavy ? wave + 4 : wave + 2;
                int pathIndex = heavy ? 1 : 0;
                int spawnDelay = heavy ? 45 : 30;
                plan.Add(new WaveSpawn(type, pathIndex, hp, ScaleDelay(spawnDelay)));
            }

            return plan;
        }

        private List<WaveSpawn> BuildFastRushWave(int wave, int totalEnemies)
        {
            var plan = new List<WaveSpawn>(totalEnemies);

            for (int i = 0; i < totalEnemies; i++)
            {
                bool breakTank = i > 0 && i % 7 == 0;
                var type = breakTank ? EnemyType.Tank : EnemyType.Fast;
                int hp = breakTank ? wave + 4 : wave + 1;
                int spawnDelay = breakTank ? 34 : 20;
                int pathIndex = breakTank ? 1 : 0;
                plan.Add(new WaveSpawn(type, pathIndex, hp, ScaleDelay(spawnDelay)));
            }

            return plan;
        }

        private List<WaveSpawn> BuildHeavyWave(int wave, int totalEnemies)
        {
            var plan = new List<WaveSpawn>(totalEnemies);

            for (int i = 0; i < totalEnemies; i++)
            {
                bool tank = i % 2 == 0;
                var type = tank ? EnemyType.Tank : EnemyType.Normal;
                int hp = tank ? wave + 5 : wave + 2;
                int spawnDelay = tank ? 48 : 34;
                plan.Add(new WaveSpawn(type, 1, hp, ScaleDelay(spawnDelay)));
            }

            return plan;
        }

        private List<WaveSpawn> BuildMixedWave(int wave, int totalEnemies)
        {
            var plan = new List<WaveSpawn>(totalEnemies);

            for (int i = 0; i < totalEnemies; i++)
            {
                if (i % 6 == 5)
                {
                    plan.Add(new WaveSpawn(EnemyType.Tank, 1, wave + 5, ScaleDelay(40)));
                }
                else if (i % 3 == 1)
                {
                    plan.Add(new WaveSpawn(EnemyType.Fast, 0, wave + 1, ScaleDelay(24)));
                }
                else
                {
                    plan.Add(new WaveSpawn(EnemyType.Normal, i % 2, wave + 2, ScaleDelay(30)));
                }
            }

            return plan;
        }

        private List<WaveSpawn> BuildSpikeWave(int wave, int totalEnemies)
        {
            var plan = new List<WaveSpawn>(totalEnemies);

            for (int i = 0; i < totalEnemies; i++)
            {
                if (i % 5 == 0)
                {
                    plan.Add(new WaveSpawn(EnemyType.Tank, 1, wave + 7, ScaleDelay(36)));
                }
                else if (i % 2 == 0)
                {
                    plan.Add(new WaveSpawn(EnemyType.Fast, 0, wave + 2, ScaleDelay(24)));
                }
                else
                {
                    plan.Add(new WaveSpawn(EnemyType.Normal, 0, wave + 3, ScaleDelay(28)));
                }
            }

            return plan;
        }

        private int ScaleCount(int rawCount)
        {
            return Math.Max(5, (int)MathF.Round(rawCount * difficulty.WaveCountMultiplier));
        }

        private int ScaleDelay(int baseDelay)
        {
            return Math.Max(16, (int)MathF.Round(baseDelay * difficulty.SpawnDelayMultiplier));
        }
    }
}
