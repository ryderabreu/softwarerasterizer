using System;
using System.Numerics;

namespace GraphicsLibrary
{
    public static class PrimitiveGenerator
    {
        public static Mesh CreateCube(float size = 1f)
        {
            Mesh mesh = new Mesh();
            float h = size / 2f;

            Vector3[] positions =
            {
                new Vector3(-h,-h,-h),
                new Vector3( h,-h,-h),
                new Vector3( h, h,-h),
                new Vector3(-h, h,-h),
                new Vector3(-h,-h, h),
                new Vector3( h,-h, h),
                new Vector3( h, h, h),
                new Vector3(-h, h, h)
            };

            int[,] faces =
            {
                {0,3,2,1},
                {4,5,6,7},
                {0,1,5,4},
                {3,7,6,2},
                {0,4,7,3},
                {1,2,6,5}
            };

            Vector3[] normals =
            {
                new Vector3(0,0,-1),
                new Vector3(0,0,1),
                new Vector3(0,-1,0),
                new Vector3(0,1,0),
                new Vector3(-1,0,0),
                new Vector3(1,0,0)
            };

            Vector2 uv0 = new Vector2(0,0);
            Vector2 uv1 = new Vector2(1,0);
            Vector2 uv2 = new Vector2(1,1);
            Vector2 uv3 = new Vector2(0,1);

            for (int i = 0; i < 6; i++)
            {
                int a = faces[i,0];
                int b = faces[i,1];
                int c = faces[i,2];
                int d = faces[i,3];

                Vector3 normal = normals[i];
                Color color = Color.White;

                Vertex v0 = new Vertex(positions[a], normal, color, uv0);
                Vertex v1 = new Vertex(positions[b], normal, color, uv1);
                Vertex v2 = new Vertex(positions[c], normal, color, uv2);
                Vertex v3 = new Vertex(positions[d], normal, color, uv3);

                mesh.AddTriangle(new Triangle(v0, v1, v2));
                mesh.AddTriangle(new Triangle(v0, v2, v3));
            }

            return mesh;
        }

        public static Mesh CreatePyramid(float size = 1f, float height = 1f)
        {
            Mesh mesh = new Mesh();
            float h = size / 2f;

            Vector3 top = new Vector3(0, height, 0);

            Vector3[] baseVerts =
            {
                new Vector3(-h,0,-h),
                new Vector3( h,0,-h),
                new Vector3( h,0, h),
                new Vector3(-h,0, h)
            };

            Vertex b0 = new Vertex(baseVerts[0], new Vector3(0,-1,0), Color.White, new Vector2(0,0));
            Vertex b1 = new Vertex(baseVerts[1], new Vector3(0,-1,0), Color.White, new Vector2(1,0));
            Vertex b2 = new Vertex(baseVerts[2], new Vector3(0,-1,0), Color.White, new Vector2(1,1));
            Vertex b3 = new Vertex(baseVerts[3], new Vector3(0,-1,0), Color.White, new Vector2(0,1));

            mesh.AddTriangle(new Triangle(b0,b1,b2));
            mesh.AddTriangle(new Triangle(b0,b2,b3));

            Vector2 uvBottomLeft = new Vector2(0,0);
            Vector2 uvBottomRight = new Vector2(1,0);
            Vector2 uvTop = new Vector2(0.5f,1);

            for (int i = 0; i < 4; i++)
            {
                Vector3 p0 = baseVerts[i];
                Vector3 p1 = baseVerts[(i+1)%4];

                Vector3 normal = Vector3.Cross(top - p0, p1 - p0).Normalized();

                Vertex v0 = new Vertex(p0, normal, Color.White, uvBottomLeft);
                Vertex v1 = new Vertex(p1, normal, Color.White, uvBottomRight);
                Vertex v2 = new Vertex(top, normal, Color.White, uvTop);

                mesh.AddTriangle(new Triangle(v0,v1,v2));
            }

            return mesh;
        }

        public static Mesh CreatePlane(float size = 1f, int subdivisions = 10)
        {
            Mesh mesh = new Mesh();
            float h = size / 2f;
            Vector3 normal = new Vector3(0, 1, 0);

            float step = size / subdivisions;

            for (int z = 0; z < subdivisions; z++)
            {
                for (int x = 0; x < subdivisions; x++)
                {
                    float x0 = -h + x * step;
                    float x1 = x0 + step;

                    float z0 = -h + z * step;
                    float z1 = z0 + step;

                    float u0 = (float)x / subdivisions;
                    float u1 = (float)(x + 1) / subdivisions;

                    float v0 = (float)z / subdivisions;
                    float v1 = (float)(z + 1) / subdivisions;

                    Vertex v0p = new Vertex(new Vector3(x0, 0, z0), normal, Color.White, new Vector2(u0, v0));
                    Vertex v1p = new Vertex(new Vector3(x1, 0, z0), normal, Color.White, new Vector2(u1, v0));
                    Vertex v2p = new Vertex(new Vector3(x1, 0, z1), normal, Color.White, new Vector2(u1, v1));
                    Vertex v3p = new Vertex(new Vector3(x0, 0, z1), normal, Color.White, new Vector2(u0, v1));

                    mesh.AddTriangle(new Triangle(v0p, v2p, v1p));
                    mesh.AddTriangle(new Triangle(v0p, v3p, v2p));
                }
            }

            return mesh;
        }

        public static Mesh CreateSphere(float radius = 1f, int segments = 16, int rings = 16)
        {
            Mesh mesh = new Mesh();

            for (int y = 0; y < rings; y++)
            {
                float v0 = (float)y / rings;
                float v1 = (float)(y + 1) / rings;

                float theta0 = v0 * MathF.PI;
                float theta1 = v1 * MathF.PI;

                for (int x = 0; x < segments; x++)
                {
                    float u0 = (float)x / segments;
                    float u1 = (float)(x + 1) / segments;

                    float phi0 = u0 * MathF.PI * 2;
                    float phi1 = u1 * MathF.PI * 2;

                    Vector3 p0 = Spherical(radius, theta0, phi0);
                    Vector3 p1 = Spherical(radius, theta1, phi0);
                    Vector3 p2 = Spherical(radius, theta1, phi1);
                    Vector3 p3 = Spherical(radius, theta0, phi1);

                    Vector2 uv0 = new Vector2(u0, v0);
                    Vector2 uv1 = new Vector2(u0, v1);
                    Vector2 uv2 = new Vector2(u1, v1);
                    Vector2 uv3 = new Vector2(u1, v0);

                    Vertex vtx0 = new Vertex(p0, p0.Normalized(), Color.White, uv0);
                    Vertex vtx1 = new Vertex(p1, p1.Normalized(), Color.White, uv1);
                    Vertex vtx2 = new Vertex(p2, p2.Normalized(), Color.White, uv2);
                    Vertex vtx3 = new Vertex(p3, p3.Normalized(), Color.White, uv3);

                    mesh.AddTriangle(new Triangle(vtx0, vtx2, vtx1));
                    mesh.AddTriangle(new Triangle(vtx0, vtx3, vtx2));
                }
            }

            return mesh;
        }

        private static Vector3 Spherical(float r, float theta, float phi)
        {
            float sinTheta = MathF.Sin(theta);
            return new Vector3(
                r * sinTheta * MathF.Cos(phi),
                r * MathF.Cos(theta),
                r * sinTheta * MathF.Sin(phi)
            );
        }
    }
}