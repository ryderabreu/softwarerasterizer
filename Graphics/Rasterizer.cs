using System;

namespace GraphicsLibrary
{
    public class Rasterizer
    {
        private FrameBuffer _frameBuffer;
        private float[,] _depthBuffer;

        public Rasterizer(FrameBuffer frameBuffer)
        {
            _frameBuffer = frameBuffer;
            _depthBuffer = new float[_frameBuffer.Width, _frameBuffer.Height];
        }

        public void Clear(Color clearColor)
        {
            if (_depthBuffer.GetLength(0) != _frameBuffer.Width ||
                _depthBuffer.GetLength(1) != _frameBuffer.Height)
            {
                _depthBuffer = new float[_frameBuffer.Width, _frameBuffer.Height];
            }

            for (int y = 0; y < _frameBuffer.Height; y++)
            {
                for (int x = 0; x < _frameBuffer.Width; x++)
                {
                    _frameBuffer.ColorBuffer[x, y] = clearColor;
                    _depthBuffer[x, y] = float.PositiveInfinity;
                }
            }
        }

        public void DrawMesh(Mesh mesh, VertexShader vs, FragmentShader fs)
        {
            foreach (var tri in mesh.Triangles)
            {
                var v0 = vs(tri.V0);
                var v1 = vs(tri.V1);
                var v2 = vs(tri.V2);

                RasterizeTriangle(v0, v1, v2, fs);
            }
        }

        private void RasterizeTriangle(
            VertexShaderOutput v0,
            VertexShaderOutput v1,
            VertexShaderOutput v2,
            FragmentShader fs)
        {
            int x0 = (int)((v0.NDCPosition.X * 0.5f + 0.5f) * _frameBuffer.Width);
            int y0 = (int)((1f - (v0.NDCPosition.Y * 0.5f + 0.5f)) * _frameBuffer.Height);

            int x1 = (int)((v1.NDCPosition.X * 0.5f + 0.5f) * _frameBuffer.Width);
            int y1 = (int)((1f - (v1.NDCPosition.Y * 0.5f + 0.5f)) * _frameBuffer.Height);

            int x2 = (int)((v2.NDCPosition.X * 0.5f + 0.5f) * _frameBuffer.Width);
            int y2 = (int)((1f - (v2.NDCPosition.Y * 0.5f + 0.5f)) * _frameBuffer.Height);

            int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2)));
            int maxX = Math.Min(_frameBuffer.Width - 1, Math.Max(x0, Math.Max(x1, x2)));
            int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2)));
            int maxY = Math.Min(_frameBuffer.Height - 1, Math.Max(y0, Math.Max(y1, y2)));

            float area = EdgeFunction(x0, y0, x1, y1, x2, y2);
            if (area == 0f) return;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float w0 = EdgeFunction(x1, y1, x2, y2, x, y);
                    float w1 = EdgeFunction(x2, y2, x0, y0, x, y);
                    float w2 = EdgeFunction(x0, y0, x1, y1, x, y);

                    if (area < 0)
                    {
                        w0 = -w0;
                        w1 = -w1;
                        w2 = -w2;
                    }

                    if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                    {
                        w0 /= Math.Abs(area);
                        w1 /= Math.Abs(area);
                        w2 /= Math.Abs(area);

                        float iw0 = 1f / v0.ClipPosition.W;
                        float iw1 = 1f / v1.ClipPosition.W;
                        float iw2 = 1f / v2.ClipPosition.W;

                        float invW = w0 * iw0 + w1 * iw1 + w2 * iw2;
                        float invWFinal = 1f / invW;

                        float depth =
                            w0 * v0.NDCPosition.Z +
                            w1 * v1.NDCPosition.Z +
                            w2 * v2.NDCPosition.Z;
                        
                        if (depth <= _depthBuffer[x, y])
                        {
                            _depthBuffer[x, y] = depth;

                            Vector3 worldPos =
                                (v0.WorldPosition * w0 * iw0 +
                                 v1.WorldPosition * w1 * iw1 +
                                 v2.WorldPosition * w2 * iw2) * invWFinal;

                            Vector3 normal =
                                (v0.Normal * w0 * iw0 +
                                 v1.Normal * w1 * iw1 +
                                 v2.Normal * w2 * iw2) * invWFinal;

                            normal = normal.Normalized();

                            float r =
                                (v0.Color.R * w0 * iw0 +
                                 v1.Color.R * w1 * iw1 +
                                 v2.Color.R * w2 * iw2) * invWFinal;

                            float g =
                                (v0.Color.G * w0 * iw0 +
                                 v1.Color.G * w1 * iw1 +
                                 v2.Color.G * w2 * iw2) * invWFinal;

                            float b =
                                (v0.Color.B * w0 * iw0 +
                                 v1.Color.B * w1 * iw1 +
                                 v2.Color.B * w2 * iw2) * invWFinal;

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
            }
        }

        private float EdgeFunction(int x0, int y0, int x1, int y1, int x2, int y2)
        {
            return (x2 - x0) * (y1 - y0) - (x1 - x0) * (y2 - y0);
        }
    }
}