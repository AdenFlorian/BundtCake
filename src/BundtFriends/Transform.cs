using System.Numerics;

namespace BundtCake
{
    public class Transform
    {
        public Vector3 Position = new Vector3();
        public Vector3 Rotation = new Vector3();
        public Vector3 Scale = new Vector3(1f, 1f, 1f);

        public Vector3 Forward
        {
            get
            {
                return Vector3.Transform(Vector3.UnitZ, CreateRotationMatrix());
            }
        }

        public Vector3 Right
        {
            get
            {
                return Vector3.Transform(Vector3.UnitX, CreateRotationMatrix());
            }
        }

        public Vector3 Up
        {
            get
            {
                return Vector3.Transform(Vector3.UnitY, CreateRotationMatrix());
            }
        }

        public Matrix4x4 CreateModelMatrix()
        {
            var translation = CreateTranslationMatrix();
            var rotation = CreateRotationMatrix();
            var scale = CreateScaleMatrix();

            return scale * rotation * translation;
        }

        public Matrix4x4 CreateTranslationMatrix()
        {
            return Matrix4x4.CreateTranslation(Position);
        }

        public Matrix4x4 CreateRotationMatrix()
        {
            var rotation = Matrix4x4.CreateRotationX(BundtMaths.DegressToRadians(Rotation.X));
            rotation *= Matrix4x4.CreateRotationY(BundtMaths.DegressToRadians(Rotation.Y));
            rotation *= Matrix4x4.CreateRotationZ(BundtMaths.DegressToRadians(Rotation.Z));
            return rotation;
        }

        public Matrix4x4 CreateScaleMatrix()
        {
            return Matrix4x4.CreateScale(Scale);
        }
    }
}