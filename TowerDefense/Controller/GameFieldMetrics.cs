namespace TowerDefense.Controller
{
    public readonly record struct GameFieldMetrics(int Cols, int Rows, int CellSize)
    {
        public int PixelWidth => Cols * CellSize;
        public int PixelHeight => Rows * CellSize;
    }
}
