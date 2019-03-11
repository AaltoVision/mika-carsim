﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Random;
using MathNet.Spatial.Euclidean;
using UnityEngine;
using CarSim.Randomization;

//Interpolation between points with a Catmull-Rom spline
public class TrackGenerator : MonoBehaviour, IRandomizable
{
    public Vector3[] controlPointsList;
    // Distance of track edges from center
    public float trackWidth = 5f;
    public Color roadColor;
    public Color lineColor;

    // Number of random points to sample for generating the track
    public int nRandomPoints = 10;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    //Display without having to press play
    long frameNum = 0;

    void OnDrawGizmos()
    {
        //Draw the Catmull-Rom spline between the points
        for (int i = 0; i < controlPointsList.Length; i++)
        {
            Gizmos.DrawSphere(controlPointsList[i], 1f);
            DisplayCatmullRomSpline(i, true);
        }
    }

    public void Randomize(int seed) {
        GenerateControlPoints(seed);
        RandomizeTexture(seed);
        GenerateMesh();
    }

    public void GenerateControlPoints(int seed) {
        verts.Clear();
        tris.Clear();
        uvs.Clear();
        double[] x = SystemRandomSource.Doubles(nRandomPoints, seed);
        double[] z = SystemRandomSource.Doubles(nRandomPoints, seed+1);
        Point2D[] points = new Point2D[nRandomPoints];

        for (int i = 0; i < nRandomPoints; i++) {
            x[i] = x[i] * 300.0;
            z[i] = z[i] * 300.0;
            points[i] = new Point2D(x[i], z[i]);
        }

        Polygon2D hull = Polygon2D.GetConvexHullFromPoints(points, true);
        int n_vertices = hull.Count - 1;
        IEnumerator<Point2D> hullVertices = hull.GetEnumerator();
        hullVertices.MoveNext();
        Vector3[] vecs = new Vector3[n_vertices];
        for (int i = 0; i < n_vertices; i++) {
            Point2D point = hullVertices.Current;
            vecs[i] = new Vector3((float) point.X, 0f, (float) -point.Y);
            hullVertices.MoveNext();
        }

        // Move track to (0, 0, 0)
        Vector3 translate = Vector3.zero - vecs[0];
        for (int i = 0; i < vecs.Length; i++) {
            vecs[i] += translate;
        }
        controlPointsList = vecs;
    }

    public void RandomizeTexture(int seed) {
        int textureSize = 500;
        int lineWidth = textureSize / 10;
        var texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        double[] noise = SystemRandomSource.Doubles(textureSize * textureSize * 3, seed);
        double[] colors = SystemRandomSource.Doubles(2 * 3, seed+1);
        roadColor = new Color((float) colors[0], (float) colors[1], (float) colors[2], 1f);
        lineColor = new Color((float) colors[3], (float) colors[4], (float) colors[5], 1f);

        // set the pixel values
        texture.SetPixels(Enumerable.Repeat<Color>(roadColor, textureSize*textureSize).ToArray());
        for (int x = 0; x < lineWidth; x++) {
            for (int y = 0; y < textureSize; y++) {
                texture.SetPixel(x, y, lineColor);
                texture.SetPixel(textureSize-x, y, lineColor);
            }
        }
        for (int x = 0; x < textureSize; x++) {
            for (int y = 0; y < textureSize; y++) {
                int idx = y * textureSize * 3 + x;
                Color noiseColor = new Color((float)noise[idx], (float)noise[idx+1], (float)noise[idx+2], 1f);
                texture.SetPixel(x, y, texture.GetPixel(x, y) + (noiseColor * new Color(0.5f, 0.5f, 0.5f, 1f)));
            }
        }

        // Apply all SetPixel calls
        texture.Apply();
        GetComponent<Renderer>().material.mainTexture = texture;
    }
    void OnRenderObject() {
        if (frameNum % 100 == 0) {
//            RandomizeTexture(((int) (rndsrc.NextDoubles(1)[0] * 10000)));
        }
        frameNum += 1;
    }
    public void GenerateMesh() {
        //Draw the Catmull-Rom spline between the points
        for (int i = 0; i < controlPointsList.Length; i++)
        {
            DisplayCatmullRomSpline(i, false);
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh.Clear();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = verts.ToArray();

        // Add triangles (counter-clockwise winding)
        for (int i = 0; i < verts.Count - 2; i+=2) {
            tris.Add(i); tris.Add(i+2); tris.Add(i+1);
            tris.Add(i+2); tris.Add(i+3); tris.Add(i+1);
        }
        tris.Add(verts.Count-2); tris.Add(0); tris.Add(verts.Count - 1);
        tris.Add(0); tris.Add(1); tris.Add(verts.Count-1);
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    void Start()
    {
//        RandomizeTexture(((int) (rndsrc.NextDoubles(1)[0] * 10000)));
        GenerateMesh();
    }

    //Display a spline between 2 points derived with the Catmull-Rom spline algorithm
    void DisplayCatmullRomSpline(int pos, bool editor=true)
    {
        //The 4 points we need to form a spline between p1 and p2
        Vector3 p0 = controlPointsList[ClampListPos(pos - 1)];
        Vector3 p1 = controlPointsList[pos];
        Vector3 p2 = controlPointsList[ClampListPos(pos + 1)];
        Vector3 p3 = controlPointsList[ClampListPos(pos + 2)];

        //The start position of the line
        Vector3 lastPos = p1;

        //The spline's resolution
        //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
        float resolution = 0.01f;

        //How many times should we loop?
        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {
            //Which t position are we at?
            float t = i * resolution;

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3[] catPos = GetCatmullRomPosition(t, p0, p1, p2, p3);
            Vector3 newPos = catPos[0];
            Vector3 newDeriv = catPos[1];
            Vector3 dir = Vector3.Normalize(newPos - lastPos);
            Vector3 perp = new Vector3(dir.z, dir.y, -dir.x) * trackWidth;

            //Draw this line segment
            if (editor) {
                Gizmos.DrawLine(lastPos, newPos);
                Gizmos.DrawLine(newPos, newPos+perp);
                Gizmos.DrawLine(newPos, newPos-perp);
            } else {
                Vector3 uvdir = newPos - lastPos;
                Vector3 uvp = new Vector3(uvdir.z, uvdir.y, -uvdir.x) * trackWidth;
                Vector3 uv_zero = lastPos + uvp;
                Vector3 uv_one = lastPos - uvp;
                float segmentLength = uvdir.magnitude;
                float segmentWidth = trackWidth * 2f;
                Vector2 uv_1 = new Vector2(0f, (float)(i%2) * (segmentLength / segmentWidth));
                Vector2 uv_2 = new Vector2(1f, (float)(i%2) * (segmentLength / segmentWidth));
                uvs.Add(uv_1);
                uvs.Add(uv_2);
                verts.Add(newPos-perp);
                verts.Add(newPos+perp);
            }

            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }
    }

    //Clamp the list positions to allow looping
    int ClampListPos(int pos)
    {
        if (pos < 0)
        {
            pos = controlPointsList.Length - 1;
        }

        if (pos > controlPointsList.Length)
        {
            pos = 1;
        }
        else if (pos > controlPointsList.Length - 1)
        {
            pos = 0;
        }

        return pos;
    }

    //Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
    //http://www.iquilezles.org/www/articles/minispline/minispline.htm
    Vector3[] GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
        // d: b + 2 * c * t + 3 * d * t * t
        Vector3 de = 0.5f * (b + ((c*2.0f)*t) + ((d * 3.0f) * t * t));

        return new Vector3[]{pos, de};
    }
}
