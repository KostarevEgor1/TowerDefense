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

        public void StartWave() => Waves.StartNextWave();

        public void Update()
        {
            if (IsGameOver) return;

            if (Waves.ShouldSpawn(out int hp))
                Enemies.Add(new Enemy(Field.Path, Field.CellSize, hp));

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
                if (Enemies[i].IsDead)
                {
                    Score += 10;
                    Resources.EarnGold(20);
                    Enemies.RemoveAt(i);
                }
                else if (Enemies[i].ReachedEnd)
                {
                    Resources.LoseBaseHp(1);
                    Enemies.RemoveAt(i);
                }
            }

            if (Waves.IsWaveComplete(Enemies))
            {
                Resources.EarnGold(50);
                Waves.StartNextWave();
            }
        }
    }
}
