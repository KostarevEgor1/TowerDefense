namespace TowerDefense.Controller
{
    public readonly record struct GameHudState(
        bool IsGameOver,
        bool CanStartWave,
        int CurrentWave,
        int WaveProgress,
        int WaveTotal,
        float WaveRatio,
        int Gold,
        int Base1Hp,
        int Base2Hp,
        int Score,
        string SelectedTowerName,
        string WaveButtonText,
        string HintText);
}
