namespace GraphicsLibrary
{
    public readonly struct Vertex
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Color Color;
        public readonly Vector2 UV;

        public Vertex(Vector3 position, Vector3 normal, Color color, Vector2 uv)
        {
            Position = position;
            Normal = normal;
            Color = color;
            UV = uv;
        }
    }
}