using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BobAgent : Agent
{
    public Transform hoop;

    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("Bob the Free Throw Champion has entered the arena!");
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0f, 1f, 0f);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(hoop.position - transform.position);
        sensor.AddObservation(rb.velocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuous = actions.ContinuousActions;
        float forceX = continuous[0];
        float forceY = continuous[1] * 5f + 8f;
        float forceZ = continuous[2] * 3f + 12f;

        rb.AddForce(new Vector3(forceX, forceY, forceZ), ForceMode.Impulse);

        float distToHoop = Vector3.Distance(transform.position, hoop.position);
        AddReward(-0.01f);
        if (distToHoop < 1.5f)
        {
            AddReward(1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxis("Horizontal");
        continuous[1] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        continuous[2] = 1f;
    }
}
