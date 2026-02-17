using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GraphicsLibrary
{
    public class RenderWindow : Form
    {
        private readonly FrameBuffer _frameBuffer;
        private readonly Bitmap _bitmap;
        private readonly Timer _timer;

        public Action<float> OnRender; // deltaTime callback
        private DateTime _lastFrameTime;

        // Raw pixel buffer for lockbits
        private readonly byte[] _pixelBuffer;

        public RenderWindow(int width, int height)
        {
            Width = width;
            Height = height;
            Text = "Software Renderer";

            _frameBuffer = new FrameBuffer(width, height);
            _bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            _pixelBuffer = new byte[width * height * 4]; // 4 bytes per pixel (ARGB)

            DoubleBuffered = true;

            _timer = new Timer();
            _timer.Interval = 16; // ~60 FPS
            _timer.Tick += RenderLoop;
            _timer.Start();

            _lastFrameTime = DateTime.Now;
        }

        public FrameBuffer FrameBuffer => _frameBuffer;

        private void RenderLoop(object sender, EventArgs e)
        {
            float deltaTime = (float)(DateTime.Now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = DateTime.Now;

            OnRender?.Invoke(deltaTime);

            Present();
            Invalidate();
        }

        private void Present()
        {
            int width = _frameBuffer.Width;
            int height = _frameBuffer.Height;

            // Copy FrameBuffer colors into raw pixel buffer
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = _frameBuffer.ColorBuffer[x, y];
                    int index = (y * width + x) * 4;

                    _pixelBuffer[index + 0] = (byte)(Math.Clamp(c.B, 0f, 1f) * 255f); // Blue
                    _pixelBuffer[index + 1] = (byte)(Math.Clamp(c.G, 0f, 1f) * 255f); // Green
                    _pixelBuffer[index + 2] = (byte)(Math.Clamp(c.R, 0f, 1f) * 255f); // Red
                    _pixelBuffer[index + 3] = (byte)(Math.Clamp(c.A, 0f, 1f) * 255f); // Alpha
                }
            }

            // Lock bitmap and copy raw pixels
            BitmapData bmpData = _bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            Marshal.Copy(_pixelBuffer, 0, bmpData.Scan0, _pixelBuffer.Length);
            _bitmap.UnlockBits(bmpData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_bitmap, 0, 0, Width, Height);
        }
    }
}
