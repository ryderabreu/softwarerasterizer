using System;

namespace GraphicsLibrary
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _target;
        private Vector3 _up = Vector3.UnitY;
        private float _fieldOfView = MathF.PI / 3f;
        private float _aspectRatio = 1f;
        private float _nearPlane = 0.1f;
        private float _farPlane = 1000f;

        private Matrix4x4 _cachedVP;
        private bool _vpDirty = true;

        public Vector3 Position  { get => _position;  set { _position  = value; _vpDirty = true; } }
        public Vector3 Target    { get => _target;    set { _target    = value; _vpDirty = true; } }
        public Vector3 Up        { get => _up;        set { _up        = value; _vpDirty = true; } }
        public float FieldOfView { get => _fieldOfView; set { _fieldOfView = value; _vpDirty = true; } }
        public float AspectRatio { get => _aspectRatio; set { _aspectRatio = value; _vpDirty = true; } }
        public float NearPlane   { get => _nearPlane;   set { _nearPlane   = value; _vpDirty = true; } }
        public float FarPlane    { get => _farPlane;    set { _farPlane    = value; _vpDirty = true; } }

        public Camera(Vector3 position, Vector3 target, float aspectRatio)
        {
            _position    = position;
            _target      = target;
            _aspectRatio = aspectRatio;
        }

        public void UpdateAspectRatio(float newAspect) => AspectRatio = newAspect;

        public Matrix4x4 ViewMatrix()
        {
            Vector3 forward = (_target - _position).Normalized();
            Vector3 right   = Vector3.Cross(forward, _up).Normalized();
            Vector3 up      = Vector3.Cross(right, forward);

            return new Matrix4x4(
                right.X,    right.Y,    right.Z,    -Vector3.Dot(right, _position),
                up.X,       up.Y,       up.Z,       -Vector3.Dot(up, _position),
                -forward.X, -forward.Y, -forward.Z,  Vector3.Dot(forward, _position),
                0,          0,          0,           1);
        }

        public Matrix4x4 ProjectionMatrix()
        {
            float f  = 1f / MathF.Tan(_fieldOfView * 0.5f);
            float nf = 1f / (_nearPlane - _farPlane);

            return new Matrix4x4(
                f / _aspectRatio, 0, 0,                              0,
                0,                f, 0,                              0,
                0,                0, (_farPlane + _nearPlane) * nf,  2f * _farPlane * _nearPlane * nf,
                0,                0, -1,                             0);
        }

        public Matrix4x4 ViewProjectionMatrix()
        {
            if (_vpDirty)
            {
                _cachedVP = ProjectionMatrix() * ViewMatrix();
                _vpDirty  = false;
            }
            return _cachedVP;
        }

        public void Orbit(float yawDegrees, float pitchDegrees)
        {
            Vector3 offset = _position - _target;

            float yawRad   = yawDegrees   * (MathF.PI / 180f);
            float pitchRad = pitchDegrees * (MathF.PI / 180f);

            offset = Quaternion.RotateVector(offset, Quaternion.FromAxisAngle(_up, yawRad));

            Vector3 right = Vector3.Cross(offset, _up).Normalized();
            offset = Quaternion.RotateVector(offset, Quaternion.FromAxisAngle(right, pitchRad));

            _position = _target + offset;
            Vector3 forward  = (_target - _position).Normalized();
            Vector3 rightAxis = Vector3.Cross(_up, forward).Normalized();
            _up = Vector3.Cross(forward, rightAxis).Normalized();
            _vpDirty = true;
        }

        public void Rotate(float yawDegrees, float pitchDegrees)
        {
            float yawRad   = yawDegrees   * (MathF.PI / 180f);
            float pitchRad = pitchDegrees * (MathF.PI / 180f);

            Vector3 forward = (_target - _position).Normalized();
            forward = Quaternion.RotateVector(forward, Quaternion.FromAxisAngle(_up, yawRad));

            Vector3 right = Vector3.Cross(forward, _up).Normalized();
            forward = Quaternion.RotateVector(forward, Quaternion.FromAxisAngle(right, pitchRad));

            _target = _position + forward;
            Vector3 newRight = Vector3.Cross(forward, _up).Normalized();
            _up = Vector3.Cross(newRight, forward).Normalized();
            _vpDirty = true;
        }

        public void Translate(Vector3 delta)
        {
            Vector3 forward = (_target - _position).Normalized();
            Vector3 right   = Vector3.Cross(forward, _up).Normalized();
            Vector3 move    = right * delta.X + _up * delta.Y + forward * delta.Z;

            _position += move;
            _target   += move;
            _vpDirty   = true;
        }
    }
}
