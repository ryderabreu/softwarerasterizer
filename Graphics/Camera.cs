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
        public float NearPlane { get; set; } = 1f;
        public float FarPlane { get; set; } = 20f;

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

        public void Rotate(float yawDegrees, float pitchDegrees)
        {
            Matrix4x4 yawMatrix = Matrix4x4.RotationY(yawDegrees * (float)Math.PI / 180f);
            Matrix4x4 pitchMatrix = Matrix4x4.RotationX(pitchDegrees * (float)Math.PI / 180f);

            Target = pitchMatrix * yawMatrix * Target;
        }

        public void Translate(Vector3 delta)
        {
            Target = Matrix4x4.Translation(delta) * Target;
        }
    }
}