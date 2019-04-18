using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CarSim;
using CarSim.Randomization;
using MathNet.Numerics.Random;
using System.IO;

public class GroundPlane : MonoBehaviour, IRandomizable
{
    Color materialColor;
    private bool randomizeTextures = true;
    private bool useRandomFiles = false;
    private List<Texture2D> textures = new List<Texture2D>();

    private readonly static string randomizeTextureArg = "--randomize-texture";
    private readonly static string textureFileArg = "--texture-file";

    public void Randomize(SystemRandomSource rnd) {
        if (randomizeTextures) {
            if(useRandomFiles) LoadRandomTexture(rnd);
            else RandomizeTexture(rnd);
        }
    }

    void Awake() {
        randomizeTextures = Utils.ArgExists(randomizeTextureArg);
        useRandomFiles = Utils.ArgExists(textureFileArg);
        if(useRandomFiles)
            getTextures(Utils.GetArg(textureFileArg));
    }

    void getTextures(string textureFilePath) {
        foreach (string filename in File.ReadAllLines(textureFilePath)) {
            textures.Add(DiskLoadImage(filename));
        }
    }

    Texture2D DiskLoadImage(string filePath) {
         Texture2D tex = null;
         if (File.Exists(filePath))     {
             tex = new Texture2D(2, 2);
             tex.LoadImage(File.ReadAllBytes(filePath));
         }
         return tex;
     }

    void LoadRandomTexture(SystemRandomSource rnd) {
        GetComponent<Renderer>().material.color = Color.white;
        GetComponent<Renderer>().material.mainTexture = textures[rnd.Next() % textures.Count];
    }

    void RandomizeTexture(SystemRandomSource rnd) {
        Destroy(GetComponent<Renderer>().material.mainTexture);
        int textureSize = 500;
        var texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        double[] noise = rnd.NextDoubles(textureSize * textureSize * 3);
        double[] colors = rnd.NextDoubles(3);
        Color rndColor = new Color((float) colors[0],
                                   (float) colors[1],
                                   (float) colors[2],
                                   1f);

        // set the pixel values
        texture.SetPixels(Enumerable.Repeat<Color>(rndColor,
                    textureSize*textureSize).ToArray());

        for (int x = 0; x < textureSize; x++) {
            for (int y = 0; y < textureSize; y++) {
                int idx = y * textureSize * 3 + x;
                Color noiseColor = new Color((float)noise[idx], (float)noise[idx+1], (float)noise[idx+2], 1f);
                texture.SetPixel(x, y, texture.GetPixel(x, y) * noiseColor);
            }
        }

        // Apply all SetPixel calls
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        GetComponent<Renderer>().material.color = rndColor;
        GetComponent<Renderer>().material.mainTexture = texture;
    }
}
