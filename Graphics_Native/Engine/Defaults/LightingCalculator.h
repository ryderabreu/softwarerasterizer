#pragma once
#include <algorithm>
#include "../Components/DirectionalLight.h"
#include "../Core/ShadowMap.h"
#include "../Core/ShaderStructs.h"
#include "../Core/Renderer.h"
#include "../Components/Scene.h"

class LightingCalculator {
public:
    DirectionalLight light;
    ShadowMap&       shadowMap;
    Matrix4x4        lightMatrix;
    float            lightAmbient;
    float            shadowAmbient;
    float            bias;
    bool             ShadowMapDirty = true;

    LightingCalculator(DirectionalLight light, ShadowMap& shadowmap, Vector3 viewpoint,
                       float perspectivesize = 10.0f, float lightambient = 0.1f,
                       float shadowambient = 0.1f, float b = 0.01f)
        : light(light), shadowMap(shadowmap)
        , lightAmbient(lightambient), shadowAmbient(shadowambient), bias(b)
    {
        lightMatrix = light.LightMatrix(perspectivesize, viewpoint);
    }

    void InvalidateShadow() { ShadowMapDirty = true; }

    Color Calculate(Vector3 worldPos, Color color, Vector3 normal,
                    bool frontfacing = true, bool shadows = true,
                    bool frontOnlyShadows = true) const {
        if (shadows && (!frontOnlyShadows || frontfacing)) {
            Vector4 ls = lightMatrix * Vector4{worldPos.X, worldPos.Y, worldPos.Z, 1.0f};
            int sx = (int)((ls.X * 0.5f + 0.5f) * shadowMap.Width);
            int sy = (int)((1.0f - (ls.Y * 0.5f + 0.5f)) * shadowMap.Height);
            if (sx >= 0 && sx < shadowMap.Width &&
                sy >= 0 && sy < shadowMap.Height) {
                float depth = ls.Z * 0.5f + 0.5f;
                if (depth > shadowMap.DepthBuffer[sx + sy * shadowMap.Width] + bias)
                    return color * shadowAmbient;
            }
        }
        return light.GetColor(normal, color, lightAmbient);
    }

    VertexOut ShadowVertexShader(const VertexOut& input) const {
        VertexOut r{};
        r.ClipPosition = lightMatrix * Vector4{input.WorldPosition.X,
                                               input.WorldPosition.Y,
                                               input.WorldPosition.Z, 1.0f};
        return r;
    }

    template<typename Shaders>
    void ShadowRasterize(const Scene& scene) {
        if (!ShadowMapDirty) return;
        shadowMap.Clear();
        Renderer::RenderShadowMap(scene,
            [this](const Vertex& v) -> VertexOut {
                return ShadowVertexShader(Shaders::VertexShader(v));
            }, shadowMap,
            [](const Mesh& mesh) {
                Shaders::model = mesh.model;
            });
        ShadowMapDirty = false;
    }
};

// RenderWithShadows template body lives here where both Renderer and
// LightingCalculator are fully defined, breaking the circular dependency.
template<typename Shaders>
void Renderer::RenderWithShadows(const Scene& scene, LightingCalculator& lightCalc) {
    lightCalc.ShadowRasterize<Shaders>(scene);
    Clear(Color::Black);
    DrawScene<Shaders>(scene);
}
