#include "Window.h"
#include <immintrin.h>
#include <algorithm>

#define TIMER_ID 1

Window::Window(int fbWidth, int fbHeight, const wchar_t* title)
    : _fbWidth(fbWidth), _fbHeight(fbHeight), _fb(fbWidth, fbHeight)
{
    QueryPerformanceFrequency(&_perfFreq);
    QueryPerformanceCounter(&_lastTick);

    WNDCLASSEXW wc = {};
    wc.cbSize        = sizeof(wc);
    wc.lpfnWndProc   = WndProc;
    wc.hInstance     = GetModuleHandleW(nullptr);
    wc.hCursor       = LoadCursor(nullptr, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
    wc.lpszClassName = L"GraphicsWindow";
    RegisterClassExW(&wc);

    RECT rc = { 0, 0, fbWidth / 2, fbHeight / 2 };
    AdjustWindowRect(&rc, WS_OVERLAPPEDWINDOW, FALSE);

    _hwnd = CreateWindowExW(0, L"GraphicsWindow", title,
                             WS_OVERLAPPEDWINDOW | WS_VISIBLE,
                             CW_USEDEFAULT, CW_USEDEFAULT,
                             rc.right - rc.left, rc.bottom - rc.top,
                             nullptr, nullptr, wc.hInstance, this);

    SetTimer(_hwnd, TIMER_ID, 16, nullptr);
}

Window::~Window() {
    KillTimer(_hwnd, TIMER_ID);
    DestroyDib();
}

void Window::CreateDib() {
    DestroyDib();
    HDC screenDC = GetDC(_hwnd);
    _memDC = CreateCompatibleDC(screenDC);

    BITMAPINFO bmi = {};
    bmi.bmiHeader.biSize        = sizeof(BITMAPINFOHEADER);
    bmi.bmiHeader.biWidth       =  _fbWidth;
    bmi.bmiHeader.biHeight      = -_fbHeight;
    bmi.bmiHeader.biPlanes      = 1;
    bmi.bmiHeader.biBitCount    = 32;
    bmi.bmiHeader.biCompression = BI_RGB;

    _hDib = CreateDIBSection(screenDC, &bmi, DIB_RGB_COLORS, &_dibBits, nullptr, 0);
    SelectObject(_memDC, _hDib);
    ReleaseDC(_hwnd, screenDC);
}

void Window::DestroyDib() {
    if (_memDC) { DeleteDC(_memDC);      _memDC   = nullptr; }
    if (_hDib)  { DeleteObject(_hDib);   _hDib    = nullptr; }
    _dibBits = nullptr;
}

int Window::Run() {
    MSG msg;
    while (GetMessageW(&msg, nullptr, 0, 0) > 0) {
        TranslateMessage(&msg);
        DispatchMessageW(&msg);
    }
    return (int)msg.wParam;
}

void Window::Present() {
    if (!_dibBits) return;

    const Color* src = _fb.ColorBuffer.data();
    auto*        dst = (unsigned char*)_dibBits;
    int          n   = _fbWidth * _fbHeight;

#ifdef __SSSE3__
    __m128  scale  = _mm_set1_ps(255.0f);
    __m128  zero   = _mm_setzero_ps();
    __m128  cap    = _mm_set1_ps(255.0f);
    __m128i bgra   = _mm_set_epi8(15, 12, 13, 14, 11, 8, 9, 10, 7, 4, 5, 6, 3, 0, 1, 2);

    const float* cf = (const float*)src;
    int i = 0;
    for (; i <= n - 4; i += 4) {
        __m128 c0 = _mm_loadu_ps(cf + i*4     );
        __m128 c1 = _mm_loadu_ps(cf + i*4 +  4);
        __m128 c2 = _mm_loadu_ps(cf + i*4 +  8);
        __m128 c3 = _mm_loadu_ps(cf + i*4 + 12);

        c0 = _mm_min_ps(_mm_max_ps(_mm_mul_ps(c0, scale), zero), cap);
        c1 = _mm_min_ps(_mm_max_ps(_mm_mul_ps(c1, scale), zero), cap);
        c2 = _mm_min_ps(_mm_max_ps(_mm_mul_ps(c2, scale), zero), cap);
        c3 = _mm_min_ps(_mm_max_ps(_mm_mul_ps(c3, scale), zero), cap);

        __m128i i0 = _mm_cvttps_epi32(c0);
        __m128i i1 = _mm_cvttps_epi32(c1);
        __m128i i2 = _mm_cvttps_epi32(c2);
        __m128i i3 = _mm_cvttps_epi32(c3);

        __m128i s01   = _mm_packs_epi32(i0, i1);
        __m128i s23   = _mm_packs_epi32(i2, i3);
        __m128i bytes = _mm_shuffle_epi8(_mm_packus_epi16(s01, s23), bgra);

        _mm_storeu_si128((__m128i*)(dst + i*4), bytes);
    }
    for (; i < n; i++) {
        const Color& c = src[i];
        dst[i*4  ] = (unsigned char)std::clamp(c.B * 255.0f, 0.0f, 255.0f);
        dst[i*4+1] = (unsigned char)std::clamp(c.G * 255.0f, 0.0f, 255.0f);
        dst[i*4+2] = (unsigned char)std::clamp(c.R * 255.0f, 0.0f, 255.0f);
        dst[i*4+3] = (unsigned char)std::clamp(c.A * 255.0f, 0.0f, 255.0f);
    }
#else
    for (int i = 0; i < n; i++) {
        const Color& c = src[i];
        dst[i*4  ] = (unsigned char)std::clamp(c.B * 255.0f, 0.0f, 255.0f);
        dst[i*4+1] = (unsigned char)std::clamp(c.G * 255.0f, 0.0f, 255.0f);
        dst[i*4+2] = (unsigned char)std::clamp(c.R * 255.0f, 0.0f, 255.0f);
        dst[i*4+3] = (unsigned char)std::clamp(c.A * 255.0f, 0.0f, 255.0f);
    }
#endif
}

void Window::BlitToHDC(HDC hdc) {
    if (!_memDC) return;

    RECT cr;
    GetClientRect(_hwnd, &cr);
    int cw = cr.right - cr.left;
    int ch = cr.bottom - cr.top;

    float sx = (float)cw / _fbWidth;
    float sy = (float)ch / _fbHeight;
    float s  = sx < sy ? sx : sy;

    int dw = (int)(_fbWidth  * s);
    int dh = (int)(_fbHeight * s);
    int ox = (cw - dw) / 2;
    int oy = (ch - dh) / 2;

    SetStretchBltMode(hdc, COLORONCOLOR);
    StretchBlt(hdc, ox, oy, dw, dh, _memDC, 0, 0, _fbWidth, _fbHeight, SRCCOPY);

    // Paint only the letterbox bars — never blank the image area
    HBRUSH black = (HBRUSH)GetStockObject(BLACK_BRUSH);
    if (oy > 0)        { RECT r = {0, 0, cw, oy};           FillRect(hdc, &r, black); }
    if (oy+dh < ch)    { RECT r = {0, oy+dh, cw, ch};       FillRect(hdc, &r, black); }
    if (ox > 0)        { RECT r = {0, oy, ox, oy+dh};       FillRect(hdc, &r, black); }
    if (ox+dw < cw)    { RECT r = {ox+dw, oy, cw, oy+dh};  FillRect(hdc, &r, black); }
}

void Window::OnPaint() {
    PAINTSTRUCT ps;
    HDC hdc = BeginPaint(_hwnd, &ps);
    BlitToHDC(hdc);
    EndPaint(_hwnd, &ps);
}

void Window::OnSize(int w, int h) {
    if (CameraReference)
        CameraReference->UpdateAspectRatio((float)_fbWidth / _fbHeight);
}

LRESULT CALLBACK Window::WndProc(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp) {
    Window* self = nullptr;

    if (msg == WM_NCCREATE) {
        auto* cs = (CREATESTRUCTW*)lp;
        self = (Window*)cs->lpCreateParams;
        SetWindowLongPtrW(hwnd, GWLP_USERDATA, (LONG_PTR)self);
        self->_hwnd = hwnd;
    } else {
        self = (Window*)GetWindowLongPtrW(hwnd, GWLP_USERDATA);
    }

    if (!self) return DefWindowProcW(hwnd, msg, wp, lp);

    switch (msg) {
    case WM_CREATE:
        self->CreateDib();
        return 0;

    case WM_TIMER:
        if (wp == TIMER_ID) {
            LARGE_INTEGER now;
            QueryPerformanceCounter(&now);
            float dt = (float)(now.QuadPart - self->_lastTick.QuadPart)
                       / (float)self->_perfFreq.QuadPart;
            self->_lastTick = now;

            if (self->OnRender) self->OnRender(dt);

            self->Present();

            // Paint directly — bypasses WM_ERASEBKGND and the WM_PAINT dispatch
            HDC hdc = GetDC(hwnd);
            self->BlitToHDC(hdc);
            ReleaseDC(hwnd, hdc);

            // Validate so Windows doesn't queue a redundant WM_PAINT
            ValidateRect(hwnd, nullptr);
        }
        return 0;

    case WM_ERASEBKGND:
        return 1;  // Suppress background erasure — we paint every pixel ourselves

    case WM_PAINT:
        self->OnPaint();  // Handles restores / expose events
        return 0;

    case WM_KEYDOWN:
        self->_keys.insert((int)wp);
        return 0;

    case WM_KEYUP:
        self->_keys.erase((int)wp);
        return 0;

    case WM_SIZE:
        self->OnSize(LOWORD(lp), HIWORD(lp));
        return 0;

    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;
    }

    return DefWindowProcW(hwnd, msg, wp, lp);
}
