namespace GraphicsLibrary
{
    public class DefaultShaders : Shader
    {
        public static Texture texture { get; set; }

        private static Matrix4x4 _model;
        private static Matrix4x4 _mvp;

        public static Matrix4x4 model
        {
            get => _model;
            set
            {
                _model = value;
                _mvp   = Program.camera.ViewProjectionMatrix() * value;
            }
        }

        public static VertexOut VertexShader(Vertex input)
        {
            return VertexCalculator.ProjectWithModel(input, _mvp, _model);
        }

        public static Color FragmentShader(FragmentIn input)
        {
            return texture.Sample(input.UV) * Program.lightCalc.Calculate(
                worldposition: input.WorldPosition,
                color:         input.Color,
                normal:        input.Normal,
                frontfacing:   input.FrontFace,
                shadows:       true,
                frontOnlyShadows: true
            );
        }
    }
}
