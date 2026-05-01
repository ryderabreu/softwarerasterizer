#pragma once
#include <vector>
#include "Triangle.h"
#include "Texture.h"
#include "../Math/Matrix4x4.h"

class Mesh {
public:
    std::vector<Triangle> Triangles;
    Matrix4x4             model = Matrix4x4::Identity;
    Texture*              texture = nullptr;
};
