#pragma once
#include <cmath>

struct Vector2 {
    float X, Y;

    Vector2() : X(0), Y(0) {}
    Vector2(float x, float y) : X(x), Y(y) {}

    static const Vector2 Zero;
    static const Vector2 One;
    static const Vector2 UnitX;
    static const Vector2 UnitY;

    float LengthSquared() const { return X*X + Y*Y; }
    float Length()        const { return sqrtf(X*X + Y*Y); }

    Vector2 Normalized() const {
        float lenSq = X*X + Y*Y;
        if (lenSq == 0.0f) return {};
        float inv = 1.0f / sqrtf(lenSq);
        return { X*inv, Y*inv };
    }

    static float  Dot(Vector2 a, Vector2 b) { return a.X*b.X + a.Y*b.Y; }

    Vector2  operator+ (Vector2 b) const { return {X+b.X, Y+b.Y}; }
    Vector2  operator- (Vector2 b) const { return {X-b.X, Y-b.Y}; }
    Vector2  operator- ()          const { return {-X, -Y}; }
    Vector2  operator* (float s)   const { return {X*s, Y*s}; }
    Vector2  operator/ (float s)   const { return {X/s, Y/s}; }
    Vector2& operator+=(Vector2 b)       { X+=b.X; Y+=b.Y; return *this; }
    Vector2& operator-=(Vector2 b)       { X-=b.X; Y-=b.Y; return *this; }
};

inline Vector2 operator*(float s, Vector2 v) { return {v.X*s, v.Y*s}; }

inline const Vector2 Vector2::Zero  = {0.0f, 0.0f};
inline const Vector2 Vector2::One   = {1.0f, 1.0f};
inline const Vector2 Vector2::UnitX = {1.0f, 0.0f};
inline const Vector2 Vector2::UnitY = {0.0f, 1.0f};