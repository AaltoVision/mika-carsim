using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MLAgents;
using MathNet.Numerics.Random;
using CarSim.Randomization;

public class RandomObjects : MonoBehaviour, IRandomizable
{
    public GameObject track;
    void DestroyObject(GameObject obj) {
        Destroy(obj.GetComponent<Renderer>().material);
        #if UNITY_EDITOR
        Destroy(obj);
        #else
        Destroy(obj);
        #endif
    }


    void DestroyChildren() {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform)
            children.Add(child.gameObject);
        children.ForEach(child => DestroyObject(child));
    }

    void GenerateChildren(SystemRandomSource rnd) {
        Vector3[] trackVertices = track.GetComponent<MeshFilter>().sharedMesh.vertices;
        int[] trackTris = track.GetComponent<MeshFilter>().sharedMesh.triangles;
        for (int i = 0; i < 500; i++) {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = transform;
            double[] pos = rnd.NextDoubles(3);
            Vector3 position = new Vector3((float) pos[0] * 900f - 600f,
                                                  (float) pos[1] * 5f -2.5f,
                                                  (float) pos[2] * 900f - 450f + 150f);

            if (Math.Abs(position.x) < 15)
                position.x = Math.Abs(position.x) / position.x * 15f;
            if (Math.Abs(position.z) < 15)
                position.z = Math.Abs(position.z) / position.z * 15f;
            cube.transform.position = position;

            double[] scale = rnd.NextDoubles(3);
            cube.transform.localScale = new Vector3(1f + (float) scale[0] * 40f,
                                                    1f + (float) scale[1] * 40f,
                                                    1f + (float) scale[2] * 40f);

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
            Bounds cubeBounds = cube.GetComponent<Renderer>().bounds;

            for (int t = 0; t < trackVertices.Length; t += 1) {
                if (cubeBounds.Contains(trackVertices[t])) {
                    DestroyObject(cube);
                    break;
                }
            }

        }
    }

    public void Randomize(SystemRandomSource rnd, ResetParameters resetParameters) {
        DestroyChildren();
        GenerateChildren(rnd);
    }
}
