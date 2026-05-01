namespace GraphicsLibrary
{
    public delegate VertexOut VertexShader(Vertex input);
    public delegate Color FragmentShader(FragmentIn input);

    public struct VertexOut
    {
        public Vector4 ClipPosition;
        public Vector3 WorldPosition;
        public Vector3 Normal;
        public Color Color;
        public Vector2 UV;
    }

    public struct FragmentIn
    {
        public Vector3 WorldPosition;
        public Vector3 Normal;
        public Color Color;
        public bool FrontFace;
        public Vector2 UV;
    }
}