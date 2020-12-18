using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class OvcarAgent : Agent
{
    readonly float[] moveSpeed = { 0, StaticClass.vMax / 3f, StaticClass.vMax * 2f/3f, StaticClass.vMax };
    readonly float[] turnSpeed = { -45f, -20f, -5f, 0f, 5f, 20f, 45f };

    private Terrain terrain;
    new private Rigidbody rigidbody;
    private Vector3 staja;

    public override void Initialize()
    {
        base.Initialize();
        terrain = GetComponentInParent<Terrain>();
        staja = terrain.center + new Vector3(60f, 0f, 0f);
        rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Convert the first action to forward movement
        float forwardAmount = moveSpeed[actionBuffers.DiscreteActions[0]];
        // Convert the second action to turning left or right
        float turn = turnSpeed[actionBuffers.DiscreteActions[1]];
        // Apply movement
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * Time.fixedDeltaTime);
        transform.Rotate(transform.up * turn * Time.fixedDeltaTime);
        if (transform.position.y < 0f || transform.position.y > 0.05f)  // tudi pes naj ne leti, rije ali se vrti na raznju
        {
            transform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
            transform.forward = new Vector3(transform.forward.x, 0.01f, transform.forward.z);
        }
        // Apply a tiny negative reward every step to encourage action
        if (MaxStep > 0) AddReward(-1f / MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        int turnAction = 0;
        if (Input.GetKey(KeyCode.W))
        {
            // move forward
            forwardAction = 3;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            turnAction = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            turnAction = 5;
        }

        // Put the actions into the array
        actionsOut.DiscreteActions.Array[0] = forwardAction;
        actionsOut.DiscreteActions.Array[1] = turnAction;
    }

    public override void OnEpisodeBegin()
    {
        terrain.ResetTerrain();
    }






    public override void CollectObservations(VectorSensor sensor)
    {
        // terrain.sheepList  (število, vektorji do najbližjih - pomnoži jih če jih je manj, GCM, najbližje tri iz moje celice, velikost celice), 
        // terrain.sheepardList (število, vektor do najbližjih - pomnoži jih če jih je manj, smer),
        // staja glede na ovcarja, GCM, najblizje in najbolj oddaljene ovce)
        // smer ovcarja

        // List<DNA> Order = terrain.sheepList.OrderBy(order => order.fitness).ToList();  // narascajoc seznam

        // Distance to the baby (1 float = 1 value)
        sensor.AddObservation(Vector3.Distance(staja, transform.position));

        // Direction to baby (1 Vector3 = 3 values)
        sensor.AddObservation((staja - transform.position).normalized);

        // Direction penguin is facing (1 Vector3 = 3 values)
        sensor.AddObservation(transform.forward);

        // 1 + 1 + 3 + 3 = 8 total values
    }

    /*
     * sistem nagrajevanja v OnActionReceived:
     AddReward(1f);  (lahko tudi negativne ali necele vrednosti)
    */
}
