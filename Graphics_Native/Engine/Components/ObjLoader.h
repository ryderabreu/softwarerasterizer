#pragma once
#include <string>
#include "../Objects/Mesh.h"

struct ObjLoader {
    static Mesh Load(const std::string& filePath);
};
