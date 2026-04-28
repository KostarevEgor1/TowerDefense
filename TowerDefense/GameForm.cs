using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TowerDefense.Controller;
using TowerDefense.Model;
using TowerDefense.View;

namespace TowerDefense
{
    public class GameForm : Form
    {
        private readonly GameController controller = new();
        private readonly GameRenderer renderer;
        private readonly Timer timer = new();
        private readonly bool tutorialMode;
        private readonly AccentButton btnWave;
        private readonly AccentButton btnBasic;
        private readonly AccentButton btnSniper;
        private readonly AccentButton btnRapid;

        private const int HudHeight = 194;
        private Rectangle fieldRect;
        private Rectangle hudRect;
        private Rectangle deckRect;
        private float fieldScale = 1f;
        private TowerType selectedType = TowerType.Basic;
        private bool gameOverShown;
        private int tutorialStep;
        private bool tutorialCompleted;
        private Point mouseCell = new(-1, -1);

        public GameForm(bool showTutorial = false)
        {
            tutorialMode = showTutorial;
            renderer = new GameRenderer(controller.Scene);

            Text = tutorialMode ? "Tower Defense - Tutorial" : "Tower Defense";
            DoubleBuffered = true;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1240, 930);

            btnWave = CreateHudButton("Начать волну", 232, VisualTheme.AccentBlue);
            btnBasic = CreateHudButton("Базовая 50", 132, VisualTheme.AccentMint);
            btnSniper = CreateHudButton("Снайпер 100", 132, VisualTheme.AccentBlue);
            btnRapid = CreateHudButton("Скоростр. 75", 140, VisualTheme.AccentAmber);

            btnWave.Font = new Font("Bahnschrift SemiBold", 11f, FontStyle.Bold);

            btnWave.Click += (_, _) =>
            {
                controller.StartWave();
                if (tutorialMode && tutorialStep == 2)
                {
                    tutorialStep = 3;
                }
            };

            btnBasic.Click += (_, _) => SelectTower(TowerType.Basic);
            btnSniper.Click += (_, _) => SelectTower(TowerType.Sniper);
            btnRapid.Click += (_, _) => SelectTower(TowerType.Rapid);

            Controls.AddRange(new Control[] { btnWave, btnBasic, btnSniper, btnRapid });

            timer.Interval = 16;
            timer.Tick += (_, _) =>
            {
                controller.Tick();
                UpdateWaveButton();

                if (tutorialMode && !tutorialCompleted && controller.CurrentWave >= 4)
                {
                    tutorialCompleted = true;
                    timer.Stop();
                    Close();
                    return;
                }

                Invalidate();

                if (controller.IsGameOver && !gameOverShown)
                {
                    gameOverShown = true;
                    timer.Stop();
                    MessageBox.Show($"Игра окончена\nСчёт: {controller.Score}", "Tower Defense");
                }
            };
            timer.Start();

            MouseClick += (_, e) =>
            {
                if (!TryGetCellFromMouse(e.Location, out int col, out int row))
                {
                    return;
                }

                bool placed = controller.TryPlaceTower(col, row, selectedType);
                if (tutorialMode && tutorialStep == 1 && placed)
                {
                    tutorialStep = 2;
                }
            };

            MouseMove += (_, e) =>
            {
                if (!TryGetCellFromMouse(e.Location, out int col, out int row))
                {
                    if (mouseCell.X != -1 || mouseCell.Y != -1)
                    {
                        mouseCell = new Point(-1, -1);
                        Invalidate();
                    }

                    return;
                }

                if (mouseCell.X != col || mouseCell.Y != row)
                {
                    mouseCell = new Point(col, row);
                    Invalidate();
                }
            };

            MouseLeave += (_, _) =>
            {
                mouseCell = new Point(-1, -1);
                Invalidate();
            };

            Resize += (_, _) =>
            {
                LayoutGameUi();
                Invalidate();
            };

            LayoutGameUi();
            HighlightSelectedTowerButton();
            UpdateWaveButton();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var bg = new LinearGradientBrush(ClientRectangle, VisualTheme.ShellTop, VisualTheme.ShellBottom, 90f))
            {
                e.Graphics.FillRectangle(bg, ClientRectangle);
            }

            DrawShellDecor(e.Graphics);

            Rectangle fieldFrame = new(fieldRect.Left - 14, fieldRect.Top - 14, fieldRect.Width + 28, fieldRect.Height + 28);
            VisualTheme.DrawRoundedPanel(
                e.Graphics,
                fieldFrame,
                32f,
                Color.FromArgb(236, 18, 28, 40),
                Color.FromArgb(242, 8, 14, 22),
                VisualTheme.PanelBorder,
                VisualTheme.PanelHighlight,
                shadowAlpha: 105);

