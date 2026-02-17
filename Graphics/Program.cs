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

        Rasterizer rasterizer = new Rasterizer(window.FrameBuffer, true);

        Mesh cube = PrimitiveGenerator.CreateSphere(1, 32, 32);

        DirectionalLight light1 = new DirectionalLight(
            direction: new Vector3(-1, -1, -1),
            color: new Color(1f, 0f, 0f),
            intensity: 1f
        );

        DirectionalLight light2 = new DirectionalLight(
            direction: new Vector3(1, 1, 1),
            color: new Color(0f, 0f, 1f),
            intensity: 1f
        );

        bool up = false, down = false, left = false, right = false, w = false, s = false, a = false, d = false;

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
                case Keys.A: a = true; break;
                case Keys.D: d = true; break;
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
                case Keys.A: a = false; break;
                case Keys.D: d = false; break;
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
                Color = input.Color,
                CameraPosition = camera.Position
            };
        };

        FragmentShader fs = input =>
        {
            return light1.getColor(input.Normal, input.Color, 0f) + light2.getColor(input.Normal, input.Color, 0f);
        };

        window.OnRender = deltaTime =>
        {
            rasterizer.Clear(Color.Black);

            if (up) camera.Orbit(0, 3f);
            if (down) camera.Orbit(0, -3f);
            if (left) camera.Orbit(3f, 0);
            if (right) camera.Orbit(-3f, 0);
            if (w) camera.Translate(new Vector3(0, 0, 0.5f));
            if (s) camera.Translate(new Vector3(0, 0, -0.5f));
            if (a) camera.Translate(new Vector3(-0.5f, 0, 0));
            if (d) camera.Translate(new Vector3(0.5f, 0, 0));

            rasterizer.DrawMesh(cube, vs, fs);
        };

        Application.Run(window);
    }
}