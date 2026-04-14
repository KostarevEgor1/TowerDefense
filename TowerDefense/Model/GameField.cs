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

        // Зарезервированные клетки под башни
        public List<Point> BuildZones { get; } = new List<Point>
        {
            // Левая сторона
            new Point(1, 5), new Point(2, 5), new Point(1, 6), new Point(2, 6),
            new Point(1, 8), new Point(2, 8), new Point(1, 9), new Point(2, 9),
            // Центр верх
            new Point(5, 1), new Point(6, 1), new Point(5, 2), new Point(6, 2),
            new Point(8, 5), new Point(9, 5), new Point(8, 6), new Point(9, 6),
            // Центр низ
            new Point(10, 9), new Point(11, 9), new Point(10, 10), new Point(11, 10),
            // Правая сторона
            new Point(15, 5), new Point(16, 5), new Point(15, 6), new Point(16, 6),
            new Point(18, 8), new Point(18, 9), new Point(18, 10)
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

        public bool IsInBuildZone(int col, int row)
        {
            return BuildZones.Exists(p => p.X == col && p.Y == row);
        }
    }
}
