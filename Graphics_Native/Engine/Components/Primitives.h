#pragma once
#include "../Objects/Mesh.h"

struct Primitives {
    static Mesh CreateSphere(float radius = 1.0f, int segments = 16, int rings = 16);
    static Mesh CreatePlane (float size   = 1.0f, int subdivisions = 10);
    static Mesh CreateCube  (float size   = 1.0f);
    static Mesh CreatePyramid(float size  = 1.0f, float height = 1.0f);
};
