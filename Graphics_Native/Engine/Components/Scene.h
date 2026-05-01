#pragma once
#include <vector>
#include "../Objects/Mesh.h"

class Scene {
public:
    std::vector<Mesh> Meshes;

    void AddMesh(Mesh mesh) { Meshes.push_back(std::move(mesh)); }
    void Clear()            { Meshes.clear(); }
};
