using System;
using System.Windows.Forms;
using GraphicsLibrary;

class Program
{
    [STAThread]
    static void Main()
    {
        RenderWindow window = new RenderWindow(1920, 1080);

        Camera camera = new Camera(
            position: new Vector3(0, 2, -5),
            target: new Vector3(0, 0, 0),
                aspectRatio: (float)window.FrameBuffer.Width / window.FrameBuffer.Height
        );
        window.CameraReference = camera;

        Rasterizer rasterizer = new Rasterizer(window.FrameBuffer);

        Mesh cube = PrimitiveGenerator.CreateCube(1f);

        DirectionalLight light = new DirectionalLight(
            direction: new Vector3(1, -1, 1),
            color: new Color(1f, 1f, 1f),
            intensity: 1f
        );

        float rotationX = 0f;
        float rotationY = 0f;

        VertexShader vs = input =>
        {
            Matrix4x4 model = Matrix4x4.RotationY(rotationY) * Matrix4x4.RotationX(rotationX);

            Vector3 worldPos = model * input.Position;
            Vector3 transformedNormal = model * input.Normal;
            Vector3 viewPos = camera.ViewMatrix() * worldPos;
            Vector4 clipPos = camera.ProjectionMatrix() * new Vector4(viewPos.X, viewPos.Y, viewPos.Z, 1f);
            Vector3 ndcPos = clipPos.ToVector3();

            return new VertexShaderOutput
            {
                ClipPosition = clipPos,
                NDCPosition = ndcPos,
                WorldPosition = worldPos,
                Normal = transformedNormal,
                Color = input.Color
            };
        };

        FragmentShader fs = input =>
        {
            return light.getColor(input.Normal, input.Color, 0.1f);
        };

        window.OnRender = deltaTime =>
        {
            window.KeyPreview = true;

            window.KeyDown += (s, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        rotationY -= 0.0005f;
                        break;
                    case Keys.Right:
                        rotationY += 0.0005f;
                        break;
                    case Keys.Up:
                        rotationX -= 0.0005f;
                        break;
                    case Keys.Down:
                        rotationX += 0.0005f;
                        break;
                }
            };

            rasterizer.Clear(Color.Black);
            rasterizer.DrawMesh(cube, vs, fs);
        };

        Application.Run(window);
    }
}