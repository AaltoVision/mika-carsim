﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarSim.Randomization;
using CarSim;
using MathNet.Numerics.Random;

public class Domain : MonoBehaviour
{
    SystemRandomSource rnd = new SystemRandomSource(0, true);
    int _seed = 124;
    long counter = 0;
    public bool captureFrames = false;
    public bool randomize = true;
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
        randomize = !Utils.ArgExists("--no-randomize");
        seed = s;
        RandomizeDomain();
    }

    public void RandomizeDomain() {
        IRandomizable[] components = GetComponentsInChildren<IRandomizable>();
        foreach (IRandomizable component in components) {
            component.Randomize(rnd);
        }
        foreach (var cam in GetComponentsInChildren<SimCamera>()) {
            cam.OnSceneChange();
        }

        car.transform.rotation = Quaternion.Euler(0f, (float) rnd.NextDouble() * 360f, 0f);
    }

    public void Reset() {
        if (randomize == true)
            RandomizeDomain();

        episodeNum++;
        foreach (var cam in GetComponentsInChildren<SimCamera>()) {
            cam.episodeNum = episodeNum;
            cam.frameNum = 0;
        }
    }
}
