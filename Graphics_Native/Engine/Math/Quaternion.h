#pragma once
#include <cmath>
#include "Vector3.h"

struct Quaternion {
    float X, Y, Z, W;

    Quaternion() : X(0), Y(0), Z(0), W(1) {}
    Quaternion(float x, float y, float z, float w) : X(x), Y(y), Z(z), W(w) {}

    static Quaternion Identity() { return {0,0,0,1}; }

    static Quaternion FromAxisAngle(Vector3 axis, float radians) {
        float half = radians * 0.5f;
        float s    = sinf(half);
        return { axis.X*s, axis.Y*s, axis.Z*s, cosf(half) };
    }

    Quaternion operator*(const Quaternion& b) const {
        return {
            W*b.X + X*b.W + Y*b.Z - Z*b.Y,
            W*b.Y - X*b.Z + Y*b.W + Z*b.X,
            W*b.Z + X*b.Y - Y*b.X + Z*b.W,
            W*b.W - X*b.X - Y*b.Y - Z*b.Z
        };
    }

    Quaternion Normalized() const {
        float invMag = 1.0f / sqrtf(X*X + Y*Y + Z*Z + W*W);
        return { X*invMag, Y*invMag, Z*invMag, W*invMag };
    }

    static Vector3 RotateVector(Vector3 v, Quaternion q) {
        Quaternion p    = { v.X, v.Y, v.Z, 0 };
        Quaternion qInv = { -q.X, -q.Y, -q.Z, q.W };
        Quaternion r    = q * p * qInv;
        return { r.X, r.Y, r.Z };
    }
};
