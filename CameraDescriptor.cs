using Silk.NET.Maths;
using System.Numerics;

namespace Szeminarium
{
    internal class CameraDescriptor
    {
        public Vector3D<float> Position { get; set; } = new(0, 0, 5);
        private float pitch = 0;
        public float yaw = 90;
        private const float speed = 1f;
        private const float sensitivity = 0.09f;
        public Vector3D<float> Front { get; private set; } = new(0, 0, -1);
        public Vector3D<float> Up { get; private set; } = new(0, 1, 0);
        public Vector3D<float> Right { get; private set; } = new(1, 0, 0);
        public Vector3D<float> Target => Position + Front;
        private static float DegreesToRadians(float degrees)
        {
            return (float)(Math.PI / 180.0) * degrees;
        }
        public void UpdateVectors()
        {
            var front = new Vector3D<float>
            (
                MathF.Cos(DegreesToRadians(yaw)) * MathF.Cos(DegreesToRadians(pitch)),
                MathF.Sin(DegreesToRadians(pitch)),
                MathF.Sin(DegreesToRadians(yaw)) * MathF.Cos(DegreesToRadians(pitch))
            );
            Front = Vector3D.Normalize(front);
            Right = Vector3D.Normalize(Vector3D.Cross(Front, new Vector3D<float>(0, 1, 0)));
            Up = Vector3D.Normalize(Vector3D.Cross(Right, Front));
        }
        public void GoFront() => Position += Front * speed;
        public void GoBack() => Position -= Front * speed;
        public void GoLeft() => Position -= Right * speed;
        public void GoRight() => Position += Right * speed;
        public void ProcessMouseMovement(float xoffset, float yoffset)
        {
            xoffset *= sensitivity;
            yoffset *= sensitivity;

            yaw += xoffset;
            pitch -= yoffset;

            if (pitch > 89.0f)
                pitch = 89.0f;
            if (pitch < -89.0f)
                pitch = -89.0f;

            UpdateVectors();
        }

        public bool firstPerson = true;
        public void SetPosition(Vector3D<float> newPosition)
        {
            if (firstPerson)
            {
                Position = newPosition;
                UpdateVectors();
            }
        }
    }
}
