using NUnit.Framework;
using TowerDefense.Model;
using System.Collections.Generic;
using System.Drawing;

namespace TowerDefense.Tests
{
    [TestFixture]
    public class EnemyTests
    {
        private static List<Point> SimplePath() => new List<Point>
        {
            new Point(0, 0), new Point(5, 0)
        };

        [Test]
        public void Enemy_StartsAtFirstPathPoint()
        {
            var e = new Enemy(SimplePath(), 40);
            Assert.That(e.X, Is.EqualTo(20f));
            Assert.That(e.Y, Is.EqualTo(20f));
        }

        [Test]
        public void Enemy_TakeDamage_ReducesHealth()
        {
            var e = new Enemy(SimplePath(), 40, health: 5);
            e.TakeDamage(2);
            Assert.That(e.Health, Is.EqualTo(3));
        }

        [Test]
        public void Enemy_IsDead_WhenHealthZero()
        {
            var e = new Enemy(SimplePath(), 40, health: 1);
            e.TakeDamage(1);
            Assert.That(e.IsDead, Is.True);
        }

        [Test]
        public void Enemy_MovesAfterUpdate()
        {
            var e = new Enemy(SimplePath(), 40);
            float startX = e.X;
            e.Update();
            Assert.That(e.X, Is.GreaterThan(startX));
        }

        [Test]
        public void Enemy_ReachesEnd_AfterEnoughUpdates()
        {
            var e = new Enemy(SimplePath(), 40);
            for (int i = 0; i < 500; i++) e.Update();
            Assert.That(e.ReachedEnd, Is.True);
        }
    }

    [TestFixture]
    public class TowerTests
    {
        [Test]
        public void Tower_PlacedOnPath_NotAllowed()
        {
            var model = new GameModel();
            var sp = model.Field.Path[0];
            model.PlaceTower(sp.X, sp.Y);
            Assert.That(model.Towers.Count, Is.EqualTo(0));
        }

        [Test]
        public void Tower_PlacedOffPath_Allowed()
        {
            var model = new GameModel();
            // (1, 0) — точно не на пути (путь идёт по Y=7 и другим строкам)
            model.PlaceTower(1, 0);
            Assert.That(model.Towers.Count, Is.EqualTo(1));
        }

        [Test]
        public void Tower_CannotBePlacedTwiceOnSameCell()
        {
            var model = new GameModel();
            model.PlaceTower(1, 0);
            model.PlaceTower(1, 0);
            Assert.That(model.Towers.Count, Is.EqualTo(1));
        }

        [Test]
        public void Tower_ShootsEnemy_InRange()
        {
            var path = new List<Point> { new Point(0, 0), new Point(10, 0) };
            var tower = new Tower(2, 0);
            var enemy = new Enemy(path, 40);
            for (int i = 0; i < 30; i++) enemy.Update();
            var target = tower.FindTarget(new List<Enemy> { enemy }, 40);
            Assert.That(target, Is.Not.Null);
        }

        [Test]
        public void Tower_DoesNotShoot_OutOfRange()
        {
            var path = new List<Point> { new Point(0, 0), new Point(5, 0) };
            var tower = new Tower(15, 15);
            var enemy = new Enemy(path, 40);
            var target = tower.FindTarget(new List<Enemy> { enemy }, 40);
            Assert.That(target, Is.Null);
        }

        [Test]
        public void CannotPlaceTower_WhenNotEnoughGold()
        {
            var model = new GameModel();
            while (model.Resources.CanAffordTower())
                model.Resources.BuyTower();
            model.PlaceTower(1, 0);
            Assert.That(model.Towers.Count, Is.EqualTo(0));
        }
    }
}
