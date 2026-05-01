using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Windows.Forms;

namespace GraphicsLibrary
{
    public class Window : Form
    {
        private readonly int _fbWidth;
        private readonly int _fbHeight;
        private readonly FrameBuffer _frameBuffer;
        private readonly Timer _timer;
        public Action<float> OnRender;
        private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        private long _lastFrameTicks;
        public Camera CameraReference { get; set; }

        private IntPtr _hMemDC;
        private IntPtr _hDib;
        private IntPtr _dibBitsPtr;

        public Window(int fbWidth = 1920, int fbHeight = 1080, string title = "Program")
        {
            _fbWidth  = fbWidth;
            _fbHeight = fbHeight;

            Text           = title;
            MinimumSize    = new Size(400, 300);
            ClientSize     = new Size(fbWidth / 2, fbHeight / 2);
            DoubleBuffered = true;

            _frameBuffer = new FrameBuffer(_fbWidth, _fbHeight);

            _timer = new Timer { Interval = 16 };
            _timer.Tick += RenderLoop;
            _timer.Start();
        }

        public FrameBuffer FrameBuffer => _frameBuffer;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            CreateDib();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DestroyDib();
            base.OnHandleDestroyed(e);
        }

        private void CreateDib()
        {
            DestroyDib();

            IntPtr screenDC = GetDC(Handle);

            _hMemDC = CreateCompatibleDC(screenDC);

            var bmi = new BITMAPINFO
            {
                bmiHeader = new BITMAPINFOHEADER
                {
                    biSize        = Marshal.SizeOf<BITMAPINFOHEADER>(),
                    biWidth       =  _fbWidth,
                    biHeight      = -_fbHeight,   // negative = top-down, matches our row order
                    biPlanes      = 1,
                    biBitCount    = 32,
                    biCompression = BI_RGB
                }
            };

            _hDib = CreateDIBSection(screenDC, ref bmi, DIB_RGB_COLORS, out _dibBitsPtr, IntPtr.Zero, 0);
            SelectObject(_hMemDC, _hDib);

            ReleaseDC(Handle, screenDC);
        }

        private void DestroyDib()
        {
            if (_hMemDC != IntPtr.Zero) { DeleteDC(_hMemDC); _hMemDC = IntPtr.Zero; }
            if (_hDib   != IntPtr.Zero) { DeleteObject(_hDib); _hDib = IntPtr.Zero; }
        }

        private void RenderLoop(object sender, EventArgs e)
        {
            long now        = _stopwatch.ElapsedTicks;
            float deltaTime = (float)(now - _lastFrameTicks) / System.Diagnostics.Stopwatch.Frequency;
            _lastFrameTicks = now;

            OnRender?.Invoke(deltaTime);

            Present();
            Invalidate();
        }

