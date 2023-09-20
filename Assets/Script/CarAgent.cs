using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System.Linq;

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
    private float lastEpisodeDistance;
    private float distanceToEnd;

    [System.Serializable]
    public class RewardAssignment
	{
		[Tooltip("Reward when agent arrives the goal")]
		public float Arrival    = 5.0f;
		[Tooltip("Reward when agent collides with objects")]
		public float Collision  = -5.0f;
		[Tooltip("Reward when agent runs out off the track")]
		public float OffTrack   = -5.0f;
		[Tooltip("Survival reward")]
		public float Survival   = 0.01f;
        [Tooltip("Stay at one spot reward")]
		public float Stay  = -0.02f;
		[Tooltip("Time limit exceeded reward")]
		public float TimeLimit  = -5.0f;
	}

    [Header("Various settings")]
    // Reward
    public RewardAssignment rewardAssignment;
    public RewardAssignment Rewards
    {
        get => rewardAssignment;
        set => rewardAssignment = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        initialPos =  transform.position;
        initalAng = transform.rotation;
        preDistance = Vector3.Distance(transform.position, endLine.position);
        lastEpisodeDistance = preDistance;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPos;
        transform.rotation = initalAng;
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        preDistance = Vector3.Distance(transform.position, endLine.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // Target and Agent positions
        sensor.AddObservation(transform.position);
        sensor.AddObservation(endLine.position);

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
        distanceToEnd = Vector3.Distance(this.transform.position, endLine.position);
        // print(distanceToEnd);
        // Dead
        if(transform.position.y < -2.0f)
        {
            print("OutOfBound(Falling)");
            SetReward(Rewards.OffTrack);
            EndEpisode();
        }else
        {
            // if move closer
            if(distanceToEnd < preDistance)
            {
                preDistance = distanceToEnd;
                SetReward(Rewards.Survival);
            }
            // if stay on the spot
            else
            {
                SetReward(Rewards.Stay);
            }
        }
        // print(distanceToEnd);
        // Reached target
        if(distanceToEnd <= 0.12f)
        {   
            print("Win");
            print(distanceToEnd);
            SetReward(Rewards.Arrival);
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
            SetReward(Rewards.OffTrack);
            EndEpisode();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "obticle"){
            print("collide with a obticle");
            SetReward(Rewards.Collision);
            EndEpisode();
        }
    }
}
