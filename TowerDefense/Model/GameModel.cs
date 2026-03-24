using System.Collections.Generic;

namespace TowerDefense.Model
{
    public class GameModel
    {
        public GameField Field { get; } = new GameField();
        public List<Enemy> Enemies { get; } = new List<Enemy>();

        private int spawnTimer;
        private int spawnedCount;
        private const int SpawnInterval = 90;
        private const int TotalEnemies  = 8;

        public void Update()
        {
            spawnTimer++;
            if (spawnTimer >= SpawnInterval && spawnedCount < TotalEnemies)
            {
                Enemies.Add(new Enemy(Field.Path, Field.CellSize));
                spawnedCount++;
                spawnTimer = 0;
            }

            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemies[i].Update();
                if (Enemies[i].ReachedEnd)
                    Enemies.RemoveAt(i);
            }
        }
    }
}
