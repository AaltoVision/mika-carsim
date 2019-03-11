using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.Random;

namespace CarSim.Randomization {
    interface IRandomizable
    {
        void Randomize(SystemRandomSource rnd);
    }
}
