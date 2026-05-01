#pragma once
#include <vector>
#include <algorithm>
#include <limits>

class ShadowMap {
public:
    const int Width;
    const int Height;

    std::vector<float> DepthBuffer;

    ShadowMap(int width, int height)
        : Width(width), Height(height)
        , DepthBuffer(width * height, std::numeric_limits<float>::max()) {}

    void Clear() {
        std::fill(DepthBuffer.begin(), DepthBuffer.end(), std::numeric_limits<float>::max());
    }
};
