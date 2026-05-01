using System;

namespace GraphicsLibrary
{
    public readonly struct Vector4
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static readonly Vector4 Zero  = new Vector4(0, 0, 0, 0);
        public static readonly Vector4 One   = new Vector4(1, 1, 1, 1);
        public static readonly Vector4 UnitX = new Vector4(1, 0, 0, 0);
        public static readonly Vector4 UnitY = new Vector4(0, 1, 0, 0);
        public static readonly Vector4 UnitZ = new Vector4(0, 0, 1, 0);
        public static readonly Vector4 UnitW = new Vector4(0, 0, 0, 1);

        public float Length =>
            MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public float LengthSquared =>
            X * X + Y * Y + Z * Z + W * W;

        public Vector4 Normalized()
        {
            float length = Length;
            if (length == 0f)
                return Zero;

            return this / length;
        }

        public static float Dot(Vector4 a, Vector4 b)
            => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

        public static Vector4 operator +(Vector4 a, Vector4 b)
            => new Vector4(
                a.X + b.X,
                a.Y + b.Y,
                a.Z + b.Z,
                a.W + b.W
            );

        public static Vector4 operator -(Vector4 a, Vector4 b)
            => new Vector4(
                a.X - b.X,
                a.Y - b.Y,
                a.Z - b.Z,
                a.W - b.W
            );

        public static Vector4 operator -(Vector4 v)
            => new Vector4(-v.X, -v.Y, -v.Z, -v.W);

        public static Vector4 operator *(Vector4 v, float scalar)
            => new Vector4(
                v.X * scalar,
                v.Y * scalar,
                v.Z * scalar,
                v.W * scalar
            );

        public static Vector4 operator *(float scalar, Vector4 v)
            => v * scalar;

        public static Vector4 operator /(Vector4 v, float scalar)
        {
            if (scalar == 0f)
                throw new DivideByZeroException();

            return new Vector4(
                v.X / scalar,
                v.Y / scalar,
                v.Z / scalar,
                v.W / scalar
            );
        }

        public Vector3 ToVector3()
            => new Vector3(X, Y, Z);

        public override string ToString()
            => $"({X}, {Y}, {Z}, {W})";
    }
}