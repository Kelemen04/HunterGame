using GrafikaSzeminarium;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Szeminarium
{
    internal class CubeArrangementModel
    {
        public bool AnimationEnabled { get; set; } = true;
        private double Time { get; set; } = 0;
        public double CenterCubeScale { get; private set; } = 1;

        public Vector3 ManPosition { get; set; } = new Vector3(0f, -3f, -35f);
        private Vector3 manVelocity = Vector3.Zero;

        public Vector3 ammoPosition = new Vector3(5f, -1f, -35f);
        public List<Vector3> FoxyPositions { get; private set; } = new List<Vector3>();
        public List<Vector3> FoxyVelocities = new List<Vector3>();
        public List<bool> FoxyAlive = new List<bool>();

        public float rad = 0;
        public CubeArrangementModel()
        {
            FoxyPositions = new List<Vector3>
            {
                new Vector3(35f, -3f, 25f),
                new Vector3(35f, -3f, 8f),
                new Vector3(35f, -3f, -10f),
                new Vector3(-35f, -3f, 25f),
                new Vector3(-35f, -3f, 8f),
                new Vector3(-35f, -3f, -10f)
            };

            FoxyVelocities = new List<Vector3>
            {
                new Vector3(-2f, 0, 0),
                new Vector3(-2f, 0, 0),
                new Vector3(-2f, 0, 0),
                new Vector3(2f, 0, 0),
                new Vector3(2f, 0, 0),
                new Vector3(2f, 0, 0),
            };

            FoxyAlive = new List<bool>
            {
                true,
                true,
                true,
                true,
                true,
                true,
            };

            bulletNr = 10;

            ManPosition = new Vector3(0f, -3f, -35f);
            manVelocity = Vector3.Zero;
        }

        public bool allDead()
        {
            for (int i = 0; i < FoxyAlive.Count; i++)
            {
                if (FoxyAlive[i] == true)
                {
                    return false;
                }
            }
            return true;
        }

        public class Bullet
        {
            public Vector3 Position;
            public Vector3 Direction;
            public float Speed;
            public bool IsActive = false;

            public void Update(float deltaTime)
            {
                Position += Direction * Speed * deltaTime;
            }
        }

        public float ManYaw { get; set; } = 90f;
        public void SetManDirectionFromCamera(Vector3 cameraFront)
        {
            var flatFront = new Vector3(cameraFront.X, 0, cameraFront.Z);
            if (flatFront.LengthSquared() > 0)
            {
                flatFront = Vector3.Normalize(flatFront);
                manVelocity = flatFront * 10f;
            }
            else
            {
                manVelocity = Vector3.Zero;
            }
        }

        public static float DegreesToRadians(float degrees)
        {
            return (float)(Math.PI / 180.0) * degrees;
        }

        public float ammoRotate = 0f;
        public void RotateAmmo()
        {
            ammoRotate += 1;
        }

        public Bullet bullet = new Bullet();
        public int bulletNr;
        public void Shoot(Vector3 cameraFront)
        {
            if (!bullet.IsActive)
            {
                bulletNr -= 1;
                var flatFront = Vector3.Normalize(new Vector3(cameraFront.X, 0, cameraFront.Z));
                var direction = Vector3.Normalize(Vector3.Lerp(flatFront, cameraFront, 0.5f));
                if (direction.LengthSquared() > 0)
                    direction = Vector3.Normalize(direction);
                else
                    direction = Vector3.UnitZ;

                bullet.Position = ManPosition + new Vector3(0.7f, 3.3f, 0);
                bullet.Direction = direction;
                bullet.Speed = 50f;
                bullet.IsActive = true;
            }
        }


        internal void AdvanceTime(double deltaTime)
        {
            if (!AnimationEnabled)
                return;

            Time += deltaTime;

            CenterCubeScale = 1 + 0.2 * Math.Sin(1.5 * Time);

            var newPos = ManPosition + manVelocity * (float)deltaTime;

            float halfSize = 50f;

            if (MathF.Abs(newPos.X) <= halfSize && MathF.Abs(newPos.Z) <= halfSize)
            {
                ManPosition = newPos;
            }

            if (MathF.Abs(ManPosition.X - ammoPosition.X) <= 1 && MathF.Abs(ManPosition.Z - ammoPosition.Z) <= 1)
            {
                bulletNr = 10;
            }

            for (int i = 0; i < FoxyPositions.Count; i++)
            {
                if (MathF.Abs(FoxyPositions[i].X - bullet.Position.X) <= 1 && MathF.Abs(FoxyPositions[i].Z - bullet.Position.Z) <= 2)
                {
                    FoxyAlive[i] = false;
                    bullet.IsActive = false;
                }
            }

            if (bullet.IsActive)
            {
                bullet.Update((float)deltaTime);

                if ((bullet.Position - ManPosition).Length() >= 100f)
                {
                    bullet.IsActive = false;
                }
            }

            Boolean rotate = false;
            for (int i = 0; i < FoxyPositions.Count; i++)
            {
                Vector3 vel = FoxyVelocities[i];
                
                if(i < 3)
                {
                    if (FoxyPositions[i].X < 5 || FoxyPositions[i].X > 35)
                    {
                        vel.X *= -1;
                        FoxyVelocities[i] = vel;
                        rotate = true;
                    }
                }
                else
                {
                    if (FoxyPositions[i].X > -5 || FoxyPositions[i].X < -35)
                    {
                        vel.X *= -1;
                        FoxyVelocities[i] = vel;
                        rotate = true;
                    }
                }

                if(i == 5 && rotate)
                {
                    rad = (rad == 0) ? 180 : 0;
                }

                FoxyPositions[i] += FoxyVelocities[i] * (float)deltaTime;
            }
        }
    }
}
