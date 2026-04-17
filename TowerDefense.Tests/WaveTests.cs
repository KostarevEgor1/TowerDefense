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
        }

        [Test]
        public void AfterStartNextWave_WaveInProgress()
        {
            var wm = new WaveManager();
            wm.StartNextWave();
            Assert.That(wm.WaveInProgress, Is.True);
            Assert.That(wm.CurrentWave, Is.EqualTo(1));
        }

        [Test]
        public void WaveNumber_IncrementsEachWave()
        {
            var wm = new WaveManager();
            wm.StartNextWave();
            wm.StartNextWave();
            Assert.That(wm.CurrentWave, Is.EqualTo(2));
        }

        [Test]
        public void ShouldSpawn_ReturnsFalse_WhenWaveNotStarted()
        {
            var wm = new WaveManager();
            bool spawned = wm.ShouldSpawn(out _, out _);
            Assert.That(spawned, Is.False);
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
            // Уничтожаем первую часть базы
            for (int i = 0; i < 100; i++) r.LoseBase1Hp(1);
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

        [Test]
        public void BaseHp_DoesNotGoNegative_GameOverAtZero()
        {
            var r = new ResourceManager();
            // Уничтожаем вторую часть базы
            r.LoseBase2Hp(r.Base2Hp);
            Assert.That(r.IsGameOver(), Is.True);
        }
    }
}
