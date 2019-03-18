using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using CarSim.Randomization;
using MathNet.Numerics.Random;

public class RandomCamera : MonoBehaviour, IRandomizable
{
    public Shader segmentationShader;
    public Shader depthShader;
    public enum RenderMode {
        Normal,
        Segmentation,
        Depth,
    };
    public RenderMode renderMode;
    Camera hiddenCam;

    void Start() {
    }

    void SetRenderMode(RenderMode mode) {
        if (mode == RenderMode.Segmentation) {
            GetComponent<Camera>().SetReplacementShader (segmentationShader, "RenderType");
            var renderers = Object.FindObjectsOfType<Renderer>();
            var mpb = new MaterialPropertyBlock();
            foreach (var r in renderers)
            {
                var id = r.gameObject.GetInstanceID();
                var layer = r.gameObject.layer;
                var tag = r.gameObject.tag;

                mpb.SetColor("_SegColor", LayerToColor(layer));
                r.SetPropertyBlock(mpb);
            }
            GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        } else if(mode == RenderMode.Depth) {
            GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            GetComponent<Camera>().SetReplacementShader(depthShader, "RenderType");
        } else {
            GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
            GetComponent<Camera>().ResetReplacementShader();
        }
    }

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

    void Update() {
        if (Input.GetKeyDown("space")) {
            renderMode = (RenderMode)(((int)renderMode + 1) % 3);
            SetRenderMode(renderMode);
        }
    }

    public static Color LayerToColor(int layer)
    {
        float mul = (int) (layer / 8) * 0.2f;
        List<Color> colors = new List<Color>();
        for (int i = 0; i < 8; i++) {
            float b1 = (i & (1 << 0)) != 0 ? 1f-mul : 0f+mul;
            float b2 = (i & (1 << 1)) != 0 ? 1f-mul : 0f+mul;
            float b3 = (i & (1 << 2)) != 0 ? 1f-mul : 0f+mul;
            colors.Add(new Color(b1, b2, b3, 1f));
        }

        return colors[layer % 8];
    }
}
