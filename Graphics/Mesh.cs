using System.Collections.Generic;

namespace GraphicsLibrary
{
    public class Mesh
    {
        private readonly List<Triangle> _triangles = new List<Triangle>();

        public IReadOnlyList<Triangle> Triangles => _triangles;

        public void AddTriangle(Triangle triangle)
        {
            _triangles.Add(triangle);
        }

        public void Clear()
        {
            _triangles.Clear();
        }

        public void Transform(Matrix4x4 matrix)
        {
            for (int i = 0; i < _triangles.Count; i++)
            {
                var tri = _triangles[i];

                _triangles[i] = new Triangle(
                    TransformVertex(tri.V0, matrix),
                    TransformVertex(tri.V1, matrix),
                    TransformVertex(tri.V2, matrix)
                );
            }
        }

        private static Vertex TransformVertex(Vertex v, Matrix4x4 m)
        {
            Vector3 newPos = m * v.Position;
            Vector3 newNormal = (m * (v.Position + v.Normal)) - newPos;
            newNormal = newNormal.Normalized();

            return new Vertex(newPos, newNormal, v.Color, v.UV);
        }
    }
}