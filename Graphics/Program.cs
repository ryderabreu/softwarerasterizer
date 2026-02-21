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
            position: new Vector3(0, 1, -15),
            target: new Vector3(0, 1, 0),
                aspectRatio: (float)window.FrameBuffer.Width / window.FrameBuffer.Height
        );
        window.CameraReference = camera;

        Rasterizer rasterizer = new Rasterizer(window.FrameBuffer);

        Scene scene = new Scene();
        Mesh sphere = PrimitiveGenerator.CreateSphere(3 * Vector3.UnitY);
        Mesh ground = PrimitiveGenerator.CreatePlane(Vector3.Zero, 20, 40);
        scene.AddMesh(sphere);
        scene.AddMesh(ground);
        // scene.AddMesh(ObjLoader.Load(@""));

        DirectionalLight light = new DirectionalLight(
            direction: new Vector3(0, -1, -1),
            color: new Color(1f, 1f, 1f),
            intensity: 1f
        );
        
        LightCalculator lightCalc = new LightCalculator(
            light,
            new ShadowMap(1024, 1024),
            light.LightMatrix(5, 1.5f * Vector3.UnitY)
        );

        Texture texture = Texture.FromImage(@"C:\Users\ryder\source\repos\Graphics\Graphics\texture.jpg");

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

        VertexOut vs(Vertex input)
        {
            return VertexCalculator.Project(input, camera);
        };

        Color fs(FragmentIn input)
        {
            if(input.FrontFace)
                return texture.Sample(input.UV) * lightCalc.Calculate(input.WorldPosition, input.Color, input.Normal);
            else
                return texture.Sample(input.UV) * lightCalc.CalculateWithoutShadows(input.WorldPosition, input.Color, input.Normal);
        };

        window.OnRender = deltaTime =>
        {
            if (up) camera.Translate(new Vector3(0, 0.5f, 0));
            if (down) camera.Translate(new Vector3(0, -0.5f, 0));
            if (left) camera.Rotate(3f, 0);
            if (right) camera.Rotate(-3f, 0);
            if (w) camera.Translate(new Vector3(0, 0, 0.5f));
            if (s) camera.Translate(new Vector3(0, 0, -0.5f));
            if (a) camera.Translate(new Vector3(-0.5f, 0, 0));
            if (d) camera.Translate(new Vector3(0.5f, 0, 0));

            lightCalc.ShadowRasterize(scene, rasterizer);
            rasterizer.Clear(Color.Black);
            rasterizer.DrawScene(scene, vs, fs);
        };

        Application.Run(window);
    }
}