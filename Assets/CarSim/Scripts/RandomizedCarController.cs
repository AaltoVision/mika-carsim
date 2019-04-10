using System;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using CarSim.Randomization;
using MathNet.Numerics.Random;

namespace CarSim {
    public class RandomizedCarController : CarController, IRandomizable
    {

        private bool RandomizeDynamics;
        protected override void Start()
        {
            base.Start();
            RandomizeDynamics = Utils.ArgExists("--randomize-dynamics");
            Debug.Log("randomize start called");
        }

        public void Randomize(SystemRandomSource rnd)
        {
            if (RandomizeDynamics) {
                m_SlipLimit = (float) rnd.NextDouble();
                m_Topspeed = 50.0f + (float) (rnd.NextDouble() * 200.0);
                m_Downforce = (float)(rnd.NextDouble() * 50.0);
                m_BrakeTorque = (float)(rnd.NextDouble() * 10000);
                m_TractionControl = (float) rnd.NextDouble();
                m_SteerHelper = (float) rnd.NextDouble();
                m_FullTorqueOverAllWheels = (float)(2000.0 + rnd.NextDouble() * 2500);

                m_Rigidbody.mass = 200.0f + (float)(rnd.NextDouble() * 2000.0);
            }
        }
    }
}

