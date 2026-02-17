using System;
using System.Drawing;

namespace GraphicsLibrary
{
    public readonly struct Color
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        public Color(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static readonly Color White = new Color(1,1,1,1);
        public static readonly Color Red   = new Color(1,0,0,1);
        public static readonly Color Green = new Color(0,1,0,1);
        public static readonly Color Blue  = new Color(0,0,1,1);
        public static readonly Color Black = new Color(0,0,0,1);

        public System.Drawing.Color ToDrawingColor()
        {
            int r = (int)(Math.Clamp(R, 0f, 1f) * 255f);
            int g = (int)(Math.Clamp(G, 0f, 1f) * 255f);
            int b = (int)(Math.Clamp(B, 0f, 1f) * 255f);
            int a = (int)(Math.Clamp(A, 0f, 1f) * 255f);

            return System.Drawing.Color.FromArgb(a, r, g, b);
        }
    }
}