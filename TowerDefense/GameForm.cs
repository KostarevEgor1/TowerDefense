using System.Drawing;
using System.Windows.Forms;
using TowerDefense.Controller;
using TowerDefense.Model;
using TowerDefense.View;

namespace TowerDefense
{
    public class GameForm : Form
    {
        private readonly GameController controller = new GameController();
        private readonly GameRenderer   renderer;
        private readonly Timer          timer = new Timer();
        private TowerType selectedType = TowerType.Basic;
        private Point mouseCell = new Point(-1, -1);

        public GameForm()
        {
            renderer = new GameRenderer(controller.Model);
            Text = "Tower Defense v0.4 — Типы башен"; DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false;
            int fieldW = controller.Model.Field.Cols * controller.Model.Field.CellSize;
            int fieldH = controller.Model.Field.Rows * controller.Model.Field.CellSize;
            ClientSize = new Size(fieldW, fieldH + 70);

            var btnWave = new Button { Text = "Волна", Width = 80, Height = 26, Left = fieldW - 90, Top = fieldH + 6 };
            btnWave.Click += (s, e) => controller.StartWave();

            var btnBasic  = new Button { Text = "Базовая (50g)",  Width = 120, Height = 26, Left = 10, Top = fieldH + 6 };
            var btnSniper = new Button { Text = "Снайпер (100g)", Width = 120, Height = 26, Left = 140, Top = fieldH + 6 };
            var btnRapid  = new Button { Text = "Пулемёт (75g)",  Width = 120, Height = 26, Left = 270, Top = fieldH + 6 };
            btnBasic.Click  += (s, e) => selectedType = TowerType.Basic;
            btnSniper.Click += (s, e) => selectedType = TowerType.Sniper;
            btnRapid.Click  += (s, e) => selectedType = TowerType.Rapid;
            Controls.AddRange(new Control[] { btnWave, btnBasic, btnSniper, btnRapid });

            timer.Interval = 16;
            timer.Tick += (s, e) => { controller.Tick(); Invalidate(); };
            timer.Start();
            MouseClick += (s, e) =>
            {
                if (e.Y < fieldH) controller.HandleClick(e.X, e.Y, selectedType);
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
            };
            MouseLeave += (s, e) =>
            {
                mouseCell = new Point(-1, -1);
                Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            renderer.Draw(e.Graphics, mouseCell, selectedType);
            var m = controller.Model;
            int barY = m.Field.Rows * m.Field.CellSize + 40;
            e.Graphics.FillRectangle(Brushes.Black, 0, barY, ClientSize.Width, 30);
            using var font = new Font("Arial", 11);
            e.Graphics.DrawString(
                $"Золото: {m.Resources.Gold}   HP базы: {m.Resources.BaseHp}   Счёт: {m.Score}   Волна: {m.Waves.CurrentWave}   Выбрано: {selectedType}",
                font, Brushes.Yellow, 10, barY + 6);
            if (m.IsGameOver)
                e.Graphics.DrawString("GAME OVER", new Font("Arial", 30, FontStyle.Bold),
                    Brushes.Red, 250, 200);
        }
    }
}
