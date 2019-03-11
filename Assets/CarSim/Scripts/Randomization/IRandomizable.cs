using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarSim.Randomization {
    interface IRandomizable
    {
        void Randomize(int seed);
    }
}
