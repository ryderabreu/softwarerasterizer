using System;

namespace GraphicsLibrary
{
    public class Rasterizer
    {
        private readonly FrameBuffer _frameBuffer;

        public Rasterizer(FrameBuffer frameBuffer)
        {
            _frameBuffer = frameBuffer;
        }

        public void DrawMesh(
            Mesh mesh,
            VertexShader vertexShader,
            FragmentShader fragmentShader)
        {
            foreach (var triangle in mesh.Triangles)
            {
                var v0 = vertexShader(ToVSInput(triangle.V0));
                var v1 = vertexShader(ToVSInput(triangle.V1));
                var v2 = vertexShader(ToVSInput(triangle.V2));

                RasterizeTriangle(v0, v1, v2, fragmentShader);
            }
        }

        private VertexShaderInput ToVSInput(Vertex v)
        {
            return new VertexShaderInput
            {
                Position = v.Position,
                Normal = v.Normal,
                Color = v.Color
            };
        }

        private void RasterizeTriangle(
            VertexShaderOutput v0,
            VertexShaderOutput v1,
            VertexShaderOutput v2,
            FragmentShader fragmentShader)
        {
            Vector3 p0 = ToScreen(v0.ClipPosition);
            Vector3 p1 = ToScreen(v1.ClipPosition);
            Vector3 p2 = ToScreen(v2.ClipPosition);

            int minX = (int)MathF.Max(0, MathF.Min(p0.X, MathF.Min(p1.X, p2.X)));
            int maxX = (int)MathF.Min(_frameBuffer.Width - 1, MathF.Max(p0.X, MathF.Max(p1.X, p2.X)));

            int minY = (int)MathF.Max(0, MathF.Min(p0.Y, MathF.Min(p1.Y, p2.Y)));
            int maxY = (int)MathF.Min(_frameBuffer.Height - 1, MathF.Max(p0.Y, MathF.Max(p1.Y, p2.Y)));

            float area = Edge(p0, p1, p2);

            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                Vector3 p = new Vector3(x + 0.5f, y + 0.5f, 0);

                float w0 = Edge(p1, p2, p);
                float w1 = Edge(p2, p0, p);
                float w2 = Edge(p0, p1, p);

                if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                {
                    w0 /= area;
                    w1 /= area;
                    w2 /= area;

                    float depth = w0 * p0.Z + w1 * p1.Z + w2 * p2.Z;

                    if (depth < _frameBuffer.DepthBuffer[x,y])
                    {
                        _frameBuffer.DepthBuffer[x,y] = depth;

                        FragmentShaderInput input = new FragmentShaderInput
                        {
                            WorldPosition =
                                v0.WorldPosition * w0 +
                                v1.WorldPosition * w1 +
                                v2.WorldPosition * w2,

                            Normal =
                                (v0.Normal * w0 +
                                 v1.Normal * w1 +
                                 v2.Normal * w2).Normalized(),

                            Color =
                                LerpColor(v0.Color, v1.Color, v2.Color, w0, w1, w2)
                        };

                        _frameBuffer.ColorBuffer[x,y] = fragmentShader(input);
                    }
                }
            }
        }

        private Vector3 ToScreen(Vector3 ndc)
        {
            float x = (ndc.X * 0.5f + 0.5f) * _frameBuffer.Width;
            float y = (1f - (ndc.Y * 0.5f + 0.5f)) * _frameBuffer.Height;
            return new Vector3(x, y, ndc.Z);
        }

        private float Edge(Vector3 a, Vector3 b, Vector3 c)
        {
            return (c.X - a.X) * (b.Y - a.Y) -
                   (c.Y - a.Y) * (b.X - a.X);
        }

        private Color LerpColor(Color c0, Color c1, Color c2,
                                float w0, float w1, float w2)
        {
            return new Color(
                c0.R*w0 + c1.R*w1 + c2.R*w2,
                c0.G*w0 + c1.G*w1 + c2.G*w2,
                c0.B*w0 + c1.B*w1 + c2.B*w2,
                c0.A*w0 + c1.A*w1 + c2.A*w2
            );
        }
    }
}