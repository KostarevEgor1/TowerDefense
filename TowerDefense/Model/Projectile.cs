using System;

namespace TowerDefense.Model
{
    public class Projectile
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public Enemy Target { get; }
        public int Damage { get; }
        public float Speed { get; } = 8f;
        public bool HasHit { get; private set; }

        public Projectile(float startX, float startY, Enemy target, int damage)
        {
            X = startX;
            Y = startY;
            Target = target;
            Damage = damage;
        }

        public void Update()
        {
            if (HasHit || Target.IsDead) return;

            float dx = Target.X - X;
            float dy = Target.Y - Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist <= Speed)
            {
                Target.TakeDamage(Damage);
                HasHit = true;
                return;
            }

            X += (dx / dist) * Speed;
            Y += (dy / dist) * Speed;
        }
    }
}
