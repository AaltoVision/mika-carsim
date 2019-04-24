using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.Random;
using MLAgents;

namespace CarSim.Randomization {
    interface IRandomizable
    {
        void Randomize(SystemRandomSource rnd, ResetParameters resetParameters);
    }
}
