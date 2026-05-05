namespace TowerDefense.Model
{
    public enum WaveRuleset
    {
        Legacy,
        Modern
    }

    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }

    public readonly struct DifficultySettings
    {
        public DifficultyLevel Level { get; }
        public string DisplayName { get; }
        public WaveRuleset WaveRuleset { get; }
        public float EnemyHpMultiplier { get; }
        public float EnemySpeedMultiplier { get; }
        public float EnemyHpWaveGrowth { get; }
        public float WaveCountMultiplier { get; }
        public float SpawnDelayMultiplier { get; }
        public float RewardMultiplier { get; }

        public DifficultySettings(
            DifficultyLevel level,
            string displayName,
            WaveRuleset waveRuleset,
            float enemyHpMultiplier,
            float enemySpeedMultiplier,
            float enemyHpWaveGrowth,
            float waveCountMultiplier,
            float spawnDelayMultiplier,
            float rewardMultiplier)
        {
            Level = level;
            DisplayName = displayName;
            WaveRuleset = waveRuleset;
            EnemyHpMultiplier = enemyHpMultiplier;
            EnemySpeedMultiplier = enemySpeedMultiplier;
            EnemyHpWaveGrowth = enemyHpWaveGrowth;
            WaveCountMultiplier = waveCountMultiplier;
            SpawnDelayMultiplier = spawnDelayMultiplier;
            RewardMultiplier = rewardMultiplier;
        }
    }

    public static class DifficultyCatalog
    {
        public static DifficultySettings For(DifficultyLevel level)
        {
            return level switch
            {
                DifficultyLevel.Easy => new DifficultySettings(
                    level,
                    "\u041b\u0435\u0433\u043a\u0430\u044f",
                    WaveRuleset.Legacy,
                    enemyHpMultiplier: 1.0f,
                    enemySpeedMultiplier: 1.0f,
                    enemyHpWaveGrowth: 0.0f,
                    waveCountMultiplier: 1.0f,
                    spawnDelayMultiplier: 1.0f,
                    rewardMultiplier: 1.45f),
                DifficultyLevel.Hard => new DifficultySettings(
                    level,
                    "\u0421\u043b\u043e\u0436\u043d\u0430\u044f",
                    WaveRuleset.Modern,
                    enemyHpMultiplier: 1.02f,
                    enemySpeedMultiplier: 1.05f,
                    enemyHpWaveGrowth: 0.02f,
                    waveCountMultiplier: 0.9f,
                    spawnDelayMultiplier: 1.1f,
                    rewardMultiplier: 1.08f),
                _ => new DifficultySettings(
                    level,
                    "\u041e\u0431\u044b\u0447\u043d\u0430\u044f",
                    WaveRuleset.Modern,
                    enemyHpMultiplier: 0.9f,
                    enemySpeedMultiplier: 0.97f,
                    enemyHpWaveGrowth: 0.0f,
                    waveCountMultiplier: 0.78f,
                    spawnDelayMultiplier: 1.25f,
                    rewardMultiplier: 1.2f)
            };
        }
    }
}
