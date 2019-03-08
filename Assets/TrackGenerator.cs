using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.Random;
using MathNet.Spatial.Euclidean;
using UnityEngine;

//Interpolation between points with a Catmull-Rom spline
public class TrackGenerator : MonoBehaviour
{
    //Has to be at least 4 points
    public Vector3[] controlPointsList;
    public Transform track_tile;
    SystemRandomSource rndsrc = new SystemRandomSource(12345, false);
    //Are we making a line or a loop?
    public bool isLooping = true;
    List<Vector3> vertices = new List<Vector3>();
    List<int> tris = new List<int>();
    //Display without having to press play
    void OnDrawGizmos()
    {
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

        Vector3 translate = Vector3.zero - vecs[0];
        for (int i = 0; i < vecs.Length; i++) {
            vecs[i] += translate + new Vector3(0f, 0f, 10f);
        }
        controlPointsList = vecs;
    }
    void Start() 
    {
        if (controlPointsList == null) SetPoints();
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
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < uvs.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        mesh.uv = uvs;
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

        //The spline's resolution
        //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
        float resolution = 0.05f;

        //How many times should we loop?
        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {
            //Which t position are we at?
            float t = i * resolution;

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3[] catmullPosition = GetCatmullRomPosition(t, p0, p1, p2, p3);
            Vector3 newPosition = catmullPosition[0];
            Vector3 newd = catmullPosition[1];// / catmullPosition[1].magnitude;
            Vector3 newDirection = Vector3.Normalize(newd);
            Vector3 positionPlusDirection = newPosition + newDirection;
            Vector3 p = new Vector3(newDirection.z, newDirection.y, -newDirection.x) * 5.0f;
            //Draw this line segment
            if (editor) {
                Gizmos.DrawLine(newPosition, positionPlusDirection);
                Gizmos.DrawLine(newPosition, newPosition+p);
                Gizmos.DrawLine(newPosition, newPosition-p);
            } else {
                vertices.Add(newPosition-p);
                vertices.Add(newPosition+p);
            }

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

