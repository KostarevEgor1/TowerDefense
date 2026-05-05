using TowerDefense.Model;
using TowerDefense.View;

namespace TowerDefense.Controller
{
    public class GameController
    {
        private readonly GameModel model;

        public GameController(DifficultyLevel difficultyLevel = DifficultyLevel.Normal)
        {
            model = new GameModel(difficultyLevel);
            Scene = new GameSceneAdapter(model);
            FieldMetrics = new GameFieldMetrics(model.Field.Cols, model.Field.Rows, model.Field.CellSize);
        }

        public IGameScene Scene { get; }
        public GameFieldMetrics FieldMetrics { get; }
        public bool IsGameOver => model.IsGameOver;
        public int Score => model.Score;
        public int CurrentWave => model.Waves.CurrentWave;
        public bool IsWaveInProgress => model.Waves.WaveInProgress;
        public string DifficultyName => model.DifficultyName;

        public void Tick() => model.Update();
        public void StartWave() => model.StartWave();

        public void HandleClick(int pixelX, int pixelY, TowerType type = TowerType.Basic)
        {
            int col = pixelX / FieldMetrics.CellSize;
            int row = pixelY / FieldMetrics.CellSize;
            model.PlaceTower(col, row, type);
        }

        public PlayerActionResult HandlePrimaryAction(int col, int row, TowerType selectedType)
        {
            if (model.FindTower(col, row) != null)
            {
                return model.UpgradeTower(col, row)
                    ? PlayerActionResult.TowerUpgraded
                    : PlayerActionResult.None;
            }

            int beforeCount = model.Towers.Count;
            model.PlaceTower(col, row, selectedType);
            return model.Towers.Count > beforeCount
                ? PlayerActionResult.TowerPlaced
                : PlayerActionResult.None;
        }

        public PlayerActionResult HandleSecondaryAction(int col, int row)
        {
            return model.SellTower(col, row)
                ? PlayerActionResult.TowerSold
                : PlayerActionResult.None;
        }

        public GameHudState GetHudState(TowerType selectedType, System.Drawing.Point hoverCell)
        {
            bool isGameOver = model.IsGameOver;
            bool canStartWave = !isGameOver && !model.Waves.WaveInProgress;
            int waveTotal = System.Math.Max(1, model.Waves.EnemiesPerWave);
            int defeatedInWave = System.Math.Max(0, model.Waves.SpawnedInWave - model.Enemies.Count);
            float waveRatio = System.Math.Min(1f, defeatedInWave / (float)waveTotal);

            string waveButtonText = isGameOver
                ? "Игра окончена"
                : model.Waves.WaveInProgress
                    ? $"Идет волна {model.Waves.CurrentWave}"
                    : $"Начать волну {model.Waves.CurrentWave + 1}";

            var hoveredTower = model.FindTower(hoverCell.X, hoverCell.Y);
            string hintText = hoveredTower == null
                ? $"ЛКМ: построить или улучшить  |  ПКМ: продать  |  Выбрано: {GetTowerDisplayName(selectedType)}"
                : $"{GetTowerDisplayName(hoveredTower.Type)} ур.{hoveredTower.Level}  |  Улучшение: {hoveredTower.GetUpgradeCost()}  |  Продажа: {hoveredTower.GetSellValue()}";

            return new GameHudState(
                IsGameOver: isGameOver,
                CanStartWave: canStartWave,
                CurrentWave: System.Math.Max(1, model.Waves.CurrentWave),
                DefeatedInWave: defeatedInWave,
                WaveTotal: waveTotal,
                WaveRatio: waveRatio,
                Gold: model.Resources.Gold,
                Base1Hp: model.Resources.Base1Hp,
                Base2Hp: model.Resources.Base2Hp,
                Score: model.Score,
                DifficultyName: model.DifficultyName,
                WavePatternName: model.Waves.CurrentWave == 0 && !model.Waves.WaveInProgress
                    ? "Ожидание"
                    : GetWavePatternDisplayName(model.Waves.CurrentPattern),
                WaveButtonText: waveButtonText,
                HintText: hintText);
        }

        private static string GetTowerDisplayName(TowerType type)
        {
            return type switch
            {
                TowerType.Sniper => "Снайпер",
                TowerType.Rapid => "Скоростр.",
                _ => "Базовая"
            };
        }

        private static string GetWavePatternDisplayName(WavePattern pattern)
        {
            return pattern switch
            {
                WavePattern.FastRush => "Рывок",
                WavePattern.Heavy => "Тяжелая",
                WavePattern.Mixed => "Смешанная",
                WavePattern.Spike => "Штурм",
                _ => "Обычная"
            };
        }
    }
}
