namespace TowerDefense.Model
{
    public class ResourceManager
    {
        public int Gold { get; private set; } = 150;
        public int BaseHp { get; private set; } = 20;
        public const int TowerCost = 50;

        public bool CanAffordTower() => Gold >= TowerCost;
        public void BuyTower() => Gold -= TowerCost;
        public void EarnGold(int amount) => Gold += amount;
        public void LoseBaseHp(int amount) => BaseHp -= amount;
        public bool IsGameOver() => BaseHp <= 0;
    }
}
