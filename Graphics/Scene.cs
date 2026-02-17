using System.Collections.Generic;

namespace GraphicsLibrary
{
    public class Scene
    {
        private readonly List<Mesh> _meshes = new List<Mesh>();

        public IReadOnlyList<Mesh> Meshes => _meshes;

        public void AddMesh(Mesh mesh)
        {
            _meshes.Add(mesh);
        }

        public void Clear()
        {
            _meshes.Clear();
        }

        public void Transform(Matrix4x4 matrix)
        {
            for (int i = 0; i < _meshes.Count; i++)
            {
                _meshes[i].Transform(matrix);
            }
        }
    }
}