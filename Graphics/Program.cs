using System;
using System.Windows.Forms;
using System.Collections.Generic;
using GraphicsLibrary;

class Program
{
    public static Window window = new Window(1920, 1080, "Program");

    public static Camera camera = new Camera(
        position: new Vector3(0, 0, 10),
        target: new Vector3(0, 0, 0),
        aspectRatio: (float)window.FrameBuffer.Width / window.FrameBuffer.Height
    );

    public static LightingCalculator lightCalc = new LightingCalculator(
        directionallight: new DirectionalLight(
            direction: new Vector3(0, -1, -1),
            color: new Color(1f, 1f, 1f),
            intensity: 1f
        ),
        shadowmap: new ShadowMap(1024, 1024),
        viewpoint: new Vector3(0, 2, 0),
        perspectivesize: 10
    );

    [STAThread]
    static void Main()
    {
        window.CameraReference = camera;
        Renderer renderer = new Renderer(
            frameBuffer: window.FrameBuffer,
            BackfaceCulling: true,
            TwoSideRendering: false
        );

        Scene scene = new Scene();
        
        Mesh sphere = Primitives.CreateSphere(
            radius: 1,
            segments: 16,
            rings: 16
        );
        sphere.model = Matrix4x4.Identity;
        sphere.texture = Texture.FromImage(@"C:\Users\ryder\source\repos\Graphics\Graphics\texture.jpg");
        scene.AddMesh(sphere);

        Mesh ground = Primitives.CreatePlane(
            size: 20,
            subdivisions: 1
        );
        ground.model = Matrix4x4.Translation(new Vector3(0, -2, 0));
        ground.texture = Texture.FromImage(@"C:\Users\ryder\source\repos\Graphics\Graphics\texture.jpg");
        scene.AddMesh(ground);

        HashSet<Keys> pressedKeys = new HashSet<Keys>();
        window.KeyDown += (s1, e) => pressedKeys.Add(e.KeyCode);
        window.KeyUp += (s1, e) => pressedKeys.Remove(e.KeyCode);

        window.OnRender = deltaTime =>
        {
            if (pressedKeys.Contains(Keys.Left)) camera.Rotate(20f * deltaTime, 0);
            if (pressedKeys.Contains(Keys.Right)) camera.Rotate(-20f * deltaTime, 0);
            if (pressedKeys.Contains(Keys.Up)) camera.Translate(new Vector3(0, 3f, 0) * deltaTime);
            if (pressedKeys.Contains(Keys.Down)) camera.Translate(new Vector3(0, -3f, 0) * deltaTime);
            if (pressedKeys.Contains(Keys.W)) camera.Translate(new Vector3(0, 0, 3f) * deltaTime);
            if (pressedKeys.Contains(Keys.S)) camera.Translate(new Vector3(0, 0, -3f) * deltaTime);
            if (pressedKeys.Contains(Keys.A)) camera.Translate(new Vector3(-3f, 0, 0) * deltaTime);
            if (pressedKeys.Contains(Keys.D)) camera.Translate(new Vector3(3f, 0, 0) * deltaTime);
            
            renderer.RenderWithShadows<DefaultShaders>(scene, lightCalc);
        };

        Application.Run(window);
    }
}