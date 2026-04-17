namespace TowerDefense.Model
{
    public class ResourceManager
    {
        public int Gold { get; private set; } = 150;
        public int BaseHp { get; private set; } = 20; // Общее HP (для совместимости)
        public int Base1Hp { get; private set; } = 10; // HP первой части базы
        public int Base2Hp { get; private set; } = 10; // HP второй части базы
        public const int TowerCost = 50;

        public bool CanAffordTower() => Gold >= TowerCost;
        public void BuyTower() => Gold -= TowerCost;
        public void EarnGold(int amount) => Gold += amount;
        
        public void LoseBaseHp(int amount) 
        {
            BaseHp -= amount;
        }
        
        public void LoseBase1Hp(int amount)
        {
            Base1Hp -= amount;
            BaseHp -= amount;
        }
        
        public void LoseBase2Hp(int amount)
        {
            Base2Hp -= amount;
            BaseHp -= amount;
        }
        
        public bool IsGameOver() => Base1Hp <= 0 || Base2Hp <= 0;
    }
}
