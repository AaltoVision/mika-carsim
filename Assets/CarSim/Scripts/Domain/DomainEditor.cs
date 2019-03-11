#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Domain))]
public class DomainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Domain domain = (Domain) target;
        if(GUILayout.Button("Randomize Domain"))
        {
            domain.RandomizeDomain();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
