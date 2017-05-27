using System.Numerics;

namespace BundtCake
{
    public class Camera
    {
        public readonly Transform Transform = new Transform();
        public float VerticalFieldOfView = 45;
        public float NearClippingPlane = 0.1f;
        public float FarClippingPlane = 1000f;

        public Matrix4x4 CreateViewMatrix()
        {
            var forwardVector = new Vector3(0, 0, 1);

            var forwardMat = Matrix4x4.CreateRotationX(BundtMaths.DegressToRadians(Transform.Rotation.X));
            forwardMat *= Matrix4x4.CreateRotationY(BundtMaths.DegressToRadians(Transform.Rotation.Y));
            forwardMat *= Matrix4x4.CreateRotationZ(BundtMaths.DegressToRadians(Transform.Rotation.Z));

            forwardVector = Vector3.Transform(forwardVector, forwardMat);

            var upVector = new Vector3(0, 1, 0);
            upVector = Vector3.Transform(upVector, forwardMat);

            return Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + forwardVector, upVector);
        }

        public Matrix4x4 CreateProjectionMatrix(uint screenWidth, uint screenHeight)
        {
            var aspectRatio = screenWidth / (float)screenHeight;
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(BundtMaths.DegressToRadians(VerticalFieldOfView), aspectRatio, NearClippingPlane, FarClippingPlane);
            // Flipping view
            projection.M22 *= -1;

            return projection;
        }
    }
}