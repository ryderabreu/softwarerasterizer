#pragma once
#include <omp.h>

template<typename Shaders>
void Renderer::ClipAndRasterize(VertexOut v0, VertexOut v1, VertexOut v2) {
    VertexOut bufA[12], bufB[12];
    int countA = 3;
    bufA[0] = v0; bufA[1] = v1; bufA[2] = v2;

    VertexOut* input  = bufA;
    VertexOut* output = bufB;
    int inputCount = 3;

    for (int plane = 0; plane < 6; plane++) {
        if (inputCount == 0) return;
        int outCount = 0;

        const VertexOut* prev     = &input[inputCount - 1];
        bool             prevIn   = Inside(prev->ClipPosition, plane);

        for (int i = 0; i < inputCount; i++) {
            const VertexOut* cur   = &input[i];
            bool             curIn = Inside(cur->ClipPosition, plane);
            if (curIn) {
                if (!prevIn) output[outCount++] = Intersect(*prev, *cur, plane);
                output[outCount++] = *cur;
            } else if (prevIn) {
                output[outCount++] = Intersect(*prev, *cur, plane);
            }
            prev   = cur;
            prevIn = curIn;
        }

        std::swap(input, output);
        inputCount = outCount;
    }

    for (int i = 1; i < inputCount - 1; i++)
        RasterizeTriangle<Shaders>(input[0], input[i], input[i+1]);
}

