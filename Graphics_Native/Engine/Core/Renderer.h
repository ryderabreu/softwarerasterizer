#pragma once
#include <cmath>
#include <algorithm>
#include <vector>
#include <functional>
#include <limits>
#include <immintrin.h>
#include "FrameBuffer.h"
#include "ShadowMap.h"
#include "ShaderStructs.h"
#include "../Components/Scene.h"

using ShadowVS   = std::function<VertexOut(const Vertex&)>;
using MeshSetup  = std::function<void(const Mesh&)>;

class Renderer {
public:
    bool EnableBackfaceCulling = true;
    bool TwoSided              = false;

    explicit Renderer(FrameBuffer& fb)
        : _fb(fb), _depth(fb.Width * fb.Height, std::numeric_limits<float>::max()) {}

    void Clear(Color clearColor) {
        std::fill(_fb.ColorBuffer.begin(), _fb.ColorBuffer.end(), clearColor);
        std::fill(_depth.begin(), _depth.end(), std::numeric_limits<float>::max());
    }

    template<typename Shaders>
    void DrawScene(const Scene& scene) {
        for (const auto& mesh : scene.Meshes)
            DrawMesh<Shaders>(const_cast<Mesh&>(mesh));
    }

    template<typename Shaders>
    void DrawMesh(Mesh& mesh) {
        Shaders::texture = mesh.texture;
        Shaders::model   = mesh.model;
        Shaders::mvp     = _vpCache * mesh.model;

        for (const auto& tri : mesh.Triangles) {
            auto v0 = Shaders::VertexShader(tri.V0);
            auto v1 = Shaders::VertexShader(tri.V1);
            auto v2 = Shaders::VertexShader(tri.V2);
            ClipAndRasterize<Shaders>(v0, v1, v2);
        }
    }

    void SetViewProjection(const Matrix4x4& vp) { _vpCache = vp; }

    static void RenderShadowMap(const Scene& scene, ShadowVS shadowVS, ShadowMap& shadowMap,
                                MeshSetup onMesh = nullptr);

    template<typename Shaders>
    void RenderWithShadows(const Scene& scene,
                           class LightingCalculator& lightCalc);

private:
    FrameBuffer&       _fb;
    std::vector<float> _depth;
    Matrix4x4          _vpCache;

    template<typename Shaders>
    void ClipAndRasterize(VertexOut v0, VertexOut v1, VertexOut v2);

    template<typename Shaders>
    void RasterizeTriangle(const VertexOut& v0, const VertexOut& v1, const VertexOut& v2);

    static float EdgeFn(float x0, float y0, float x1, float y1, float x2, float y2) {
        return (x2 - x0) * (y1 - y0) - (x1 - x0) * (y2 - y0);
    }

    static bool Inside(const Vector4& v, int plane) {
        switch (plane) {
            case 0: return v.X >= -v.W;
            case 1: return v.X <=  v.W;
            case 2: return v.Y >= -v.W;
            case 3: return v.Y <=  v.W;
            case 4: return v.Z >= -v.W;
            case 5: return v.Z <=  v.W;
        }
        return false;
    }

    static float DistToPlane(const Vector4& v, int plane) {
        switch (plane) {
            case 0: return v.X + v.W;
            case 1: return v.W - v.X;
            case 2: return v.Y + v.W;
            case 3: return v.W - v.Y;
            case 4: return v.Z + v.W;
            case 5: return v.W - v.Z;
        }
        return 0.0f;
    }

    static VertexOut LerpVtx(const VertexOut& a, const VertexOut& b, float t) {
        auto lerp = [t](float a, float b) { return a + (b - a) * t; };
        VertexOut r;
        r.ClipPosition  = a.ClipPosition  + (b.ClipPosition  - a.ClipPosition)  * t;
        r.WorldPosition = a.WorldPosition + (b.WorldPosition - a.WorldPosition) * t;
        float nx = lerp(a.Normal.X, b.Normal.X),
              ny = lerp(a.Normal.Y, b.Normal.Y),
              nz = lerp(a.Normal.Z, b.Normal.Z);
        float invN = 1.0f / sqrtf(nx*nx + ny*ny + nz*nz);
        r.Normal = { nx*invN, ny*invN, nz*invN };
        r.Color  = { lerp(a.Color.R, b.Color.R), lerp(a.Color.G, b.Color.G),
                     lerp(a.Color.B, b.Color.B), 1.0f };
        r.UV = a.UV + (b.UV - a.UV) * t;
        return r;
    }

    static VertexOut Intersect(const VertexOut& a, const VertexOut& b, int plane) {
        float da = DistToPlane(a.ClipPosition, plane);
        float db = DistToPlane(b.ClipPosition, plane);
        return LerpVtx(a, b, da / (da - db));
    }

    static void RasterizeShadowTriangle(int x0, int y0, int x1, int y1, int x2, int y2,
                                         float z0, float z1, float z2, ShadowMap& sm);
};

#include "Renderer.inl"
