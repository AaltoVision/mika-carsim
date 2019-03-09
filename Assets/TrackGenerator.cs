using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Random;
using MathNet.Spatial.Euclidean;
using UnityEngine;

//Interpolation between points with a Catmull-Rom spline
public class TrackGenerator : MonoBehaviour
{
    //Has to be at least 4 points
    public Vector3[] controlPointsList;
    public float trackWidth = 5f;
    SystemRandomSource rndsrc = new SystemRandomSource(12345, false);
    //Are we making a line or a loop?
    public bool isLooping = true;
    public Color roadColor;
    public Color lineColor;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvcoords = new List<Vector2>();
    List<int> tris = new List<int>();
    //Display without having to press play
    long frameNum = 0;

    void OnDrawGizmos()
    {
        //Draw the Catmull-Rom spline between the points
        for (int i = 0; i < controlPointsList.Length; i++)
        {
            Gizmos.DrawSphere(controlPointsList[i], 1f);
            //Cant draw between the endpoints
            //Neither do we need to draw from the second to the last endpoint
            //...if we are not making a looping line
            if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1) && !isLooping)
            {
                continue;
            }

            DisplayCatmullRomSpline(i, true);
        }
    }
    public void SetPoints() {
        int n_random = 10;
        double[] x = rndsrc.NextDoubles(n_random);
        double[] z = rndsrc.NextDoubles(n_random);
        Point2D[] points = new Point2D[n_random];
        for (int i = 0; i < n_random; i++) {
            x[i] = x[i] * 300.0;
            z[i] = z[i] * 300.0;
            points[i] = new Point2D(x[i], z[i]);
        }
        Polygon2D hull = Polygon2D.GetConvexHullFromPoints(points, true);
        int n_vertices = hull.Count - 1;
        IEnumerator<Point2D> vertices = hull.GetEnumerator();
        vertices.MoveNext();
        Vector3[] vecs = new Vector3[n_vertices];
        for (int i = 0; i < n_vertices; i++) {
            Point2D point = vertices.Current;
            vecs[i] = new Vector3((float) point.X, 0f, (float) -point.Y);
            vertices.MoveNext();
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
            RandomizeTexture(((int) (rndsrc.NextDoubles(1)[0] * 10000)));
        }
        frameNum += 1;
    }
    void Start()
    {
        if (controlPointsList == null) SetPoints();
        RandomizeTexture(((int) (rndsrc.NextDoubles(1)[0] * 10000)));
        // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
        //Draw the Catmull-Rom spline between the points
        for (int i = 0; i < controlPointsList.Length; i++)
        {
            //Cant draw between the endpoints
            //Neither do we need to draw from the second to the last endpoint
            //...if we are not making a looping line
            if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1) && !isLooping)
            {
                continue;
            }

            DisplayCatmullRomSpline(i, false);
        }
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        List<int> triangles = new List<int>();
        for (int i = 0; i < vertices.Count - 2; i+=2) {
            triangles.Add(i); triangles.Add(i+2); triangles.Add(i+1);
            triangles.Add(i+2); triangles.Add(i+3); triangles.Add(i+1);
        }
        triangles.Add(vertices.Count-2); triangles.Add(0); triangles.Add(vertices.Count - 1);
        triangles.Add(0); triangles.Add(1); triangles.Add(vertices.Count-1);
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvcoords.ToArray();
        GetComponent<MeshCollider>().sharedMesh = mesh;
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
        Vector3 lastd;
        Vector3 lastp;

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
            Vector3[] catpos = GetCatmullRomPosition(t, p0, p1, p2, p3);
            Vector3 newPos = catpos[0];
            Vector3 newd = catpos[1];
            Vector3 dir = Vector3.Normalize(newPos - lastPos);
            Vector3 p = new Vector3(dir.z, dir.y, -dir.x) * trackWidth;
            //Draw this line segment
            if (editor) {
                Gizmos.DrawLine(lastPos, newPos);
                Gizmos.DrawLine(newPos, newPos+p);
                Gizmos.DrawLine(newPos, newPos-p);
            } else {
                Vector3 uvdir = newPos - lastPos;
                Vector3 uvp = new Vector3(uvdir.z, uvdir.y, -uvdir.x) * trackWidth;
                Vector3 uv_zero = lastPos + uvp;
                Vector3 uv_one = lastPos - uvp;
                float segmentLength = uvdir.magnitude;
                float segmentWidth = trackWidth * 2f;
                Vector2 uv_1 = new Vector2(0f, (float)(i%2) * (segmentLength / segmentWidth));
                Vector2 uv_2 = new Vector2(1f, (float)(i%2) * (segmentLength / segmentWidth));
                uvcoords.Add(uv_1);
                uvcoords.Add(uv_2);
                vertices.Add(newPos-p);
                vertices.Add(newPos+p);
            }
            lastd = newd;
            lastp = p;

            //Save this pos so we can draw the next line segment
            lastPos = newPosition;
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

