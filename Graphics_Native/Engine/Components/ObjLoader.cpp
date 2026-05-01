#include "ObjLoader.h"
#include <fstream>
#include <sstream>
#include <vector>
#include <string>
#include <stdexcept>
#include <cstdlib>

static int ParseInt(const char* s) {
    // Use strtol instead of stoi — stops at non-digit chars (\r, etc.) without throwing
    char* end;
    long v = std::strtol(s, &end, 10);
    return (end == s) ? 0 : (int)v;
}

static void ParseIndex(const std::string& token,
                        int& posIdx, int& uvIdx, int& normIdx) {
    posIdx = uvIdx = normIdx = -1;
    const char* p = token.c_str();
    // Component 0: position
    char* end;
    long v0 = std::strtol(p, &end, 10);
    if (end == p) return;
    posIdx = (int)v0 - 1;
    if (*end != '/') return;
    p = end + 1;
    // Component 1: uv (may be empty, e.g. "1//2")
    if (*p != '/') {
        long v1 = std::strtol(p, &end, 10);
        if (end != p) uvIdx = (int)v1 - 1;
        p = end;
    }
    if (*p != '/') return;
    p++;
    // Component 2: normal
    long v2 = std::strtol(p, &end, 10);
    if (end != p) normIdx = (int)v2 - 1;
}

static Vertex MakeVertex(int posIdx, int uvIdx, int normIdx,
                          const std::vector<Vector3>& positions,
                          const std::vector<Vector2>& uvs,
                          const std::vector<Vector3>& normals) {
    Vertex v;
    // Bounds-check every index — bad data should never crash
    v.Position = (posIdx  >= 0 && posIdx  < (int)positions.size()) ? positions[posIdx]  : Vector3{};
    v.Normal   = (normIdx >= 0 && normIdx < (int)normals.size())   ? normals[normIdx]   : Vector3::UnitY;
    v.UV       = (uvIdx   >= 0 && uvIdx   < (int)uvs.size())       ? uvs[uvIdx]         : Vector2::Zero;
    v.Color    = Color::White;
    return v;
}

Mesh ObjLoader::Load(const std::string& filePath) {
    std::ifstream file(filePath);
    if (!file.is_open())
        throw std::runtime_error("OBJ file not found: " + filePath);

    std::vector<Vector3> positions;
    std::vector<Vector3> normals;
    std::vector<Vector2> uvs;
    Mesh mesh;

    // Reserve generous estimates to avoid repeated reallocs
    positions.reserve(8192);
    normals.reserve(8192);
    uvs.reserve(8192);
    mesh.Triangles.reserve(16384);

    // Fixed-size face buffer — handles up to 32-gon polygons without heap allocation
    Vertex faceVerts[32];

    std::string line;
    while (std::getline(file, line)) {
        if (line.empty()) continue;
        // Strip trailing \r for CRLF files
        if (line.back() == '\r') line.pop_back();
        if (line.empty() || line[0] == '#') continue;

        const char* lp = line.c_str();
        // Skip leading whitespace
        while (*lp == ' ' || *lp == '\t') ++lp;

        if (lp[0] == 'v' && lp[1] == ' ') {
            float x = 0, y = 0, z = 0;
            sscanf(lp + 2, "%f %f %f", &x, &y, &z);
            positions.push_back({ x, y, z });

        } else if (lp[0] == 'v' && lp[1] == 'n') {
            float x = 0, y = 0, z = 0;
            sscanf(lp + 3, "%f %f %f", &x, &y, &z);
            normals.push_back(Vector3{ x, y, z }.Normalized());

        } else if (lp[0] == 'v' && lp[1] == 't') {
            float u = 0, v = 0;
            sscanf(lp + 3, "%f %f", &u, &v);
            uvs.push_back({ u, v });

        } else if (lp[0] == 'f' && lp[1] == ' ') {
            std::istringstream ss(lp + 2);
            std::string token;
            int faceCount = 0;
            while (ss >> token && faceCount < 32) {
                int p, t, n;
                ParseIndex(token, p, t, n);
                if (p < 0 || p >= (int)positions.size()) continue;
                faceVerts[faceCount++] = MakeVertex(p, t, n, positions, uvs, normals);
            }
            for (int i = 1; i < faceCount - 1; i++)
                mesh.Triangles.push_back({ faceVerts[0], faceVerts[i], faceVerts[i+1] });
        }
    }

    return mesh;
}
