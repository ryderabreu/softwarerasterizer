#pragma once
#include <cmath>

struct Vector3 {
    float X, Y, Z;

    Vector3() : X(0), Y(0), Z(0) {}
    Vector3(float x, float y, float z) : X(x), Y(y), Z(z) {}

    static const Vector3 Zero;
    static const Vector3 One;
    static const Vector3 UnitX;
    static const Vector3 UnitY;
    static const Vector3 UnitZ;

    float LengthSquared() const { return X*X + Y*Y + Z*Z; }
    float Length()        const { return sqrtf(X*X + Y*Y + Z*Z); }

    Vector3 Normalized() const {
        float lenSq = X*X + Y*Y + Z*Z;
        if (lenSq == 0.0f) return {};
        float inv = 1.0f / sqrtf(lenSq);
        return { X*inv, Y*inv, Z*inv };
    }

    static float   Dot  (Vector3 a, Vector3 b) { return a.X*b.X + a.Y*b.Y + a.Z*b.Z; }
    static Vector3 Cross(Vector3 a, Vector3 b) {
        return { a.Y*b.Z - a.Z*b.Y,
                 a.Z*b.X - a.X*b.Z,
                 a.X*b.Y - a.Y*b.X };
    }

    Vector3  operator+ (Vector3 b) const { return {X+b.X, Y+b.Y, Z+b.Z}; }
    Vector3  operator- (Vector3 b) const { return {X-b.X, Y-b.Y, Z-b.Z}; }
    Vector3  operator- ()          const { return {-X, -Y, -Z}; }
    Vector3  operator* (float s)   const { return {X*s, Y*s, Z*s}; }
    Vector3  operator/ (float s)   const { return {X/s, Y/s, Z/s}; }
    Vector3& operator+=(Vector3 b)       { X+=b.X; Y+=b.Y; Z+=b.Z; return *this; }
    Vector3& operator-=(Vector3 b)       { X-=b.X; Y-=b.Y; Z-=b.Z; return *this; }
};

inline Vector3 operator*(float s, Vector3 v) { return {v.X*s, v.Y*s, v.Z*s}; }

inline const Vector3 Vector3::Zero  = {0, 0, 0};
inline const Vector3 Vector3::One   = {1, 1, 1};
inline const Vector3 Vector3::UnitX = {1, 0, 0};
inline const Vector3 Vector3::UnitY = {0, 1, 0};
inline const Vector3 Vector3::UnitZ = {0, 0, 1};
