using System.Windows.Forms;

namespace GraphicsLibrary
{
    public static class VertexCalculator
    {
        public static VertexOut Project(Vertex input, Camera camera)
        {
            return new VertexOut
            {
                ClipPosition = camera.ViewProjectionMatrix() * new Vector4(input.Position.X, input.Position.Y, input.Position.Z, 1f),
                WorldPosition = input.Position,
                Normal = input.Normal,
                Color = input.Color,
                UV = input.UV
            };
        }
    }
}