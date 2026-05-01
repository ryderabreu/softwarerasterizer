#include "Texture.h"
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <wincodec.h>
#pragma comment(lib, "windowscodecs.lib")
#pragma comment(lib, "ole32.lib")

Texture Texture::FromImage(const std::wstring& filePath) {
    Texture tex;

    // --- COM + WIC setup ---
    HRESULT hr = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    bool comOwned = SUCCEEDED(hr);          // S_OK or S_FALSE (already init'd)

    IWICImagingFactory*    factory   = nullptr;
    IWICBitmapDecoder*     decoder   = nullptr;
    IWICBitmapFrameDecode* frame     = nullptr;
    IWICFormatConverter*   converter = nullptr;

    hr = CoCreateInstance(CLSID_WICImagingFactory, nullptr,
                          CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&factory));
    if (FAILED(hr) || !factory) goto cleanup;

    hr = factory->CreateDecoderFromFilename(filePath.c_str(), nullptr,
                                            GENERIC_READ,
                                            WICDecodeMetadataCacheOnDemand, &decoder);
    if (FAILED(hr) || !decoder) goto cleanup;

    hr = decoder->GetFrame(0, &frame);
    if (FAILED(hr) || !frame) goto cleanup;

    hr = factory->CreateFormatConverter(&converter);
    if (FAILED(hr) || !converter) goto cleanup;

    hr = converter->Initialize(frame, GUID_WICPixelFormat32bppRGBA,
                               WICBitmapDitherTypeNone, nullptr, 0.0,
                               WICBitmapPaletteTypeCustom);
    if (FAILED(hr)) goto cleanup;

    {
        UINT width = 0, height = 0;
        converter->GetSize(&width, &height);

        if (width > 0 && height > 0) {
            tex.Width  = (int)width;
            tex.Height = (int)height;
            tex.Pixels.resize(width * height);

            // Single bulk copy — same approach as the C# LockBits version
            std::vector<BYTE> raw(width * height * 4);
            converter->CopyPixels(nullptr, width * 4, (UINT)raw.size(), raw.data());

            for (UINT y = 0; y < height; y++) {
                UINT base = y * width * 4;
                for (UINT x = 0; x < width; x++) {
                    UINT o = base + x * 4;
                    // WIC GUID_WICPixelFormat32bppRGBA: R at o+0, G at o+1, B at o+2, A at o+3
                    tex.Pixels[x + y * width] = Color(
                        raw[o  ] * (1.0f / 255.0f),
                        raw[o+1] * (1.0f / 255.0f),
                        raw[o+2] * (1.0f / 255.0f),
                        raw[o+3] * (1.0f / 255.0f));
                }
            }
        }
    }

cleanup:
    if (converter) converter->Release();
    if (frame)     frame->Release();
    if (decoder)   decoder->Release();
    if (factory)   factory->Release();
    if (comOwned)  CoUninitialize();

    // If loading failed for any reason, return a 1×1 white texture as fallback
    if (tex.Pixels.empty()) {
        tex.Width = tex.Height = 1;
        tex.Pixels.push_back(Color::White);
    }

    return tex;
}
