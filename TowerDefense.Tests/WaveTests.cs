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
            for (int i = 0; i < 100; i++) r.LoseBaseHp(1);
            Assert.That(r.IsGameOver(), Is.True);
        }
    }
}
