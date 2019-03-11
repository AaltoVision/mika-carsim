using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarSim.Randomization;

public class GroundPlane : MonoBehaviour, IRandomizable
{
    Color materialColor = Random.ColorHSV();
    public void Randomize(int seed) {
        materialColor = Random.ColorHSV();
        GetComponent<Renderer>().material.color = materialColor;
    }
}
