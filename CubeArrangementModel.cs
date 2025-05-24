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
        /// <summary>
        /// Gets or sets wheather the animation should run or it should be frozen.
        /// </summary>
        public bool AnimationEnabled { get; set; } = true;

        /// <summary>
        /// The time of the simulation. It helps to calculate time dependent values.
        /// </summary>
        private double Time { get; set; } = 0;

        /// <summary>
        /// The value by which the center cube is scaled. It varies between 0.8 and 1.2 with respect to the original size.
        /// </summary>
        public double CenterCubeScale { get; private set; } = 1;

        public Vector3 MrEggPosition { get; private set; } = Vector3.Zero;

        // Példa sebesség vektor (ha billentyűzettel akarod mozgatni, ide jöhet egy input változó)
        private Vector3 mrEggVelocity = Vector3.Zero;

        public List<Vector3> FoxyPositions { get; private set; } = new List<Vector3>();

        public List<Vector3> FoxyVelocities = new List<Vector3>();

        public float rad = 0;

        public CubeArrangementModel()
        {
            // Induló pozíciók
            FoxyPositions = new List<Vector3>
            {
                new Vector3(35f, -3f, 25f),
                new Vector3(35f, -3f, 8f),
                new Vector3(35f, -3f, -10f),
                new Vector3(-35f, -3f, 25f),
                new Vector3(-35f, -3f, 8f),
                new Vector3(-35f, -3f, -10f)
            };

            // Minden Foxy kap egy irányt (pl. Z irányban előre mozog)
            FoxyVelocities = new List<Vector3>
            {
                new Vector3(-2f, 0, 0),
                new Vector3(-2f, 0, 0),
                new Vector3(-2f, 0, 0),
                new Vector3(2f, 0, 0),
                new Vector3(2f, 0, 0),
                new Vector3(2f, 0, 0),
            };
            //foxyVelocities = FoxyPositions.Select(p => new Vector3(-1f, 0, 0)).ToList();
        }

        // Ezt kívülről be tudod állítani pl. input alapján
        public void SetMrEggVelocity(Vector3 velocity)
        {
            mrEggVelocity = velocity;
        }

        internal void AdvanceTime(double deltaTime)
        {
            if (!AnimationEnabled)
                return;

            Time += deltaTime;

            CenterCubeScale = 1 + 0.2 * Math.Sin(1.5 * Time);

            MrEggPosition += mrEggVelocity * (float)deltaTime;
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
