using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Collections.Generic;
using GraphicsLibrary;

namespace MainProgram
{
    class Program
    {
        public static RenderWindow window = new RenderWindow(1920, 1080);
        public static Rasterizer rasterizer = new Rasterizer(window.FrameBuffer);
        public static Scene scene = new Scene();
        public static Texture texture = Texture.FromImage(@"C:\Users\ryder\source\repos\Graphics\Graphics\cattexture.jpg");
        public static Mesh mesh = ObjLoader.Load(@"C:\Users\ryder\source\repos\Graphics\Graphics\cat.obj");

        public static Camera camera = new Camera(
            position: new Vector3(0, 1, -15),
            target: new Vector3(0, 1, 0),
            aspectRatio: (float)window.FrameBuffer.Width / window.FrameBuffer.Height
        );
        public static DirectionalLight light = new DirectionalLight(
            direction: new Vector3(0, -1, -1),
            color: new Color(1f, 1f, 1f),
            intensity: 1f
        );
        public static LightCalculator lightCalc = new LightCalculator(
            light,
            new ShadowMap(1024, 1024),
            light.LightMatrix(5, 1.5f * Vector3.UnitY)
        );

        [STAThread]
        static void Main()
        {
            window.CameraReference = camera;
            mesh.model = Matrix4x4.RotationY(MathF.PI) * Matrix4x4.Translation(new Vector3(0, 0, -30));
            scene.AddMesh(mesh);

            HashSet<Keys> pressedKeys = new HashSet<Keys>();
            window.KeyDown += (s1, e) => pressedKeys.Add(e.KeyCode);
            window.KeyUp   += (s1, e) => pressedKeys.Remove(e.KeyCode);

            window.OnRender = deltaTime =>
            {
                if (pressedKeys.Contains(Keys.Up)) camera.Translate(new Vector3(0, 0.5f, 0));
                if (pressedKeys.Contains(Keys.Down)) camera.Translate(new Vector3(0, -0.5f, 0));
                if (pressedKeys.Contains(Keys.Left)) camera.Rotate(3f, 0);
                if (pressedKeys.Contains(Keys.Right)) camera.Rotate(-3f, 0);
                if (pressedKeys.Contains(Keys.W)) camera.Translate(new Vector3(0, 0, 0.5f));
                if (pressedKeys.Contains(Keys.S)) camera.Translate(new Vector3(0, 0, -0.5f));
                if (pressedKeys.Contains(Keys.A)) camera.Translate(new Vector3(-0.5f, 0, 0));
                if (pressedKeys.Contains(Keys.D)) camera.Translate(new Vector3(0.5f, 0, 0));
                
                lightCalc.ShadowRasterize(scene, rasterizer);
                rasterizer.Clear(Color.Black);
                rasterizer.DrawScene(scene, Shaders.vs, Shaders.fs);
            };

            Application.Run(window);
        }
    }
}