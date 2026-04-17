using System.Collections.Generic;

namespace TowerDefense.Model
{
    public class WaveManager
    {
        public int CurrentWave { get; private set; } = 0;
        public bool WaveInProgress { get; private set; }
        private int spawnTimer;
        private int spawnedInWave;
        private int enemiesPerWave => 5 + CurrentWave * 3;
        private const int SpawnInterval = 55;

        public bool ShouldSpawn(out int health, out int pathIndex)
        {
            health = 1 + CurrentWave;
            pathIndex = 0;
            if (!WaveInProgress) return false;
            spawnTimer++;
            if (spawnTimer < SpawnInterval) return false;
            if (spawnedInWave >= enemiesPerWave) return false;
            spawnTimer = 0;

            bool isStrong = (spawnedInWave % 3 == 2);
            pathIndex = isStrong ? 1 : 0;
            if (isStrong) health = health + 2;

            spawnedInWave++;
            return true;
        }

        public bool IsWaveComplete(List<Enemy> enemies) =>
            WaveInProgress && spawnedInWave >= enemiesPerWave && enemies.Count == 0;

        public void StartNextWave()
        {
            CurrentWave++;
            WaveInProgress = true;
            spawnTimer = 0;
            spawnedInWave = 0;
        }
    }
}
