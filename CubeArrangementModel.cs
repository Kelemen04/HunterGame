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

            // Pozíció frissítése a sebesség alapján
            MrEggPosition += mrEggVelocity * (float)deltaTime;
        }
    }
}
