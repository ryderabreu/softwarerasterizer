using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Collections.Generic;
using GraphicsLibrary;

namespace MainProgram
{
    class Program
    {
        public static Window window = new Window(1920, 1080);

        public static Camera camera = new Camera(
            position: new Vector3(0, 0, -60),
            target: new Vector3(0, 0, 0),
            aspectRatio: (float)window.FrameBuffer.Width / window.FrameBuffer.Height
        );
        public static LightCalculator lightCalc = new LightCalculator(
            new DirectionalLight(
                direction: new Vector3(0, -1, -1),
                color: new Color(1f, 1f, 1f),
                intensity: 1f
            ),
            new ShadowMap(1024, 1024),
            new Vector3(0, 5, 0)
        );

        [STAThread]
        static void Main()
        {
            window.CameraReference = camera;

            Rasterizer rasterizer = new Rasterizer(window.FrameBuffer);
            Scene scene = new Scene();

            Mesh cat = ObjLoader.Load(@"C:\Users\ryder\source\repos\Graphics\Graphics\cat.obj");
            cat.texture = Texture.FromImage(@"C:\Users\ryder\source\repos\Graphics\Graphics\cattexture.jpg");
            cat.model = Matrix4x4.RotationY(MathF.PI);
            scene.AddMesh(cat);

            HashSet<Keys> pressedKeys = new HashSet<Keys>();
            window.KeyDown += (s1, e) => pressedKeys.Add(e.KeyCode);
            window.KeyUp   += (s1, e) => pressedKeys.Remove(e.KeyCode);

            window.OnRender = deltaTime =>
            {
                if (pressedKeys.Contains(Keys.Left)) camera.Rotate(3f, 0);
                if (pressedKeys.Contains(Keys.Right)) camera.Rotate(-3f, 0);
                if (pressedKeys.Contains(Keys.Up)) camera.Translate(new Vector3(0, 0.5f, 0));
                if (pressedKeys.Contains(Keys.Down)) camera.Translate(new Vector3(0, -0.5f, 0));
                if (pressedKeys.Contains(Keys.W)) camera.Translate(new Vector3(0, 0, 0.5f));
                if (pressedKeys.Contains(Keys.S)) camera.Translate(new Vector3(0, 0, -0.5f));
                if (pressedKeys.Contains(Keys.A)) camera.Translate(new Vector3(-0.5f, 0, 0));
                if (pressedKeys.Contains(Keys.D)) camera.Translate(new Vector3(0.5f, 0, 0));
                
                lightCalc.ShadowRasterize(scene, rasterizer);
                rasterizer.Clear(Color.Black);
                rasterizer.DrawScene(scene, new Shaders());
            };

            Application.Run(window);
        }
    }
}