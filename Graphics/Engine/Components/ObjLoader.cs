using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GraphicsLibrary
{
    public static class ObjLoader
    {
        public static Mesh Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("OBJ file not found", filePath);

            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var mesh = new Mesh();

            foreach (var line in File.ReadLines(filePath))
            {
                string l = line.Trim();
                if (string.IsNullOrWhiteSpace(l) || l.StartsWith("#"))
                    continue;

                var parts = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v":
                        positions.Add(new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])
                        ));
                        break;

                    case "vn":
                        normals.Add(new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])
                        ).Normalized());
                        break;

                    case "vt":
                        uvs.Add(new Vector2(
                            float.Parse(parts[1]),
                            float.Parse(parts[2])
                        ));
                        break;

                    case "f":
                        if (parts.Length < 4)
                            continue;

                        var faceIndices = new List<(int pos, int uv, int norm)>();
                        for (int i = 1; i < parts.Length; i++)
                        {
                            var idx = parts[i].Split('/');
                            int p = int.Parse(idx[0]) - 1;
                            int t = idx.Length > 1 && !string.IsNullOrEmpty(idx[1]) ? int.Parse(idx[1]) - 1 : -1;
                            int n = idx.Length > 2 && !string.IsNullOrEmpty(idx[2]) ? int.Parse(idx[2]) - 1 : -1;

                            faceIndices.Add((p, t, n));
                        }

                        for (int i = 1; i < faceIndices.Count - 1; i++)
                        {
                            mesh.AddTriangle(new Triangle(
                                CreateVertex(faceIndices[0], positions, uvs, normals),
                                CreateVertex(faceIndices[i], positions, uvs, normals),
                                CreateVertex(faceIndices[i + 1], positions, uvs, normals)
                            ));
                        }
                        break;
                }
            }

            return mesh;
        }

        private static Vertex CreateVertex((int pos, int uv, int norm) idx, List<Vector3> positions, List<Vector2> uvs, List<Vector3> normals)
        {
            Vector3 position = positions[idx.pos];
            Vector3 normal = idx.norm >= 0 && idx.norm < normals.Count ? normals[idx.norm] : Vector3.UnitY;
            Vector2 uv = idx.uv >= 0 && idx.uv < uvs.Count ? uvs[idx.uv] : Vector2.Zero;
            return new Vertex(position, normal, Color.White, uv);
        }
    }
}