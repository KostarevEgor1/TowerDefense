using TowerDefense.Model;

namespace TowerDefense.Controller
{
    public class GameController
    {
        public GameModel Model { get; } = new GameModel();

        public void Tick() => Model.Update();
    }
}