            GraphicsState state = e.Graphics.Save();
            using (var fieldTransform = new Matrix(fieldScale, 0f, 0f, fieldScale, fieldRect.Left, fieldRect.Top))
            {
                e.Graphics.Transform = fieldTransform;
                renderer.Draw(e.Graphics, mouseCell, selectedType);
            }
            e.Graphics.Restore(state);

            DrawHud(e.Graphics);

            if (tutorialMode)
            {
                DrawTutorialOverlay(e.Graphics);
            }
        }

        private void SelectTower(TowerType type)
        {
            selectedType = type;
            HighlightSelectedTowerButton();
            if (tutorialMode && tutorialStep == 0)
            {
                tutorialStep = 1;
            }
        }

        private void UpdateWaveButton()
        {
            GameHudState hud = controller.GetHudState(selectedType);
            btnWave.Enabled = hud.CanStartWave;
            btnWave.Text = hud.WaveButtonText;
        }

        private void DrawHud(Graphics g)
        {
            GameHudState hud = controller.GetHudState(selectedType);
            Rectangle actionStripRect = GetActionStripRect();

            VisualTheme.DrawRoundedPanel(
                g,
                deckRect,
                28f,
                Color.FromArgb(232, 16, 26, 38),
                Color.FromArgb(238, 8, 14, 22),
                VisualTheme.PanelBorder,
                VisualTheme.PanelHighlight,
                shadowAlpha: 104,
                drawSheen: false,
                drawHighlightLine: false);

            using var sectionFont = new Font("Bahnschrift SemiBold", 9.5f, FontStyle.Bold);
            TextRenderer.DrawText(g, "УПРАВЛЕНИЕ", sectionFont, new Rectangle(deckRect.Left + 22, deckRect.Top + 12, 200, 18),
                VisualTheme.TextSecondary, TextFormatFlags.VerticalCenter);
            if (tutorialMode)
            {
                TextRenderer.DrawText(g, "ОБУЧЕНИЕ", sectionFont,
                    new Rectangle(deckRect.Right - 180, deckRect.Top + 12, 160, 18), VisualTheme.TextSecondary,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }

            VisualTheme.DrawRoundedPanel(
                g,
                actionStripRect,
                20f,
                Color.FromArgb(226, 17, 26, 38),
                Color.FromArgb(234, 9, 15, 23),
                Color.FromArgb(128, 102, 182, 178),
                Color.FromArgb(52, 255, 255, 255),
                shadowAlpha: 52,
                drawSheen: false,
                drawHighlightLine: false);

            int statY = actionStripRect.Bottom + 14;
            int cardW = 106;
            int gap = 10;
            int cardX = deckRect.Left + 18;

            DrawStatCard(g, cardX + (cardW + gap) * 0, statY, cardW, 52, "Золото", hud.Gold.ToString(), VisualTheme.AccentGold);
            DrawStatCard(g, cardX + (cardW + gap) * 1, statY, cardW, 52, "База 1", hud.Base1Hp + " ОЗ", VisualTheme.AccentBlue);
            DrawStatCard(g, cardX + (cardW + gap) * 2, statY, cardW, 52, "База 2", hud.Base2Hp + " ОЗ", VisualTheme.AccentMint);
            DrawStatCard(g, cardX + (cardW + gap) * 3, statY, cardW, 52, "Счёт", hud.Score.ToString(), VisualTheme.AccentCoral);
            DrawStatCard(g, cardX + (cardW + gap) * 4, statY, cardW, 52, "Башня", hud.SelectedTowerName, VisualTheme.TowerAccent(selectedType));

            Rectangle progressRect = new(deckRect.Right - 250, statY, 228, 52);
            VisualTheme.DrawRoundedPanel(
                g,
                progressRect,
                18f,
                Color.FromArgb(228, 18, 25, 35),
                Color.FromArgb(232, 10, 15, 22),
                Color.FromArgb(145, 126, 210, 205),
                Color.FromArgb(48, 255, 255, 255),
                shadowAlpha: 64,
                drawSheen: false,
                drawHighlightLine: false);

            using var waveFont = new Font("Bahnschrift SemiBold", 10f, FontStyle.Bold);
            TextRenderer.DrawText(g, $"ВОЛНА {hud.CurrentWave}", waveFont, new Rectangle(progressRect.Left + 14, progressRect.Top + 8, 120, 16),
                VisualTheme.TextPrimary, TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, $"{hud.WaveProgress}/{hud.WaveTotal}", waveFont, new Rectangle(progressRect.Right - 64, progressRect.Top + 8, 48, 16),
                VisualTheme.TextSecondary, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

            Rectangle barRect = new(progressRect.Left + 14, progressRect.Top + 27, progressRect.Width - 28, 12);
            using (var back = new SolidBrush(Color.FromArgb(86, 15, 21, 28)))
            using (var fill = new LinearGradientBrush(
                new Rectangle(barRect.X, barRect.Y, Math.Max(1, (int)(barRect.Width * hud.WaveRatio)), barRect.Height),
                VisualTheme.AccentBlue,
                VisualTheme.AccentMint,
                0f))
            using (var border = new Pen(Color.FromArgb(118, 214, 236, 230), 1.2f))
            {
                g.FillRectangle(back, barRect);
                g.FillRectangle(fill, barRect.X + 1, barRect.Y + 1, Math.Max(0, (int)((barRect.Width - 2) * hud.WaveRatio)), barRect.Height - 2);
                g.DrawRectangle(border, barRect);
            }

            using var hintFont = new Font("Segoe UI", 9.2f, FontStyle.Regular);
            TextRenderer.DrawText(g, hud.HintText, hintFont, new Rectangle(deckRect.Left + 22, deckRect.Bottom - 28, deckRect.Width - 44, 16),
                VisualTheme.TextSecondary, TextFormatFlags.VerticalCenter);
        }

        private static void DrawStatCard(Graphics g, int x, int y, int w, int h, string title, string value, Color accent)
        {
            Rectangle rect = new(x, y, w, h);
            VisualTheme.DrawRoundedPanel(
                g,
                rect,
                16f,
                Color.FromArgb(224, 18, 26, 36),
                Color.FromArgb(230, 10, 16, 24),
                Color.FromArgb(150, accent),
                Color.FromArgb(56, 255, 255, 255),
                shadowAlpha: 58,
                drawSheen: false,
                drawHighlightLine: false);

            using var titleFont = new Font("Segoe UI", 8.2f, FontStyle.Regular);
            using var valueFont = new Font("Bahnschrift SemiBold", 11.5f, FontStyle.Bold);
            TextRenderer.DrawText(g, title, titleFont, new Rectangle(x + 10, y + 8, w - 20, 14), VisualTheme.TextSecondary,
                TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, value, valueFont, new Rectangle(x + 10, y + 24, w - 20, 18), accent,
                TextFormatFlags.VerticalCenter);
        }

        private void DrawTutorialOverlay(Graphics g)
        {
            if (tutorialCompleted)
            {
                return;
            }

            string message = tutorialStep switch
            {
                0 => "Шаг 1: выберите тип башни",
                1 => "Шаг 2: поставьте башню на отмеченную клетку",
                2 => "Шаг 3: начните первую волну",
                _ => controller.CurrentWave < 3
                    ? $"Шаг 4: переживите учебные волны ({controller.CurrentWave}/3)"
                    : "Последняя учебная волна. Удержите обе линии."
            };

            using var font = new Font("Bahnschrift SemiBold", 12.5f, FontStyle.Bold);
            Size textSize = TextRenderer.MeasureText(message, font);
            Rectangle boxRect = new(
                fieldRect.Left + (fieldRect.Width - textSize.Width - 34) / 2,
                fieldRect.Top + 18,
                textSize.Width + 34,
                textSize.Height + 16);

            VisualTheme.DrawRoundedPanel(
                g,
                boxRect,
                18f,
                Color.FromArgb(232, 16, 24, 34),
                Color.FromArgb(236, 8, 14, 20),
                Color.FromArgb(170, 255, 218, 124),
                Color.FromArgb(70, 255, 255, 255),
                shadowAlpha: 74);

            TextRenderer.DrawText(g, message, font, boxRect, VisualTheme.TextPrimary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private bool TryGetCellFromMouse(Point location, out int col, out int row)
        {
            col = -1;
            row = -1;

            if (!fieldRect.Contains(location))
            {
                return false;
            }

            int localX = location.X - fieldRect.Left;
            int localY = location.Y - fieldRect.Top;
            float scaledCellSize = controller.FieldMetrics.CellSize * fieldScale;
            if (fieldScale <= 0f)
            {
                return false;
            }

            if (scaledCellSize <= 0f)
            {
                return false;
            }

            col = (int)Math.Floor(localX / scaledCellSize);
            row = (int)Math.Floor(localY / scaledCellSize);
            col = Math.Max(0, Math.Min(controller.FieldMetrics.Cols - 1, col));
            row = Math.Max(0, Math.Min(controller.FieldMetrics.Rows - 1, row));
            return true;
        }

        private void LayoutGameUi()
        {
            int nativeFieldW = controller.FieldMetrics.PixelWidth;
            int nativeFieldH = controller.FieldMetrics.PixelHeight;
            const int outerMargin = 18;
            const int gap = 26;
            int deckHeight = HudHeight;
            int deckTop = ClientSize.Height - deckHeight - outerMargin;
            int availableFieldHeight = deckTop - gap - outerMargin;
            int availableFieldWidth = ClientSize.Width - outerMargin * 2;
            float widthScale = availableFieldWidth / (float)Math.Max(1, nativeFieldW);
            float heightScale = availableFieldHeight / (float)Math.Max(1, nativeFieldH);
            fieldScale = Math.Min(1f, Math.Min(widthScale, heightScale));
            fieldScale = Math.Max(0.1f, fieldScale);

            int fieldW = Math.Max(1, (int)Math.Round(nativeFieldW * fieldScale));
            int fieldH = Math.Max(1, (int)Math.Round(nativeFieldH * fieldScale));
            int top = outerMargin + Math.Max(0, (availableFieldHeight - fieldH) / 2);
            int left = (ClientSize.Width - fieldW) / 2;

            fieldRect = new Rectangle(left, top, fieldW, fieldH);
            hudRect = new Rectangle(0, deckTop, ClientSize.Width, deckHeight);
            int minButtonDeckWidth = 18 + 118 * 3 + 10 * 2 + 18 + 220 + 18;
            int minStatsDeckWidth = 18 + (106 * 5) + (10 * 4) + 24 + 228 + 22;
            int minDeckWidth = Math.Max(minButtonDeckWidth, minStatsDeckWidth);
            int targetDeckWidth = Math.Max(fieldRect.Width + 40, minDeckWidth);
            int deckWidth = Math.Min(targetDeckWidth, Math.Max(280, ClientSize.Width - outerMargin * 2));
            int deckLeft = (ClientSize.Width - deckWidth) / 2;
            deckRect = new Rectangle(deckLeft, deckTop, deckWidth, deckHeight);

            LayoutHudButtons();
        }

        private AccentButton CreateHudButton(string text, int width, Color color)
        {
            return new AccentButton
            {
                Text = text,
                Width = width,
                Height = 44,
                BaseColor = color,
                GlowColor = color,
                SquareStyle = true
            };
        }

        private void HighlightSelectedTowerButton()
        {
            btnBasic.Selected = selectedType == TowerType.Basic;
            btnSniper.Selected = selectedType == TowerType.Sniper;
            btnRapid.Selected = selectedType == TowerType.Rapid;
            btnBasic.Invalidate();
            btnSniper.Invalidate();
            btnRapid.Invalidate();
        }

        private void DrawShellDecor(Graphics g)
        {
            DrawAmbientGlow(g, new RectangleF(-60, -40, 300, 220), VisualTheme.AccentBlue, 28);
            DrawAmbientGlow(g, new RectangleF(ClientSize.Width - 280, -10, 280, 220), VisualTheme.AccentAmber, 20);
            DrawAmbientGlow(g, new RectangleF(ClientSize.Width / 2f - 180f, ClientSize.Height - 220, 360, 180), VisualTheme.AccentMint, 18);

            using var pen = new Pen(VisualTheme.ShellGlow, 1.2f);
            g.DrawArc(pen, -40, 40, 200, 160, 210, 110);
            g.DrawArc(pen, ClientSize.Width - 180, 34, 180, 140, -30, 110);
        }

        private static void DrawAmbientGlow(Graphics g, RectangleF rect, Color color, int alpha)
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

        private Rectangle GetActionStripRect()
        {
            return new Rectangle(deckRect.Left + 18, deckRect.Top + 28, deckRect.Width - 36, 52);
        }

        private void LayoutHudButtons()
        {
            Rectangle actionStripRect = GetActionStripRect();
            const int towerGap = 10;
            const int groupGap = 18;
            int waveWidth = Math.Clamp((int)Math.Round(actionStripRect.Width * 0.31f), 220, 280);
            int towerAreaWidth = Math.Max(360, actionStripRect.Width - waveWidth - groupGap);
            int towerButtonWidth = Math.Max(112, (towerAreaWidth - towerGap * 2) / 3);
            int lastTowerWidth = towerAreaWidth - towerButtonWidth * 3 - towerGap * 2;
            int buttonTop = actionStripRect.Top + 4;

            btnBasic.SetBounds(actionStripRect.Left, buttonTop, towerButtonWidth, 44);
            btnSniper.SetBounds(btnBasic.Right + towerGap, buttonTop, towerButtonWidth, 44);
            btnRapid.SetBounds(btnSniper.Right + towerGap, buttonTop, towerButtonWidth + lastTowerWidth, 44);
            btnWave.SetBounds(actionStripRect.Right - waveWidth, buttonTop, waveWidth, 44);
            HighlightSelectedTowerButton();
        }
    }
}
