using System.Configuration;
using System.Reflection;

namespace GraphicsLibrary
{
    public class LightCalculator
    {
        public DirectionalLight light;
        public ShadowMap shadowMap;
        public Matrix4x4 lightMatrix;
        public float lightAmbient;
        public float shadowAmbient;
        public float bias;

        public LightCalculator(DirectionalLight directionallight, ShadowMap shadowmap, Matrix4x4 lightmatrix, float lightambient = 0.1f, float shadowambient = 0.3f, float b = 0.01f)
        {
            light = directionallight;
            shadowMap = shadowmap;
            lightMatrix = lightmatrix;
            lightAmbient = lightambient;
            shadowAmbient = shadowambient;
            bias = b;
        }

        public Color Calculate(Vector3 WorldPos, Color color, Vector3 normal)
        {
            Vector3 worldPos = WorldPos;
            Vector4 lightSpace = lightMatrix * new Vector4(worldPos.X, worldPos.Y, worldPos.Z, 1f);

            int sx = (int)(((lightSpace.X * 0.5f) + 0.5f) * shadowMap.Width);
            int sy = (int)((1f - ((lightSpace.Y * 0.5f) + 0.5f)) * shadowMap.Height);

            bool inShadow = false;

            if (sx >= 0 && sx < shadowMap.Width &&
                sy >= 0 && sy < shadowMap.Height)
            {
                float shadowDepth = shadowMap.DepthBuffer[sx, sy];
                float currentDepth = (lightSpace.Z * 0.5f) + 0.5f;

                if (currentDepth > shadowDepth + bias)
                    inShadow = true;
            }

            if (inShadow)
                return color * shadowAmbient;

            return light.getColor(normal, color, lightAmbient);
        }

        public Color CalculateWithoutShadows(Vector3 WorldPos, Color color, Vector3 normal)
        {
            return light.getColor(normal, color, lightAmbient);
        }

        public VertexOut ShadowVertexShader(Vertex input)
        {
            VertexOut output = new VertexOut();
            output.ClipPosition = lightMatrix * new Vector4(input.Position.X, input.Position.Y, input.Position.Z, 1f);
            return output;
        }

        public void ShadowRasterize(Scene scene, Rasterizer rasterizer)
        {
            shadowMap.Clear();
            rasterizer.RenderShadowMap(scene, ShadowVertexShader, shadowMap);
        }
    }
}