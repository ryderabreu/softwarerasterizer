using System;

namespace GraphicsLibrary
{
    public readonly struct Matrix4x4
    {
        private readonly float[,] m;

        public Matrix4x4(float[,] values)
        {
            if (values.GetLength(0) != 4 || values.GetLength(1) != 4)
                throw new ArgumentException("Matrix must be 4x4.");

            m = values;
        }

        public float this[int row, int col] => m[row, col];

        public static Matrix4x4 Identity => new Matrix4x4(new float[,]
        {
            {1,0,0,0},
            {0,1,0,0},
            {0,0,1,0},
            {0,0,0,1}
        });

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            float[,] result = new float[4,4];

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    float sum = 0f;
                    for (int k = 0; k < 4; k++)
                        sum += a[row, k] * b[k, col];

                    result[row, col] = sum;
                }
            }

            return new Matrix4x4(result);
        }

        public static Vector4 operator *(Matrix4x4 mat, Vector4 vec)
        {
            float x =
                mat[0,0] * vec.X +
                mat[0,1] * vec.Y +
                mat[0,2] * vec.Z +
                mat[0,3] * vec.W;

            float y =
                mat[1,0] * vec.X +
                mat[1,1] * vec.Y +
                mat[1,2] * vec.Z +
                mat[1,3] * vec.W;

            float z =
                mat[2,0] * vec.X +
                mat[2,1] * vec.Y +
                mat[2,2] * vec.Z +
                mat[2,3] * vec.W;

            float w =
                mat[3,0] * vec.X +
                mat[3,1] * vec.Y +
                mat[3,2] * vec.Z +
                mat[3,3] * vec.W;

            return new Vector4(x, y, z, w);
        }

        public static Vector3 operator *(Matrix4x4 mat, Vector3 vec)
        {
            Vector4 v = new Vector4(vec.X, vec.Y, vec.Z, 1f);
            Vector4 result = mat * v;

            return new Vector3(result.X, result.Y, result.Z);
        }

        public static Matrix4x4 Translation(Vector3 t)
        {
            return new Matrix4x4(new float[,]
            {
                {1,0,0,t.X},
                {0,1,0,t.Y},
                {0,0,1,t.Z},
                {0,0,0,1}
            });
        }

        public static Matrix4x4 Scale(Vector3 s)
        {
            return new Matrix4x4(new float[,]
            {
                {s.X,0,0,0},
                {0,s.Y,0,0},
                {0,0,s.Z,0},
                {0,0,0,1}
            });
        }

        public static Matrix4x4 RotationX(float radians)
        {
            float c = MathF.Cos(radians);
            float s = MathF.Sin(radians);

            return new Matrix4x4(new float[,]
            {
                {1, 0, 0, 0},
                {0, c,-s, 0},
                {0, s, c, 0},
                {0, 0, 0, 1}
            });
        }

        public static Matrix4x4 RotationY(float radians)
        {
            float c = MathF.Cos(radians);
            float s = MathF.Sin(radians);

            return new Matrix4x4(new float[,]
            {
                { c, 0, s, 0},
                { 0, 1, 0, 0},
                {-s, 0, c, 0},
                { 0, 0, 0, 1}
            });
        }

        public static Matrix4x4 RotationZ(float radians)
        {
            float c = MathF.Cos(radians);
            float s = MathF.Sin(radians);

            return new Matrix4x4(new float[,]
            {
                {c,-s, 0, 0},
                {s, c, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            });
        }

        public static Matrix4x4 AxisAngleRotation(Vector3 axis, float radians)
        {
            axis = axis.Normalized();

            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;

            float c = MathF.Cos(radians);
            float s = MathF.Sin(radians);
            float t = 1 - c;

            return new Matrix4x4(new float[,]
            {
                {t*x*x + c,     t*x*y - s*z,   t*x*z + s*y,   0},
                {t*x*y + s*z,   t*y*y + c,     t*y*z - s*x,   0},
                {t*x*z - s*y,   t*y*z + s*x,   t*z*z + c,     0},
                {0,              0,             0,            1}
            });
        }

        public static Matrix4x4 Orthographic(
            float left, float right,
            float bottom, float top,
            float near, float far)
        {
            return new Matrix4x4(new float[,]
            {
                { 2f/(right-left), 0, 0, -(right+left)/(right-left) },
                { 0, 2f/(top-bottom), 0, -(top+bottom)/(top-bottom) },
                { 0, 0, -2f/(far-near), -(far+near)/(far-near) },
                { 0, 0, 0, 1 }
            });
        }

        public override string ToString()
        {
            return
                $"[{m[0,0]}, {m[0,1]}, {m[0,2]}, {m[0,3]}]\n" +
                $"[{m[1,0]}, {m[1,1]}, {m[1,2]}, {m[1,3]}]\n" +
                $"[{m[2,0]}, {m[2,1]}, {m[2,2]}, {m[2,3]}]\n" +
                $"[{m[3,0]}, {m[3,1]}, {m[3,2]}, {m[3,3]}]";
        }
    }
}