using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class CarAgent : Agent
{
    public Rigidbody rBody;
    public Camera cam;
    public float forceMultiplier = 20;
    public float rotationMultiplier = 100;
    public Transform endLine;
    private Vector3 initialPos;
    private Quaternion initalAng;
    private float preDistance;
    private float totalDistance;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        initialPos =  transform.position;
        initalAng = transform.rotation;
        preDistance = Vector3.Distance(transform.localPosition, endLine.localPosition);
        totalDistance = preDistance;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPos;
        transform.rotation = initalAng;
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        preDistance = Vector3.Distance(transform.localPosition, endLine.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // Target and Agent positions
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(endLine.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        // Agent rotation (in degrees)
        sensor.AddObservation(transform.eulerAngles.y);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 3 (forward, rotation, brake)
        float forwardSignal = actionBuffers.ContinuousActions[0];
        float rotationSignal = actionBuffers.ContinuousActions[1];
        // float brakeSignal = actionBuffers.ContinuousActions[2];

        // Move Forward the car
        rBody.AddForce(transform.forward * forwardSignal * forceMultiplier);
        // Brake the car
        // if (brakeSignal > 0)
        // {
        //     Vector3 brakeForce = -rBody.velocity.normalized * brakeSignal * forceMultiplier;
        //     rBody.AddForce(brakeForce);
        // }
        
        // Rotate the car
        transform.Rotate(Vector3.up * rotationSignal * rotationMultiplier * Time.deltaTime);

        // Rewards
        float distanceToEnd = Vector3.Distance(this.transform.localPosition, endLine.localPosition);

        // Dead
        if(transform.localPosition.y < 0.0f)
        {
            print("OutOfBound(Falling)");
            SetReward(-50.0f);
            EndEpisode();
        }else
        {
            // if move closer
            if(distanceToEnd < preDistance)
            {
                preDistance = distanceToEnd;
                // print("Rotate");
                // print(rotationSignal);
                // print("Forward");
                // print(forwardSignal);
                SetReward(0.5f);
            }
            // if stay on the spot
            else
            {
                SetReward(-0.1f);
            }
        }
        // Reached target
        if(distanceToEnd <= 0.5f)
        {   
            print("Win");
            SetReward(100.0f);
            EndEpisode();
        }
    }

    // public override void OnMaximumStepsReached()
	// {
	// 	print("Maximum steps reached");
	// 	SetReward(Rewards.TimeLimit);
    //     EndEpisode();
	// }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");  // Forward/Backward
        continuousActionsOut[1] = Input.GetAxis("Horizontal");  // Rotation
        // continuousActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f; // Brake
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "outofbound"){
            print("OutOfBound");
            SetReward(-50.0f);
            EndEpisode();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "obticle"){
            print("collide with a obticle");
            SetReward(-50.0f);
            EndEpisode();
        }
    }
}
