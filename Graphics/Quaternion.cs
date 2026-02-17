using System;

namespace GraphicsLibrary
{
    public struct Quaternion
    {
        public float X, Y, Z, W;

        public Quaternion(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }

        public static Quaternion Identity => new Quaternion(0, 0, 0, 1);

        public static Quaternion FromAxisAngle(Vector3 axis, float radians)
        {
            float half = radians * 0.5f;
            float s = (float)Math.Sin(half);
            return new Quaternion(axis.X * s, axis.Y * s, axis.Z * s, (float)Math.Cos(half));
        }

        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.W*b.X + a.X*b.W + a.Y*b.Z - a.Z*b.Y,
                a.W*b.Y - a.X*b.Z + a.Y*b.W + a.Z*b.X,
                a.W*b.Z + a.X*b.Y - a.Y*b.X + a.Z*b.W,
                a.W*b.W - a.X*b.X - a.Y*b.Y - a.Z*b.Z
            );
        }

        public Quaternion Normalized()
        {
            float mag = (float)Math.Sqrt(X*X + Y*Y + Z*Z + W*W);
            return new Quaternion(X/mag, Y/mag, Z/mag, W/mag);
        }
    }
}