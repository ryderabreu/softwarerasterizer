#include "Engine/Core/Window.h"
#include "Engine/Core/Renderer.h"
#include "Engine/Components/Camera.h"
#include "Engine/Components/Scene.h"
#include "Engine/Components/Primitives.h"
#include "Engine/Components/DirectionalLight.h"
#include "Engine/Defaults/DefaultShaders.h"
#include "Engine/Defaults/LightingCalculator.h"
#include "Engine/Objects/Texture.h"
#include "Engine/Components/ObjLoader.h"

Camera *g_camera = nullptr;
LightingCalculator *g_lightCalc = nullptr;

int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int)
{
    Window window(1920, 1080, L"Graphics");

    Camera camera(
        {0, 0, 10},
        {0, 0, 0},
        (float)window.GetFrameBuffer().Width / window.GetFrameBuffer().Height);

    ShadowMap shadowMap(1024, 1024);
    DirectionalLight light({0, -1, -1}, Color::White, 1.0f);
    LightingCalculator lightCalc(light, shadowMap, {0, 2, 0}, 10.0f);

    g_camera = &camera;
    g_lightCalc = &lightCalc;

    window.CameraReference = &camera;

    Renderer renderer(window.GetFrameBuffer());

    Scene scene;

    Texture sphereTex = Texture::FromImage(L"C:\\Users\\ryder\\source\\repos\\Graphics\\Graphics_Native\\texture.jpg");
    Texture groundTex = Texture::FromImage(L"C:\\Users\\ryder\\source\\repos\\Graphics\\Graphics_Native\\texture.jpg");

    Mesh sphere = Primitives::CreateSphere(1.0f, 16, 16);
    sphere.model = Matrix4x4::Identity;
    sphere.texture = &sphereTex;
    scene.AddMesh(std::move(sphere));

    Mesh ground = Primitives::CreatePlane(20.0f, 1);
    ground.model = Matrix4x4::Translation({0, -2, 0});
    ground.texture = &groundTex;
    scene.AddMesh(std::move(ground));

    // Texture catTex = Texture::FromImage(L"C:\\Users\\ryder\\source\\repos\\Graphics\\Graphics_Native\\cattexture.jpg");
    // Mesh cat = ObjLoader::Load("C:\\Users\\ryder\\source\\repos\\Graphics\\Graphics_Native\\cat.obj");

    // cat.model = Matrix4x4::Identity;
    // cat.texture = &catTex;
    // scene.AddMesh(std::move(cat));

    window.OnRender = [&](float dt)
    {
        if (window.IsKeyDown(VK_LEFT))
            camera.Rotate(20.0f * dt, 0);
        if (window.IsKeyDown(VK_RIGHT))
            camera.Rotate(-20.0f * dt, 0);
        if (window.IsKeyDown(VK_UP))
            camera.Translate({0, 5.0f * dt, 0});
        if (window.IsKeyDown(VK_DOWN))
            camera.Translate({0, -5.0f * dt, 0});
        if (window.IsKeyDown('W'))
            camera.Translate({0, 0, 5.0f * dt});
        if (window.IsKeyDown('S'))
            camera.Translate({0, 0, -5.0f * dt});
        if (window.IsKeyDown('A'))
            camera.Translate({-5.0f * dt, 0, 0});
        if (window.IsKeyDown('D'))
            camera.Translate({5.0f * dt, 0, 0});

        renderer.SetViewProjection(camera.ViewProjectionMatrix());
        renderer.RenderWithShadows<DefaultTexturedShaders>(scene, lightCalc);
    };

    return window.Run();
}
