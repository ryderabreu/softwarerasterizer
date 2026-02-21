using System.Configuration;
using GraphicsLibrary;

namespace MainProgram
{
    public class MyShaders : Shader
    {
        public static Texture texture { get; set; }
        public static Matrix4x4 model { get; set; }

        public static VertexOut VertexShader(Vertex input)
        {
            return VertexCalculator.ProjectWithModel(input, Program.camera, model);
        }

        public static Color FragmentShader(FragmentIn input)
        {
            if(input.FrontFace)
                return texture.Sample(input.UV) * Program.lightCalc.Calculate(input.WorldPosition, input.Color, input.Normal);
            else
                return texture.Sample(input.UV) * Program.lightCalc.CalculateWithoutShadows(input.WorldPosition, input.Color, input.Normal);
        }
    }
}