        private unsafe void Present()
        {
            if (_dibBitsPtr == IntPtr.Zero) return;

            var colorBuffer = _frameBuffer.ColorBuffer;
            int n = colorBuffer.Length;

            ref byte pbRef = ref *(byte*)_dibBitsPtr;

            if (Ssse3.IsSupported)
            {
                var bgraShuffle = Vector128.Create((byte)2, 1, 0, 3, 6, 5, 4, 7, 10, 9, 8, 11, 14, 13, 12, 15);
                var scale = Vector128.Create(255f);
                var zero  = Vector128<float>.Zero;
                var cap   = Vector128.Create(255f);

                var colorSpan = MemoryMarshal.Cast<Color, float>(colorBuffer.AsSpan());
                ref float cfRef = ref MemoryMarshal.GetReference(colorSpan);

                int i = 0;
                for (; i <= n - 4; i += 4)
                {
                    nuint fi = (nuint)(i * 4);

                    var c0 = Vector128.LoadUnsafe(ref cfRef, fi);
                    var c1 = Vector128.LoadUnsafe(ref cfRef, fi + 4);
                    var c2 = Vector128.LoadUnsafe(ref cfRef, fi + 8);
                    var c3 = Vector128.LoadUnsafe(ref cfRef, fi + 12);

                    c0 = Sse.Min(Sse.Max(Sse.Multiply(c0, scale), zero), cap);
                    c1 = Sse.Min(Sse.Max(Sse.Multiply(c1, scale), zero), cap);
                    c2 = Sse.Min(Sse.Max(Sse.Multiply(c2, scale), zero), cap);
                    c3 = Sse.Min(Sse.Max(Sse.Multiply(c3, scale), zero), cap);

                    var i0 = Sse2.ConvertToVector128Int32WithTruncation(c0);
                    var i1 = Sse2.ConvertToVector128Int32WithTruncation(c1);
                    var i2 = Sse2.ConvertToVector128Int32WithTruncation(c2);
                    var i3 = Sse2.ConvertToVector128Int32WithTruncation(c3);

                    var s01   = Sse2.PackSignedSaturate(i0, i1);
                    var s23   = Sse2.PackSignedSaturate(i2, i3);
                    var bytes = Ssse3.Shuffle(Sse2.PackUnsignedSaturate(s01, s23), bgraShuffle);

                    Vector128.StoreUnsafe(bytes, ref pbRef, (nuint)(i * 4));
                }

                for (; i < n; i++)
                {
                    var c     = colorBuffer[i];
                    nuint off = (nuint)(i * 4);
                    System.Runtime.CompilerServices.Unsafe.Add(ref pbRef, off    ) = (byte)Math.Clamp(c.B * 255f, 0f, 255f);
                    System.Runtime.CompilerServices.Unsafe.Add(ref pbRef, off + 1) = (byte)Math.Clamp(c.G * 255f, 0f, 255f);
                    System.Runtime.CompilerServices.Unsafe.Add(ref pbRef, off + 2) = (byte)Math.Clamp(c.R * 255f, 0f, 255f);
                    System.Runtime.CompilerServices.Unsafe.Add(ref pbRef, off + 3) = (byte)Math.Clamp(c.A * 255f, 0f, 255f);
                }
            }
            else
            {
                byte* ptr = (byte*)_dibBitsPtr;
                for (int i = 0; i < n; i++)
                {
                    var c   = colorBuffer[i];
                    int off = i * 4;
                    ptr[off    ] = (byte)Math.Clamp(c.B * 255f, 0f, 255f);
                    ptr[off + 1] = (byte)Math.Clamp(c.G * 255f, 0f, 255f);
                    ptr[off + 2] = (byte)Math.Clamp(c.R * 255f, 0f, 255f);
                    ptr[off + 3] = (byte)Math.Clamp(c.A * 255f, 0f, 255f);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_hMemDC == IntPtr.Zero) return;

            float scaleX = (float)ClientSize.Width  / _fbWidth;
            float scaleY = (float)ClientSize.Height / _fbHeight;
            float scale  = MathF.Min(scaleX, scaleY);

            int drawWidth  = (int)(_fbWidth  * scale);
            int drawHeight = (int)(_fbHeight * scale);
            int offsetX    = (ClientSize.Width  - drawWidth)  / 2;
            int offsetY    = (ClientSize.Height - drawHeight) / 2;

            IntPtr hdc = e.Graphics.GetHdc();

            // COLORONCOLOR drops eliminated pixels (nearest-neighbour equivalent).
            // The default BLACKONWHITE mode ANDs merged pixels, causing darkening.
            SetStretchBltMode(hdc, COLORONCOLOR);

            PatBlt(hdc, 0, 0, ClientSize.Width, ClientSize.Height, BLACKNESS);
            StretchBlt(hdc, offsetX, offsetY, drawWidth, drawHeight,
                       _hMemDC, 0, 0, _fbWidth, _fbHeight, SRCCOPY);

            e.Graphics.ReleaseHdc(hdc);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CameraReference?.UpdateAspectRatio((float)_fbWidth / _fbHeight);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer?.Dispose();
            DestroyDib();
            base.Dispose(disposing);
        }

        // ── Win32 P/Invoke ────────────────────────────────────────────────────

        [DllImport("gdi32")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32")] private static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);
        [DllImport("gdi32")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);
        [DllImport("gdi32")] private static extern bool   DeleteDC(IntPtr hdc);
        [DllImport("gdi32")] private static extern bool   DeleteObject(IntPtr ho);
        [DllImport("gdi32")] private static extern bool   StretchBlt(IntPtr hdcDst, int x, int y, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, uint rop);
        [DllImport("gdi32")] private static extern bool   PatBlt(IntPtr hdc, int x, int y, int w, int h, uint rop);
        [DllImport("gdi32")] private static extern int    SetStretchBltMode(IntPtr hdc, int mode);
        [DllImport("user32")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32")] private static extern int    ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private const uint SRCCOPY      = 0x00CC0020;
        private const uint BLACKNESS    = 0x00000042;
        private const int  COLORONCOLOR = 3;
        private const uint BI_RGB         = 0;
        private const uint DIB_RGB_COLORS = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int   biSize;
            public int   biWidth;
            public int   biHeight;
            public short biPlanes;
            public short biBitCount;
            public uint  biCompression;
            public uint  biSizeImage;
            public int   biXPelsPerMeter;
            public int   biYPelsPerMeter;
            public uint  biClrUsed;
            public uint  biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public uint             bmiColors;
        }
    }
}
