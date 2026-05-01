#pragma once
#include "../Core/ShaderStructs.h"
#include "../Objects/Texture.h"
#include "../Math/Matrix4x4.h"
#include "VertexCalculator.h"
#include "LightingCalculator.h"

// Globals defined in main.cpp — mirrors Program.camera / Program.lightCalc
extern Camera*             g_camera;
extern LightingCalculator* g_lightCalc;

struct DefaultShaders {
    static inline Texture*  texture = nullptr;
    static inline Matrix4x4 model;
    static inline Matrix4x4 mvp;

    static VertexOut VertexShader(const Vertex& input) {
        return VertexCalculator::ProjectWithModel(input, mvp, model);
    }

    static Color FragmentShader(const FragmentIn& input) {
        return g_lightCalc->Calculate(
            input.WorldPosition,
            g_lightCalc->light.LightColor,    // base color comes from texture sample
            input.Normal,
            input.FrontFace,
            true, true
        );
    }
};

// Texture-aware default fragment shader — use this instead of DefaultShaders
// when meshes carry per-pixel texture colours (the usual case)
struct DefaultTexturedShaders {
    static inline Texture*  texture = nullptr;
    static inline Matrix4x4 model;
    static inline Matrix4x4 mvp;

    static VertexOut VertexShader(const Vertex& input) {
        return VertexCalculator::ProjectWithModel(input, mvp, model);
    }

    static Color FragmentShader(const FragmentIn& input) {
        Color texColor = texture ? texture->Sample(input.UV) : Color::White;
        return texColor * g_lightCalc->Calculate(
            input.WorldPosition,
            input.Color,
            input.Normal,
            input.FrontFace,
            true, true
        );
    }
};
