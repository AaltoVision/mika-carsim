using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideCheck : MonoBehaviour
{
    public TemplateAgent agent;
    public GameObject track;
    public long frameNum {get; set;}
    private bool isOnTrack = true;

    void Awake()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject == track) {
            isOnTrack = false;
            agent.SetOnTrack(false);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject == track) {
            agent.SetOnTrack(true);
            isOnTrack = true;
        }
    }

    void Update() {
    }
}
