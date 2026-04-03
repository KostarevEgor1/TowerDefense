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
        private readonly Button         btnWave = new Button();
        private Point mouseCell = new Point(-1, -1);

        public GameForm()
        {
            renderer = new GameRenderer(controller.Model);
            Text = "Tower Defense v0.3 — Волны и ресурсы";
            DoubleBuffered = true; FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false;
            int fieldW = controller.Model.Field.Cols * controller.Model.Field.CellSize;
            int fieldH = controller.Model.Field.Rows * controller.Model.Field.CellSize;
            ClientSize = new Size(fieldW, fieldH + 60);

            btnWave.Text = "Начать волну"; btnWave.Width = 120; btnWave.Height = 28;
            btnWave.Left = fieldW - 130; btnWave.Top = fieldH + 16;
            btnWave.Click += (s, e) => controller.StartWave();
            Controls.Add(btnWave);

            timer.Interval = 16;
            timer.Tick += (s, e) => { controller.Tick(); Invalidate(); };
            timer.Start();
            MouseClick += (s, e) =>
            {
                if (e.Y < fieldH) controller.HandleClick(e.X, e.Y);
            };
            MouseMove += (s, e) =>
            {
                if (e.Y < fieldH)
                {
                    int col = e.X / controller.Model.Field.CellSize;
                    int row = e.Y / controller.Model.Field.CellSize;
                    if (mouseCell.X != col || mouseCell.Y != row)
                    {
                        mouseCell = new Point(col, row);
                        Invalidate();
                    }
                }
                else
                {
                    if (mouseCell.X != -1)
                    {
                        mouseCell = new Point(-1, -1);
                        Invalidate();
                    }
                }
            };
            MouseLeave += (s, e) =>
            {
                mouseCell = new Point(-1, -1);
                Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            renderer.Draw(e.Graphics, mouseCell);
            var m = controller.Model;
            int barY = m.Field.Rows * m.Field.CellSize;
            e.Graphics.FillRectangle(Brushes.Black, 0, barY, ClientSize.Width, 60);
            using var font = new Font("Arial", 11);
            e.Graphics.DrawString(
                $"Золото: {m.Resources.Gold}   HP базы: {m.Resources.BaseHp}   Счёт: {m.Score}   Волна: {m.Waves.CurrentWave}   [Клик — башня (50g)]",
                font, Brushes.Yellow, 10, barY + 6);
            if (m.IsGameOver)
                e.Graphics.DrawString("GAME OVER", new Font("Arial", 30, FontStyle.Bold),
                    Brushes.Red, 280, barY - 100);
        }
    }
}
