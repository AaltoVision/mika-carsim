using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Random;
using MathNet.Spatial.Euclidean;
using UnityEngine;
using CarSim;
using CarSim.Randomization;

public class TextureRandomizable : MonoBehaviour, IRandomizable {
    protected void RandomizeTexture(SystemRandomSource rnd) {
    	Destroy(GetComponent<Renderer>().material.mainTexture);
        (Color color, Texture2D texture) = TextureHandler.Instance().RandomColorTexture(rnd);
        Texture2D newTexture = Instantiate(texture);
        GetComponent<Renderer>().material.mainTexture = newTexture;
        GetComponent<Renderer>().material.color = color;
    }

    public virtual void Randomize(SystemRandomSource rnd) {
    	if(Utils.randomizeTextures())
    		RandomizeTexture(rnd);
    }
}