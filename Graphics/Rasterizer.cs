using System;
using System.Collections.Generic;
using System.Numerics;

namespace GraphicsLibrary
{
    public class Rasterizer
    {
        private FrameBuffer _frameBuffer;

        private float[,] _depthBuffer;

        public bool EnableBackfaceCulling { get; set; } = true;

        public bool TwoSided = true;

        public Rasterizer(FrameBuffer frameBuffer, bool BackfaceCulling = true, bool TwoSideRendering = false)
        {
            _frameBuffer = frameBuffer;
            _depthBuffer = new float[_frameBuffer.Width, _frameBuffer.Height];
            EnableBackfaceCulling = BackfaceCulling;
            TwoSided = TwoSideRendering;
        }

        public void Clear(Color clearColor)
        {
            for (int y = 0; y < _frameBuffer.Height; y++)
            {
                for (int x = 0; x < _frameBuffer.Width; x++)
                {
                    _frameBuffer.ColorBuffer[x, y] = clearColor;
                    _depthBuffer[x, y] = float.MaxValue;
                }
            }
        }

        public void DrawScene(Scene scene, VertexShader vs, FragmentShader fs)
        {
            foreach (var mesh in scene.Meshes)
            {
                DrawMesh(mesh, vs, fs);
            }
        }
        
        public void DrawMesh(Mesh mesh, VertexShader vs, FragmentShader fs)
        {
            foreach (var tri in mesh.Triangles)
            {
                var v0 = vs(tri.V0);
                var v1 = vs(tri.V1);
                var v2 = vs(tri.V2);

                ClipAndRasterize(v0, v1, v2, fs);
            }
        }

        private void ClipAndRasterize(VertexOut v0, VertexOut v1, VertexOut v2, FragmentShader fs)
        {
            List<VertexOut> poly = new List<VertexOut> { v0, v1, v2 };

            for (int plane = 0; plane < 6; plane++)
            {
                poly = ClipPolygonAgainstPlane(poly, plane);
                if (poly.Count == 0) return;
            }

            for (int i = 1; i < poly.Count - 1; i++)
            {
                RasterizeTriangle(poly[0], poly[i], poly[i + 1], fs);
            }
        }

