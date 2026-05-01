#pragma once
#include <cmath>

struct Vector4 {
    float X, Y, Z, W;

    Vector4() : X(0), Y(0), Z(0), W(0) {}
    Vector4(float x, float y, float z, float w) : X(x), Y(y), Z(z), W(w) {}

    Vector4  operator+ (Vector4 b) const { return {X+b.X, Y+b.Y, Z+b.Z, W+b.W}; }
    Vector4  operator- (Vector4 b) const { return {X-b.X, Y-b.Y, Z-b.Z, W-b.W}; }
    Vector4  operator* (float s)   const { return {X*s, Y*s, Z*s, W*s}; }
    Vector4& operator+=(Vector4 b)       { X+=b.X; Y+=b.Y; Z+=b.Z; W+=b.W; return *this; }
};

inline Vector4 operator*(float s, Vector4 v) { return {v.X*s, v.Y*s, v.Z*s, v.W*s}; }
