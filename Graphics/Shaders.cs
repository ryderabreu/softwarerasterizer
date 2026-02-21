using System.Configuration;
using GraphicsLibrary;

namespace MainProgram
{
    public class Shaders : Shader
    {
        public override Texture texture { get; set; }
        public override Matrix4x4 model { get; set; }

        public override VertexOut VertexShader(Vertex input)
        {
            return VertexCalculator.ProjectWithModel(input, Program.camera, model);
        }

        public override Color FragmentShader(FragmentIn input)
        {
            if(input.FrontFace)
                return texture.Sample(input.UV) * Program.lightCalc.Calculate(input.WorldPosition, input.Color, input.Normal);
            else
                return texture.Sample(input.UV) * Program.lightCalc.CalculateWithoutShadows(input.WorldPosition, input.Color, input.Normal);
        }
    }
}