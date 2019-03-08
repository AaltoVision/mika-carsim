﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using MLAgents;

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

    public override void CollectObservations()
    {
        AddVectorObs(Vector3.Dot(forward, car.GetComponent<Rigidbody>().velocity));
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        Rigidbody body = car.GetComponent<Rigidbody>();
        forward = body.transform.forward;

        if (body.transform.position.y < -0.25F) {
            Done();
            SetReward(0.0f);
            return;
        }

        float motor = vectorAction[0];
        float steer = vectorAction[1];
        float footBrake = 0.0F;
        float handBrake = 0.0F;
        if (motor < 0.0F) {
            footBrake = motor;
            motor = 0.0F;
        }
        m_Car.Move(steer, motor, footBrake, handBrake);

        float reward = Vector3.Dot(forward, car.GetComponent<Rigidbody>().velocity) / rewardDivider;
        SetReward(reward);
    }

    public override void AgentReset()
    {
        car.transform.position = new Vector3(0.0f, 1.0f, 10.0f);
        car.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        Rigidbody body = car.GetComponent<Rigidbody>();
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        m_Car = car.GetComponent<CarController>();
        forward = car.GetComponent<Rigidbody>().transform.forward;
    }

    public override void AgentOnDone()
    {
        AgentReset();
    }
}