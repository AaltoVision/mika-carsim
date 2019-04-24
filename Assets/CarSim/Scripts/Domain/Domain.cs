using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using CarSim.Randomization;
using CarSim;
using MathNet.Numerics.Random;

public class Domain : MonoBehaviour
{
    SystemRandomSource rnd = new SystemRandomSource(0, true);
    int _seed = 124;
    long counter = 0;
    public Academy academy;
    public bool captureFrames = false;
    public int seed {
        get { return _seed; }
        set {
            _seed = value;
            rnd = new SystemRandomSource(value, true);
        }
    }
    public GameObject car;

    private long episodeNum = 0;

    void Awake() {
    }

    public void UpdateRandomSource() {
        rnd = new SystemRandomSource(seed, true);
    }

    // Start is called before the first frame update
    void Start() {
        int s = int.TryParse(Utils.GetArg("--seed"), out s) ? s : _seed;
        seed = s;
        RandomizeDomain();
    }

    public void RandomizeDomain() {
        ResetParameters resetParams = academy.resetParameters;
        IRandomizable[] components = GetComponentsInChildren<IRandomizable>();
        foreach (IRandomizable component in components) {
            component.Randomize(rnd, resetParams);
        }
        foreach (var cam in GetComponentsInChildren<SimCamera>()) {
            cam.OnSceneChange();
        }
        float rotation = (rnd.NextDouble() < 0.5) ? 0f : 180f;
        car.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
    }

    public void Reset() {
        RandomizeDomain();

        episodeNum++;
        foreach (var cam in GetComponentsInChildren<SimCamera>()) {
            cam.episodeNum = episodeNum;
            cam.frameNum = 0;
        }
    }
}
