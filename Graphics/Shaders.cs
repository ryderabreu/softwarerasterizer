using GraphicsLibrary;

namespace MainProgram
{
    public static class Shaders
    {
        public static Texture texture = Texture.FromImage(@"C:\Users\ryder\source\repos\Graphics\Graphics\cattexture.jpg");
        public static Matrix4x4 model;

        public static VertexOut vs(Vertex input)
        {
            return VertexCalculator.ProjectWithModel(input, Program.camera, model);
        }

        public static Color fs(FragmentIn input)
        {
            if(input.FrontFace)
                return texture.Sample(input.UV) * Program.lightCalc.Calculate(input.WorldPosition, input.Color, input.Normal);
            else
                return texture.Sample(input.UV) * Program.lightCalc.CalculateWithoutShadows(input.WorldPosition, input.Color, input.Normal);
        }
    }
}