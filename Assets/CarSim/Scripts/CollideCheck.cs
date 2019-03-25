using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideCheck : MonoBehaviour
{
    public TemplateAgent agent;
    public GameObject track;
    public long frameNum {get; set;}

    void Awake()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject == track) {
            if (frameNum > 30) {
                frameNum = 0;
                agent.OnCrash();
            }
        }
    }

    void Update() {
        frameNum++;
    }
}
