using System.Collections.Generic;
using System.Drawing;

namespace TowerDefense.Model
{
    public class GameModel
    {
        public GameField Field { get; } = new GameField();
        public List<Enemy>  Enemies { get; } = new List<Enemy>();
        public List<Tower>  Towers  { get; } = new List<Tower>();
        public ResourceManager Resources { get; } = new ResourceManager();
        public int Score { get; private set; }

        private int spawnTimer;
        private int spawnedCount;
        private const int SpawnInterval = 90;
        private const int TotalEnemies  = 10;

        public bool CanPlaceTower(int col, int row) =>
            !Field.IsOnPath(col, row) &&
            !Towers.Exists(t => t.Col == col && t.Row == row) &&
            Resources.CanAffordTower();

        public void PlaceTower(int col, int row)
        {
            if (!CanPlaceTower(col, row)) return;
            Towers.Add(new Tower(col, row));
            Resources.BuyTower();
        }

        public void Update()
        {
            spawnTimer++;
            if (spawnTimer >= SpawnInterval && spawnedCount < TotalEnemies)
            {
                Enemies.Add(new Enemy(Field.Path, Field.CellSize));
                spawnedCount++; spawnTimer = 0;
            }

            foreach (var tower in Towers)
                tower.TryShoot(Enemies, Field.CellSize, out _);

            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemies[i].Update();
                if (Enemies[i].IsDead)  { Score += 10; Resources.EarnGold(20); Enemies.RemoveAt(i); }
                else if (Enemies[i].ReachedEnd) { Enemies.RemoveAt(i); }
            }
        }
    }
}
