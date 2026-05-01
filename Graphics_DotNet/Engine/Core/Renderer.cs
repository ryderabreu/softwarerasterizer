using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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
            Array.Fill(_frameBuffer.ColorBuffer, clearColor);
            Array.Fill(_depthBuffer, float.MaxValue);
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

            int fbWidth = _frameBuffer.Width;
            var colorBuffer = _frameBuffer.ColorBuffer;
            var depthBuffer = _depthBuffer;

            // Pre-extract vertex attributes once per triangle — constant for every pixel
            float wp0x = v0.WorldPosition.X, wp0y = v0.WorldPosition.Y, wp0z = v0.WorldPosition.Z;
            float wp1x = v1.WorldPosition.X, wp1y = v1.WorldPosition.Y, wp1z = v1.WorldPosition.Z;
            float wp2x = v2.WorldPosition.X, wp2y = v2.WorldPosition.Y, wp2z = v2.WorldPosition.Z;
            float n0x = v0.Normal.X, n0y = v0.Normal.Y, n0z = v0.Normal.Z;
            float n1x = v1.Normal.X, n1y = v1.Normal.Y, n1z = v1.Normal.Z;
            float n2x = v2.Normal.X, n2y = v2.Normal.Y, n2z = v2.Normal.Z;
            float c0r = v0.Color.R, c0g = v0.Color.G, c0b = v0.Color.B;
            float c1r = v1.Color.R, c1g = v1.Color.G, c1b = v1.Color.B;
            float c2r = v2.Color.R, c2g = v2.Color.G, c2b = v2.Color.B;
            float uv0x = v0.UV.X, uv0y = v0.UV.Y;
            float uv1x = v1.UV.X, uv1y = v1.UV.Y;
            float uv2x = v2.UV.X, uv2y = v2.UV.Y;

            // Per-triangle SIMD constants (broadcast scalars into 8-wide vectors)
            var ex0Steps = Vector256.Create(ex0) * Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
            var ex1Steps = Vector256.Create(ex1) * Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
            var ex2Steps = Vector256.Create(ex2) * Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
            var invW0V   = Vector256.Create(invW0);
            var invW1V   = Vector256.Create(invW1);
            var invW2V   = Vector256.Create(invW2);
            var ndc0zV   = Vector256.Create(ndc0.Z);
            var ndc1zV   = Vector256.Create(ndc1.Z);
            var ndc2zV   = Vector256.Create(ndc2.Z);
            var zeroV    = Vector256<float>.Zero;
            var halfV    = Vector256.Create(0.5f);
            var oneV     = Vector256.Create(1f);

            void ProcessTileRow(int ty)
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
                        float w0 = w0_row, w1 = w1_row, w2 = w2_row;
                        int x = startX;

                        // SIMD path: process 8 pixels per iteration
                        if (Avx.IsSupported)
                        {
                            for (; x <= endX - 7; x += 8)
                            {
                                // Weights for pixels x, x+1, ..., x+7
                                var w0V = Vector256.Create(w0) + ex0Steps;
                                var w1V = Vector256.Create(w1) + ex1Steps;
                                var w2V = Vector256.Create(w2) + ex2Steps;

                                // Inside test: all three barycentric weights >= 0
                                int insideBits = Avx.MoveMask(Avx.And(Avx.And(
                                    Avx.Compare(w0V, zeroV, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling),
                                    Avx.Compare(w1V, zeroV, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling)),
                                    Avx.Compare(w2V, zeroV, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling)));

                                if (insideBits != 0)
                                {
                                    // Perspective-correct barycentrics for all 8
                                    var bw0  = w0V * invW0V;
                                    var bw1  = w1V * invW1V;
                                    var bw2  = w2V * invW2V;
                                    var WV   = oneV / (bw0 + bw1 + bw2);
                                    var b0V  = bw0 * WV;
                                    var b1V  = bw1 * WV;
                                    var b2V  = bw2 * WV;

                                    // Depth for all 8
                                    var depthV = halfV + halfV * (ndc0zV * b0V + ndc1zV * b1V + ndc2zV * b2V);

                                    // Depth test against stored values
                                    int baseIdx = x + y * fbWidth;
                                    var stored  = Vector256.LoadUnsafe(ref depthBuffer[baseIdx]);
                                    int surviveBits = insideBits & Avx.MoveMask(
                                        Avx.Compare(depthV, stored, FloatComparisonMode.OrderedLessThanNonSignaling));

                                    // Shade only pixels that survive both tests
                                    while (surviveBits != 0)
                                    {
                                        int bit = BitOperations.TrailingZeroCount(surviveBits);
                                        float b0 = b0V.GetElement(bit);
                                        float b1 = b1V.GetElement(bit);
                                        float b2 = b2V.GetElement(bit);
                                        int idx  = baseIdx + bit;

                                        depthBuffer[idx] = depthV.GetElement(bit);

                                        Vector3 worldPos = new Vector3(wp0x*b0 + wp1x*b1 + wp2x*b2, wp0y*b0 + wp1y*b1 + wp2y*b2, wp0z*b0 + wp1z*b1 + wp2z*b2);
                                        float   nx = n0x*b0 + n1x*b1 + n2x*b2, ny = n0y*b0 + n1y*b1 + n2y*b2, nz = n0z*b0 + n1z*b1 + n2z*b2;
                                        float   invNLen = 1f / MathF.Sqrt(nx*nx + ny*ny + nz*nz);
                                        Vector3 normal   = new Vector3(nx * invNLen, ny * invNLen, nz * invNLen);
                                        Vector2 uv       = new Vector2(uv0x*b0 + uv1x*b1 + uv2x*b2, uv0y*b0 + uv1y*b1 + uv2y*b2);

                                        colorBuffer[idx] = Shaders.FragmentShader(new FragmentIn
                                        {
                                            WorldPosition = worldPos,
                                            Normal        = normal,
                                            Color         = new Color(c0r*b0 + c1r*b1 + c2r*b2, c0g*b0 + c1g*b1 + c2g*b2, c0b*b0 + c1b*b1 + c2b*b2, 1f),
                                            UV            = uv,
                                            FrontFace     = frontFace
                                        });

                                        surviveBits &= surviveBits - 1;
                                    }
                                }

                                w0 += ex0 * 8;
                                w1 += ex1 * 8;
                                w2 += ex2 * 8;
                            }
                        }

                        // Scalar path: full fallback or tail pixels after last SIMD block
                        for (; x <= endX; x++)
                        {
                            if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                            {
                                float sum = w0 * invW0 + w1 * invW1 + w2 * invW2;
                                float W   = 1f / sum;
                                float b0  = w0 * invW0 * W;
                                float b1  = w1 * invW1 * W;
                                float b2  = w2 * invW2 * W;

                                float depth = ndc0.Z * b0 + ndc1.Z * b1 + ndc2.Z * b2;
                                depth = depth * 0.5f + 0.5f;
                                int idx = x + y * fbWidth;
                                ref float depthRef = ref depthBuffer[idx];

                                if (depth < depthRef)
                                {
                                    depthRef = depth;

                                    Vector3 worldPos = new Vector3(wp0x*b0 + wp1x*b1 + wp2x*b2, wp0y*b0 + wp1y*b1 + wp2y*b2, wp0z*b0 + wp1z*b1 + wp2z*b2);
                                    float   nx = n0x*b0 + n1x*b1 + n2x*b2, ny = n0y*b0 + n1y*b1 + n2y*b2, nz = n0z*b0 + n1z*b1 + n2z*b2;
                                    float   invNLen = 1f / MathF.Sqrt(nx*nx + ny*ny + nz*nz);
                                    Vector3 normal   = new Vector3(nx * invNLen, ny * invNLen, nz * invNLen);
                                    Vector2 uv       = new Vector2(uv0x*b0 + uv1x*b1 + uv2x*b2, uv0y*b0 + uv1y*b1 + uv2y*b2);

                                    colorBuffer[idx] = Shaders.FragmentShader(new FragmentIn
                                    {
                                        WorldPosition = worldPos,
                                        Normal        = normal,
                                        Color         = new Color(c0r*b0 + c1r*b1 + c2r*b2, c0g*b0 + c1g*b1 + c2g*b2, c0b*b0 + c1b*b1 + c2b*b2, 1f),
                                        UV            = uv,
                                        FrontFace     = frontFace
                                    });
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
            }

            int totalTiles = (tileMaxX - tileMinX + 1) * (tileMaxY - tileMinY + 1);
            if (totalTiles > 4)
                Parallel.For(tileMinY, tileMaxY + 1, ProcessTileRow);
            else
                for (int ty = tileMinY; ty <= tileMaxY; ty++) ProcessTileRow(ty);
        }

        private static float EdgeFunction(float x0, float y0, float x1, float y1, float x2, float y2)
        {
            return (x2 - x0) * (y1 - y0) - (x1 - x0) * (y2 - y0);
        }

        private static bool Inside(Vector4 v, int plane) => plane switch
        {
            0 => v.X >= -v.W,
            1 => v.X <=  v.W,
            2 => v.Y >= -v.W,
            3 => v.Y <=  v.W,
            4 => v.Z >= -v.W,
            5 => v.Z <=  v.W,
            _ => false
        };

        private static VertexOut Intersect(VertexOut a, VertexOut b, int plane)
        {
            float da = DistanceToPlane(a.ClipPosition, plane);
            float db = DistanceToPlane(b.ClipPosition, plane);
            return LerpVertex(a, b, da / (da - db));
        }

        private static float DistanceToPlane(Vector4 v, int plane) => plane switch
        {
            0 => v.X + v.W,
            1 => v.W - v.X,
            2 => v.Y + v.W,
            3 => v.W - v.Y,
            4 => v.Z + v.W,
            5 => v.W - v.Z,
            _ => 0f
        };

        private static VertexOut LerpVertex(VertexOut a, VertexOut b, float t)
        {
            return new VertexOut
            {
                ClipPosition  = a.ClipPosition  + (b.ClipPosition  - a.ClipPosition)  * t,
                WorldPosition = a.WorldPosition + (b.WorldPosition - a.WorldPosition) * t,
                Normal        = (a.Normal + (b.Normal - a.Normal) * t).Normalized(),
                Color = new Color(
                    a.Color.R + (b.Color.R - a.Color.R) * t,
                    a.Color.G + (b.Color.G - a.Color.G) * t,
                    a.Color.B + (b.Color.B - a.Color.B) * t,
                    1f),
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
                        v0.ClipPosition.Z, v1.ClipPosition.Z, v2.ClipPosition.Z,
                        shadowMap);
                }
            }
        }

        private static void RasterizeShadowTriangle(
            int x0, int y0, int x1, int y1, int x2, int y2,
            float z0, float z1, float z2, ShadowMap shadowMap)
        {
            float area = EdgeFunction(x0, y0, x1, y1, x2, y2);
            if (MathF.Abs(area) < 1e-6f) return;
            float invArea = 1f / area;

            int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2)));
            int maxX = Math.Min(shadowMap.Width  - 1, Math.Max(x0, Math.Max(x1, x2)));
            int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2)));
            int maxY = Math.Min(shadowMap.Height - 1, Math.Max(y0, Math.Max(y1, y2)));

            const int tileSize = 32;
            int tileMinX = minX / tileSize, tileMaxX = maxX / tileSize;
            int tileMinY = minY / tileSize, tileMaxY = maxY / tileSize;

            float ex0 = y2 - y1, ey0 = x1 - x2;
            float ex1 = y0 - y2, ey1 = x2 - x0;
            float ex2 = y1 - y0, ey2 = x0 - x1;

            int smWidth  = shadowMap.Width;
            var depthBuf = shadowMap.DepthBuffer;

            // Per-triangle SIMD constants
            var ex0Steps  = Vector256.Create(ex0) * Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
            var ex1Steps  = Vector256.Create(ex1) * Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
            var ex2Steps  = Vector256.Create(ex2) * Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
            var z0V       = Vector256.Create(z0);
            var z1V       = Vector256.Create(z1);
            var z2V       = Vector256.Create(z2);
            var invAreaV  = Vector256.Create(invArea);
            var zeroV     = Vector256<float>.Zero;
            var halfV     = Vector256.Create(0.5f);

            void ProcessShadowRow(int ty)
            {
                int startY = ty * tileSize;
                int endY   = Math.Min(startY + tileSize - 1, maxY);

                for (int tx = tileMinX; tx <= tileMaxX; tx++)
                {
                    int startX = tx * tileSize;
                    int endX   = Math.Min(startX + tileSize - 1, maxX);

                    float w0_row = EdgeFunction(x1, y1, x2, y2, startX + 0.5f, startY + 0.5f);
                    float w1_row = EdgeFunction(x2, y2, x0, y0, startX + 0.5f, startY + 0.5f);
                    float w2_row = EdgeFunction(x0, y0, x1, y1, startX + 0.5f, startY + 0.5f);

                    for (int y = startY; y <= endY; y++)
                    {
                        float w0 = w0_row, w1 = w1_row, w2 = w2_row;
                        int x = startX;

                        if (Avx.IsSupported)
                        {
                            for (; x <= endX - 7; x += 8)
                            {
                                var w0V = Vector256.Create(w0) + ex0Steps;
                                var w1V = Vector256.Create(w1) + ex1Steps;
                                var w2V = Vector256.Create(w2) + ex2Steps;

                                int insideBits = Avx.MoveMask(Avx.And(Avx.And(
                                    Avx.Compare(w0V, zeroV, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling),
                                    Avx.Compare(w1V, zeroV, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling)),
                                    Avx.Compare(w2V, zeroV, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling)));

                                if (insideBits != 0)
                                {
                                    // Shadow depth: affine interpolation (orthographic projection)
                                    var depthV = halfV + halfV * (z0V * w0V + z1V * w1V + z2V * w2V) * invAreaV;

                                    int baseIdx = x + y * smWidth;
                                    var stored  = Vector256.LoadUnsafe(ref depthBuf[baseIdx]);
                                    int surviveBits = insideBits & Avx.MoveMask(
                                        Avx.Compare(depthV, stored, FloatComparisonMode.OrderedLessThanNonSignaling));

                                    while (surviveBits != 0)
                                    {
                                        int bit = BitOperations.TrailingZeroCount(surviveBits);
                                        depthBuf[baseIdx + bit] = depthV.GetElement(bit);
                                        surviveBits &= surviveBits - 1;
                                    }
                                }

                                w0 += ex0 * 8;
                                w1 += ex1 * 8;
                                w2 += ex2 * 8;
                            }
                        }

                        for (; x <= endX; x++)
                        {
                            if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                            {
                                float depth = (w0 * z0 + w1 * z1 + w2 * z2) * invArea;
                                depth = depth * 0.5f + 0.5f;
                                int idx = x + y * smWidth;
                                ref float depthRef = ref depthBuf[idx];
                                if (depth < depthRef) depthRef = depth;
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
            }

            int totalTiles = (tileMaxX - tileMinX + 1) * (tileMaxY - tileMinY + 1);
            if (totalTiles > 4)
                Parallel.For(tileMinY, tileMaxY + 1, ProcessShadowRow);
            else
                for (int ty = tileMinY; ty <= tileMaxY; ty++) ProcessShadowRow(ty);
        }

        public void RenderWithShadows<Shaders>(Scene scene, LightingCalculator lightCalc) where Shaders : Shader
        {
            lightCalc.ShadowRasterize<Shaders>(scene);
            Clear(Color.Black);
            DrawScene<Shaders>(scene);
        }
    }
}
