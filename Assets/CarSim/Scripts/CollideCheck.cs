using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideCheck : MonoBehaviour
{
    public TemplateAgent agent;

    void Awake()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    private void OnTriggerExit(Collider collider)
    {
        agent.OnCrash();
    }
}
