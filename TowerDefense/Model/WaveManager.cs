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
        private const int SpawnInterval = 60;

        public bool ShouldSpawn(out int health)
        {
            health = 1 + CurrentWave;
            if (!WaveInProgress) { health = 1; return false; }
            spawnTimer++;
            if (spawnTimer < SpawnInterval) return false;
            if (spawnedInWave >= enemiesPerWave) return false;
            spawnTimer = 0;
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
