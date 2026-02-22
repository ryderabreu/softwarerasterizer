using System;
using System.Runtime.CompilerServices;

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
            float diff = MathF.Max(Vector3.Dot(normal.Normalized(), -Direction.Normalized()), 0f);

            float r = (ambient + diff * Intensity) * baseColor.R * Color.R;
            float g = (ambient + diff * Intensity) * baseColor.G * Color.G;
            float b = (ambient + diff * Intensity) * baseColor.B * Color.B;

            return new Color(r, g, b, 1f);
        }

        public Matrix4x4 LightMatrix(float size, Vector3 viewPoint)
        {
            Vector3 lightDir = Direction.Normalized();
            Vector3 forward = -lightDir;

            Vector3 right;
            Vector3 up;

            if (MathF.Abs(lightDir.X) < 0.0001f && MathF.Abs(lightDir.Z) < 0.0001f)
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

            Matrix4x4 view = new Matrix4x4(new float[,]
            {
                { right.X,   right.Y,   right.Z,   -Vector3.Dot(right, viewPoint) },
                { up.X,      up.Y,      up.Z,      -Vector3.Dot(up, viewPoint) },
                { forward.X, forward.Y, forward.Z, -Vector3.Dot(forward, viewPoint) },
                { 0, 0, 0, 1 }
            });

            Matrix4x4 proj = Matrix4x4.Orthographic(
                -size, size,
                -size, size,
                0.1f, 50f
            );

            return proj * view;
        }
    }
}