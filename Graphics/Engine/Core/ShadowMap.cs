namespace GraphicsLibrary
{
    public class ShadowMap
    {
        public int Width { get; }
        public int Height { get; }

        public float[,] DepthBuffer;

        public ShadowMap(int width, int height)
        {
            Width = width;
            Height = height;
            DepthBuffer = new float[width, height];
            Clear();
        }

        public void Clear()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    DepthBuffer[x, y] = float.MaxValue;
        }
    }
}
