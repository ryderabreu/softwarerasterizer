#pragma once
#include <string>
#include <vector>
#include <algorithm>
#include "../Math/Vector2.h"
#include "Color.h"

class Texture {
public:
    int            Width  = 0;
    int            Height = 0;
    std::vector<Color> Pixels;

    Color Sample(Vector2 uv) const {
        int x = (int)(std::clamp(uv.X, 0.0f, 1.0f) * (Width  - 1));
        int y = (int)((1.0f - std::clamp(uv.Y, 0.0f, 1.0f)) * (Height - 1));
        return Pixels[x + y * Width];
    }

    static Texture FromImage(const std::wstring& filePath);
};
