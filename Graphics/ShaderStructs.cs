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
        public Vector3 CameraPosition;
    }

    public struct FragmentIn
    {
        public Vector3 WorldPosition;
        public Vector3 Normal;
        public Color Color;
    }
}