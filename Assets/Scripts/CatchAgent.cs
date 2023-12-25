using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatchAgent : Agent
{
	Rigidbody rBody;
	public Transform Target;
	public bool targetMovesItself;
	public float forceMultiplier = 50;
	private float startEpisodeTime;

	void Start()
	{
		rBody = GetComponent<Rigidbody>();
	}

	public override void OnEpisodeBegin()
	{
		startEpisodeTime = Time.time;
		if (!targetMovesItself)
			Target.transform.localPosition = new Vector3(Random.value * 60 - 30, 0.5f, Random.value * 60 - 30);
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		// Target and Agent positions
		sensor.AddObservation(Target.localPosition);
		sensor.AddObservation(transform.localPosition);

		// Agent velocity
		sensor.AddObservation(rBody.velocity.x);
		sensor.AddObservation(rBody.velocity.z);
	}
	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		// Actions, size = 2
		var controlSignal = Vector3.zero;
		controlSignal.x = actionBuffers.ContinuousActions[0];
		controlSignal.z = actionBuffers.ContinuousActions[1];
		rBody.AddForce(controlSignal * forceMultiplier);

		//Rewards
		var distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

		if (distanceToTarget < 1.42f)
		{
			SetReward(Math.Clamp(startEpisodeTime - Time.time,1f, 100f) );
			EndEpisode();
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.CompareTag("Wall"))
		{
			SetReward(-5f);
			rBody.angularVelocity = Vector3.zero;
			rBody.velocity = Vector3.zero;
			transform.localPosition = new Vector3(Random.value * 60 - 30, 0.5f, Random.value * 60 - 30);
			EndEpisode();
		}
	}

	public override void Heuristic(in ActionBuffers actionsOut)
	{
		var continuousActionsOut = actionsOut.ContinuousActions;
		continuousActionsOut[0] = Input.GetAxis("Vertical");
		continuousActionsOut[1] = Input.GetAxis("Horizontal");
	}
}
