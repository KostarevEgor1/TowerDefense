using System.Collections.Generic;

namespace TowerDefense.Model
{
    public class GameModel
    {
        public GameField Field { get; } = new GameField();
        public List<Enemy>  Enemies { get; } = new List<Enemy>();
        public List<Tower>  Towers  { get; } = new List<Tower>();
        public List<Projectile> Projectiles { get; } = new List<Projectile>();
        public WaveManager  Waves   { get; } = new WaveManager();
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

        public void StartWave() => Waves.StartNextWave();

        public void Update()
        {
            if (IsGameOver) return;
            if (Waves.ShouldSpawn(out int hp))
            {
                var type = (hp > 3) ? EnemyType.Tank : (hp > 1 ? EnemyType.Normal : EnemyType.Fast);
                Enemies.Add(new Enemy(Field.Path, Field.CellSize, hp, type));
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
                if (Projectiles[i].HasHit || Projectiles[i].Target.IsDead)
                    Projectiles.RemoveAt(i);
            }
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemies[i].Update();
                if (Enemies[i].IsDead)  { Score += 10; Resources.EarnGold(20); Enemies.RemoveAt(i); }
                else if (Enemies[i].ReachedEnd) { Resources.LoseBaseHp(1); Enemies.RemoveAt(i); }
            }
            if (Waves.IsWaveComplete(Enemies)) { Resources.EarnGold(50); Waves.StartNextWave(); }
        }
    }
}
