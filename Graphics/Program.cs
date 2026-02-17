using System;
using System.Windows.Forms;
using GraphicsLibrary;

class Program
{
    [STAThread]
    static void Main()
    {
        float rotationX = 0f;
        float rotationY = 0f;
        float rotationSpeed = 0.0005f;

        DirectionalLight light = new DirectionalLight(
            new Vector3(1, 1, 1),
            new Color(1f, 1f, 1f),
            intensity: 1f
        );

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
            window.KeyPreview = true;

            window.KeyDown += (s, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        rotationY -= rotationSpeed;
                        break;
                    case Keys.Right:
                        rotationY += rotationSpeed;
                        break;
                    case Keys.Up:
                        rotationX -= rotationSpeed;
                        break;
                    case Keys.Down:
                        rotationX += rotationSpeed;
                        break;
                }
            };

            window.FrameBuffer.Clear(Color.Black);

            Matrix4x4 vp = camera.ViewProjectionMatrix();

            VertexShader vs = input =>
            {
                Matrix4x4 model = Matrix4x4.RotationY(rotationY) * Matrix4x4.RotationX(rotationX);

                Vector3 worldPos = model * input.Position;
                Vector3 transformedNormal = model * input.Normal;

                Vector3 clipPos = camera.ViewProjectionMatrix() * worldPos;

                return new VertexShaderOutput
                {
                    ClipPosition = clipPos,
                    WorldPosition = worldPos,
                    Normal = transformedNormal,
                    Color = input.Color
                };
            };

            FragmentShader fs = input =>
            {
                return light.getColor(input.Normal, input.Color, 0.1f);
            };

            rasterizer.DrawMesh(cube, vs, fs);
        };

        Application.Run(window);
    }
}