using System;

namespace GraphicsLibrary
{
    public class DirectionalLight
    {
        public Vector3 Direction { get; set; } = new Vector3(0, -1, -1).Normalized();
        public Color Color { get; set; } = new Color(1f, 1f, 1f, 1f);
        public float Intensity { get; set; } = 1f;

        public DirectionalLight(Vector3 direction, Color color, float intensity = 1f)
        {
            Direction = direction.Normalized();
            Color = color;
            Intensity = intensity;
        }

        public Color getColor(Vector3 normal, Color baseColor, float ambient)
        {
            // Direction is pre-normalized in constructor; normal is pre-normalized by rasterizer
            float diff = MathF.Max(Vector3.Dot(normal, -Direction), 0f);

            float lit = ambient + diff * Intensity;
            return new Color(lit * baseColor.R * Color.R, lit * baseColor.G * Color.G, lit * baseColor.B * Color.B, 1f);
        }

        public Matrix4x4 LightMatrix(float size, Vector3 viewPoint)
        {
            Vector3 forward = -Direction; // already normalized

            Vector3 right;
            Vector3 up;

            if (MathF.Abs(Direction.X) < 0.0001f && MathF.Abs(Direction.Z) < 0.0001f)
            {
                right = new Vector3(1, 0, 0);
                up = new Vector3(0, 0, 1);
            }
            else
            {
                up = Vector3.UnitY;
                right = Vector3.Cross(up, forward).Normalized();
                up = Vector3.Cross(forward, right);
            }

            Matrix4x4 view = new Matrix4x4(
                right.X,   right.Y,   right.Z,   -Vector3.Dot(right, viewPoint),
                up.X,      up.Y,      up.Z,      -Vector3.Dot(up, viewPoint),
                forward.X, forward.Y, forward.Z, -Vector3.Dot(forward, viewPoint),
                0, 0, 0, 1);

            Matrix4x4 proj = Matrix4x4.Orthographic(-size, size, -size, size, 0.1f, 50f);

            return proj * view;
        }
    }
}
