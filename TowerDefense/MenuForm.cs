using System.Drawing;
using System.Windows.Forms;

namespace TowerDefense
{
    public class MenuForm : Form
    {
        public MenuForm()
        {
            Text = "Tower Defense — Главное меню";
            ClientSize = new Size(400, 300);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(20, 40, 20);

            var title = new Label
            {
                Text = "TOWER DEFENSE",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.LimeGreen,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Width = 380, Top = 50, Left = 10
            };

            var btnStart = new Button
            {
                Text = "Начать игру", Width = 160, Height = 40,
                Left = 120, Top = 120, Font = new Font("Arial", 13)
            };
            btnStart.Click += (s, e) =>
            {
                Hide();
                var game = new GameForm(showTutorial: false);
                game.FormClosed += (gs, ge) => Close();
                game.Show();
            };

            var btnTutorial = new Button
            {
                Text = "Обучение", Width = 160, Height = 40,
                Left = 120, Top = 170, Font = new Font("Arial", 13)
            };
            btnTutorial.Click += (s, e) =>
            {
                Hide();
                var game = new GameForm(showTutorial: true);
                game.FormClosed += (gs, ge) => 
                {
                    // После обучения показываем меню снова
                    Show();
                };
                game.Show();
            };

            var btnExit = new Button
            {
                Text = "Выход", Width = 160, Height = 40,
                Left = 120, Top = 220, Font = new Font("Arial", 13)
            };
            btnExit.Click += (s, e) => Close();

            var hint = new Label
            {
                Text = "Ставьте башни кликом. Кнопки башен внизу.\nДождитесь волны врагов!",
                ForeColor = Color.LightGray, Font = new Font("Arial", 9),
                Width = 380, Top = 260, Left = 10,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            Controls.AddRange(new Control[] { title, btnStart, btnTutorial, btnExit, hint });
        }
    }
}
