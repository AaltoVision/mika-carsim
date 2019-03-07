using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reachGoal : MonoBehaviour {

    public int goal = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        goal = 1;
    }
}
