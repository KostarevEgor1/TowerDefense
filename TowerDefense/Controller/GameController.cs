using TowerDefense.Model;

namespace TowerDefense.Controller
{
    public class GameController
    {
        public GameModel Model { get; } = new GameModel();

        public void Tick() => Model.Update();

        public void HandleClick(int pixelX, int pixelY)
        {
            int col = pixelX / Model.Field.CellSize;
            int row = pixelY / Model.Field.CellSize;
            Model.PlaceTower(col, row);
        }
    }
}
