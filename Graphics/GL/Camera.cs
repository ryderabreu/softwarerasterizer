using System;

namespace GraphicsLibrary
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; } = Vector3.UnitY;

        public float FieldOfView { get; set; } = MathF.PI / 3f;
        public float AspectRatio { get; set; } = 1f;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 1000;

        public Camera(Vector3 position, Vector3 target, float aspectRatio)
        {
            Position = position;
            Target = target;
            AspectRatio = aspectRatio;
        }

        public void UpdateAspectRatio(float newAspect)
        {
            AspectRatio = newAspect;
        }

        public Matrix4x4 ViewMatrix()
        {
            Vector3 forward = (Target - Position).Normalized();
            Vector3 right = Vector3.Cross(forward, Up).Normalized();
            Vector3 up = Vector3.Cross(right, forward);

            return new Matrix4x4(new float[,]
            {
                { right.X,   right.Y,   right.Z,   -Vector3.Dot(right, Position) },
                { up.X,      up.Y,      up.Z,      -Vector3.Dot(up, Position) },
                { -forward.X,-forward.Y,-forward.Z, Vector3.Dot(forward, Position) },
                { 0,         0,         0,         1 }
            });
        }

        public Matrix4x4 ProjectionMatrix()
        {
            float f = 1f / MathF.Tan(FieldOfView / 2f);
            float nf = 1f / (NearPlane - FarPlane);

            return new Matrix4x4(new float[,]
            {
                { f / AspectRatio, 0, 0, 0 },
                { 0, f, 0, 0 },
                { 0, 0, (FarPlane + NearPlane) * nf, 2 * FarPlane * NearPlane * nf },
                { 0, 0, -1, 0 }
            });
        }

        public Matrix4x4 ViewProjectionMatrix()
        {
            return ProjectionMatrix() * ViewMatrix();
        }

        public void Orbit(float yawDegrees, float pitchDegrees)
        {
            Vector3 offset = Position - Target;

            float yawRad = yawDegrees * (float)Math.PI / 180f;
            float pitchRad = pitchDegrees * (float)Math.PI / 180f;

            Quaternion qYaw = Quaternion.FromAxisAngle(Up, yawRad);
            offset = Quaternion.RotateVector(offset, qYaw);

            Vector3 right = Vector3.Cross(offset, Up).Normalized();
            Quaternion qPitch = Quaternion.FromAxisAngle(right, pitchRad);

            offset = Quaternion.RotateVector(offset, qPitch);
            Position = Target + offset;

            Vector3 forward = (Target - Position).Normalized();
            Vector3 rightAxis = Vector3.Cross(Up, forward).Normalized();
            Up = Vector3.Cross(forward, rightAxis).Normalized();
        }

        public void Rotate(float yawDegrees, float pitchDegrees)
        {
            float yawRad = yawDegrees * (float)Math.PI / 180f;
            float pitchRad = pitchDegrees * (float)Math.PI / 180f;

            Vector3 forward = (Target - Position).Normalized();

            Quaternion qYaw = Quaternion.FromAxisAngle(Up, yawRad);
            forward = Quaternion.RotateVector(forward, qYaw);

            Vector3 right = Vector3.Cross(forward, Up).Normalized();
            Quaternion qPitch = Quaternion.FromAxisAngle(right, pitchRad);
            forward = Quaternion.RotateVector(forward, qPitch);

            Target = Position + forward;

            Vector3 newRight = Vector3.Cross(forward, Up).Normalized();
            Up = Vector3.Cross(newRight, forward).Normalized();
        }

        public void Translate(Vector3 delta)
        {
            Vector3 forward = (Target - Position).Normalized();
            Vector3 right = Vector3.Cross(forward, Up).Normalized();
            Vector3 up = Up;

            Position += right * delta.X + up * delta.Y + forward * delta.Z;
            Target   += right * delta.X + up * delta.Y + forward * delta.Z;
        }
    }
}