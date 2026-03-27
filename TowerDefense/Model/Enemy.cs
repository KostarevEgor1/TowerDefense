using System.Collections.Generic;
using System.Drawing;

namespace TowerDefense.Model
{
    public class Enemy
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Speed { get; } = 1.5f;
        public bool ReachedEnd { get; private set; }
        public int MaxHealth { get; }
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;

        private readonly List<Point> path;
        private int segmentIndex;
        private float segmentProgress;
        private readonly int cellSize;

        public Enemy(List<Point> path, int cellSize, int health = 3)
        {
            this.path = path; this.cellSize = cellSize;
            MaxHealth = health; Health = health;
            X = path[0].X * cellSize + cellSize / 2f;
            Y = path[0].Y * cellSize + cellSize / 2f;
        }

        public void TakeDamage(int dmg) => Health -= dmg;

        public void Update()
        {
            if (segmentIndex >= path.Count - 1) { ReachedEnd = true; return; }
            segmentProgress += Speed;
            var from = path[segmentIndex]; var to = path[segmentIndex + 1];
            float dx = (to.X - from.X) * cellSize; float dy = (to.Y - from.Y) * cellSize;
            float segLen = System.MathF.Sqrt(dx * dx + dy * dy);
            while (segmentProgress >= segLen)
            {
                segmentProgress -= segLen; segmentIndex++;
                if (segmentIndex >= path.Count - 1) { ReachedEnd = true; return; }
                from = path[segmentIndex]; to = path[segmentIndex + 1];
                dx = (to.X - from.X) * cellSize; dy = (to.Y - from.Y) * cellSize;
                segLen = System.MathF.Sqrt(dx * dx + dy * dy);
            }
            float t = segLen > 0 ? segmentProgress / segLen : 0;
            X = from.X * cellSize + cellSize / 2f + dx * t;
            Y = from.Y * cellSize + cellSize / 2f + dy * t;
        }
    }
}
