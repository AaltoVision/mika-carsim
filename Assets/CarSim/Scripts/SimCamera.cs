using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using CarSim.Randomization;
using CarSim;
using MathNet.Numerics.Random;
using MLAgents;

public class SimCamera : MonoBehaviour, IRandomizable
{
    public enum ShaderMode {
        Default,
        Segmentation,
        Depth
    }

    public Shader defaultShader;
    public Shader segmentationShader;
    public Shader depthShader;
    public Shader canonicalShader;
    public ShaderMode shaderMode = ShaderMode.Default;

    private int saveEvery = 4;
    private int saveWidth = 640;
    private int saveHeight = 480;
    private bool saveFrames = false;

    private float originalCameraHeight;

    List<Camera> cameras = new List<Camera>();
    List<Shader> shaders = new List<Shader>();
    Object[] cubemaps = null;

    public long episodeNum { get; set; }
    public long frameNum { get; set; }

    void Start() {
        shaders.Add(defaultShader);
        shaders.Add(segmentationShader);
        shaders.Add(depthShader);
        cameras.Add(GetComponent<Camera>());
        cameras.Add(CreateCamera("segmentation"));
        cameras.Add(CreateCamera("depth"));

        if (Utils.ArgExists("--segmentation"))
            shaderMode = ShaderMode.Segmentation;
        else if (Utils.ArgExists("--depth"))
            shaderMode = ShaderMode.Depth;

        UpdateCameras();
        OnSceneChange();

        saveFrames = Utils.ArgExists("--save-frames");
        saveWidth = int.TryParse(Utils.GetArg("--save-width"), out saveWidth) ? saveWidth : 640;
        saveHeight = int.TryParse(Utils.GetArg("--save-height"), out saveHeight) ? saveHeight : 480;
        saveEvery = int.TryParse(Utils.GetArg("--save-every"), out saveEvery) ? saveEvery : 4;

        originalCameraHeight = transform.position.y;
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
    }

    private void RandomizeFov(SystemRandomSource rnd) {
        float fov = (float) rnd.NextDouble();
        GetComponent<Camera>().fieldOfView = 50f + fov * 70f;
    }

    private void RandomizeHeight(SystemRandomSource rnd) {
        float newHeight = (float) (rnd.NextDouble() * 2.0 * originalCameraHeight + 0.3);
        Vector3 position = transform.position;
        transform.position = new Vector3(
                position.x,
                newHeight,
                position.z
            );
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
        int i = (int) shaderMode;
        for (int j = i; j < i+3; j++) {
            int k = j % 3;
            if (k == 0) {
                cameras[k].targetDisplay = j - i;
                continue;
            }
            Camera cam = cameras[k];
            cam.CopyFrom(mainCam);
            cam.RemoveAllCommandBuffers();
            cam.SetReplacementShader(shaders[k], "RenderType");
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
        var renderers = Object.FindObjectsOfType<Renderer>();
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {
            var id = r.gameObject.GetInstanceID();
            var layer = r.gameObject.layer;
            var tag = r.gameObject.tag;

            mpb.SetColor("_SegColor", LayerToColor(layer));
            mpb.SetColor("_CanColor", LayerToColor(layer));
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
            string filename = $"dataset/{cam.name}_{episodeNum:D4}_{frameNum:D8}.png";
            System.IO.File.WriteAllBytes(filename, bytes);
        }
    }
}
