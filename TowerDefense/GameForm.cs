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
        private bool gameOverShown;
        private readonly bool tutorialMode;
        private int tutorialStep = 0;
        private bool tutorialCompleted = false;
        private Point mouseCell = new Point(-1, -1);

        public GameForm(bool showTutorial = false)
        {
            tutorialMode = showTutorial;
            renderer = new GameRenderer(controller.Model);
            Text = tutorialMode ? "Tower Defense — Обучение" : "Tower Defense v1.0"; 
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            int fieldW = controller.Model.Field.Cols * controller.Model.Field.CellSize;
            int fieldH = controller.Model.Field.Rows * controller.Model.Field.CellSize;
            ClientSize = new Size(fieldW, fieldH + 70);

            var btnWave   = new Button { Text = "Следующая волна", Width = 140, Height = 26, Left = fieldW - 150, Top = fieldH + 36 };
            var btnBasic  = new Button { Text = "Базовая (50g)",   Width = 120, Height = 26, Left = 10,  Top = fieldH + 6 };
            var btnSniper = new Button { Text = "Снайпер (100g)",  Width = 120, Height = 26, Left = 140, Top = fieldH + 6 };
            var btnRapid  = new Button { Text = "Пулемёт (75g)",   Width = 120, Height = 26, Left = 270, Top = fieldH + 6 };
            btnWave.Click   += (s, e) => 
            {
                controller.StartWave();
                if (tutorialMode && tutorialStep == 2) tutorialStep = 3;
            };
            btnBasic.Click  += (s, e) => 
            {
                selectedType = TowerType.Basic;
                if (tutorialMode && tutorialStep == 0) tutorialStep = 1;
            };
            btnSniper.Click += (s, e) => 
            {
                selectedType = TowerType.Sniper;
                if (tutorialMode && tutorialStep == 0) tutorialStep = 1;
            };
            btnRapid.Click  += (s, e) => 
            {
                selectedType = TowerType.Rapid;
                if (tutorialMode && tutorialStep == 0) tutorialStep = 1;
            };
            Controls.AddRange(new Control[] { btnWave, btnBasic, btnSniper, btnRapid });

            timer.Interval = 16;
            timer.Tick += (s, e) =>
            {
                controller.Tick();
                
                // Проверка завершения обучения после 3 волн
                if (tutorialMode && !tutorialCompleted && controller.Model.Waves.CurrentWave >= 4)
                {
                    tutorialCompleted = true;
                    timer.Stop();
                    Close();
                    return;
                }
                
                Invalidate();
                if (controller.Model.IsGameOver && !gameOverShown)
                {
                    gameOverShown = true;
                    timer.Stop();
                    MessageBox.Show($"Игра окончена!\nСчёт: {controller.Model.Score}", "GAME OVER");
                }
            };
            timer.Start();
            MouseClick += (s, e) =>
            {
                if (e.Y < fieldH)
                {
                    int prevTowerCount = controller.Model.Towers.Count;
                    controller.HandleClick(e.X, e.Y, selectedType);
                    if (tutorialMode && tutorialStep == 1 && controller.Model.Towers.Count > prevTowerCount)
                        tutorialStep = 2;
                }
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
            int barY = m.Field.Rows * m.Field.CellSize + 36;
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(30, 30, 30)), 0, barY, ClientSize.Width, 34);
            using var font = new Font("Arial", 11);
            e.Graphics.DrawString(
                $"Золото: {m.Resources.Gold}   База1: {m.Resources.Base1Hp}HP   База2: {m.Resources.Base2Hp}HP   Счёт: {m.Score}   Волна: {m.Waves.CurrentWave}   Выбрано: {selectedType}",
                font, Brushes.Yellow, 10, barY + 8);

            if (tutorialMode)
            {
                DrawTutorialOverlay(e.Graphics);
            }
        }

        private void DrawTutorialOverlay(Graphics g)
        {
            // Если обучение завершено, не показываем подсказки
            if (tutorialCompleted) return;
            
            string message = tutorialStep switch
            {
                0 => "Шаг 1: Выберите тип башни внизу",
                1 => "Шаг 2: Кликните на зелёную клетку, чтобы поставить башню",
                2 => "Шаг 3: Нажмите 'Следующая волна', чтобы начать",
                3 => controller.Model.Waves.CurrentWave < 3 
                    ? $"Шаг 4: Защищайте базу! Волна {controller.Model.Waves.CurrentWave}/3"
                    : "Последняя волна обучения! Держитесь!",
                _ => ""
            };

            if (string.IsNullOrEmpty(message)) return;

            using var font = new Font("Arial", 14, FontStyle.Bold);
            var size = g.MeasureString(message, font);
            int boxW = (int)size.Width + 40;
            int boxH = (int)size.Height + 30;
            int boxX = (ClientSize.Width - boxW) / 2;
            int boxY = 20;

            g.FillRectangle(new SolidBrush(Color.FromArgb(220, 0, 0, 0)), boxX, boxY, boxW, boxH);
            g.DrawRectangle(new Pen(Color.Yellow, 3), boxX, boxY, boxW, boxH);
            g.DrawString(message, font, Brushes.White, boxX + 20, boxY + 15);
        }
    }
}
