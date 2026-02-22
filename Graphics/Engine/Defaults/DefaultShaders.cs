using System.Configuration;
using GraphicsLibrary;

namespace GraphicsLibrary
{
    public class DefaultShaders : Shader
    {
        public static Texture texture { get; set; }
        public static Matrix4x4 model { get; set; }

        public static VertexOut VertexShader(Vertex input)
        {
            return VertexCalculator.ProjectWithModel(input, Program.camera, model);
        }

        public static Color FragmentShader(FragmentIn input)
        {
            return texture.Sample(input.UV) * Program.lightCalc.Calculate(
                worldposition: input.WorldPosition,
                color: input.Color,
                normal: input.Normal,
                frontfacing: input.FrontFace,
                shadows: true,
                frontOnlyShadows: true
            );
        }
    }
}