        private void RasterizeTriangle(VertexOut v0, VertexOut v1, VertexOut v2, FragmentShader fs)
        {
            Vector3 ndc0 = PerspectiveDivide(v0.ClipPosition);
            Vector3 ndc1 = PerspectiveDivide(v1.ClipPosition);
            Vector3 ndc2 = PerspectiveDivide(v2.ClipPosition);

            int x0 = ToScreenX(ndc0.X);
            int y0 = ToScreenY(ndc0.Y);
            int x1 = ToScreenX(ndc1.X);
            int y1 = ToScreenY(ndc1.Y);
            int x2 = ToScreenX(ndc2.X);
            int y2 = ToScreenY(ndc2.Y);

            float area = EdgeFunction(x0, y0, x1, y1, x2, y2);
            if (area == 0f) return;

            bool frontFace = true;
            if (EnableBackfaceCulling && !TwoSided && area >= 0f) return;
            if (TwoSided) frontFace = area < 0f;

            int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2)));
            int maxX = Math.Min(_frameBuffer.Width - 1, Math.Max(x0, Math.Max(x1, x2)));
            int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2)));
            int maxY = Math.Min(_frameBuffer.Height - 1, Math.Max(y0, Math.Max(y1, y2)));

            float invW0 = 1f / v0.ClipPosition.W;
            float invW1 = 1f / v1.ClipPosition.W;
            float invW2 = 1f / v2.ClipPosition.W;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float w0 = EdgeFunction(x1, y1, x2, y2, x, y) / area;
                    float w1 = EdgeFunction(x2, y2, x0, y0, x, y) / area;
                    float w2 = 1f - w0 - w1;

                    if (w0 < 0 || w1 < 0 || w2 < 0) continue;

                    float sum = w0 * invW0 + w1 * invW1 + w2 * invW2;
                    float W = 1f / sum;
                    w0 = w0 * invW0 * W;
                    w1 = w1 * invW1 * W;
                    w2 = w2 * invW2 * W;

                    Vector3 worldPos =
                        v0.WorldPosition * w0 +
                        v1.WorldPosition * w1 +
                        v2.WorldPosition * w2;

                    Vector3 normal =
                        (v0.Normal * w0 + v1.Normal * w1 + v2.Normal * w2).Normalized();

                    Vector3 colorVec =
                        new Vector3(
                            v0.Color.R * w0 + v1.Color.R * w1 + v2.Color.R * w2,
                            v0.Color.G * w0 + v1.Color.G * w1 + v2.Color.G * w2,
                            v0.Color.B * w0 + v1.Color.B * w1 + v2.Color.B * w2
                        );

                    Vector2 uv =
                        v0.UV * w0 +
                        v1.UV * w1 +
                        v2.UV * w2;

                    float depth =
                        v0.ClipPosition.Z * w0 +
                        v1.ClipPosition.Z * w1 +
                        v2.ClipPosition.Z * w2;

                    // 6️⃣ Convert depth to 0..1 (assuming OpenGL-style NDC -1..1)
                    depth = depth * 0.5f + 0.5f;

                    if (depth >= _depthBuffer[x, y]) continue;
                    _depthBuffer[x, y] = depth;

                    // 7️⃣ Build fragment input and shade
                    var fragInput = new FragmentIn
                    {
                        WorldPosition = worldPos,
                        Normal = normal,
                        Color = new Color(colorVec.X, colorVec.Y, colorVec.Z, 1f),
                        UV = uv,
                        FrontFace = frontFace
                    };

                    _frameBuffer.ColorBuffer[x, y] = fs(fragInput);
                }
            }
        }

        private List<VertexOut> ClipPolygonAgainstPlane(List<VertexOut> input, int plane)
        {
            List<VertexOut> output = new List<VertexOut>();

            for (int i = 0; i < input.Count; i++)
            {
                VertexOut current = input[i];
                VertexOut prev = input[(i - 1 + input.Count) % input.Count];

                bool currentInside = Inside(current.ClipPosition, plane);
                bool prevInside = Inside(prev.ClipPosition, plane);

                if (currentInside)
                {
                    if (!prevInside)
                        output.Add(Intersect(prev, current, plane));

                    output.Add(current);
                }
                else if (prevInside)
                {
                    output.Add(Intersect(prev, current, plane));
                }
            }

            return output;
        }

        private float EdgeFunction(int x0, int y0, int x1, int y1, int x2, int y2)
        {
            return (x2 - x0) * (y1 - y0) - (x1 - x0) * (y2 - y0);
        }

        private Vector3 PerspectiveDivide(Vector4 clip)
        {
            float invW = 1f / clip.W;
            return new Vector3(
                clip.X * invW,
                clip.Y * invW,
                clip.Z * invW
            );
        }

        private int ToScreenX(float ndcX)
        {
            return (int)((ndcX * 0.5f + 0.5f) * _frameBuffer.Width);
        }

        private int ToScreenY(float ndcY)
        {
            return (int)((1f - (ndcY * 0.5f + 0.5f)) * _frameBuffer.Height);
        }

        private bool Inside(Vector4 v, int plane)
        {
            switch (plane)
            {
                case 0: return v.X >= -v.W;
                case 1: return v.X <=  v.W;
                case 2: return v.Y >= -v.W;
                case 3: return v.Y <=  v.W;
                case 4: return v.Z >= -v.W;
                case 5: return v.Z <=  v.W; 
            }
            return false;
        }

        private VertexOut Intersect(VertexOut a, VertexOut b, int plane)
        {
            Vector4 A = a.ClipPosition;
            Vector4 B = b.ClipPosition;

            float da = DistanceToPlane(A, plane);
            float db = DistanceToPlane(B, plane);

            float t = da / (da - db);

            return LerpVertex(a, b, t);
        }

        private float DistanceToPlane(Vector4 v, int plane)
        {
            switch (plane)
            {
                case 0: return v.X + v.W;
                case 1: return v.W - v.X;
                case 2: return v.Y + v.W;
                case 3: return v.W - v.Y;
                case 4: return v.Z + v.W;
                case 5: return v.W - v.Z;
            }
            return 0f;
        }

        private VertexOut LerpVertex(VertexOut a, VertexOut b, float t)
        {
            return new VertexOut
            {
                ClipPosition = a.ClipPosition + (b.ClipPosition - a.ClipPosition) * t,
                WorldPosition = a.WorldPosition + (b.WorldPosition - a.WorldPosition) * t,
                Normal = (a.Normal + (b.Normal - a.Normal) * t).Normalized(),
                Color = new Color(
                    a.Color.R + (b.Color.R - a.Color.R) * t,
                    a.Color.G + (b.Color.G - a.Color.G) * t,
                    a.Color.B + (b.Color.B - a.Color.B) * t,
                    1f
                ),
                UV = a.UV + (b.UV - a.UV) * t
            };
        }

        public void RenderShadowMap(Scene scene, VertexShader shadowVS, ShadowMap shadowMap)
        {
            foreach (var mesh in scene.Meshes)
            {
                foreach (var tri in mesh.Triangles)
                {
                    var v0 = shadowVS(tri.V0);
                    var v1 = shadowVS(tri.V1);
                    var v2 = shadowVS(tri.V2);

                    int x0 = (int)((v0.ClipPosition.X * 0.5f + 0.5f) * shadowMap.Width);
                    int y0 = (int)((1f - (v0.ClipPosition.Y * 0.5f + 0.5f)) * shadowMap.Height);

                    int x1 = (int)((v1.ClipPosition.X * 0.5f + 0.5f) * shadowMap.Width);
                    int y1 = (int)((1f - (v1.ClipPosition.Y * 0.5f + 0.5f)) * shadowMap.Height);

                    int x2 = (int)((v2.ClipPosition.X * 0.5f + 0.5f) * shadowMap.Width);
                    int y2 = (int)((1f - (v2.ClipPosition.Y * 0.5f + 0.5f)) * shadowMap.Height);

                    RasterizeShadowTriangle(
                        x0, y0, x1, y1, x2, y2,
                        v0.ClipPosition.Z,
                        v1.ClipPosition.Z,
                        v2.ClipPosition.Z,
                        shadowMap
                    );
                }
            }
        }

        private void RasterizeShadowTriangle(
            int x0, int y0,
            int x1, int y1,
            int x2, int y2,
            float z0, float z1, float z2,
            ShadowMap shadowMap)
        {
            int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2)));
            int maxX = Math.Min(shadowMap.Width - 1, Math.Max(x0, Math.Max(x1, x2)));
            int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2)));
            int maxY = Math.Min(shadowMap.Height - 1, Math.Max(y0, Math.Max(y1, y2)));

            float area = EdgeFunction(x0, y0, x1, y1, x2, y2);
            if (area == 0f) return;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float w0 = EdgeFunction(x1,y1,x2,y2,x,y) / area;
                    float w1 = EdgeFunction(x2,y2,x0,y0,x,y) / area;
                    float w2 = EdgeFunction(x0,y0,x1,y1,x,y) / area;

                    if (w0 < 0 || w1 < 0 || w2 < 0)
                        continue;

                    float depth = (w0 * z0 + w1 * z1 + w2 * z2);
                    depth = depth * 0.5f + 0.5f;

                    if (depth < shadowMap.DepthBuffer[x, y])
                        shadowMap.DepthBuffer[x, y] = depth;
                }
            }
        }
    }
}