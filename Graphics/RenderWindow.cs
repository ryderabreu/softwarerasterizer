using System;
using System.Drawing;
using System.Windows.Forms;

namespace GraphicsLibrary
{
    public class RenderWindow : Form
    {
        private readonly FrameBuffer _frameBuffer;
        private readonly Bitmap _bitmap;
        private readonly Timer _timer;

        public Action<float> OnRender;

        private DateTime _lastFrameTime;

        public RenderWindow(int width, int height)
        {
            Width = width;
            Height = height;
            Text = "Software Renderer";

            _frameBuffer = new FrameBuffer(width, height);
            _bitmap = new Bitmap(width, height);

            DoubleBuffered = true;

            _timer = new Timer();
            _timer.Interval = 16;
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
            for (int x = 0; x < _frameBuffer.Width; x++)
            for (int y = 0; y < _frameBuffer.Height; y++)
            {
                _bitmap.SetPixel(x, y, _frameBuffer.ColorBuffer[x, y].ToDrawingColor());
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_bitmap, 0, 0, Width, Height);
        }
    }
}