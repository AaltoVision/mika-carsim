using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideCheck : MonoBehaviour
{
    public TemplateAgent agent;
    public GameObject track;
    public GameObject ground;

    private bool isOnTrack = false;
    void Awake()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject == track) {
                Debug.Log("Track Enter");
                isOnTrack = true;
        } else if (collider.gameObject == ground)
                Debug.Log("Ground Enter");
    }
    private void OnTriggerExit(Collider collider)
    {
        if (isOnTrack) {
            isOnTrack = false;
            agent.OnCrash();
        }
    }
}
