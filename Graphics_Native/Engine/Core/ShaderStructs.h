#pragma once
#include "../Math/Vector2.h"
#include "../Math/Vector3.h"
#include "../Math/Vector4.h"
#include "../Objects/Color.h"
#include "../Objects/Vertex.h"

struct VertexOut {
    Vector4 ClipPosition;
    Vector3 WorldPosition;
    Vector3 Normal;
    Color   Color;
    Vector2 UV;
};

struct FragmentIn {
    Vector3 WorldPosition;
    Vector3 Normal;
    Color   Color;
    Vector2 UV;
    bool    FrontFace;
};
