using System.Collections.Generic;
using NUnit.Framework;
using TowerDefense.Model;

namespace TowerDefense.Tests
{
    [TestFixture]
    public class WaveManagerTests
    {
        [Test]
        public void WaveNotInProgress_Initially()
        {
            var wm = new WaveManager();
            Assert.That(wm.WaveInProgress, Is.False);
            Assert.That(wm.CurrentWave, Is.EqualTo(0));
        }

        [Test]
        public void AfterStartNextWave_WaveInProgress()
        {
            var wm = new WaveManager();
            wm.StartNextWave();
            Assert.That(wm.WaveInProgress, Is.True);
            Assert.That(wm.CurrentWave, Is.EqualTo(1));
            Assert.That(wm.CurrentPattern, Is.EqualTo(WavePattern.Standard));
        }

        [Test]
        public void WaveNumber_IncrementsEachWave()
        {
            var wm = new WaveManager();
            wm.StartNextWave();
            wm.CompleteWave();
            wm.StartNextWave();
            Assert.That(wm.CurrentWave, Is.EqualTo(2));
        }

        [Test]
        public void ShouldSpawn_ReturnsFalse_WhenWaveNotStarted()
        {
            var wm = new WaveManager();
            bool spawned = wm.ShouldSpawn(out _);
            Assert.That(spawned, Is.False);
        }

        [Test]
        public void FifthWave_IsSpikePattern()
        {
            var wm = new WaveManager();
            for (int i = 0; i < 5; i++)
            {
                wm.StartNextWave();
                wm.CompleteWave();
            }

            Assert.That(wm.CurrentWave, Is.EqualTo(5));
            Assert.That(wm.CurrentPattern, Is.EqualTo(WavePattern.Spike));
            Assert.That(wm.IsSpikeWave, Is.True);
        }

        [Test]
        public void EasyDifficulty_FirstWave_MatchesV6LegacyCadence()
        {
            var wm = new WaveManager(DifficultyCatalog.For(DifficultyLevel.Easy));
            wm.StartNextWave();

            var spawns = CaptureWave(wm);

            Assert.That(spawns.Count, Is.EqualTo(8));
            AssertWaveSpawn(spawns[0], EnemyType.Fast, 0, 2, 55);
            AssertWaveSpawn(spawns[1], EnemyType.Fast, 0, 2, 55);
            AssertWaveSpawn(spawns[2], EnemyType.Normal, 1, 4, 55);
            AssertWaveSpawn(spawns[3], EnemyType.Fast, 0, 2, 55);
            AssertWaveSpawn(spawns[4], EnemyType.Fast, 0, 2, 55);
            AssertWaveSpawn(spawns[5], EnemyType.Normal, 1, 4, 55);
            AssertWaveSpawn(spawns[6], EnemyType.Fast, 0, 2, 55);
            AssertWaveSpawn(spawns[7], EnemyType.Fast, 0, 2, 55);
            Assert.That(wm.IsSpikeWave, Is.False);
        }

        [Test]
        public void FirstWavePressure_Increases_FromEasy_ToNormal_ToHard()
        {
            float easyPressure = CalculateFirstWavePressure(DifficultyLevel.Easy);
            float normalPressure = CalculateFirstWavePressure(DifficultyLevel.Normal);
            float hardPressure = CalculateFirstWavePressure(DifficultyLevel.Hard);

            Assert.That(normalPressure, Is.GreaterThan(easyPressure));
            Assert.That(hardPressure, Is.GreaterThan(normalPressure));
        }

        private static List<WaveSpawn> CaptureWave(WaveManager wm)
        {
            var spawns = new List<WaveSpawn>();
            int guard = 0;
            while (spawns.Count < wm.EnemiesPerWave && guard < 10000)
            {
                if (wm.ShouldSpawn(out var spawn))
                {
                    spawns.Add(spawn);
                }

                guard++;
            }

            Assert.That(spawns.Count, Is.EqualTo(wm.EnemiesPerWave));
            return spawns;
        }

        private static float CalculateFirstWavePressure(DifficultyLevel level)
        {
            var difficulty = DifficultyCatalog.For(level);
            var wm = new WaveManager(difficulty);
            wm.StartNextWave();

            float pressure = 0f;
            foreach (var spawn in CaptureWave(wm))
            {
                pressure += spawn.BaseHealth * difficulty.EnemyHpMultiplier * difficulty.EnemySpeedMultiplier / spawn.SpawnDelay;
            }

            return pressure;
        }

        private static void AssertWaveSpawn(WaveSpawn spawn, EnemyType type, int pathIndex, int baseHealth, int spawnDelay)
        {
            Assert.That(spawn.Type, Is.EqualTo(type));
            Assert.That(spawn.PathIndex, Is.EqualTo(pathIndex));
            Assert.That(spawn.BaseHealth, Is.EqualTo(baseHealth));
            Assert.That(spawn.SpawnDelay, Is.EqualTo(spawnDelay));
        }
    }

    [TestFixture]
    public class ResourceManagerTests
    {
        [Test]
        public void CanAffordTower_WhenEnoughGold()
        {
            var r = new ResourceManager();
            Assert.That(r.CanAffordTower(), Is.True);
        }

        [Test]
        public void BuyTower_DeductsGold()
        {
            var r = new ResourceManager();
            int before = r.Gold;
            r.BuyTower();
            Assert.That(r.Gold, Is.EqualTo(before - ResourceManager.TowerCost));
        }

        [Test]
        public void LoseBaseHp_TriggersGameOver()
        {
            var r = new ResourceManager();
            for (int i = 0; i < 100; i++)
            {
                r.LoseBase1Hp(1);
            }

            Assert.That(r.IsGameOver(), Is.True);
        }

        [Test]
        public void EarnGold_IncreasesGold()
        {
            var r = new ResourceManager();
            int before = r.Gold;
            r.EarnGold(50);
            Assert.That(r.Gold, Is.EqualTo(before + 50));
        }
    }
}
