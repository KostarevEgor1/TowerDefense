#nullable enable

using NUnit.Framework;
using TowerDefense.Model;

namespace TowerDefense.Tests
{
    [TestFixture]
    public class DifficultyTests
    {
        [Test]
        public void HardDifficulty_SpawnsTougherAndFasterEnemies_ThanEasy()
        {
            var easy = new GameModel(DifficultyLevel.Easy);
            var hard = new GameModel(DifficultyLevel.Hard);
            easy.StartWave();
            hard.StartWave();

            Enemy? easyEnemy = null;
            Enemy? hardEnemy = null;

            for (int i = 0; i < 300; i++)
            {
                easy.Update();
                hard.Update();
                easyEnemy ??= easy.Enemies.Count > 0 ? easy.Enemies[0] : null;
                hardEnemy ??= hard.Enemies.Count > 0 ? hard.Enemies[0] : null;
                if (easyEnemy != null && hardEnemy != null)
                {
                    break;
                }
            }

            Assert.That(easyEnemy, Is.Not.Null);
            Assert.That(hardEnemy, Is.Not.Null);
            Assert.That(hardEnemy!.MaxHealth, Is.GreaterThan(easyEnemy!.MaxHealth));
            Assert.That(hardEnemy.Speed, Is.GreaterThan(easyEnemy.Speed));
            Assert.That(easyEnemy.GoldReward, Is.GreaterThan(hardEnemy.GoldReward));
        }
    }

    [TestFixture]
    public class EconomyTests
    {
        [Test]
        public void UpgradeTower_DeductsGold_AndIncreasesLevel()
        {
            var model = new GameModel();
            model.PlaceTower(1, 6, TowerType.Basic);
            var tower = model.FindTower(1, 6);

            Assert.That(tower, Is.Not.Null);
            int upgradeCost = tower!.GetUpgradeCost();
            int goldBefore = model.Resources.Gold;

            bool upgraded = model.UpgradeTower(1, 6);

            Assert.That(upgraded, Is.True);
            Assert.That(tower.Level, Is.EqualTo(2));
            Assert.That(model.Resources.Gold, Is.EqualTo(goldBefore - upgradeCost));
        }

        [Test]
        public void SellTower_ReturnsSeventyPercent_OfTotalInvestment()
        {
            var model = new GameModel();
            model.PlaceTower(1, 6, TowerType.Basic);
            model.UpgradeTower(1, 6);
            var tower = model.FindTower(1, 6);

            Assert.That(tower, Is.Not.Null);
            int sellValue = tower!.GetSellValue();
            int goldBeforeSell = model.Resources.Gold;

            bool sold = model.SellTower(1, 6);

            Assert.That(sold, Is.True);
            Assert.That(model.FindTower(1, 6), Is.Null);
            Assert.That(model.Resources.Gold, Is.EqualTo(goldBeforeSell + sellValue));
        }
    }

    [TestFixture]
    public class WaveControlTests
    {
        [Test]
        public void WaveDoesNotAutoStart_AfterCompletion()
        {
            var model = new GameModel(DifficultyLevel.Easy);
            model.StartWave();

            for (int i = 0; i < 7000; i++)
            {
                model.Update();
                if (!model.Waves.WaveInProgress)
                {
                    break;
                }
            }

            Assert.That(model.Waves.CurrentWave, Is.EqualTo(1));
            Assert.That(model.Waves.WaveInProgress, Is.False);

            model.StartWave();
            Assert.That(model.Waves.CurrentWave, Is.EqualTo(2));
            Assert.That(model.Waves.WaveInProgress, Is.True);
        }
    }
}
