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
    public float forceMultiplier = 20;
    public Transform endLine;
    private Vector3 initialPos;
    private float preDistance;
    private float totalDistance;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        initialPos =  transform.position;
        preDistance = Vector3.Distance(transform.localPosition, endLine.localPosition);
        totalDistance = preDistance;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPos;
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
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToEnd = Vector3.Distance(this.transform.localPosition, endLine.localPosition);

        // Dead
        if(transform.localPosition.y < 0.0f){
            EndEpisode();
        }else{
            // if move closer
            if(distanceToEnd < preDistance){
                preDistance = distanceToEnd;
                SetReward(0.1f);
            }
            // if stay on the spot
            else{
                SetReward(-0.1f);
            }
        }
        // Reached target
        if(distanceToEnd <= 0.0f)
        {   
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
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "outofbound"){
            print("OutOfBound");
            SetReward(-5.0f);
            EndEpisode();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "obticle"){
            print("collide with a obticle");
            SetReward(-10.0f);
            EndEpisode();
        }
    }
}
