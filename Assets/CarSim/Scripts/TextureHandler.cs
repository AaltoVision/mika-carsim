using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.Random;
using CarSim;
using CarSim.Randomization;
using System.IO;
using System.Linq;
using UnityEngine;
     
public class TextureHandler {
     
    private List<Texture2D> textures = new List<Texture2D>();
    public static TextureHandler _instance = null;

    /* singleton */
    public static TextureHandler Instance() { 
         
        if (_instance==null)
        {
            _instance=new TextureHandler();
 
        }
        return _instance;
    }
    
    public TextureHandler() {
        if(Utils.useTextureFiles())  
            getTextures(Utils.randomFilePath());
    }

    void getTextures(string textureFilePath) {
        foreach (string filename in File.ReadAllLines(textureFilePath)) {
            textures.Add(DiskLoadImage(filename));
        }
    }

    Texture2D DiskLoadImage(string filePath) {
         Texture2D tex = null;
         if (File.Exists(filePath))     {
             tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
             tex.LoadImage(File.ReadAllBytes(filePath));
         }
         return tex;
    }

    public Texture2D NextTexture(SystemRandomSource rnd) {
    	return textures[rnd.Next() % textures.Count];
    }

    public (Color, Texture2D) RandomizeTexturePixels(SystemRandomSource rnd) {
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
        return (rndColor, texture);
    }

    public (Color, Texture2D) RandomColorTexture(SystemRandomSource rnd) {
    	if(Utils.useTextureFiles()) {
    		return (Color.white, NextTexture(rnd));
    	} else {
    		return RandomizeTexturePixels(rnd);
    	}
    }
}
