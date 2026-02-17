namespace GraphicsLibrary
{
    public delegate VertexShaderOutput VertexShader(Vertex input);
    public delegate Color FragmentShader(FragmentShaderInput input);

    public struct VertexShaderOutput
    {
        public Vector3 ClipPosition;
        public Vector3 WorldPosition;
        public Vector3 Normal;
        public Color Color;
        public Vector3 CameraPosition;
    }

    public struct FragmentShaderInput
    {
        public Vector3 WorldPosition;
        public Vector3 Normal;
        public Color Color;
    }
}