#pragma once
#include <cmath>
#include <algorithm>
#include "../Math/Matrix4x4.h"
#include "../Objects/Color.h"

class DirectionalLight {
public:
    Vector3 Direction = Vector3{0,-1,-1}.Normalized();
    Color   LightColor = Color::White;
    float   Intensity  = 1.0f;

    DirectionalLight(Vector3 direction, Color color, float intensity = 1.0f)
        : Direction(direction.Normalized()), LightColor(color), Intensity(intensity) {}

    Color GetColor(Vector3 normal, Color baseColor, float ambient) const {
        float diff = std::max(Vector3::Dot(normal, -Direction), 0.0f);
        float lit  = ambient + diff * Intensity;
        return { lit * baseColor.R * LightColor.R,
                 lit * baseColor.G * LightColor.G,
                 lit * baseColor.B * LightColor.B, 1.0f };
    }

    Matrix4x4 LightMatrix(float size, Vector3 viewPoint) const {
        Vector3 forward = -Direction;
        Vector3 right, up;

        if (fabsf(Direction.X) < 0.0001f && fabsf(Direction.Z) < 0.0001f) {
            right = Vector3::UnitX;
            up    = Vector3::UnitZ;
        } else {
            up    = Vector3::UnitY;
            right = Vector3::Cross(up, forward).Normalized();
            up    = Vector3::Cross(forward, right);
        }

        Matrix4x4 view = {
            right.X,   right.Y,   right.Z,   -Vector3::Dot(right,   viewPoint),
            up.X,      up.Y,      up.Z,      -Vector3::Dot(up,      viewPoint),
            forward.X, forward.Y, forward.Z, -Vector3::Dot(forward, viewPoint),
            0, 0, 0, 1
        };

        return Matrix4x4::Orthographic(-size, size, -size, size, 0.1f, 50.0f) * view;
    }
};
