using System.Collections.Generic;
using TowerDefense.Model;

namespace TowerDefense.View
{
    public interface IGameScene
    {
        GameField Field { get; }
        IReadOnlyList<Enemy> Enemies { get; }
        IReadOnlyList<Tower> Towers { get; }
        IReadOnlyList<Projectile> Projectiles { get; }
        IReadOnlyList<ImpactEffect> ImpactEffects { get; }
        int Gold { get; }
        int Base1Hp { get; }
        int Base2Hp { get; }
        bool IsGameOver { get; }
    }
}
