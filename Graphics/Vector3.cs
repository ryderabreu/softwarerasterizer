using System;

namespace GraphicsLibrary
{
    public readonly struct Vector3
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        public static readonly Vector3 One = new Vector3(1, 1, 1);
        public static readonly Vector3 UnitX = new Vector3(1, 0, 0);
        public static readonly Vector3 UnitY = new Vector3(0, 1, 0);
        public static readonly Vector3 UnitZ = new Vector3(0, 0, 1);

        public float Length => MathF.Sqrt(X * X + Y * Y + Z * Z);
        public float LengthSquared => X * X + Y * Y + Z * Z;

        public Vector3 Normalized()
        {
            float length = Length;
            if (length == 0f)
                throw new InvalidOperationException("Cannot normalize zero vector.");

            return this / length;
        }

        public static float Dot(Vector3 a, Vector3 b)
            => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static Vector3 Cross(Vector3 a, Vector3 b)
            => new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );

        public static Vector3 operator +(Vector3 a, Vector3 b)
            => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3 operator -(Vector3 a, Vector3 b)
            => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3 operator -(Vector3 v)
            => new Vector3(-v.X, -v.Y, -v.Z);

        public static Vector3 operator *(Vector3 v, float scalar)
            => new Vector3(v.X * scalar, v.Y * scalar, v.Z * scalar);

        public static Vector3 operator *(float scalar, Vector3 v)
            => v * scalar;

        public static Vector3 operator /(Vector3 v, float scalar)
        {
            if (scalar == 0f)
                throw new DivideByZeroException();

            return new Vector3(v.X / scalar, v.Y / scalar, v.Z / scalar);
        }

        public override string ToString()
            => $"({X}, {Y}, {Z})";
    }
}