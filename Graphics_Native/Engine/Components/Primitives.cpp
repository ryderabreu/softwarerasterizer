#include "Primitives.h"
#include <cmath>

static Vector3 Spherical(float r, float theta, float phi) {
    float sinT = sinf(theta);
    return { r * sinT * cosf(phi), r * cosf(theta), r * sinT * sinf(phi) };
}

Mesh Primitives::CreateSphere(float radius, int segments, int rings) {
    Mesh mesh;
    constexpr float PI = 3.14159265f;

    for (int y = 0; y < rings; y++) {
        float v0 = (float)y       / rings, v1 = (float)(y+1) / rings;
        float t0 = v0 * PI,               t1 = v1 * PI;

        for (int x = 0; x < segments; x++) {
            float u0 = (float)x       / segments, u1 = (float)(x+1) / segments;
            float p0 = u0 * PI * 2,               p1 = u1 * PI * 2;

            Vector3 pos0 = Spherical(radius, t0, p0);
            Vector3 pos1 = Spherical(radius, t1, p0);
            Vector3 pos2 = Spherical(radius, t1, p1);
            Vector3 pos3 = Spherical(radius, t0, p1);

            Vertex vtx0 = { pos0, pos0.Normalized(), Color::White, {u0, v0} };
            Vertex vtx1 = { pos1, pos1.Normalized(), Color::White, {u0, v1} };
            Vertex vtx2 = { pos2, pos2.Normalized(), Color::White, {u1, v1} };
            Vertex vtx3 = { pos3, pos3.Normalized(), Color::White, {u1, v0} };

            mesh.Triangles.push_back({ vtx0, vtx2, vtx1 });
            mesh.Triangles.push_back({ vtx0, vtx3, vtx2 });
        }
    }
    return mesh;
}

Mesh Primitives::CreatePlane(float size, int subdivisions) {
    Mesh mesh;
    float h    = size / 2.0f;
    float step = size / subdivisions;
    Vector3 normal = { 0, 1, 0 };

    for (int z = 0; z < subdivisions; z++) {
        for (int x = 0; x < subdivisions; x++) {
            float x0 = -h + x * step, x1 = x0 + step;
            float z0 = -h + z * step, z1 = z0 + step;
            float u0 = (float)x / subdivisions, u1 = (float)(x+1) / subdivisions;
            float v0 = (float)z / subdivisions, v1 = (float)(z+1) / subdivisions;

            Vertex vA = { {x0,0,z0}, normal, Color::White, {u0,v0} };
            Vertex vB = { {x1,0,z0}, normal, Color::White, {u1,v0} };
            Vertex vC = { {x1,0,z1}, normal, Color::White, {u1,v1} };
            Vertex vD = { {x0,0,z1}, normal, Color::White, {u0,v1} };

            mesh.Triangles.push_back({ vA, vC, vB });
            mesh.Triangles.push_back({ vA, vD, vC });
        }
    }
    return mesh;
}

Mesh Primitives::CreateCube(float size) {
    Mesh mesh;
    float h = size / 2.0f;

    Vector3 pos[8] = {
        {-h,-h,-h}, { h,-h,-h}, { h, h,-h}, {-h, h,-h},
        {-h,-h, h}, { h,-h, h}, { h, h, h}, {-h, h, h}
    };
    int faces[6][4] = {
        {0,3,2,1},{4,5,6,7},{0,1,5,4},{3,7,6,2},{0,4,7,3},{1,2,6,5}
    };
    Vector3 norms[6] = {
        {0,0,-1},{0,0,1},{0,-1,0},{0,1,0},{-1,0,0},{1,0,0}
    };
    Vector2 uvs[4] = { {0,0},{1,0},{1,1},{0,1} };

    for (int i = 0; i < 6; i++) {
        Vertex v[4];
        for (int j = 0; j < 4; j++)
            v[j] = { pos[faces[i][j]], norms[i], Color::White, uvs[j] };
        mesh.Triangles.push_back({ v[0], v[1], v[2] });
        mesh.Triangles.push_back({ v[0], v[2], v[3] });
    }
    return mesh;
}

Mesh Primitives::CreatePyramid(float size, float height) {
    Mesh mesh;
    float h = size / 2.0f;

    Vector3 top     = { 0, height, 0 };
    Vector3 base[4] = { {-h,0,-h},{h,0,-h},{h,0,h},{-h,0,h} };

    Vertex b0 = { base[0], {0,-1,0}, Color::White, {0,0} };
    Vertex b1 = { base[1], {0,-1,0}, Color::White, {1,0} };
    Vertex b2 = { base[2], {0,-1,0}, Color::White, {1,1} };
    Vertex b3 = { base[3], {0,-1,0}, Color::White, {0,1} };
    mesh.Triangles.push_back({ b0, b1, b2 });
    mesh.Triangles.push_back({ b0, b2, b3 });

    for (int i = 0; i < 4; i++) {
        Vector3 p0 = base[i], p1 = base[(i+1)%4];
        Vector3 n  = Vector3::Cross(top - p0, p1 - p0).Normalized();
        mesh.Triangles.push_back({
            { p0,  n, Color::White, {0,0} },
            { p1,  n, Color::White, {1,0} },
            { top, n, Color::White, {0.5f,1} }
        });
    }
    return mesh;
}
