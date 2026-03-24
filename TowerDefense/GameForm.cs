using System.Drawing;
using System.Windows.Forms;
using TowerDefense.Controller;
using TowerDefense.View;

namespace TowerDefense
{
    public class GameForm : Form
    {
        private readonly GameController controller = new GameController();
        private readonly GameRenderer   renderer;
        private readonly Timer          timer = new Timer();

        public GameForm()
        {
            renderer = new GameRenderer(controller.Model);
            Text            = "Tower Defense v0.1 — Неделя 1";
            DoubleBuffered  = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            int fieldW = controller.Model.Field.Cols * controller.Model.Field.CellSize;
            int fieldH = controller.Model.Field.Rows * controller.Model.Field.CellSize;
            ClientSize = new Size(fieldW, fieldH + 30);
            timer.Interval = 16;
            timer.Tick += (s, e) => { controller.Tick(); Invalidate(); };
            timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            renderer.Draw(e.Graphics);
            int barY = controller.Model.Field.Rows * controller.Model.Field.CellSize;
            e.Graphics.FillRectangle(Brushes.Black, 0, barY, ClientSize.Width, 30);
            using var font = new Font("Arial", 11);
            e.Graphics.DrawString(
                $"Враги на поле: {controller.Model.Enemies.Count}",
                font, Brushes.White, 10, barY + 6);
        }
    }
}
