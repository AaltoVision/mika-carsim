using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarSim.Randomization;
using MathNet.Numerics.Random;

public class RandomObjects : MonoBehaviour, IRandomizable
{
    void DestroyObject(GameObject obj) {
        #if UNITY_EDITOR
        Destroy(obj);
        #else
        Destroy(obj);
        #endif
    }

    List<GameObject> children = new List<GameObject>();

    void DestroyChildren() {
        children.ForEach(child => DestroyObject(child));
        children.Clear();
    }

    void GenerateChildren(SystemRandomSource rnd) {
        for (int i = 0; i < 100; i++) {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = transform;
            double[] pos = rnd.NextDoubles(3);
            cube.transform.position = new Vector3((float) pos[0] * 900f - 600f,
                                                  (float) pos[1] * 5f -2.5f,
                                                  (float) pos[2] * 900f - 450f + 150f);

            double[] scale = rnd.NextDoubles(3);
            cube.transform.localScale = new Vector3(1f + (float) scale[0] * 5f,
                                                    1f + (float) scale[1] * 5f,
                                                    1f + (float) scale[2] * 5f);

            double[] rot = rnd.NextDoubles(3);
            cube.transform.eulerAngles = new Vector3((float) rot[0] * 180,
                                                     (float) rot[1] * 180,
                                                     (float) rot[2] * 180);
            double[] color = rnd.NextDoubles(3);
            cube.GetComponent<Renderer>().material.color = new Color((float) color[0],
                                                                           (float) color[1],
                                                                           (float) color[2],
                                                                           1f);
            cube.layer = 10;
            cube.hideFlags = HideFlags.HideAndDontSave;
            children.Add(cube);
        }
    }

    public void Randomize(SystemRandomSource rnd) {
        DestroyChildren();
        GenerateChildren(rnd);
    }
}
