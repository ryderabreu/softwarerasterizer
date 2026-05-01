#pragma once
#include <cmath>
#include "../Math/Matrix4x4.h"
#include "../Math/Quaternion.h"

class Camera {
public:
    Camera(Vector3 position, Vector3 target, float aspectRatio)
        : _position(position), _target(target), _aspectRatio(aspectRatio) {}

    Vector3 GetPosition()    const { return _position; }
    Vector3 GetTarget()      const { return _target; }
    Vector3 GetUp()          const { return _up; }
    float   GetFieldOfView() const { return _fov; }
    float   GetAspectRatio() const { return _aspectRatio; }
    float   GetNearPlane()   const { return _near; }
    float   GetFarPlane()    const { return _far; }

    void SetPosition(Vector3 v)   { _position    = v;    _dirty = true; }
    void SetTarget  (Vector3 v)   { _target      = v;    _dirty = true; }
    void SetUp      (Vector3 v)   { _up          = v;    _dirty = true; }
    void SetFov     (float   f)   { _fov         = f;    _dirty = true; }
    void SetAspect  (float   f)   { _aspectRatio = f;    _dirty = true; }
    void SetNear    (float   f)   { _near        = f;    _dirty = true; }
    void SetFar     (float   f)   { _far         = f;    _dirty = true; }

    void UpdateAspectRatio(float a) { SetAspect(a); }

    Matrix4x4 ViewMatrix() const {
        Vector3 forward = (_target - _position).Normalized();
        Vector3 right   = Vector3::Cross(forward, _up).Normalized();
        Vector3 up      = Vector3::Cross(right, forward);
        return {
            right.X,    right.Y,    right.Z,    -Vector3::Dot(right,   _position),
            up.X,       up.Y,       up.Z,       -Vector3::Dot(up,      _position),
           -forward.X, -forward.Y, -forward.Z,   Vector3::Dot(forward, _position),
            0,          0,          0,           1
        };
    }

    Matrix4x4 ProjectionMatrix() const {
        float f  = 1.0f / tanf(_fov * 0.5f);
        float nf = 1.0f / (_near - _far);
        return {
            f / _aspectRatio, 0, 0,                      0,
            0,                f, 0,                      0,
            0,                0, (_far + _near) * nf,    2.0f * _far * _near * nf,
            0,                0, -1,                     0
        };
    }

    const Matrix4x4& ViewProjectionMatrix() {
        if (_dirty) {
            _cachedVP = ProjectionMatrix() * ViewMatrix();
            _dirty    = false;
        }
        return _cachedVP;
    }

    void Orbit(float yawDeg, float pitchDeg) {
        Vector3 offset = _position - _target;
        float yr = yawDeg   * (3.14159265f / 180.0f);
        float pr = pitchDeg * (3.14159265f / 180.0f);
        offset = Quaternion::RotateVector(offset, Quaternion::FromAxisAngle(_up, yr));
        Vector3 right = Vector3::Cross(offset, _up).Normalized();
        offset = Quaternion::RotateVector(offset, Quaternion::FromAxisAngle(right, pr));
        _position = _target + offset;
        Vector3 fwd   = (_target - _position).Normalized();
        Vector3 rAxis = Vector3::Cross(_up, fwd).Normalized();
        _up    = Vector3::Cross(fwd, rAxis).Normalized();
        _dirty = true;
    }

    void Rotate(float yawDeg, float pitchDeg) {
        float yr = yawDeg   * (3.14159265f / 180.0f);
        float pr = pitchDeg * (3.14159265f / 180.0f);
        Vector3 fwd = (_target - _position).Normalized();
        fwd = Quaternion::RotateVector(fwd, Quaternion::FromAxisAngle(_up, yr));
        Vector3 right = Vector3::Cross(fwd, _up).Normalized();
        fwd = Quaternion::RotateVector(fwd, Quaternion::FromAxisAngle(right, pr));
        _target = _position + fwd;
        Vector3 nr = Vector3::Cross(fwd, _up).Normalized();
        _up    = Vector3::Cross(nr, fwd).Normalized();
        _dirty = true;
    }

    void Translate(Vector3 delta) {
        Vector3 fwd   = (_target - _position).Normalized();
        Vector3 right = Vector3::Cross(fwd, _up).Normalized();
        Vector3 move  = right * delta.X + _up * delta.Y + fwd * delta.Z;
        _position += move;
        _target   += move;
        _dirty     = true;
    }

private:
    Vector3   _position;
    Vector3   _target;
    Vector3   _up          = Vector3::UnitY;
    float     _fov         = 3.14159265f / 3.0f;
    float     _aspectRatio = 1.0f;
    float     _near        = 0.1f;
    float     _far         = 1000.0f;
    Matrix4x4 _cachedVP;
    bool      _dirty       = true;
};
