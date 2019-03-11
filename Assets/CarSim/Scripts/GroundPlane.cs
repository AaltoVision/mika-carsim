using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarSim.Randomization;
using MathNet.Numerics.Random;

public class GroundPlane : MonoBehaviour, IRandomizable
{
    Color materialColor;
    public void Randomize(SystemRandomSource rnd) {
        materialColor = Random.ColorHSV();
        GetComponent<Renderer>().sharedMaterial.color = materialColor;
    }
}
