using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarSim.Randomization;
using MathNet.Numerics.Random;

public class GroundPlane : MonoBehaviour, IRandomizable
{
    Color materialColor;
    public void Randomize(SystemRandomSource rnd) {
        double[] color = rnd.NextDoubles(3);
        GetComponent<Renderer>().sharedMaterial.color = new Color(
            (float) color[0],
            (float) color[1],
            (float) color[2],
            1f
        );
    }
}
