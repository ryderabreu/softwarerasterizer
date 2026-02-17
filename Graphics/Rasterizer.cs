using System;

namespace GraphicsLibrary
{
    public class Rasterizer
    {
        private FrameBuffer _frameBuffer;
        private float[,] _depthBuffer;

        public bool EnableBackfaceCulling { get; set; } = true;

        public Rasterizer(FrameBuffer frameBuffer, bool backfaceCulling = true)
        {
            _frameBuffer = frameBuffer;
            _depthBuffer = new float[_frameBuffer.Width, _frameBuffer.Height];
            EnableBackfaceCulling = backfaceCulling;
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
                DrawMesh(mesh, vs, fs);
        }

        public void DrawMesh(Mesh mesh, VertexShader vs, FragmentShader fs)
        {
            foreach (var tri in mesh.Triangles)
            {
                var v0 = vs(tri.V0);
                var v1 = vs(tri.V1);
                var v2 = vs(tri.V2);

                if (EnableBackfaceCulling)
                {
                    Vector3 e1 = v1.WorldPosition - v0.WorldPosition;
                    Vector3 e2 = v2.WorldPosition - v0.WorldPosition;
                    Vector3 triNormal = Vector3.Cross(e1, e2);

                    Vector3 viewDir = v0.WorldPosition - v0.CameraPosition;

                    if (Vector3.Dot(triNormal, viewDir) >= 0f)
                        continue;
                }

                int x0 = (int)((v0.ClipPosition.X * 0.5f + 0.5f) * _frameBuffer.Width);
                int y0 = (int)((1f - (v0.ClipPosition.Y * 0.5f + 0.5f)) * _frameBuffer.Height);

                int x1 = (int)((v1.ClipPosition.X * 0.5f + 0.5f) * _frameBuffer.Width);
                int y1 = (int)((1f - (v1.ClipPosition.Y * 0.5f + 0.5f)) * _frameBuffer.Height);

                int x2 = (int)((v2.ClipPosition.X * 0.5f + 0.5f) * _frameBuffer.Width);
                int y2 = (int)((1f - (v2.ClipPosition.Y * 0.5f + 0.5f)) * _frameBuffer.Height);

                RasterizeTriangle(v0, v1, v2, x0, y0, x1, y1, x2, y2, fs);
            }
        }

        private void RasterizeTriangle(
            VertexShaderOutput v0,
            VertexShaderOutput v1,
            VertexShaderOutput v2,
            int x0, int y0,
            int x1, int y1,
            int x2, int y2,
            FragmentShader fs)
        {
            int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2)));
            int maxX = Math.Min(_frameBuffer.Width - 1, Math.Max(x0, Math.Max(x1, x2)));
            int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2)));
            int maxY = Math.Min(_frameBuffer.Height - 1, Math.Max(y0, Math.Max(y1, y2)));

            float area = EdgeFunction(x0, y0, x1, y1, x2, y2);
            if (area == 0f)
                return;

            float iw0 = 1f / v0.ClipPosition.Z;
            float iw1 = 1f / v1.ClipPosition.Z;
            float iw2 = 1f / v2.ClipPosition.Z;

            Vector3 world0 = v0.WorldPosition * iw0;
            Vector3 world1 = v1.WorldPosition * iw1;
            Vector3 world2 = v2.WorldPosition * iw2;

            Vector3 normal0 = v0.Normal * iw0;
            Vector3 normal1 = v1.Normal * iw1;
            Vector3 normal2 = v2.Normal * iw2;

            float r0 = v0.Color.R * iw0;
            float g0 = v0.Color.G * iw0;
            float b0 = v0.Color.B * iw0;

            float r1 = v1.Color.R * iw1;
            float g1 = v1.Color.G * iw1;
            float b1 = v1.Color.B * iw1;

            float r2 = v2.Color.R * iw2;
            float g2 = v2.Color.G * iw2;
            float b2 = v2.Color.B * iw2;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float w0 = EdgeFunction(x1, y1, x2, y2, x, y) / area;
                    float w1 = EdgeFunction(x2, y2, x0, y0, x, y) / area;
                    float w2 = 1f - w0 - w1;

                    if (w0 < 0f || w1 < 0f || w2 < 0f)
                        continue;

                    float iw = w0 * iw0 + w1 * iw1 + w2 * iw2;
                    float invIw = 1f / iw;

                    float depth =
                        (w0 * v0.ClipPosition.Z * iw0 +
                         w1 * v1.ClipPosition.Z * iw1 +
                         w2 * v2.ClipPosition.Z * iw2) * invIw;

                    if (depth >= _depthBuffer[x, y])
                        continue;

                    _depthBuffer[x, y] = depth;

                    Vector3 worldPos =
                        (world0 * w0 + world1 * w1 + world2 * w2) * invIw;

                    Vector3 normal =
                        ((normal0 * w0 + normal1 * w1 + normal2 * w2) * invIw).Normalized();

                    float r =
                        (r0 * w0 + r1 * w1 + r2 * w2) * invIw;

                    float g =
                        (g0 * w0 + g1 * w1 + g2 * w2) * invIw;

                    float b =
                        (b0 * w0 + b1 * w1 + b2 * w2) * invIw;

                    var fragInput = new FragmentShaderInput
                    {
                        WorldPosition = worldPos,
                        Normal = normal,
                        Color = new Color(r, g, b, 1f)
                    };

                    _frameBuffer.ColorBuffer[x, y] = fs(fragInput);
                }
            }
        }

        private float EdgeFunction(int x0, int y0, int x1, int y1, int x2, int y2)
        {
            return (x2 - x0) * (y1 - y0) - (x1 - x0) * (y2 - y0);
        }
    }
}