namespace TowerDefense.Controller
{
    public readonly record struct GameHudState(
        bool IsGameOver,
        bool CanStartWave,
        int CurrentWave,
        int DefeatedInWave,
        int WaveTotal,
        float WaveRatio,
        int Gold,
        int Base1Hp,
        int Base2Hp,
        int Score,
        string DifficultyName,
        string WavePatternName,
        string WaveButtonText,
        string HintText);
}
