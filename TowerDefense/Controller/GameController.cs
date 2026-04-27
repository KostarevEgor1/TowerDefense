using TowerDefense.Model;
using TowerDefense.View;

namespace TowerDefense.Controller
{
    public class GameController
    {
        private readonly GameModel model = new();

        public GameController()
        {
            Scene = new GameSceneAdapter(model);
            FieldMetrics = new GameFieldMetrics(model.Field.Cols, model.Field.Rows, model.Field.CellSize);
        }

        public IGameScene Scene { get; }
        public GameFieldMetrics FieldMetrics { get; }
        public bool IsGameOver => model.IsGameOver;
        public int Score => model.Score;
        public int CurrentWave => model.Waves.CurrentWave;
        public bool IsWaveInProgress => model.Waves.WaveInProgress;

        public void Tick() => model.Update();

        public void StartWave() => model.StartWave();

        public bool TryPlaceTower(int col, int row, TowerType type = TowerType.Basic)
        {
            int beforeCount = model.Towers.Count;
            model.PlaceTower(col, row, type);
            return model.Towers.Count > beforeCount;
        }

        public GameHudState GetHudState(TowerType selectedType)
        {
            bool isGameOver = model.IsGameOver;
            bool canStartWave = !isGameOver && model.Waves.CurrentWave == 0 && !model.Waves.WaveInProgress;
            int waveTotal = System.Math.Max(1, model.Waves.EnemiesPerWave);
            int waveProgress = System.Math.Min(model.Waves.SpawnedInWave, waveTotal);
            float waveRatio = System.Math.Min(1f, waveProgress / (float)waveTotal);

            string waveButtonText;
            if (isGameOver)
            {
                waveButtonText = "Игра окончена";
            }
            else if (canStartWave)
            {
                waveButtonText = "Начать волну 1";
            }
            else
            {
                waveButtonText = model.Waves.WaveInProgress
                    ? $"Идет волна {model.Waves.CurrentWave}"
                    : "Готовится новая волна";
            }

            return new GameHudState(
                IsGameOver: isGameOver,
                CanStartWave: canStartWave,
                CurrentWave: System.Math.Max(1, model.Waves.CurrentWave),
                WaveProgress: waveProgress,
                WaveTotal: waveTotal,
                WaveRatio: waveRatio,
                Gold: model.Resources.Gold,
                Base1Hp: model.Resources.Base1Hp,
                Base2Hp: model.Resources.Base2Hp,
                Score: model.Score,
                SelectedTowerName: GetTowerDisplayName(selectedType),
                WaveButtonText: waveButtonText,
                HintText: $"ЛКМ: поставить башню  |  Выбрано: {GetTowerDisplayName(selectedType)}");
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
    }
}
