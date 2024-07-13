namespace BriLib
{
    public class TwoDimensionalBoundingBox
    {
        public float X;
        public float Y;
        public float Radius;

        public TwoDimensionalBoundingBox(float centerX, float centerY, float radius)
        {
            X = centerX;
            Y = centerY;
            Radius = radius;
        }

        public bool Intersects(float x, float y)
        {
            return (x >= X - Radius && x < X + Radius && y >= Y - Radius && y < Y + Radius);
        }

        public bool Intersects(TwoDimensionalBoundingBox box)
        {
            var boxLeft = box.X - box.Radius;
            var boxRight = box.X + box.Radius;
            var boxTop = box.Y + box.Radius;
            var boxBot = box.Y - box.Radius;

            var myLeft = X - Radius;
            var myRight = X + Radius;
            var myTop = Y + Radius;
            var myBot = Y - Radius;

            if (myRight < boxLeft) return false;
            if (myLeft > boxRight) return false;
            if (myTop < boxBot) return false;
            if (myBot > boxTop) return false;
            return true;
        }

        public float BoundsDistance(float x, float y)
        {
            var xDist = System.Math.Abs(x - X) - Radius;
            var yDist = System.Math.Abs(y - Y) - Radius;

            if (yDist > 0 && xDist < 0) return yDist;
            if (yDist < 0 && xDist > 0) return xDist;
            if (yDist <= 0 && xDist <= 0) return System.Math.Min(xDist, yDist);

            return (xDist.Sq() + yDist.Sq()).Sqrt();
        }

        public override string ToString()
        {
            return string.Format("[TwoDimensionalBoundingBox x={0}, y={1}, radius={2}]", X, Y, Radius);
        }
    }
}
