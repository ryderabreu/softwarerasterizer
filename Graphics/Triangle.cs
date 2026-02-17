namespace GraphicsLibrary
{
    public readonly struct Triangle
    {
        public readonly Vertex V0;
        public readonly Vertex V1;
        public readonly Vertex V2;

        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        public Vector3 FaceNormal
        {
            get
            {
                var edge1 = V1.Position - V0.Position;
                var edge2 = V2.Position - V0.Position;
                return Vector3.Cross(edge1, edge2).Normalized();
            }
        }
    }
}