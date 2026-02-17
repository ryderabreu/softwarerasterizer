using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using GraphicsLibrary;
using Microsoft.VisualBasic.Devices;

class Program
{
    [STAThread]
    static void Main()
    {
        RenderWindow window = new RenderWindow(1920, 1080);

        Camera camera = new Camera(
            position: new Vector3(0, 0, -5),
            target: new Vector3(0, 0, 0),
                aspectRatio: (float)window.FrameBuffer.Width / window.FrameBuffer.Height
        );
        window.CameraReference = camera;

        Rasterizer rasterizer = new Rasterizer(window.FrameBuffer);

        Mesh cube = PrimitiveGenerator.CreateSphere(1, 32, 32);

        DirectionalLight light = new DirectionalLight(
            direction: new Vector3(-1, -1, 1),
            color: new Color(1f, 1f, 1f),
            intensity: 1f
        );

        bool up = false, down = false, left = false, right = false, w = false, s = false;

        window.KeyDown += (s1, e) =>
        {
            switch (e.KeyCode)
            {
                case Keys.Up: up = true; break;
                case Keys.Down: down = true; break;
                case Keys.Left: left = true; break;
                case Keys.Right: right = true; break;
                case Keys.W: w = true; break;
                case Keys.S: s = true; break;
            }
        };

        window.KeyUp += (s1, e) =>
        {
            switch (e.KeyCode)
            {
                case Keys.Up: up = false; break;
                case Keys.Down: down = false; break;
                case Keys.Left: left = false; break;
                case Keys.Right: right = false; break;
                case Keys.W: w = false; break;
                case Keys.S: s = false; break;
            }
        };

        VertexShader vs = input =>
        {
            Vector3 clipPos = camera.ViewProjectionMatrix() * input.Position;

            return new VertexShaderOutput
            {
                ClipPosition = clipPos,
                WorldPosition = input.Position,
                Normal = input.Normal,
                Color = input.Color
            };
        };

        FragmentShader fs = input =>
        {
            return light.getColor(input.Normal, input.Color, 0.1f);
        };

        window.OnRender = deltaTime =>
        {
            rasterizer.Clear(Color.Black);

            if (up) camera.Orbit(0, -3f);
            if (down) camera.Orbit(0, 3f);
            if (left) camera.Orbit(3f, 0);
            if (right) camera.Orbit(-3f, 0);
            if (w) camera.Translate(new Vector3(0, 0, 0.5f));
            if (s) camera.Translate(new Vector3(0, 0, -0.5f));

            rasterizer.DrawMesh(cube, vs, fs);
        };

        Application.Run(window);
    }
}