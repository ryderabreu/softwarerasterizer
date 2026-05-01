#pragma once
#include "../Math/Vector2.h"
#include "../Math/Vector3.h"
#include "Color.h"

struct Vertex {
    Vector3 Position;
    Vector3 Normal;
    Color   Color;
    Vector2 UV;
};
