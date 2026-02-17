namespace GraphicsLibrary
{
    public class FrameBuffer
    {
        public readonly int Width;
        public readonly int Height;

        public readonly Color[,] ColorBuffer;
        public readonly float[,] DepthBuffer;

        public FrameBuffer(int width, int height)
        {
            Width = width;
            Height = height;

            ColorBuffer = new Color[width, height];
            DepthBuffer = new float[width, height];

            Clear(Color.Black);
        }

        public void Clear(Color clearColor)
        {
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                ColorBuffer[x,y] = clearColor;
                DepthBuffer[x,y] = float.MaxValue;
            }
        }
    }
}