using NUnit.Framework;
using TowerDefense.Model;
using System.Collections.Generic;

namespace TowerDefense.Tests
{
    [TestFixture]
    public class PathTests
    {
        [Test]
        public void GameField_HasTwoPaths_Initially()
        {
            var field = new GameField();
            Assert.That(field.ActivePaths.Count, Is.EqualTo(2));
        }

        [Test]
        public void ShiftPath_ChangesFirstPath_OnWave3()
        {
            var field = new GameField();
            var originalY = field.ActivePaths[0][1].Y;
            field.ShiftPathForWave(3);
            var newY = field.ActivePaths[0][1].Y;
            Assert.That(newY, Is.Not.EqualTo(originalY));
        }

        [Test]
        public void ShiftPath_DoesNotChange_OnNonMultipleOf3()
        {
            var field = new GameField();
            var originalPath = new List<System.Drawing.Point>(field.ActivePaths[0]);
            field.ShiftPathForWave(2);
            Assert.That(field.ActivePaths[0][1].Y, Is.EqualTo(originalPath[1].Y));
        }

        [Test]
        public void SecondPath_NotAffected_ByShift()
        {
            var field = new GameField();
            var secondPathBefore = new List<System.Drawing.Point>(field.ActivePaths[1]);
            field.ShiftPathForWave(3);
            for (int i = 0; i < secondPathBefore.Count; i++)
                Assert.That(field.ActivePaths[1][i], Is.EqualTo(secondPathBefore[i]));
        }

        [Test]
        public void Tower_CannotBePlaced_OnFirstActivePath()
        {
            var model = new GameModel();
            var pt = model.Field.ActivePaths[0][1];
            model.PlaceTower(pt.X, pt.Y);
            Assert.That(model.Towers.Count, Is.EqualTo(0));
        }

        [Test]
        public void Tower_CannotBePlaced_OnSecondActivePath()
        {
            var model = new GameModel();
            var pt = model.Field.ActivePaths[1][1];
            model.PlaceTower(pt.X, pt.Y);
            Assert.That(model.Towers.Count, Is.EqualTo(0));
        }

        [Test]
        public void FastEnemies_SpawnOnPath0()
        {
            var model = new GameModel();
            model.StartWave();
            for (int i = 0; i < 200; i++) model.Update();
            bool anyFast = false;
            foreach (var e in model.Enemies)
                if (e.Type == EnemyType.Fast)
                    anyFast = true;
            Assert.That(anyFast, Is.True);
        }

        [Test]
        public void ShiftPath_TwiceOnMultiplesOf3_AlternatesDirection()
        {
            var field = new GameField();
            int originalY = field.ActivePaths[0][1].Y;
            field.ShiftPathForWave(3);
            int afterWave3 = field.ActivePaths[0][1].Y;
            field.ShiftPathForWave(6);
            int afterWave6 = field.ActivePaths[0][1].Y;
            // Волна 3 и волна 6 должны давать разные сдвиги
            Assert.That(afterWave3, Is.Not.EqualTo(afterWave6));
        }
    }
}
