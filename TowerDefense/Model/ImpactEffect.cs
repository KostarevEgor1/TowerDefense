namespace TowerDefense.Model
{
    public class ImpactEffect
    {
        public float X { get; }
        public float Y { get; }
        public int Lifetime { get; private set; }
        public int MaxLifetime { get; }

        public bool IsExpired => Lifetime <= 0;

        public ImpactEffect(float x, float y, int lifetime = 10)
        {
            X = x;
            Y = y;
            Lifetime = lifetime;
            MaxLifetime = lifetime;
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
