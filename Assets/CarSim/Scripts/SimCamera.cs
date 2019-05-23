using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using CarSim.Randomization;
using CarSim;
using MathNet.Numerics.Random;
using MLAgents;
using System;

public class SimCamera : MonoBehaviour, IRandomizable
{
    public enum ShaderMode {
        Default,
        Segmentation,
        Depth,
        Canonical
    }

    public Shader defaultShader;
    public Shader segmentationShader;
    public Shader depthShader;
    public Shader canonicalShader;
    public ShaderMode shaderMode = ShaderMode.Default;

    public Texture2D canTextureTrack;
    public Texture2D canTextureGround;

    public Agent agent;

    private int saveEvery = 4;
    private int saveWidth = 640;
    private int saveHeight = 480;
    private bool saveFrames = false;

    List<Camera> cameras = new List<Camera>();
    List<Shader> shaders = new List<Shader>();
    UnityEngine.Object[] cubemaps = null;

    public long episodeNum { get; set; }
    public long frameNum { get; set; }

    private string timestamp = null;

    void Start() {
        shaders.Add(canonicalShader);
        //shaders.Add(segmentationShader);
        //shaders.Add(depthShader);
        //shaders.Add(canonicalShader);
        cameras.Add(GetComponent<Camera>());
        cameras.Add(CreateCamera("segmentation"));
        cameras.Add(CreateCamera("depth"));
        cameras.Add(CreateCamera("canonical"));

        timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

        if (Utils.ArgExists("--segmentation"))
            shaderMode = ShaderMode.Segmentation;
        else if (Utils.ArgExists("--depth"))
            shaderMode = ShaderMode.Depth;
        else if (Utils.ArgExists("--canonical"))
            shaderMode = ShaderMode.Canonical;

        UpdateCameras();
        OnSceneChange();

        saveFrames = Utils.ArgExists("--save-frames");
        saveWidth = int.TryParse(Utils.GetArg("--save-width"), out saveWidth) ? saveWidth : 640;
        saveHeight = int.TryParse(Utils.GetArg("--save-height"), out saveHeight) ? saveHeight : 480;
        saveEvery = int.TryParse(Utils.GetArg("--save-every"), out saveEvery) ? saveEvery : 4;
    }

    public void Randomize(SystemRandomSource rnd, ResetParameters resetParameters) {
        if(cubemaps == null)
            cubemaps = Resources.LoadAll("Cubemaps");
        bool shouldRandomizeCubeMap = (float)rnd.NextDouble() < resetParameters["random_cubemap"];
        if (cubemaps.Length > 0 && shouldRandomizeCubeMap) {
            int rand = (int) (rnd.NextDouble() * cubemaps.Length);
            Cubemap cmap = (Cubemap)cubemaps[rand];
            GetComponent<Skybox>().material.SetTexture("_Tex", cmap);
        }
        if (((float) rnd.NextDouble()) < resetParameters["random_fov"]) {
            RandomizeFov(rnd);
        }
        if ((float) rnd.NextDouble() < resetParameters["random_camera_height"]) {
            RandomizeHeight(rnd);
        }
        RandomizeRotation(rnd);
    }

    private void RandomizeRotation(SystemRandomSource rnd) {
        float x = (float) rnd.NextDouble() * 5.0f - 2.5f;
        float y = transform.rotation.eulerAngles.y;
        float z = (float) rnd.NextDouble() * 5.0f - 2.5f;
        transform.rotation = Quaternion.Euler(
                x,
                y,
                z
            );
    }

    private void RandomizeFov(SystemRandomSource rnd) {
        float fov = (float) rnd.NextDouble();
        GetComponent<Camera>().fieldOfView = 50f + fov * 70f;
    }

    private void RandomizeHeight(SystemRandomSource rnd) {
        float newHeight = (float) (rnd.NextDouble() + 0.5);
        Vector3 position = transform.position;
        transform.position = new Vector3(
                position.x,
                newHeight,
                position.z
            );
    }

    public static Color LayerToColor(int layer)
    {
        if(layer == 8)
            return new Color(0.0f, 1.0f, 0.0f);
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

    private Camera CreateCamera(string name)
    {
        GameObject obj = new GameObject(name, typeof(Camera));
        obj.hideFlags = HideFlags.HideAndDontSave;
        obj.transform.parent = transform;

        Camera camera = obj.GetComponent<Camera>();
        return camera;
    }

    public void UpdateCameras() {
        if (cameras.Count == 0) {
            return;
        }
        Camera mainCam = GetComponent<Camera>();
        int n_cameras = cameras.Count;
        int i = (int) shaderMode;
        for (int j = i; j < i+n_cameras; j++) {
            int k = j % n_cameras;
            if (k == 0) {
                cameras[k].targetDisplay = j - i;
                //continue;
            }
            Camera cam = cameras[k];
            cam.CopyFrom(mainCam);
            cam.RemoveAllCommandBuffers();
            cam.SetReplacementShader(shaders[0], "RenderType");
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.targetDisplay = j - i;
//            cam.enabled = false;
        }
    }

    void LateUpdate() {
        frameNum++;
        if (frameNum % saveEvery == 0 && saveFrames)
            StartCoroutine(SaveFrames());
    }

    public void OnSceneChange() {
        var renderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {
            var id = r.gameObject.GetInstanceID();
            var layer = r.gameObject.layer;
            var tag = r.gameObject.tag;

            mpb.SetColor("_SegColor", LayerToColor(layer));
            mpb.SetColor("_CanColor", Color.white);
            Texture canTex = r.sharedMaterial.mainTexture;
            if (canTex != null) {
                if (layer == 8)
                    canTex = canTextureTrack;
                else if (layer == 9)
                    canTex = canTextureGround;

                mpb.SetTexture("_CanTex", canTex);
            }
            r.SetPropertyBlock(mpb);
        }
        UpdateCameras();
    }

    private IEnumerator SaveFrames() {
        yield return new WaitForEndOfFrame();
        foreach(Camera cam in cameras) {
            int width = saveWidth; int height = saveHeight;
            RenderTexture rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            cam.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = $"dataset/{cam.name}_{episodeNum:D4}_{frameNum:D8}_{timestamp}.png";
            System.IO.File.WriteAllBytes(filename, bytes);
            Destroy(screenShot);
        }
    }
}
