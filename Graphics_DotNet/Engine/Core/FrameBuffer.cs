namespace GraphicsLibrary
{
    public class FrameBuffer
    {
        public readonly int Width;
        public readonly int Height;

        public readonly Color[] ColorBuffer;

        public FrameBuffer(int width, int height)
        {
            Width  = width;
            Height = height;

            ColorBuffer = new Color[width * height];
            System.Array.Fill(ColorBuffer, Color.Black);
        }

        public void Clear(Color clearColor) => System.Array.Fill(ColorBuffer, clearColor);
    }
}
