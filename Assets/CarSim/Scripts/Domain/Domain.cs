using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarSim.Randomization;
using MathNet.Numerics.Random;

public class Domain : MonoBehaviour
{
    public int seed = 0;

    SystemRandomSource rnd = new SystemRandomSource(0, true);

    // Start is called before the first frame update
    void Start() {
        RandomizeDomain();
    }

    public void RandomizeDomain() {
        IRandomizable[] components = GetComponentsInChildren<IRandomizable>();
        foreach (IRandomizable component in components) {
            component.Randomize(rnd);
        }
    }
}
