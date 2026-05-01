using System;

namespace GraphicsLibrary
{
    public readonly struct Matrix4x4
    {
        public readonly float M00, M01, M02, M03;
        public readonly float M10, M11, M12, M13;
        public readonly float M20, M21, M22, M23;
        public readonly float M30, M31, M32, M33;

        public Matrix4x4(
            float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33)
        {
            M00 = m00; M01 = m01; M02 = m02; M03 = m03;
            M10 = m10; M11 = m11; M12 = m12; M13 = m13;
            M20 = m20; M21 = m21; M22 = m22; M23 = m23;
            M30 = m30; M31 = m31; M32 = m32; M33 = m33;
        }

        public float this[int row, int col] => (row * 4 + col) switch
        {
            0  => M00, 1  => M01, 2  => M02, 3  => M03,
            4  => M10, 5  => M11, 6  => M12, 7  => M13,
            8  => M20, 9  => M21, 10 => M22, 11 => M23,
            12 => M30, 13 => M31, 14 => M32, 15 => M33,
            _  => 0f
        };

        public static readonly Matrix4x4 Identity = new Matrix4x4(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            return new Matrix4x4(
                a.M00*b.M00 + a.M01*b.M10 + a.M02*b.M20 + a.M03*b.M30,
                a.M00*b.M01 + a.M01*b.M11 + a.M02*b.M21 + a.M03*b.M31,
                a.M00*b.M02 + a.M01*b.M12 + a.M02*b.M22 + a.M03*b.M32,
                a.M00*b.M03 + a.M01*b.M13 + a.M02*b.M23 + a.M03*b.M33,

                a.M10*b.M00 + a.M11*b.M10 + a.M12*b.M20 + a.M13*b.M30,
                a.M10*b.M01 + a.M11*b.M11 + a.M12*b.M21 + a.M13*b.M31,
                a.M10*b.M02 + a.M11*b.M12 + a.M12*b.M22 + a.M13*b.M32,
                a.M10*b.M03 + a.M11*b.M13 + a.M12*b.M23 + a.M13*b.M33,

                a.M20*b.M00 + a.M21*b.M10 + a.M22*b.M20 + a.M23*b.M30,
                a.M20*b.M01 + a.M21*b.M11 + a.M22*b.M21 + a.M23*b.M31,
                a.M20*b.M02 + a.M21*b.M12 + a.M22*b.M22 + a.M23*b.M32,
                a.M20*b.M03 + a.M21*b.M13 + a.M22*b.M23 + a.M23*b.M33,

                a.M30*b.M00 + a.M31*b.M10 + a.M32*b.M20 + a.M33*b.M30,
                a.M30*b.M01 + a.M31*b.M11 + a.M32*b.M21 + a.M33*b.M31,
                a.M30*b.M02 + a.M31*b.M12 + a.M32*b.M22 + a.M33*b.M32,
                a.M30*b.M03 + a.M31*b.M13 + a.M32*b.M23 + a.M33*b.M33);
        }

        public static Vector4 operator *(Matrix4x4 mat, Vector4 vec)
        {
            return new Vector4(
                mat.M00*vec.X + mat.M01*vec.Y + mat.M02*vec.Z + mat.M03*vec.W,
                mat.M10*vec.X + mat.M11*vec.Y + mat.M12*vec.Z + mat.M13*vec.W,
                mat.M20*vec.X + mat.M21*vec.Y + mat.M22*vec.Z + mat.M23*vec.W,
                mat.M30*vec.X + mat.M31*vec.Y + mat.M32*vec.Z + mat.M33*vec.W);
        }

        public static Vector3 operator *(Matrix4x4 mat, Vector3 vec)
        {
            return new Vector3(
                mat.M00*vec.X + mat.M01*vec.Y + mat.M02*vec.Z + mat.M03,
                mat.M10*vec.X + mat.M11*vec.Y + mat.M12*vec.Z + mat.M13,
                mat.M20*vec.X + mat.M21*vec.Y + mat.M22*vec.Z + mat.M23);
        }

        public static Matrix4x4 Translation(Vector3 t) => new Matrix4x4(
            1, 0, 0, t.X,
            0, 1, 0, t.Y,
            0, 0, 1, t.Z,
            0, 0, 0, 1);

        public static Matrix4x4 Scale(Vector3 s) => new Matrix4x4(
            s.X, 0,   0,   0,
            0,   s.Y, 0,   0,
            0,   0,   s.Z, 0,
            0,   0,   0,   1);

        public static Matrix4x4 RotationX(float radians)
        {
            float c = MathF.Cos(radians);
            float s = MathF.Sin(radians);
            return new Matrix4x4(
                1,  0,  0, 0,
                0,  c, -s, 0,
                0,  s,  c, 0,
                0,  0,  0, 1);
        }

        public static Matrix4x4 RotationY(float radians)
        {
            float c = MathF.Cos(radians);
            float s = MathF.Sin(radians);
            return new Matrix4x4(
                 c, 0, s, 0,
                 0, 1, 0, 0,
                -s, 0, c, 0,
                 0, 0, 0, 1);
        }

        public static Matrix4x4 RotationZ(float radians)
        {
            float c = MathF.Cos(radians);
            float s = MathF.Sin(radians);
            return new Matrix4x4(
                c, -s, 0, 0,
                s,  c, 0, 0,
                0,  0, 1, 0,
                0,  0, 0, 1);
        }

        public static Matrix4x4 AxisAngleRotation(Vector3 axis, float radians)
        {
            axis = axis.Normalized();
            float x = axis.X, y = axis.Y, z = axis.Z;
            float c = MathF.Cos(radians);
            float s = MathF.Sin(radians);
            float t = 1f - c;
            return new Matrix4x4(
                t*x*x + c,   t*x*y - s*z, t*x*z + s*y, 0,
                t*x*y + s*z, t*y*y + c,   t*y*z - s*x, 0,
                t*x*z - s*y, t*y*z + s*x, t*z*z + c,   0,
                0,           0,           0,            1);
        }

        public static Matrix4x4 Orthographic(
            float left, float right,
            float bottom, float top,
            float near, float far)
        {
            return new Matrix4x4(
                2f/(right-left), 0,               0,               -(right+left)/(right-left),
                0,               2f/(top-bottom), 0,               -(top+bottom)/(top-bottom),
                0,               0,               -2f/(far-near),  -(far+near)/(far-near),
                0,               0,               0,               1);
        }

        public override string ToString()
        {
            return
                $"[{M00}, {M01}, {M02}, {M03}]\n" +
                $"[{M10}, {M11}, {M12}, {M13}]\n" +
                $"[{M20}, {M21}, {M22}, {M23}]\n" +
                $"[{M30}, {M31}, {M32}, {M33}]";
        }
    }
}
