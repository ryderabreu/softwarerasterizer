using System;
using System.Drawing;

namespace GraphicsLibrary
{
    public class Texture
    {
        public int Width;
        public int Height;
        public Color[,] Pixels;

        public Color Sample(Vector2 uv)
        {
            int x = (int)(uv.X * (Width - 1));
            int y = (int)((1 - uv.Y) * (Height - 1));
            return Pixels[x, y];
        }

        public static Texture FromImage(string filePath)
        {
            using (Bitmap bmp = new Bitmap(filePath))
            {
                int width = bmp.Width;
                int height = bmp.Height;
                Color[,] pixels = new Color[width, height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        System.Drawing.Color c = bmp.GetPixel(x, y);
                        pixels[x, y] = new Color(
                            c.R / 255f,
                            c.G / 255f,
                            c.B / 255f,
                            c.A / 255f
                        );
                    }
                }

                return new Texture
                {
                    Width = width,
                    Height = height,
                    Pixels = pixels
                };
            }
        }
    }
}