using System.Collections.Generic;

namespace TowerDefense.Model
{
    public class GameModel
    {
        public GameField Field { get; } = new GameField();
        public List<Enemy> Enemies { get; } = new List<Enemy>();
        public List<Tower> Towers { get; } = new List<Tower>();
        public List<Projectile> Projectiles { get; } = new List<Projectile>();
        public List<ImpactEffect> ImpactEffects { get; } = new List<ImpactEffect>();
        public WaveManager Waves { get; } = new WaveManager();
        public ResourceManager Resources { get; } = new ResourceManager();
        public int Score { get; private set; }
        public bool IsGameOver => Resources.IsGameOver();

        public bool CanPlaceTower(int col, int row, TowerType type = TowerType.Basic)
        {
            int cost = new Tower(0, 0, type).Cost;
            return Field.IsInBuildZone(col, row) &&
                   !Towers.Exists(t => t.Col == col && t.Row == row) &&
                   Resources.Gold >= cost;
        }

        public void PlaceTower(int col, int row, TowerType type = TowerType.Basic)
        {
            if (!CanPlaceTower(col, row, type)) return;
            var t = new Tower(col, row, type);
            Towers.Add(t);
            Resources.EarnGold(-t.Cost);
        }

        // Волна запускается вручную или автоматически по завершении предыдущей
        public void StartWave()
        {
            Waves.StartNextWave();
        }

        public void Update()
        {
            if (IsGameOver) return;

            if (Waves.ShouldSpawn(out int hp, out int pathIndex))
            {
                var path = Field.ActivePaths[pathIndex];
                var type = pathIndex == 1
                    ? (hp > 4 ? EnemyType.Tank : EnemyType.Normal)
                    : EnemyType.Fast;
                Enemies.Add(new Enemy(path, Field.CellSize, hp, type, pathIndex));
            }

            foreach (var tower in Towers)
            {
                if (tower.TryShoot(Enemies, Field.CellSize, out _, out var projectile))
                {
                    if (projectile != null)
                        Projectiles.Add(projectile);
                }
            }

            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                Projectiles[i].Update();
                if (Projectiles[i].HasHit)
                {
                    ImpactEffects.Add(new ImpactEffect(Projectiles[i].X, Projectiles[i].Y, ImpactEffectType.Hit));
                    Projectiles.RemoveAt(i);
                }
                else if (Projectiles[i].Target.IsDead)
                {
                    Projectiles.RemoveAt(i);
                }
            }

            for (int i = ImpactEffects.Count - 1; i >= 0; i--)
            {
                ImpactEffects[i].Update();
                if (ImpactEffects[i].IsExpired)
                    ImpactEffects.RemoveAt(i);
            }

            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemies[i].Update();
                if (Enemies[i].IsDead) 
                { 
                    ImpactEffects.Add(new ImpactEffect(Enemies[i].X, Enemies[i].Y, ImpactEffectType.Death));
                    Score += 10; 
                    Resources.EarnGold(20); 
                    Enemies.RemoveAt(i); 
                }
                else if (Enemies[i].ReachedEnd) 
                { 
                    // Уменьшаем HP соответствующей части базы
                    if (Enemies[i].PathIndex == 0)
                        Resources.LoseBase1Hp(1);
                    else
                        Resources.LoseBase2Hp(1);
                    Enemies.RemoveAt(i); 
                }
            }

            if (Waves.IsWaveComplete(Enemies))
            {
                Resources.EarnGold(50);
                Field.ShiftPathForWave(Waves.CurrentWave + 1);
                Waves.StartNextWave();
            }
        }
    }
}
