using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class TemplateAgent : Agent {
    const float rewardDivider = 100.0F;
    public GameObject car;

    private CarController m_Car;
    private Vector3 forward;

    // Use this for initialization
    public override void InitializeAgent()
    {
        AgentReset();
    }

    public override List<float> CollectState()
    {
        List<float> state = new List<float>();
        state.Add(Vector3.Dot(forward, car.GetComponent<Rigidbody>().velocity));
        return state;
    }

    public override void AgentStep(float[] act)
    {
        Rigidbody body = car.GetComponent<Rigidbody>();
        forward = body.transform.forward;

        if (body.transform.position.y < -0.25F) {
            done = true;
            reward = 0.0F;
            return;
        }

        float motor = act[0];
        float steer = act[1];
        float footBrake = 0.0F;
        float handBrake = 0.0F;
        if (motor < 0.0F) {
            footBrake = -motor;
            motor = 0.0F;
        }
        m_Car.Move(steer, motor, footBrake, handBrake);

        reward = Vector3.Dot(forward, car.GetComponent<Rigidbody>().velocity) / rewardDivider;
    }

    public override void AgentReset()
    {
        car.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
        car.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        car.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_Car = car.GetComponent<CarController>();
        forward = car.GetComponent<Rigidbody>().transform.forward;
    }

    public override void AgentOnDone()
    {
        AgentReset();
    }
}
