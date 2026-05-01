#pragma once
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <functional>
#include <unordered_set>
#include "FrameBuffer.h"
#include "../Components/Camera.h"

class Window {
public:
    std::function<void(float)> OnRender;
    Camera*                    CameraReference = nullptr;

    Window(int fbWidth = 1920, int fbHeight = 1080, const wchar_t* title = L"Program");
    ~Window();

    FrameBuffer& GetFrameBuffer() { return _fb; }

    int Run();

    bool IsKeyDown(int vkCode) const { return _keys.count(vkCode) > 0; }

private:
    int         _fbWidth, _fbHeight;
    FrameBuffer _fb;
    HWND        _hwnd   = nullptr;
    HDC         _memDC  = nullptr;
    HBITMAP     _hDib   = nullptr;
    void*       _dibBits = nullptr;
    LARGE_INTEGER _perfFreq{};
    LARGE_INTEGER _lastTick{};
    std::unordered_set<int> _keys;

    void CreateDib();
    void DestroyDib();
    void Present();
    void BlitToHDC(HDC hdc);
    void OnPaint();
    void OnSize(int w, int h);

    static LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp);
};
