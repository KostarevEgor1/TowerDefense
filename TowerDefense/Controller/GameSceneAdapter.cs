using TowerDefense.Model;
using TowerDefense.View;

namespace TowerDefense.Controller
{
    internal sealed class GameSceneAdapter : IGameScene
    {
        private readonly GameModel model;

        public GameSceneAdapter(GameModel model)
        {
            this.model = model;
        }

        public GameField Field => model.Field;
        public System.Collections.Generic.IReadOnlyList<Enemy> Enemies => model.Enemies;
        public System.Collections.Generic.IReadOnlyList<Tower> Towers => model.Towers;
        public System.Collections.Generic.IReadOnlyList<Projectile> Projectiles => model.Projectiles;
        public System.Collections.Generic.IReadOnlyList<ImpactEffect> ImpactEffects => model.ImpactEffects;
        public int Gold => model.Resources.Gold;
        public int Base1Hp => model.Resources.Base1Hp;
        public int Base2Hp => model.Resources.Base2Hp;
        public bool IsGameOver => model.IsGameOver;
    }
}
