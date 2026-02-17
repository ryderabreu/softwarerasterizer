namespace GraphicsLibrary
{
    public delegate VertexShaderOutput VertexShader(Vertex input);
    public delegate Color FragmentShader(FragmentShaderInput input);

public struct VertexShaderOutput
{
    public Vector4 ClipPosition;
    public Vector3 NDCPosition;
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