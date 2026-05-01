#pragma once
#include <vector>
#include <algorithm>
#include "../Objects/Color.h"

class FrameBuffer {
public:
    const int Width;
    const int Height;

    std::vector<Color> ColorBuffer;

    FrameBuffer(int width, int height)
        : Width(width), Height(height)
        , ColorBuffer(width * height, Color::Black) {}

    void Clear(Color clearColor) {
        std::fill(ColorBuffer.begin(), ColorBuffer.end(), clearColor);
    }
};
