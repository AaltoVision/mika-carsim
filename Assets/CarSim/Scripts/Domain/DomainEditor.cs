#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Domain))]
public class DomainEditor : Editor
{
    int seed = 0;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Domain domain = (Domain) target;
        int newSeed = EditorGUILayout.IntField("Seed:", seed);
        if (newSeed != seed) {
            domain.seed = newSeed;
            domain.UpdateRandomSource();
        }

        seed = newSeed;
        if(GUILayout.Button("Randomize Domain"))
        {
            domain.RandomizeDomain();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