template<typename Shaders>
void Renderer::RasterizeTriangle(const VertexOut& v0, const VertexOut& v1, const VertexOut& v2) {
    float invW0 = 1.0f / v0.ClipPosition.W;
    float invW1 = 1.0f / v1.ClipPosition.W;
    float invW2 = 1.0f / v2.ClipPosition.W;

    float ndcX0 = v0.ClipPosition.X * invW0, ndcY0 = v0.ClipPosition.Y * invW0, ndcZ0 = v0.ClipPosition.Z * invW0;
    float ndcX1 = v1.ClipPosition.X * invW1, ndcY1 = v1.ClipPosition.Y * invW1, ndcZ1 = v1.ClipPosition.Z * invW1;
    float ndcX2 = v2.ClipPosition.X * invW2, ndcY2 = v2.ClipPosition.Y * invW2, ndcZ2 = v2.ClipPosition.Z * invW2;

    float W = (float)_fb.Width, H = (float)_fb.Height;
    float fx0 = (ndcX0 * 0.5f + 0.5f) * W, fy0 = (1.0f - (ndcY0 * 0.5f + 0.5f)) * H;
    float fx1 = (ndcX1 * 0.5f + 0.5f) * W, fy1 = (1.0f - (ndcY1 * 0.5f + 0.5f)) * H;
    float fx2 = (ndcX2 * 0.5f + 0.5f) * W, fy2 = (1.0f - (ndcY2 * 0.5f + 0.5f)) * H;

    // Guard against NaN/Inf from degenerate clip coords (e.g. W near zero)
    if (!std::isfinite(fx0) || !std::isfinite(fy0) ||
        !std::isfinite(fx1) || !std::isfinite(fy1) ||
        !std::isfinite(fx2) || !std::isfinite(fy2)) return;

    float area = EdgeFn(fx0,fy0, fx1,fy1, fx2,fy2);
    if (fabsf(area) < 1e-6f) return;

    bool frontFace = area > 0.0f;
    if (EnableBackfaceCulling && !TwoSided && !frontFace) return;

    int minX = std::max(0,             (int)floorf(std::min({fx0,fx1,fx2})));
    int maxX = std::min(_fb.Width  - 1, (int)ceilf (std::max({fx0,fx1,fx2})));
    int minY = std::max(0,             (int)floorf(std::min({fy0,fy1,fy2})));
    int maxY = std::min(_fb.Height - 1, (int)ceilf (std::max({fy0,fy1,fy2})));
    if (minX > maxX || minY > maxY) return;  // Triangle fully outside viewport

    float ex0 = fy2-fy1, ey0 = fx1-fx2;
    float ex1 = fy0-fy2, ey1 = fx2-fx0;
    float ex2 = fy1-fy0, ey2 = fx0-fx1;

    constexpr int tileSize = 32;
    int tileMinX = minX / tileSize, tileMaxX = maxX / tileSize;
    int tileMinY = minY / tileSize, tileMaxY = maxY / tileSize;
    int totalTiles = (tileMaxX - tileMinX + 1) * (tileMaxY - tileMinY + 1);

    int fbWidth = _fb.Width;
    Color* colorBuf = _fb.ColorBuffer.data();
    float* depthBuf = _depth.data();

    float wp0x = v0.WorldPosition.X, wp0y = v0.WorldPosition.Y, wp0z = v0.WorldPosition.Z;
    float wp1x = v1.WorldPosition.X, wp1y = v1.WorldPosition.Y, wp1z = v1.WorldPosition.Z;
    float wp2x = v2.WorldPosition.X, wp2y = v2.WorldPosition.Y, wp2z = v2.WorldPosition.Z;
    float n0x = v0.Normal.X, n0y = v0.Normal.Y, n0z = v0.Normal.Z;
    float n1x = v1.Normal.X, n1y = v1.Normal.Y, n1z = v1.Normal.Z;
    float n2x = v2.Normal.X, n2y = v2.Normal.Y, n2z = v2.Normal.Z;
    float c0r = v0.Color.R, c0g = v0.Color.G, c0b = v0.Color.B;
    float c1r = v1.Color.R, c1g = v1.Color.G, c1b = v1.Color.B;
    float c2r = v2.Color.R, c2g = v2.Color.G, c2b = v2.Color.B;
    float uv0x = v0.UV.X, uv0y = v0.UV.Y;
    float uv1x = v1.UV.X, uv1y = v1.UV.Y;
    float uv2x = v2.UV.X, uv2y = v2.UV.Y;

    __m256 ex0Steps = _mm256_mul_ps(_mm256_set1_ps(ex0), _mm256_set_ps(7,6,5,4,3,2,1,0));
    __m256 ex1Steps = _mm256_mul_ps(_mm256_set1_ps(ex1), _mm256_set_ps(7,6,5,4,3,2,1,0));
    __m256 ex2Steps = _mm256_mul_ps(_mm256_set1_ps(ex2), _mm256_set_ps(7,6,5,4,3,2,1,0));
    __m256 invW0V   = _mm256_set1_ps(invW0);
    __m256 invW1V   = _mm256_set1_ps(invW1);
    __m256 invW2V   = _mm256_set1_ps(invW2);
    __m256 ndc0zV   = _mm256_set1_ps(ndcZ0);
    __m256 ndc1zV   = _mm256_set1_ps(ndcZ1);
    __m256 ndc2zV   = _mm256_set1_ps(ndcZ2);
    __m256 zeroV    = _mm256_setzero_ps();
    __m256 halfV    = _mm256_set1_ps(0.5f);
    __m256 oneV     = _mm256_set1_ps(1.0f);

    auto processRow = [&](int ty) {
        int startY = ty * tileSize;
        int endY   = std::min(startY + tileSize - 1, maxY);

        for (int tx = tileMinX; tx <= tileMaxX; tx++) {
            int startX = tx * tileSize;
            int endX   = std::min(startX + tileSize - 1, maxX);

            float w0r = EdgeFn(fx1,fy1, fx2,fy2, startX+0.5f, startY+0.5f);
            float w1r = EdgeFn(fx2,fy2, fx0,fy0, startX+0.5f, startY+0.5f);
            float w2r = EdgeFn(fx0,fy0, fx1,fy1, startX+0.5f, startY+0.5f);

            for (int y = startY; y <= endY; y++) {
                float w0 = w0r, w1 = w1r, w2 = w2r;
                int x = startX;

#ifdef __AVX__
                for (; x <= endX - 7; x += 8) {
                    __m256 w0V = _mm256_add_ps(_mm256_set1_ps(w0), ex0Steps);
                    __m256 w1V = _mm256_add_ps(_mm256_set1_ps(w1), ex1Steps);
                    __m256 w2V = _mm256_add_ps(_mm256_set1_ps(w2), ex2Steps);

                    int insideBits = _mm256_movemask_ps(_mm256_and_ps(_mm256_and_ps(
                        _mm256_cmp_ps(w0V, zeroV, _CMP_GE_OQ),
                        _mm256_cmp_ps(w1V, zeroV, _CMP_GE_OQ)),
                        _mm256_cmp_ps(w2V, zeroV, _CMP_GE_OQ)));

                    if (insideBits) {
                        __m256 bw0   = _mm256_mul_ps(w0V, invW0V);
                        __m256 bw1   = _mm256_mul_ps(w1V, invW1V);
                        __m256 bw2   = _mm256_mul_ps(w2V, invW2V);
                        __m256 WV    = _mm256_div_ps(oneV, _mm256_add_ps(_mm256_add_ps(bw0, bw1), bw2));
                        __m256 b0V   = _mm256_mul_ps(bw0, WV);
                        __m256 b1V   = _mm256_mul_ps(bw1, WV);
                        __m256 b2V   = _mm256_mul_ps(bw2, WV);

                        __m256 depthV = _mm256_add_ps(halfV, _mm256_mul_ps(halfV,
                            _mm256_add_ps(_mm256_add_ps(
                                _mm256_mul_ps(ndc0zV, b0V),
                                _mm256_mul_ps(ndc1zV, b1V)),
                                _mm256_mul_ps(ndc2zV, b2V))));

                        int baseIdx = x + y * fbWidth;
                        __m256 stored = _mm256_loadu_ps(&depthBuf[baseIdx]);
                        int surviveBits = insideBits & _mm256_movemask_ps(
                            _mm256_cmp_ps(depthV, stored, _CMP_LT_OQ));

                        while (surviveBits) {
                            int bit = _tzcnt_u32((unsigned)surviveBits);
                            float b0 = ((float*)&b0V)[bit];
                            float b1 = ((float*)&b1V)[bit];
                            float b2 = ((float*)&b2V)[bit];
                            int   idx = baseIdx + bit;

                            depthBuf[idx] = ((float*)&depthV)[bit];

                            Vector3 wp = { wp0x*b0+wp1x*b1+wp2x*b2,
                                           wp0y*b0+wp1y*b1+wp2y*b2,
                                           wp0z*b0+wp1z*b1+wp2z*b2 };
                            float nx_ = n0x*b0+n1x*b1+n2x*b2,
                                  ny_ = n0y*b0+n1y*b1+n2y*b2,
                                  nz_ = n0z*b0+n1z*b1+n2z*b2;
                            float invN = 1.0f / sqrtf(nx_*nx_+ny_*ny_+nz_*nz_);
                            Vector2 uv = { uv0x*b0+uv1x*b1+uv2x*b2,
                                           uv0y*b0+uv1y*b1+uv2y*b2 };

                            colorBuf[idx] = Shaders::FragmentShader({
                                wp,
                                { nx_*invN, ny_*invN, nz_*invN },
                                { c0r*b0+c1r*b1+c2r*b2, c0g*b0+c1g*b1+c2g*b2,
                                  c0b*b0+c1b*b1+c2b*b2, 1.0f },
                                uv, frontFace
                            });

                            surviveBits &= surviveBits - 1;
                        }
                    }

                    w0 += ex0 * 8; w1 += ex1 * 8; w2 += ex2 * 8;
                }
#endif
                for (; x <= endX; x++) {
                    if (w0 >= 0 && w1 >= 0 && w2 >= 0) {
                        float sum = w0*invW0 + w1*invW1 + w2*invW2;
                        float Wv  = 1.0f / sum;
                        float b0  = w0*invW0*Wv, b1 = w1*invW1*Wv, b2 = w2*invW2*Wv;
                        float depth = (ndcZ0*b0 + ndcZ1*b1 + ndcZ2*b2) * 0.5f + 0.5f;

                        int idx = x + y * fbWidth;
                        if (depth < depthBuf[idx]) {
                            depthBuf[idx] = depth;

                            Vector3 wp = { wp0x*b0+wp1x*b1+wp2x*b2,
                                           wp0y*b0+wp1y*b1+wp2y*b2,
                                           wp0z*b0+wp1z*b1+wp2z*b2 };
                            float nx_ = n0x*b0+n1x*b1+n2x*b2,
                                  ny_ = n0y*b0+n1y*b1+n2y*b2,
                                  nz_ = n0z*b0+n1z*b1+n2z*b2;
                            float invN = 1.0f / sqrtf(nx_*nx_+ny_*ny_+nz_*nz_);
                            Vector2 uv = { uv0x*b0+uv1x*b1+uv2x*b2,
                                           uv0y*b0+uv1y*b1+uv2y*b2 };

                            colorBuf[idx] = Shaders::FragmentShader({
                                wp,
                                { nx_*invN, ny_*invN, nz_*invN },
                                { c0r*b0+c1r*b1+c2r*b2, c0g*b0+c1g*b1+c2g*b2,
                                  c0b*b0+c1b*b1+c2b*b2, 1.0f },
                                uv, frontFace
                            });
                        }
                    }
                    w0 += ex0; w1 += ex1; w2 += ex2;
                }

                w0r += ey0; w1r += ey1; w2r += ey2;
            }
        }
    };

    if (totalTiles > 4) {
        #pragma omp parallel for schedule(dynamic)
        for (int ty = tileMinY; ty <= tileMaxY; ty++)
            processRow(ty);
    } else {
        for (int ty = tileMinY; ty <= tileMaxY; ty++)
            processRow(ty);
    }
}
