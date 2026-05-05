using System.Collections.Generic;
using System.Drawing;

namespace TowerDefense.Model
{
    public class GameField
    {
        public int Cols { get; } = 20;
        public int Rows { get; } = 15;
        public int CellSize { get; } = 40;

        public List<List<Point>> BasePaths { get; } = new List<List<Point>>
        {
            new List<Point>
            {
                // Путь 1: y=4 (может сдвигаться на y=3 или y=5)
                new Point(0, 4), new Point(4, 4), new Point(4, 1),
                new Point(9, 1), new Point(9, 4), new Point(14, 4),
                new Point(14, 1), new Point(19, 1)
            },
            new List<Point>
            {
                // Путь 2: y=10 (фиксированный)
                new Point(0, 10), new Point(5, 10), new Point(5, 13),
                new Point(10, 13), new Point(10, 10), new Point(15, 10),
                new Point(15, 13), new Point(19, 13)
            }
        };

        public List<List<Point>> ActivePaths { get; private set; }

        // Зарезервированные клетки под башни (рядом с путями на y=6,7,8,9)
        public List<Point> BuildZones { get; } = new List<Point>
        {
            // Левая сторона (между путями)
            new Point(1, 6), new Point(2, 6), new Point(3, 6),
            new Point(1, 7), new Point(2, 7), new Point(3, 7),
            new Point(1, 8), new Point(2, 8), new Point(3, 8),
            // Центр-лево
            new Point(6, 6), new Point(7, 6), new Point(8, 6),
            new Point(6, 7), new Point(7, 7), new Point(8, 7),
            new Point(6, 8), new Point(7, 8), new Point(8, 8),
            // Центр-право
            new Point(11, 6), new Point(12, 6), new Point(13, 6),
            new Point(11, 7), new Point(12, 7), new Point(13, 7),
            new Point(11, 8), new Point(12, 8), new Point(13, 8),
            // Правая сторона
            new Point(16, 6), new Point(17, 6), new Point(18, 6),
            new Point(16, 7), new Point(17, 7), new Point(18, 7),
            new Point(16, 8), new Point(17, 8), new Point(18, 8)
        };

        public GameField()
        {
            ActivePaths = new List<List<Point>>(BasePaths);
        }

        public bool ShiftPathForWave(int wave)
        {
            if (wave % 3 != 0)
            {
                return false;
            }

            // Сдвигаем только первый путь между y=3, y=4, y=5
            int targetY = wave % 6 == 0 ? 3 : 5;

            var shifted = new List<Point>();
            foreach (var p in BasePaths[0])
            {
                // Меняем только y координату на целевую для горизонтальных сегментов
                if (p.Y == 4) // Горизонтальные сегменты на y=4
                {
                    shifted.Add(new Point(p.X, targetY));
                }
                else
                {
                    shifted.Add(p);
                }
            }

            ActivePaths = new List<List<Point>> { shifted, BasePaths[1] };
            return true;
        }

        public bool IsOnAnyPath(int col, int row)
        {
            foreach (var path in ActivePaths)
                if (IsOnPath(path, col, row)) return true;
            return false;
        }

        public int PathIndexForCell(int col, int row)
        {
            for (int i = 0; i < ActivePaths.Count; i++)
                if (IsOnPath(ActivePaths[i], col, row)) return i;
            return -1;
        }

        public static bool IsOnPath(List<Point> path, int col, int row)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var a = path[i];
                var b = path[i + 1];
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
            // Статические build zones
            if (BuildZones.Exists(p => p.X == col && p.Y == row))
                return true;
            
            // Динамические build zones вплотную к путям
            return IsAdjacentToPath(col, row);
        }

        private bool IsAdjacentToPath(int col, int row)
        {
            // Проверяем, находится ли клетка вплотную к пути (сверху или снизу)
            // Путь 1 может быть на y=3,4,5
            int path1Y = ActivePaths[0][0].Y; // Получаем текущую y координату первого пути
            
            // Клетки вплотную к пути 1 (сверху или снизу)
            if ((row == path1Y - 1 || row == path1Y + 1) && 
                (col == 2 || col == 7 || col == 12 || col == 17))
                return true;
            
            // Клетки вплотную к пути 2 (y=10, сверху или снизу)
            if ((row == 9 || row == 11) && 
                (col == 2 || col == 7 || col == 12 || col == 17))
                return true;
            
            return false;
        }
    }
}
