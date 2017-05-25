namespace BundtCake
{
    public class Camera
    {
        public Transform Transform = new Transform();
        public float VerticalFieldOfView = 45;
        public float NearClippingPlane = 0.1f;
        public float FarClippingPlane = 1000f;
    }
}