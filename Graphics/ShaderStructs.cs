namespace GraphicsLibrary
{
    public delegate VertexShaderOutput VertexShader(VertexShaderInput input);
    public delegate Color FragmentShader(FragmentShaderInput input);

    public struct VertexShaderInput
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
    }

    public struct VertexShaderOutput
    {
        public Vector3 ClipPosition;
        public Vector3 WorldPosition;
        public Vector3 Normal;
        public Color Color;
    }

    public struct FragmentShaderInput
    {
        public Vector3 WorldPosition;
        public Vector3 Normal;
        public Color Color;
    }
}