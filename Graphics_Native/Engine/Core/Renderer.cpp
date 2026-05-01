#include "Renderer.h"
#include "../Defaults/LightingCalculator.h"

void Renderer::RenderShadowMap(const Scene& scene, ShadowVS shadowVS, ShadowMap& shadowMap,
                                MeshSetup onMesh) {
    for (const auto& mesh : scene.Meshes) {
        if (onMesh) onMesh(mesh);
        for (const auto& tri : mesh.Triangles) {
            VertexOut vs0 = shadowVS(tri.V0);
            VertexOut vs1 = shadowVS(tri.V1);
            VertexOut vs2 = shadowVS(tri.V2);

            auto ndcToScreen = [&](const VertexOut& v, int& ox, int& oy) {
                ox = (int)((v.ClipPosition.X * 0.5f + 0.5f) * shadowMap.Width);
                oy = (int)((1.0f - (v.ClipPosition.Y * 0.5f + 0.5f)) * shadowMap.Height);
            };

            int x0,y0, x1,y1, x2,y2;
            ndcToScreen(vs0, x0, y0);
            ndcToScreen(vs1, x1, y1);
            ndcToScreen(vs2, x2, y2);

            RasterizeShadowTriangle(x0,y0, x1,y1, x2,y2,
                                    vs0.ClipPosition.Z,
                                    vs1.ClipPosition.Z,
                                    vs2.ClipPosition.Z,
                                    shadowMap);
        }
    }
}

void Renderer::RasterizeShadowTriangle(int x0, int y0, int x1, int y1, int x2, int y2,
                                        float z0, float z1, float z2, ShadowMap& sm) {
    float area = EdgeFn((float)x0,(float)y0, (float)x1,(float)y1, (float)x2,(float)y2);
    if (fabsf(area) < 1e-6f) return;
    float invArea = 1.0f / area;

    int minX = std::max(0,         std::min({x0,x1,x2}));
    int maxX = std::min(sm.Width  -1, std::max({x0,x1,x2}));
    int minY = std::max(0,         std::min({y0,y1,y2}));
    int maxY = std::min(sm.Height -1, std::max({y0,y1,y2}));

    float ex0 = (float)(y2-y1), ey0 = (float)(x1-x2);
    float ex1 = (float)(y0-y2), ey1 = (float)(x2-x0);
    float ex2 = (float)(y1-y0), ey2 = (float)(x0-x1);

    constexpr int tileSize = 32;
    int tileMinX = minX/tileSize, tileMaxX = maxX/tileSize;
    int tileMinY = minY/tileSize, tileMaxY = maxY/tileSize;
    int totalTiles = (tileMaxX - tileMinX + 1) * (tileMaxY - tileMinY + 1);

    int     smW  = sm.Width;
    float*  dbuf = sm.DepthBuffer.data();

    auto processRow = [&](int ty) {
        int startY = ty * tileSize, endY = std::min(startY + tileSize - 1, maxY);
        for (int tx = tileMinX; tx <= tileMaxX; tx++) {
            int startX = tx * tileSize, endX = std::min(startX + tileSize - 1, maxX);
            float w0r = EdgeFn((float)x1,(float)y1, (float)x2,(float)y2, startX+0.5f, startY+0.5f);
            float w1r = EdgeFn((float)x2,(float)y2, (float)x0,(float)y0, startX+0.5f, startY+0.5f);
            float w2r = EdgeFn((float)x0,(float)y0, (float)x1,(float)y1, startX+0.5f, startY+0.5f);
            for (int y = startY; y <= endY; y++) {
                float w0 = w0r, w1 = w1r, w2 = w2r;
                for (int x = startX; x <= endX; x++) {
                    if (w0 >= 0 && w1 >= 0 && w2 >= 0) {
                        float depth = (w0*z0 + w1*z1 + w2*z2) * invArea;
                        depth = depth * 0.5f + 0.5f;
                        int idx = x + y * smW;
                        if (depth < dbuf[idx]) dbuf[idx] = depth;
                    }
                    w0 += ex0; w1 += ex1; w2 += ex2;
                }
                w0r += ey0; w1r += ey1; w2r += ey2;
            }
        }
    };

    if (totalTiles > 4) {
        #pragma omp parallel for schedule(dynamic)
        for (int ty = tileMinY; ty <= tileMaxY; ty++) processRow(ty);
    } else {
        for (int ty = tileMinY; ty <= tileMaxY; ty++) processRow(ty);
    }
}
