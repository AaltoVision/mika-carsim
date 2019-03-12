using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarSim.Randomization;
using MathNet.Numerics.Random;

public class RandomCamera : MonoBehaviour, IRandomizable
{
    public void Randomize(SystemRandomSource rnd) {
        double[] color = rnd.NextDoubles(3);
        GetComponent<Skybox>().material.SetColor("_SkyTint", new Color(
            (float) color[0],
            (float) color[1],
            (float) color[2],
            1f
        ));
        float fov = (float) rnd.NextDouble();
        GetComponent<Camera>().fieldOfView = 50f + fov * 70f;
    }
}
