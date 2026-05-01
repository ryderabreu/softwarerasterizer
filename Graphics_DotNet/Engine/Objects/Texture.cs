using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GraphicsLibrary
{
    public class Texture
    {
        public int Width;
        public int Height;
        public Color[] Pixels;

        public Color Sample(Vector2 uv)
        {
            int x = (int)(Math.Clamp(uv.X, 0f, 1f) * (Width  - 1));
            int y = (int)((1f - Math.Clamp(uv.Y, 0f, 1f)) * (Height - 1));
            return Pixels[x + y * Width];
        }

        public static Texture FromImage(string filePath)
        {
            using (Bitmap bmp = new Bitmap(filePath))
            {
                int width  = bmp.Width;
                int height = bmp.Height;

                BitmapData data = bmp.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                Color[] pixels;
                try
                {
                    int stride    = Math.Abs(data.Stride);
                    byte[] raw    = new byte[height * stride];
                    Marshal.Copy(data.Scan0, raw, 0, raw.Length);

                    pixels = new Color[width * height];
                    for (int y = 0; y < height; y++)
                    {
                        int rowBase = y * stride;
                        int pixBase = y * width;
                        for (int x = 0; x < width; x++)
                        {
                            int o = rowBase + x * 4;
                            // Format32bppArgb is stored as BGRA in memory
                            pixels[pixBase + x] = new Color(
                                raw[o + 2] * (1f / 255f),
                                raw[o + 1] * (1f / 255f),
                                raw[o    ] * (1f / 255f),
                                raw[o + 3] * (1f / 255f));
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }

                return new Texture { Width = width, Height = height, Pixels = pixels };
            }
        }
    }
}
