using System;
using System.Windows.Forms;
using GraphicsLibrary;

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();

        RenderWindow window = new RenderWindow(800, 600);

        Mesh cube = PrimitiveGenerator.CreateCube(1f);

        Camera camera = new Camera(
            new Vector3(0, 2, -5),
            new Vector3(0, 0, 0),
            800f / 600f
        );

        Rasterizer rasterizer = new Rasterizer(window.FrameBuffer);

        window.OnRender = deltaTime =>
        {
            window.FrameBuffer.Clear(Color.Black);

            Matrix4x4 vp = camera.ViewProjectionMatrix();

            VertexShader vs = input =>
            {
                return new VertexShaderOutput
                {
                    ClipPosition = vp * input.Position,
                    WorldPosition = input.Position,
                    Normal = input.Normal,
                    Color = input.Color
                };
            };

            FragmentShader fs = input =>
            {
                return input.Color;
            };

            rasterizer.DrawMesh(cube, vs, fs);
        };

        Application.Run(window);
    }
}