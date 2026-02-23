using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphicsLibrary
{
    public interface Shader {
        public static abstract Texture texture { get; set; }
        public static abstract Matrix4x4 model { get; set; }

        public static abstract VertexOut VertexShader(Vertex v);
        public static abstract Color FragmentShader(FragmentIn v);
    }

    public class Renderer
    {
        private FrameBuffer _frameBuffer;

        private float[] _depthBuffer;

        public bool EnableBackfaceCulling { get; set; } = true;

        public bool TwoSided = true;

        public Renderer(FrameBuffer frameBuffer, bool BackfaceCulling = true, bool TwoSideRendering = false)
        {
            _frameBuffer = frameBuffer;
            _depthBuffer = new float[_frameBuffer.Width * _frameBuffer.Height];
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
                    _depthBuffer[x + y * _frameBuffer.Width] = float.MaxValue;
                }
            }
        }

        public void DrawScene<Shaders>(Scene scene) where Shaders : Shader
        {
            foreach (var mesh in scene.Meshes)
            {
                DrawMesh<Shaders>(mesh);
            }
        }
        
        public void DrawMesh<Shaders>(Mesh mesh) where Shaders : Shader
        {
            Shaders.texture = mesh.texture;
            Shaders.model = mesh.model;

            foreach (var tri in mesh.Triangles)
            {
                var v0 = Shaders.VertexShader(tri.V0);
                var v1 = Shaders.VertexShader(tri.V1);
                var v2 = Shaders.VertexShader(tri.V2);

                ClipAndRasterize<Shaders>(v0, v1, v2);
            }
        }

        private void ClipAndRasterize<Shaders>(VertexOut v0, VertexOut v1, VertexOut v2)
            where Shaders : Shader
        {
            Span<VertexOut> bufferA = stackalloc VertexOut[12];
            Span<VertexOut> bufferB = stackalloc VertexOut[12];

            int countA = 3;
            bufferA[0] = v0;
            bufferA[1] = v1;
            bufferA[2] = v2;

            Span<VertexOut> input = bufferA;
            Span<VertexOut> output = bufferB;
            int inputCount = countA;

            for (int plane = 0; plane < 6; plane++)
            {
                if (inputCount == 0)
                    return;

                int outputCount = 0;

                VertexOut prev = input[inputCount - 1];
                bool prevInside = Inside(prev.ClipPosition, plane);

                for (int i = 0; i < inputCount; i++)
                {
                    VertexOut current = input[i];
                    bool currentInside = Inside(current.ClipPosition, plane);

                    if (currentInside)
                    {
                        if (!prevInside)
                        {
                            output[outputCount++] = Intersect(prev, current, plane);
                        }

                        output[outputCount++] = current;
                    }
                    else if (prevInside)
                    {
                        output[outputCount++] = Intersect(prev, current, plane);
                    }

                    prev = current;
                    prevInside = currentInside;
                }

                var temp = input;
                input = output;
                output = temp;

                inputCount = outputCount;
            }

            for (int i = 1; i < inputCount - 1; i++)
            {
                RasterizeTriangle<Shaders>(input[0], input[i], input[i + 1]);
            }
        }

        private void RasterizeTriangle<Shaders>(VertexOut v0, VertexOut v1, VertexOut v2) where Shaders : Shader
        {
            float invW0 = 1f / v0.ClipPosition.W;
            float invW1 = 1f / v1.ClipPosition.W;
            float invW2 = 1f / v2.ClipPosition.W;

            Vector3 ndc0 = new Vector3(v0.ClipPosition.X * invW0, v0.ClipPosition.Y * invW0, v0.ClipPosition.Z * invW0);
            Vector3 ndc1 = new Vector3(v1.ClipPosition.X * invW1, v1.ClipPosition.Y * invW1, v1.ClipPosition.Z * invW1);
            Vector3 ndc2 = new Vector3(v2.ClipPosition.X * invW2, v2.ClipPosition.Y * invW2, v2.ClipPosition.Z * invW2);

            float fx0 = (ndc0.X * 0.5f + 0.5f) * _frameBuffer.Width;
            float fy0 = (1f - (ndc0.Y * 0.5f + 0.5f)) * _frameBuffer.Height;
            float fx1 = (ndc1.X * 0.5f + 0.5f) * _frameBuffer.Width;
            float fy1 = (1f - (ndc1.Y * 0.5f + 0.5f)) * _frameBuffer.Height;
            float fx2 = (ndc2.X * 0.5f + 0.5f) * _frameBuffer.Width;
            float fy2 = (1f - (ndc2.Y * 0.5f + 0.5f)) * _frameBuffer.Height;

            float area = EdgeFunction(fx0, fy0, fx1, fy1, fx2, fy2);
            if (MathF.Abs(area) < 1e-6f) return;

            bool frontFace = area > 0f;
            if (EnableBackfaceCulling && !TwoSided && !frontFace) return;

            float invArea = 1f / area;

            int minX = Math.Max(0, (int)MathF.Floor(MathF.Min(fx0, MathF.Min(fx1, fx2))));
            int maxX = Math.Min(_frameBuffer.Width - 1, (int)MathF.Ceiling(MathF.Max(fx0, MathF.Max(fx1, fx2))));
            int minY = Math.Max(0, (int)MathF.Floor(MathF.Min(fy0, MathF.Min(fy1, fy2))));
            int maxY = Math.Min(_frameBuffer.Height - 1, (int)MathF.Ceiling(MathF.Max(fy0, MathF.Max(fy1, fy2))));

            float ex0 = fy2 - fy1, ey0 = fx1 - fx2;
            float ex1 = fy0 - fy2, ey1 = fx2 - fx0;
            float ex2 = fy1 - fy0, ey2 = fx0 - fx1;

            const int tileSize = 32;
            int tileMinX = minX / tileSize;
            int tileMaxX = maxX / tileSize;
            int tileMinY = minY / tileSize;
            int tileMaxY = maxY / tileSize;

            Parallel.For(tileMinY, tileMaxY + 1, ty =>
            {
                int startY = ty * tileSize;
                int endY = Math.Min(startY + tileSize - 1, maxY);

                for (int tx = tileMinX; tx <= tileMaxX; tx++)
                {
                    int startX = tx * tileSize;
                    int endX = Math.Min(startX + tileSize - 1, maxX);

                    float w0_row = EdgeFunction(fx1, fy1, fx2, fy2, startX + 0.5f, startY + 0.5f);
                    float w1_row = EdgeFunction(fx2, fy2, fx0, fy0, startX + 0.5f, startY + 0.5f);
                    float w2_row = EdgeFunction(fx0, fy0, fx1, fy1, startX + 0.5f, startY + 0.5f);

                    for (int y = startY; y <= endY; y++)
                    {
                        float w0 = w0_row;
                        float w1 = w1_row;
                        float w2 = w2_row;

                        for (int x = startX; x <= endX; x++)
                        {
                            if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                            {
                                float sum = w0 * invW0 + w1 * invW1 + w2 * invW2;
                                float W = 1f / sum;
                                float b0 = w0 * invW0 * W;
                                float b1 = w1 * invW1 * W;
                                float b2 = w2 * invW2 * W;

                                float depth = ndc0.Z * b0 + ndc1.Z * b1 + ndc2.Z * b2;
                                depth = depth * 0.5f + 0.5f;
                                int idx = x + y * _frameBuffer.Width;
                                ref float depthRef = ref _depthBuffer[idx];

                                if (depth < depthRef)
                                {
                                    depthRef = depth;

                                    Vector3 worldPos = v0.WorldPosition * b0 + v1.WorldPosition * b1 + v2.WorldPosition * b2;
                                    Vector3 normal = (v0.Normal * b0 + v1.Normal * b1 + v2.Normal * b2).Normalized();
                                    Vector3 colorVec = v0.Color.ToVector3() * b0 + v1.Color.ToVector3() * b1 + v2.Color.ToVector3() * b2;
                                    Vector2 uv = (v0.UV * invW0 * w0 + v1.UV * invW1 * w1 + v2.UV * invW2 * w2) * W;

                                    var fragInput = new FragmentIn
                                    {
                                        WorldPosition = worldPos,
                                        Normal = normal,
                                        Color = new Color(colorVec.X, colorVec.Y, colorVec.Z, 1f),
                                        UV = uv,
                                        FrontFace = frontFace
                                    };

                                    _frameBuffer.ColorBuffer[x, y] = Shaders.FragmentShader(fragInput);
                                }
                            }

                            w0 += ex0;
                            w1 += ex1;
                            w2 += ex2;
                        }

                        w0_row += ey0;
                        w1_row += ey1;
                        w2_row += ey2;
                    }
                }
            });
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

        private static float EdgeFunction(float x0, float y0, float x1, float y1, float x2, float y2)
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

        public static void RenderShadowMap<Shaders>(Scene scene, VertexShader shadowVS, ShadowMap shadowMap) where Shaders : Shader
        {
            foreach (var mesh in scene.Meshes)
            {  
                Shaders.texture = mesh.texture;
                Shaders.model = mesh.model;
                
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

        private static void RasterizeShadowTriangle(
            int x0, int y0,
            int x1, int y1,
            int x2, int y2,
            float z0, float z1, float z2,
            ShadowMap shadowMap)
        {
            float area = EdgeFunction(x0, y0, x1, y1, x2, y2);
            if (MathF.Abs(area) < 1e-6f) return;
            float invArea = 1f / area;

            int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2)));
            int maxX = Math.Min(shadowMap.Width - 1, Math.Max(x0, Math.Max(x1, x2)));
            int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2)));
            int maxY = Math.Min(shadowMap.Height - 1, Math.Max(y0, Math.Max(y1, y2)));

            const int tileSize = 32;
            int tileMinX = minX / tileSize;
            int tileMaxX = maxX / tileSize;
            int tileMinY = minY / tileSize;
            int tileMaxY = maxY / tileSize;

            float ex0 = y2 - y1, ey0 = x1 - x2;
            float ex1 = y0 - y2, ey1 = x2 - x0;
            float ex2 = y1 - y0, ey2 = x0 - x1;

            Parallel.For(tileMinY, tileMaxY + 1, ty =>
            {
                int startY = ty * tileSize;
                int endY = Math.Min(startY + tileSize - 1, maxY);

                for (int tx = tileMinX; tx <= tileMaxX; tx++)
                {
                    int startX = tx * tileSize;
                    int endX = Math.Min(startX + tileSize - 1, maxX);

                    float w0_row = EdgeFunction(x1, y1, x2, y2, startX + 0.5f, startY + 0.5f);
                    float w1_row = EdgeFunction(x2, y2, x0, y0, startX + 0.5f, startY + 0.5f);
                    float w2_row = EdgeFunction(x0, y0, x1, y1, startX + 0.5f, startY + 0.5f);

                    for (int y = startY; y <= endY; y++)
                    {
                        float w0 = w0_row;
                        float w1 = w1_row;
                        float w2 = w2_row;

                        for (int x = startX; x <= endX; x++)
                        {
                            if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                            {
                                float depth = (w0 * z0 + w1 * z1 + w2 * z2) * invArea;
                                depth = depth * 0.5f + 0.5f;

                                int idx = x + y * shadowMap.Width;
                                ref float depthRef = ref shadowMap.DepthBuffer[idx];
                                if (depth < depthRef)
                                {
                                    depthRef = depth;
                                }
                            }

                            w0 += ex0;
                            w1 += ex1;
                            w2 += ex2;
                        }

                        w0_row += ey0;
                        w1_row += ey1;
                        w2_row += ey2;
                    }
                }
            });
        }

        public void RenderWithShadows<Shaders>(Scene scene, LightingCalculator lightCalc) where Shaders : Shader
        {
            lightCalc.ShadowRasterize<Shaders>(scene);
            Clear(Color.Black);
            DrawScene<Shaders>(scene);
        }
    }
}