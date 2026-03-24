using System.Collections.Generic;
using System.Drawing;

namespace TowerDefense.Model
{
    public class GameField
    {
        public int Cols { get; } = 20;
        public int Rows { get; } = 15;
        public int CellSize { get; } = 40;

        public List<Point> Path { get; } = new List<Point>
        {
            new Point(0,  7), new Point(3,  7), new Point(3,  3),
            new Point(7,  3), new Point(7, 11), new Point(12, 11),
            new Point(12, 4), new Point(17, 4), new Point(17, 11),
            new Point(19, 11)
        };

        public bool IsOnPath(int col, int row)
        {
            for (int i = 0; i < Path.Count - 1; i++)
            {
                var a = Path[i];
                var b = Path[i + 1];
                if (a.Y == b.Y && row == a.Y &&
                    col >= System.Math.Min(a.X, b.X) && col <= System.Math.Max(a.X, b.X))
                    return true;
                if (a.X == b.X && col == a.X &&
                    row >= System.Math.Min(a.Y, b.Y) && row <= System.Math.Max(a.Y, b.Y))
                    return true;
            }
            return false;
        }
    }
}
