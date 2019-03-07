using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human_detection_agent : Agent {

    public GameObject prefab, unitychan_prefab;
    public GameObject goal;
    public float motorMax, steerMax;

    private GameObject car, unitychan;
    private Vector3 forward;
    private WheelCollider fl, fr, hl, hr;

    private Vector3 car_front = new Vector3(0, 0, -3);

    // Use this for initialization
    public override void InitializeAgent()
    {
        AgentReset();
    }

    public override List<float> CollectState()
	{
		List<float> state = new List<float>();
        state.Add(Vector3.Dot(forward, car.GetComponent<Rigidbody>().velocity));
        state.Add(unitychan.transform.rotation.eulerAngles.y);
        state.Add(Vector3.Distance(unitychan.transform.position, car_front));
        return state;
	}

	public override void AgentStep(float[] act)
	{
        
        //Debug.Log(observations[0].transform.position);

        if (unitychan.transform.position.y < -0.1f) //random unitychan appearance
        {
            DestroyImmediate(unitychan);
            Vector3 translation = new Vector3(Random.Range(0, 4.0f), -0.5f, -7.0f - Random.Range(0, 5.0f));
            unitychan = Instantiate(unitychan_prefab, car.transform.position+translation, Quaternion.Euler(0.0f, Random.Range(0, 359.0f), 0.0f));
        }

        float angle_z = car.transform.rotation.eulerAngles.z > 180 ? 360 - car.transform.rotation.eulerAngles.z : car.transform.rotation.eulerAngles.z;
        if ( Mathf.Abs(angle_z) > 10)
        {
            reward = 0;
            done = true;
            return;
        }
        if (goal.GetComponent<reachGoal>().goal == 1)
        {
            goal.GetComponent<reachGoal>().goal = 0;
            reward = 100;
            done = true;
            return;
        }
        if (car.transform.position.y < 0.4f)
        {
            reward = -50;
            done = true;
            return;
        }
        float motor = - act[0];
        float steer = act[1] * steerMax;
        hl.motorTorque = motor * motorMax;
        hr.motorTorque = motor * motorMax;
        Vector3 position;
        Quaternion rotation;

        fl.steerAngle = steer;
        fr.steerAngle = steer;
        fl.GetWorldPose(out position, out rotation);
        fl.transform.rotation = rotation;
        fr.transform.rotation = rotation;
        hr.GetWorldPose(out position, out rotation);
        hl.transform.rotation = rotation;
        hr.transform.rotation = rotation;
        forward = Vector3.Normalize(fl.transform.position - hl.transform.position);
        reward = Vector3.Dot(forward, car.GetComponent<Rigidbody>().velocity)/5;
        if (reward < 0)
        {
            reward *= 4;
        }
    }

	public override void AgentReset()
	{
        DestroyImmediate(car);
        DestroyImmediate(unitychan);
        car = Instantiate(prefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.Euler(0.0f, 0.0f, 0.0f));
        unitychan = Instantiate(unitychan_prefab, new Vector3(Random.Range(0, 4.0f), 0, -7.0f-Random.Range(0, 5.0f)), Quaternion.Euler(0.0f, Random.Range(0, 359.0f), 0.0f));
        car.GetComponent<Rigidbody>().velocity = Vector3.zero;
        observations[0] = car.transform.Find("Camera").GetComponent<Camera>();
        observations[1] = car.transform.Find("human_cam").GetComponent<Camera>();
        fl = car.transform.Find("Alloys01").Find("fl").GetComponent<WheelCollider>();
        fr = car.transform.Find("Alloys01").Find("fr").GetComponent<WheelCollider>();
        hl = car.transform.Find("Alloys01").Find("hl").GetComponent<WheelCollider>();
        hr = car.transform.Find("Alloys01").Find("hr").GetComponent<WheelCollider>();
        forward = Vector3.Normalize(fl.transform.position - hl.transform.position);
    }

	public override void AgentOnDone()
	{
        AgentReset();
	}
}
