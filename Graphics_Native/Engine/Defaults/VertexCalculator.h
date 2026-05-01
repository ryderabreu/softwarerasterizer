#pragma once
#include "../Core/ShaderStructs.h"
#include "../Components/Camera.h"

struct VertexCalculator {
    // Optimized path: MVP pre-baked per mesh by the caller
    static VertexOut ProjectWithModel(const Vertex& input,
                                      const Matrix4x4& mvp,
                                      const Matrix4x4& model) {
        VertexOut r;
        r.ClipPosition  = mvp   * Vector4{input.Position.X, input.Position.Y, input.Position.Z, 1.0f};
        r.WorldPosition = model * input.Position;
        r.Normal        = input.Normal;
        r.Color         = input.Color;
        r.UV            = input.UV;
        return r;
    }

    static VertexOut Project(const Vertex& input, Camera& camera) {
        VertexOut r;
        r.ClipPosition  = camera.ViewProjectionMatrix() * Vector4{input.Position.X, input.Position.Y, input.Position.Z, 1.0f};
        r.WorldPosition = input.Position;
        r.Normal        = input.Normal;
        r.Color         = input.Color;
        r.UV            = input.UV;
        return r;
    }
};
