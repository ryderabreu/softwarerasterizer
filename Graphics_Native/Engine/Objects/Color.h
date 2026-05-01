#pragma once
#include "../Math/Vector3.h"

struct Color {
    float R, G, B, A;

    Color() : R(0), G(0), B(0), A(1) {}
    Color(float r, float g, float b, float a = 1.0f) : R(r), G(g), B(b), A(a) {}

    static const Color White;
    static const Color Black;
    static const Color Red;
    static const Color Green;
    static const Color Blue;

    Vector3 ToVector3() const { return { R, G, B }; }

    Color operator+(Color b) const { return {R+b.R, G+b.G, B+b.B, A+b.A}; }
    Color operator-(Color b) const { return {R-b.R, G-b.G, B-b.B, A-b.A}; }
    Color operator*(Color b) const { return {R*b.R, G*b.G, B*b.B, A*b.A}; }
    Color operator*(float s) const { return {R*s,   G*s,   B*s,   A*s  }; }
};

inline Color operator*(float s, Color c) { return c * s; }

inline const Color Color::White = {1,1,1,1};
inline const Color Color::Black = {0,0,0,1};
inline const Color Color::Red   = {1,0,0,1};
inline const Color Color::Green = {0,1,0,1};
inline const Color Color::Blue  = {0,0,1,1};
