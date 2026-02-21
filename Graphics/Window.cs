using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GraphicsLibrary
{
    public class Window : Form
    {
        private readonly int _fbWidth;
        private readonly int _fbHeight;
        private readonly FrameBuffer _frameBuffer;
        private readonly Bitmap _bitmap;
        private readonly byte[] _pixelBuffer;
        private readonly Timer _timer;
        public Action<float> OnRender;
        private DateTime _lastFrameTime;
        public Camera CameraReference { get; set; }

        public Window(int fbWidth = 1920, int fbHeight = 1080)
        {
            _fbWidth = fbWidth;
            _fbHeight = fbHeight;

            Text = "Program";
            MinimumSize = new Size(400, 300);
            ClientSize = new Size(fbWidth / 2, fbHeight / 2);
            DoubleBuffered = true;

            _frameBuffer = new FrameBuffer(_fbWidth, _fbHeight);
            _bitmap = new Bitmap(_fbWidth, _fbHeight, PixelFormat.Format32bppArgb);
            _pixelBuffer = new byte[_fbWidth * _fbHeight * 4];

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
            for (int y = 0; y < _fbHeight; y++)
            for (int x = 0; x < _fbWidth; x++)
            {
                Color c = _frameBuffer.ColorBuffer[x, y];
                int index = (y * _fbWidth + x) * 4;

                _pixelBuffer[index + 0] = (byte)(Math.Clamp(c.B, 0f, 1f) * 255f);
                _pixelBuffer[index + 1] = (byte)(Math.Clamp(c.G, 0f, 1f) * 255f);
                _pixelBuffer[index + 2] = (byte)(Math.Clamp(c.R, 0f, 1f) * 255f);
                _pixelBuffer[index + 3] = (byte)(Math.Clamp(c.A, 0f, 1f) * 255f);
            }

            BitmapData bmpData = _bitmap.LockBits(
                new Rectangle(0, 0, _fbWidth, _fbHeight),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            Marshal.Copy(_pixelBuffer, 0, bmpData.Scan0, _pixelBuffer.Length);
            _bitmap.UnlockBits(bmpData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            
            float scaleX = (float)ClientSize.Width / _frameBuffer.Width;
            float scaleY = (float)ClientSize.Height / _frameBuffer.Height;
            
            float scale = MathF.Min(scaleX, scaleY);

            int drawWidth = (int)(_frameBuffer.Width * scale);
            int drawHeight = (int)(_frameBuffer.Height * scale);

            int offsetX = (ClientSize.Width - drawWidth) / 2;
            int offsetY = (ClientSize.Height - drawHeight) / 2;

            e.Graphics.Clear(System.Drawing.Color.Black);
            e.Graphics.DrawImage(_bitmap, offsetX, offsetY, drawWidth, drawHeight);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (CameraReference != null)
            {
                CameraReference.UpdateAspectRatio((float)_frameBuffer.Width / _frameBuffer.Height);
            }
        }
    }
}