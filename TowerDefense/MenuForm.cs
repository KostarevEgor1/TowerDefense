using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TowerDefense.Model;
using TowerDefense.View;

namespace TowerDefense
{
    public class MenuForm : Form
    {
        private readonly GlassPanel cardPanel;
        private readonly ComboBox difficultyBox;
        private readonly Label titleLabel;
        private readonly Label subtitleLabel;
        private readonly Label featureLabel;
        private readonly Label footerLabel;

        public MenuForm()
        {
            Text = "Tower Defense";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1160, 820);
            DoubleBuffered = true;

            cardPanel = new GlassPanel
            {
                Size = new Size(660, 420),
                FillTop = Color.FromArgb(226, 18, 28, 40),
                FillBottom = Color.FromArgb(236, 8, 14, 22),
                BorderColor = VisualTheme.PanelBorder,
                HighlightColor = VisualTheme.PanelHighlight
            };

            titleLabel = new Label
            {
                Text = "TOWER DEFENSE",
                Font = new Font("Bahnschrift SemiBold", 38, FontStyle.Bold),
                ForeColor = VisualTheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 600,
                Height = 72,
                Left = 30,
                Top = 34,
                BackColor = Color.Transparent
            };

            subtitleLabel = new Label
            {
                Text = "Выберите режим и сложность",
                Font = new Font("Segoe UI", 13f, FontStyle.Regular),
                ForeColor = VisualTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 600,
                Height = 28,
                Left = 30,
                Top = 104,
                BackColor = Color.Transparent
            };

            featureLabel = new Label
            {
                Text = string.Empty,
                Font = new Font("Bahnschrift SemiBold", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(218, 230, 236),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 600,
                Height = 24,
                Left = 30,
                Top = 148,
                BackColor = Color.Transparent,
                Visible = false
            };

            var difficultyLabel = new Label
            {
                Text = "Сложность",
                Font = new Font("Bahnschrift SemiBold", 11f, FontStyle.Bold),
                ForeColor = VisualTheme.TextPrimary,
                Width = 120,
                Height = 24,
                Left = 116,
                Top = 154,
                BackColor = Color.Transparent
            };

            difficultyBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed,
                FlatStyle = FlatStyle.Flat,
                Width = 250,
                Left = 246,
                Top = 148,
                ItemHeight = 28,
                Font = new Font("Bahnschrift SemiBold", 10.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(20, 30, 42),
                ForeColor = VisualTheme.TextPrimary
            };
            difficultyBox.Items.AddRange(new object[] { "Легкая", "Обычная", "Сложная" });
            difficultyBox.SelectedIndex = 1;
            difficultyBox.DrawItem += DrawDifficultyItem;

            var btnStart = CreateMenuButton("Играть", VisualTheme.AccentMint, 214);
            var btnTutorial = CreateMenuButton("Обучение", VisualTheme.AccentBlue, 274);
            var btnExit = CreateMenuButton("Выход", VisualTheme.AccentCoral, 334);

            footerLabel = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10f, FontStyle.Italic),
                ForeColor = VisualTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 580,
                Height = 40,
                Left = 40,
                Top = 360,
                BackColor = Color.Transparent,
                Visible = false
            };

            btnStart.Click += (_, _) =>
            {
                Hide();
                var game = new GameForm(ReadDifficulty(), showTutorial: false);
                game.FormClosed += (_, _) =>
                {
                    if (game.ReturnToMenuRequested)
                    {
                        Show();
                        return;
                    }

                    Close();
                };
                game.Show();
            };

            btnTutorial.Click += (_, _) =>
            {
                Hide();
                var game = new GameForm(ReadDifficulty(), showTutorial: true);
                game.FormClosed += (_, _) => Show();
                game.Show();
            };

            btnExit.Click += (_, _) => Close();

            cardPanel.Controls.AddRange(new Control[]
            {
                titleLabel, subtitleLabel, featureLabel, difficultyLabel, difficultyBox, btnStart, btnTutorial, btnExit, footerLabel
            });
            Controls.Add(cardPanel);

            Resize += (_, _) => LayoutMenu();
            LayoutMenu();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var grad = new LinearGradientBrush(ClientRectangle, VisualTheme.ShellTop, VisualTheme.ShellBottom, 90f);
            e.Graphics.FillRectangle(grad, ClientRectangle);

            DrawGlow(e.Graphics, new RectangleF(60, 80, 320, 220), VisualTheme.AccentBlue, 26);
            DrawGlow(e.Graphics, new RectangleF(ClientSize.Width - 360, 60, 300, 220), VisualTheme.AccentAmber, 20);
            DrawGlow(e.Graphics, new RectangleF(ClientSize.Width / 2f - 260f, ClientSize.Height - 260, 520, 200), VisualTheme.AccentMint, 18);

            using var ringPen = new Pen(Color.FromArgb(60, 184, 226, 220), 1.8f);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawArc(ringPen, ClientSize.Width / 2 - 520, ClientSize.Height / 2 - 340, 1040, 680, 202, 136);
            e.Graphics.DrawArc(ringPen, ClientSize.Width / 2 - 480, ClientSize.Height / 2 - 300, 960, 600, 18, 122);
        }

        private AccentButton CreateMenuButton(string text, Color baseColor, int top)
        {
            return new AccentButton
            {
                Text = text,
                Width = 320,
                Height = 44,
                Left = 170,
                Top = top,
                BaseColor = baseColor,
                GlowColor = baseColor,
                Font = new Font("Bahnschrift SemiBold", 13f, FontStyle.Bold)
            };
        }

        private void LayoutMenu()
        {
            cardPanel.Left = (ClientSize.Width - cardPanel.Width) / 2;
            cardPanel.Top = (ClientSize.Height - cardPanel.Height) / 2;
        }

        private DifficultyLevel ReadDifficulty()
        {
            return difficultyBox.SelectedItem?.ToString() switch
            {
                "Легкая" => DifficultyLevel.Easy,
                "Сложная" => DifficultyLevel.Hard,
                _ => DifficultyLevel.Normal
            };
        }

        private void DrawDifficultyItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            e.DrawBackground();
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color accent = e.Index switch
            {
                0 => VisualTheme.AccentMint,
                2 => VisualTheme.AccentCoral,
                _ => VisualTheme.AccentBlue
            };

            using var bg = new SolidBrush(selected ? Color.FromArgb(66, accent) : Color.FromArgb(28, 40, 52));
            using var border = new Pen(Color.FromArgb(120, accent), 1f);
            e.Graphics.FillRectangle(bg, e.Bounds);
            e.Graphics.DrawRectangle(border, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
            string label = difficultyBox.Items[e.Index]?.ToString() ?? string.Empty;
            TextRenderer.DrawText(e.Graphics, label, e.Font ?? Font, e.Bounds, VisualTheme.TextPrimary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static void DrawGlow(Graphics g, RectangleF rect, Color color, int alpha)
        {
            using var path = new GraphicsPath();
            path.AddEllipse(rect);
            using var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(alpha, color),
                SurroundColors = new[] { Color.FromArgb(0, color) }
            };
            g.FillPath(brush, path);
        }
    }
}
