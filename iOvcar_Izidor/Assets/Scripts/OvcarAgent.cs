using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;

public class OvcarAgent : Agent
{
    readonly float[] moveSpeed = { StaticClass.vMax / 3f, StaticClass.vMax * 2f/3f, StaticClass.vMax };
    readonly float[] turnSpeed = { -40f, -10f, 0f, 10f, 40f };

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
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        transform.forward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        if (transform.position.y < 0f || transform.position.y > 0.05f)  // tudi pes naj ne leti, rije ali se vrti na raznju
        {
            transform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
        }
        transform.Rotate(transform.up * turn * Time.deltaTime);

        float doNajblizje = 1000;
        Vector3 GCM = new Vector3(0f, 0f, 0f);
        foreach (GameObject o in terrain.sheepList)
        {
            doNajblizje = Mathf.Min((o.transform.position - transform.position).magnitude, doNajblizje);
            GCM += o.transform.position;
        }
        float razprsenost = 0;
        if (terrain.sheepList.Count > 0) GCM /= terrain.sheepList.Count;
        foreach (GameObject o in terrain.sheepList)
        {
            razprsenost += Mathf.Pow((GCM - o.transform.position).magnitude, 2);
        }
        AddReward((1000f - Mathf.Pow(doNajblizje, 2)) / 1000000f * Time.deltaTime - Time.deltaTime / 2);   // majhna nagrada za blizu vsaj kaksni ovci
        if (terrain.sheepList.Count > 0)
        {
            AddReward((2500 - razprsenost / terrain.sheepList.Count) / 5000 * Time.deltaTime);     // vecja nagrada ce je creda bolj skupaj
            // AddReward((50f - (GCM - staja).magnitude) / 500f * Time.deltaTime);     // se vecja nagrada ce je creda blizu staji
        }
        // v terrain se izvedejo tudi nagrade za ovce ko pridejo v stajo
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        int turnAction = 0;
        if (Input.GetKey(KeyCode.W))
        {
            // move forward
            forwardAction = 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            turnAction = 0;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            turnAction = 4;
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
        Vector3 GCM = new Vector3(0f, 0f, 0f);
        Vector3 najdljeStaji = new Vector3(0f, 0f, 0f);
        float razdalja = 0f;
        foreach (GameObject o in terrain.sheepList)
        {
            if ((staja - o.transform.position).magnitude > razdalja)
            {
                razdalja = (staja - o.transform.position).magnitude;
                najdljeStaji = o.transform.position - transform.position;
            }
            GCM += o.transform.position;
        }
        if (terrain.sheepList.Count > 0)
        {
            GCM /= terrain.sheepList.Count;
        }
        sensor.AddObservation((GCM - transform.position).normalized);  // 1 Vector3 = 3 values
        sensor.AddObservation((GCM - transform.position).magnitude);  // 1 float = 1 value

        sensor.AddObservation(terrain.sheepList.Count);  // 1 float = 1 value
        sensor.AddObservation(terrain.sheepardList.Count);  // 1 float = 1 value

        List<Vector3> razdaljeDoOvc = new List<Vector3>();
        foreach (GameObject o in terrain.sheepList) razdaljeDoOvc.Add(o.transform.position - GCM);
        razdaljeDoOvc = razdaljeDoOvc.OrderBy(order => - order.magnitude).ToList();  // urejen seznam razdalij ovc do crede padajoc
        List<Vector3> razdaljeDoOvcarjev = new List<Vector3>();
        foreach (GameObject o in terrain.sheepardList) razdaljeDoOvcarjev.Add(o.transform.position - transform.position);
        razdaljeDoOvcarjev = razdaljeDoOvcarjev.OrderBy(order => order.magnitude).ToList();  // urejen seznam razdalij do ovcarjev

        for (int i = 0; i < 5; i++)       // 5 Vector3, 5 float = 20 values
        {
            sensor.AddObservation(razdaljeDoOvc[i >= terrain.sheepList.Count ? 0 : i].normalized);
            sensor.AddObservation(razdaljeDoOvc[i >= terrain.sheepList.Count ? 0 : i].magnitude);
        }
        // Direction penguin is facing (1 Vector3 = 3 values)
        sensor.AddObservation(transform.forward);

        sensor.AddObservation((GCM - staja).normalized);  // 1 Vector3 = 3 values
        sensor.AddObservation((GCM - staja).magnitude);  // 1 float = 1 value

        sensor.AddObservation(najdljeStaji.normalized);  // 1 Vector3 = 3 values
        sensor.AddObservation(najdljeStaji.magnitude);  // 1 float = 1 value
        sensor.AddObservation(razdalja);  // 1 float = 1 value

        // (3 + 1) + (1 + 1) + 20 + 3 + (3 + 1 + 3 + 1 + 1) = 38 total values
    }
}
