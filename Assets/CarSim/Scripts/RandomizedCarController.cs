using System;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using CarSim.Randomization;
using MathNet.Numerics.Random;
using MLAgents;

namespace CarSim {
    public class RandomizedCarController : CarController, IRandomizable
    {
        private float defaultTopSpeed;
        private float defaultMass;
        private float defaultSlipLimit;
        private float defaultBrakeTorque;
        private float defaultMaxSteerAngle;

        private bool initialized;
        protected override void Start()
        {
            base.Start();
            initialized = true;

            defaultTopSpeed = m_Topspeed;
            defaultMass = m_Rigidbody.mass;
            defaultSlipLimit = m_SlipLimit;
            defaultBrakeTorque = m_BrakeTorque;
            defaultMaxSteerAngle = m_MaximumSteerAngle;
        }

        public void Randomize(SystemRandomSource rnd, ResetParameters resetParameters)
        {
            if (initialized && rnd.NextDouble() < resetParameters["random_dynamics"]) {
                m_Topspeed = defaultTopSpeed + defaultTopSpeed * random(rnd);
                if (m_Rigidbody) {
                    m_Rigidbody.mass = defaultMass + defaultMass * random(rnd);
                }
                m_SlipLimit = defaultSlipLimit + defaultSlipLimit * random(rnd);
                m_BrakeTorque = defaultBrakeTorque + defaultBrakeTorque * random(rnd);
                m_MaximumSteerAngle = defaultMaxSteerAngle + defaultMaxSteerAngle * random(rnd);
            }
        }

        private float random(SystemRandomSource rnd) {
            return (float)rnd.NextDouble() - 0.5f;
        }
    }
}

