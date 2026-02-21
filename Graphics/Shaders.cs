using GraphicsLibrary;

namespace MainProgram
{
    public static class Shaders
    {
        public static Matrix4x4 model;
        public static VertexOut vs(Vertex input)
        {
            return VertexCalculator.ProjectWithModel(input, Program.camera, model);
        }

        public static Color fs(FragmentIn input)
        {
            if(input.FrontFace)
                return Program.texture.Sample(input.UV) * Program.lightCalc.Calculate(input.WorldPosition, input.Color, input.Normal);
            else
                return Program.texture.Sample(input.UV) * Program.lightCalc.CalculateWithoutShadows(input.WorldPosition, input.Color, input.Normal);
        }
    }
}