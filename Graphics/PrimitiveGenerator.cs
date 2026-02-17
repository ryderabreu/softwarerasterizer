using System;

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
                {0,1,2,3},
                {5,4,7,6},
                {4,0,3,7},
                {1,5,6,2},
                {3,2,6,7},
                {4,5,1,0}
            };

            Vector3[] normals =
            {
                new Vector3(0,0,-1),
                new Vector3(0,0,1),
                new Vector3(-1,0,0),
                new Vector3(1,0,0),
                new Vector3(0,1,0),
                new Vector3(0,-1,0)
            };

            for (int i = 0; i < 6; i++)
            {
                int a = faces[i,0];
                int b = faces[i,1];
                int c = faces[i,2];
                int d = faces[i,3];

                Vector3 normal = normals[i];
                Color color = Color.White;

                Vertex v0 = new Vertex(positions[a], normal, color);
                Vertex v1 = new Vertex(positions[b], normal, color);
                Vertex v2 = new Vertex(positions[c], normal, color);
                Vertex v3 = new Vertex(positions[d], normal, color);

                mesh.AddTriangle(new Triangle(v0, v1, v2));
                mesh.AddTriangle(new Triangle(v0, v2, v3));
            }

            return mesh;
        }

        public static Mesh CreatePlane(float size = 1f)
        {
            Mesh mesh = new Mesh();
            float h = size / 2f;

            Vector3 normal = new Vector3(0,1,0);

            Vertex v0 = new Vertex(new Vector3(-h,0,-h), normal, Color.White);
            Vertex v1 = new Vertex(new Vector3( h,0,-h), normal, Color.White);
            Vertex v2 = new Vertex(new Vector3( h,0, h), normal, Color.White);
            Vertex v3 = new Vertex(new Vector3(-h,0, h), normal, Color.White);

            mesh.AddTriangle(new Triangle(v0,v1,v2));
            mesh.AddTriangle(new Triangle(v0,v2,v3));

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

            Vector3 baseNormal = new Vector3(0,-1,0);
            Vertex b0 = new Vertex(baseVerts[0], baseNormal, Color.White);
            Vertex b1 = new Vertex(baseVerts[1], baseNormal, Color.White);
            Vertex b2 = new Vertex(baseVerts[2], baseNormal, Color.White);
            Vertex b3 = new Vertex(baseVerts[3], baseNormal, Color.White);

            mesh.AddTriangle(new Triangle(b0,b2,b1));
            mesh.AddTriangle(new Triangle(b0,b3,b2));

            for (int i = 0; i < 4; i++)
            {
                Vector3 p0 = baseVerts[i];
                Vector3 p1 = baseVerts[(i+1)%4];

                Vector3 normal = Vector3.Cross(p1 - p0, top - p0).Normalized();

                Vertex v0 = new Vertex(p0, normal, Color.White);
                Vertex v1 = new Vertex(p1, normal, Color.White);
                Vertex v2 = new Vertex(top, normal, Color.White);

                mesh.AddTriangle(new Triangle(v0,v1,v2));
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

                    Vertex vtx0 = new Vertex(p0, p0.Normalized(), Color.White);
                    Vertex vtx1 = new Vertex(p1, p1.Normalized(), Color.White);
                    Vertex vtx2 = new Vertex(p2, p2.Normalized(), Color.White);
                    Vertex vtx3 = new Vertex(p3, p3.Normalized(), Color.White);

                    mesh.AddTriangle(new Triangle(vtx0, vtx1, vtx2));
                    mesh.AddTriangle(new Triangle(vtx0, vtx2, vtx3));
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