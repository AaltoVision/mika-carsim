#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TrackGenerator))]
public class TrackGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TrackGenerator myScript = (TrackGenerator)target;
        if(GUILayout.Button("Generate track"))
        {
            myScript.GenerateControlPoints();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
