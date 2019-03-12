using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarSim.Randomization;
using MathNet.Numerics.Random;

public class Domain : MonoBehaviour
{
    SystemRandomSource rnd = new SystemRandomSource(0, true);
    int _seed = 0;
    public int seed {
        get { return _seed; }
        set {
            _seed = value;
            rnd = new SystemRandomSource(value, true);
        }
    }

    void Awake() {
    }

    public void UpdateRandomSource() {
        rnd = new SystemRandomSource(seed, true);
    }

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
