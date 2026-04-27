namespace TowerDefense.Model
{
    public enum ImpactEffectType
    {
        Hit,
        Death
    }

    public class ImpactEffect
    {
        public float X { get; }
        public float Y { get; }
        public int Lifetime { get; private set; }
        public int MaxLifetime { get; }
        public ImpactEffectType Type { get; }

        public bool IsExpired => Lifetime <= 0;

        public ImpactEffect(float x, float y, ImpactEffectType type = ImpactEffectType.Hit)
        {
            X = x;
            Y = y;
            Type = type;
            MaxLifetime = type == ImpactEffectType.Death ? 16 : 10;
            Lifetime = MaxLifetime;
        }

        public void Update()
        {
            if (Lifetime > 0)
            {
                Lifetime--;
            }
        }
    }
}
