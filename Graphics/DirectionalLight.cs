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
            float diff = MathF.Max(Vector3.Dot(normal.Normalized(), -Direction.Normalized()), 0f);

            float r = (ambient + diff * Intensity) * baseColor.R * Color.R;
            float g = (ambient + diff * Intensity) * baseColor.G * Color.G;
            float b = (ambient + diff * Intensity) * baseColor.B * Color.B;

            return new Color(r, g, b, 1f);
        }
    }
}