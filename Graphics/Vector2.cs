using System;

namespace GraphicsLibrary
{
    public readonly struct Vector2
    {
        public readonly float X;
        public readonly float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static readonly Vector2 Zero = new Vector2(0, 0);
        public static readonly Vector2 One = new Vector2(1, 1);
        public static readonly Vector2 UnitX = new Vector2(1, 0);
        public static readonly Vector2 UnitY = new Vector2(0, 1);

        public float Length => MathF.Sqrt(X * X + Y * Y);
        public float LengthSquared => X * X + Y * Y;

        public Vector2 Normalized()
        {
            float length = Length;
            if (length == 0f)
                return Vector2.Zero;

            return this / length;
        }

        public static float Dot(Vector2 a, Vector2 b)
            => a.X * b.X + a.Y * b.Y;

        public static Vector2 operator +(Vector2 a, Vector2 b)
            => new Vector2(a.X + b.X, a.Y + b.Y);

        public static Vector2 operator -(Vector2 a, Vector2 b)
            => new Vector2(a.X - b.X, a.Y - b.Y);

        public static Vector2 operator -(Vector2 v)
            => new Vector2(-v.X, -v.Y);

        public static Vector2 operator *(Vector2 v, float scalar)
            => new Vector2(v.X * scalar, v.Y * scalar);

        public static Vector2 operator *(float scalar, Vector2 v)
            => v * scalar;

        public static Vector2 operator /(Vector2 v, float scalar)
        {
            if (scalar == 0f)
                throw new DivideByZeroException();

            return new Vector2(v.X / scalar, v.Y / scalar);
        }

        public override string ToString()
            => $"({X}, {Y})";
    }
}