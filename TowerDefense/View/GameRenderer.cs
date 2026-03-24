using System.Drawing;
using TowerDefense.Model;

namespace TowerDefense.View
{
    public class GameRenderer
    {
        private readonly GameModel model;

        public GameRenderer(GameModel model) => this.model = model;

        public void Draw(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(34, 85, 34));

            var field = model.Field;
            using var gridPen = new Pen(Color.FromArgb(25, 0, 0, 0));
            for (int col = 0; col <= field.Cols; col++)
                g.DrawLine(gridPen, col * field.CellSize, 0,
                    col * field.CellSize, field.Rows * field.CellSize);
            for (int row = 0; row <= field.Rows; row++)
                g.DrawLine(gridPen, 0, row * field.CellSize,
                    field.Cols * field.CellSize, row * field.CellSize);
            using var pathBrush = new SolidBrush(Color.FromArgb(200, 160, 120));
            for (int col = 0; col < field.Cols; col++)
                for (int row = 0; row < field.Rows; row++)
                    if (field.IsOnPath(col, row))
                        g.FillRectangle(pathBrush,
                            col * field.CellSize, row * field.CellSize,
                            field.CellSize, field.CellSize);
            using var arrowPen = new Pen(Color.FromArgb(100, 80, 50), 1.5f);
            for (int i = 0; i < field.Path.Count - 1; i++)
            {
                int ax = field.Path[i].X     * field.CellSize + field.CellSize / 2;
                int ay = field.Path[i].Y     * field.CellSize + field.CellSize / 2;
                int bx = field.Path[i + 1].X * field.CellSize + field.CellSize / 2;
                int by = field.Path[i + 1].Y * field.CellSize + field.CellSize / 2;
                g.DrawLine(arrowPen, ax, ay, bx, by);
            }
            using var labelFont = new Font("Arial", 10, FontStyle.Bold);
            var sp = field.Path[0];
            var ep = field.Path[field.Path.Count - 1];
            g.FillRectangle(Brushes.LimeGreen,
                sp.X * field.CellSize, sp.Y * field.CellSize, field.CellSize, field.CellSize);
            g.DrawString("S", labelFont, Brushes.Black,
                sp.X * field.CellSize + 12, sp.Y * field.CellSize + 11);
            g.FillRectangle(Brushes.Crimson,
                ep.X * field.CellSize, ep.Y * field.CellSize, field.CellSize, field.CellSize);
            g.DrawString("E", labelFont, Brushes.White,
                ep.X * field.CellSize + 12, ep.Y * field.CellSize + 11);
            foreach (var enemy in model.Enemies)
            {
                float ex = enemy.X - 14;
                float ey = enemy.Y - 14;
                g.FillEllipse(Brushes.DarkRed, ex, ey, 28, 28);
                g.FillEllipse(Brushes.OrangeRed, ex + 3, ey + 3, 22, 22);
            }
        }
    }
}
