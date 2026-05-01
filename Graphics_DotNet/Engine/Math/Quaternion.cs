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
            float s = MathF.Sin(half);
            return new Quaternion(axis.X * s, axis.Y * s, axis.Z * s, MathF.Cos(half));
        }

        public static Vector3 RotateVector(Vector3 v, Quaternion q)
        {
            Quaternion p = new Quaternion(v.X, v.Y, v.Z, 0);
            Quaternion qConj = new Quaternion(-q.X, -q.Y, -q.Z, q.W);
            Quaternion rotated = q * p * qConj;
            return new Vector3(rotated.X, rotated.Y, rotated.Z);
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
            float invMag = 1f / MathF.Sqrt(X*X + Y*Y + Z*Z + W*W);
            return new Quaternion(X*invMag, Y*invMag, Z*invMag, W*invMag);
        }
    }
}