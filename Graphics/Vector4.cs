using System;

namespace GraphicsLibrary
{
    public struct Vector4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Vector4 operator +(Vector4 a, Vector4 b)
            => new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);

        public static Vector4 operator -(Vector4 a, Vector4 b)
            => new Vector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);

        public static Vector4 operator *(Vector4 v, float s)
            => new Vector4(v.X * s, v.Y * s, v.Z * s, v.W * s);

        public static Vector4 operator *(float s, Vector4 v)
            => v * s;

        public static Vector4 operator /(Vector4 v, float s)
            => new Vector4(v.X / s, v.Y / s, v.Z / s, v.W / s);

        public Vector3 ToVector3()
        {
            if (W == 0) return new Vector3(0, 0, 0);
            return new Vector3(X / W, Y / W, Z / W);
        }
    }
}