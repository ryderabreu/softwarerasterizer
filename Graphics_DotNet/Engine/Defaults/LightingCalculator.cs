namespace GraphicsLibrary
{
    public class LightingCalculator
    {
        public DirectionalLight light;
        public ShadowMap shadowMap;
        public Matrix4x4 lightMatrix;
        public float lightAmbient;
        public float shadowAmbient;
        public float bias;

        public bool ShadowMapDirty = true;

        public LightingCalculator(DirectionalLight directionallight, ShadowMap shadowmap, Vector3 viewpoint, float perspectivesize = 10, float lightambient = 0.1f, float shadowambient = 0.1f, float b = 0.01f)
        {
            light         = directionallight;
            shadowMap     = shadowmap;
            lightAmbient  = lightambient;
            shadowAmbient = shadowambient;
            bias          = b;
            lightMatrix   = light.LightMatrix(perspectivesize, viewpoint);
        }

        public void InvalidateShadow() => ShadowMapDirty = true;

        public Color Calculate(Vector3 worldposition, Color color, Vector3 normal, bool frontfacing = true, bool shadows = true, bool frontOnlyShadows = true)
        {
            if (shadows && (!frontOnlyShadows || frontfacing))
            {
                Vector4 lightSpace = lightMatrix * new Vector4(worldposition.X, worldposition.Y, worldposition.Z, 1f);

                int sx = (int)((lightSpace.X * 0.5f + 0.5f) * shadowMap.Width);
                int sy = (int)((1f - (lightSpace.Y * 0.5f + 0.5f)) * shadowMap.Height);

                if (sx >= 0 && sx < shadowMap.Width &&
                    sy >= 0 && sy < shadowMap.Height)
                {
                    float currentDepth = lightSpace.Z * 0.5f + 0.5f;
                    if (currentDepth > shadowMap.DepthBuffer[sx + sy * shadowMap.Width] + bias)
                        return color * shadowAmbient;
                }
            }

            return light.getColor(normal, color, lightAmbient);
        }

        public VertexOut ShadowVertexShader(VertexOut input)
        {
            return new VertexOut
            {
                ClipPosition = lightMatrix * new Vector4(input.WorldPosition.X, input.WorldPosition.Y, input.WorldPosition.Z, 1f)
            };
        }

        public void ShadowRasterize<Shaders>(Scene scene) where Shaders : Shader
        {
            if (!ShadowMapDirty) return;
            shadowMap.Clear();
            Renderer.RenderShadowMap<Shaders>(scene, input => ShadowVertexShader(Shaders.VertexShader(input)), shadowMap);
            ShadowMapDirty = false;
        }
    }
}
