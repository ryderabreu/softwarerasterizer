#pragma once
#include <cmath>
#include "Vector3.h"
#include "Vector4.h"

struct Matrix4x4 {
    float M00, M01, M02, M03;
    float M10, M11, M12, M13;
    float M20, M21, M22, M23;
    float M30, M31, M32, M33;

    Matrix4x4() : M00(0),M01(0),M02(0),M03(0),
                  M10(0),M11(0),M12(0),M13(0),
                  M20(0),M21(0),M22(0),M23(0),
                  M30(0),M31(0),M32(0),M33(0) {}

    Matrix4x4(float m00, float m01, float m02, float m03,
              float m10, float m11, float m12, float m13,
              float m20, float m21, float m22, float m23,
              float m30, float m31, float m32, float m33)
        : M00(m00),M01(m01),M02(m02),M03(m03)
        , M10(m10),M11(m11),M12(m12),M13(m13)
        , M20(m20),M21(m21),M22(m22),M23(m23)
        , M30(m30),M31(m31),M32(m32),M33(m33) {}

    static const Matrix4x4 Identity;

    Matrix4x4 operator*(const Matrix4x4& b) const {
        return {
            M00*b.M00 + M01*b.M10 + M02*b.M20 + M03*b.M30,
            M00*b.M01 + M01*b.M11 + M02*b.M21 + M03*b.M31,
            M00*b.M02 + M01*b.M12 + M02*b.M22 + M03*b.M32,
            M00*b.M03 + M01*b.M13 + M02*b.M23 + M03*b.M33,

            M10*b.M00 + M11*b.M10 + M12*b.M20 + M13*b.M30,
            M10*b.M01 + M11*b.M11 + M12*b.M21 + M13*b.M31,
            M10*b.M02 + M11*b.M12 + M12*b.M22 + M13*b.M32,
            M10*b.M03 + M11*b.M13 + M12*b.M23 + M13*b.M33,

            M20*b.M00 + M21*b.M10 + M22*b.M20 + M23*b.M30,
            M20*b.M01 + M21*b.M11 + M22*b.M21 + M23*b.M31,
            M20*b.M02 + M21*b.M12 + M22*b.M22 + M23*b.M32,
            M20*b.M03 + M21*b.M13 + M22*b.M23 + M23*b.M33,

            M30*b.M00 + M31*b.M10 + M32*b.M20 + M33*b.M30,
            M30*b.M01 + M31*b.M11 + M32*b.M21 + M33*b.M31,
            M30*b.M02 + M31*b.M12 + M32*b.M22 + M33*b.M32,
            M30*b.M03 + M31*b.M13 + M32*b.M23 + M33*b.M33
        };
    }

    Vector4 operator*(Vector4 v) const {
        return {
            M00*v.X + M01*v.Y + M02*v.Z + M03*v.W,
            M10*v.X + M11*v.Y + M12*v.Z + M13*v.W,
            M20*v.X + M21*v.Y + M22*v.Z + M23*v.W,
            M30*v.X + M31*v.Y + M32*v.Z + M33*v.W
        };
    }

    Vector3 operator*(Vector3 v) const {
        return {
            M00*v.X + M01*v.Y + M02*v.Z + M03,
            M10*v.X + M11*v.Y + M12*v.Z + M13,
            M20*v.X + M21*v.Y + M22*v.Z + M23
        };
    }

    static Matrix4x4 Translation(Vector3 t) {
        return { 1,0,0,t.X, 0,1,0,t.Y, 0,0,1,t.Z, 0,0,0,1 };
    }

    static Matrix4x4 Scale(Vector3 s) {
        return { s.X,0,0,0, 0,s.Y,0,0, 0,0,s.Z,0, 0,0,0,1 };
    }

    static Matrix4x4 RotationX(float r) {
        float c = cosf(r), s = sinf(r);
        return { 1,0,0,0, 0,c,-s,0, 0,s,c,0, 0,0,0,1 };
    }

    static Matrix4x4 RotationY(float r) {
        float c = cosf(r), s = sinf(r);
        return { c,0,s,0, 0,1,0,0, -s,0,c,0, 0,0,0,1 };
    }

    static Matrix4x4 RotationZ(float r) {
        float c = cosf(r), s = sinf(r);
        return { c,-s,0,0, s,c,0,0, 0,0,1,0, 0,0,0,1 };
    }

    static Matrix4x4 AxisAngleRotation(Vector3 axis, float r) {
        axis = axis.Normalized();
        float x = axis.X, y = axis.Y, z = axis.Z;
        float c = cosf(r), s = sinf(r), t = 1.0f - c;
        return {
            t*x*x+c,     t*x*y-s*z,  t*x*z+s*y,  0,
            t*x*y+s*z,   t*y*y+c,    t*y*z-s*x,  0,
            t*x*z-s*y,   t*y*z+s*x,  t*z*z+c,    0,
            0,           0,          0,           1
        };
    }

    static Matrix4x4 Orthographic(float left, float right,
                                   float bottom, float top,
                                   float nearZ, float farZ) {
        return {
            2.0f/(right-left), 0,                0,                -(right+left)/(right-left),
            0,                 2.0f/(top-bottom), 0,                -(top+bottom)/(top-bottom),
            0,                 0,                -2.0f/(farZ-nearZ),-(farZ+nearZ)/(farZ-nearZ),
            0,                 0,                0,                 1
        };
    }
};

inline const Matrix4x4 Matrix4x4::Identity = {
    1,0,0,0,
    0,1,0,0,
    0,0,1,0,
    0,0,0,1
};
