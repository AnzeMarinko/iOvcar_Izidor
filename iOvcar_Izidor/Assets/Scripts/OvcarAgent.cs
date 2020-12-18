using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;

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

        float doNajblizje = 1000;
        Vector3 GCM = new Vector3(0f, 0f, 0f);
        foreach (GameObject o in terrain.sheepList)
        {
            doNajblizje = Mathf.Min((o.transform.position - transform.position).magnitude, doNajblizje);
            GCM += o.transform.position;
        }
        float razprsenost = 0;
        foreach (GameObject o in terrain.sheepList)
        {
            razprsenost += Mathf.Pow((GCM - o.transform.position).magnitude, 2);
        }
        AddReward(- doNajblizje / 10000f);   // majhna nagrada za blizu vsaj kaksni ovci
        if (terrain.sheepList.Count > 0)
        {
            AddReward(- Mathf.Sqrt(razprsenost / terrain.sheepList.Count) / 500);     // vecja nagrada ce je creda bolj skupaj
            AddReward(-(GCM / terrain.sheepList.Count - staja).magnitude / 50f);     // se vecja nagrada ce je creda blizu staji
        }
        // v terrain se izvedejo tudi nagrade za oce ko pridejo v stajo
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
        List<Vector3> razdaljeDoOvc = new List<Vector3>();
        foreach (GameObject o in terrain.sheepList) razdaljeDoOvc.Add(o.transform.position - transform.position);
        razdaljeDoOvc = razdaljeDoOvc.OrderBy(order => order.magnitude).ToList();  // urejen seznam razdalij do ovc
        List<Vector3> razdaljeDoOvcarjev = new List<Vector3>();
        foreach (GameObject o in terrain.sheepardList) razdaljeDoOvcarjev.Add(o.transform.position - transform.position);
        razdaljeDoOvcarjev = razdaljeDoOvcarjev.OrderBy(order => order.magnitude).ToList();  // urejen seznam razdalij do ovcarjev
        
        Vector3 GCM = new Vector3(0f, 0f, 0f);
        int velikostCelice = 0;
        foreach (GameObject o in terrain.sheepList)
        {
            GCM += o.transform.position;
            if (o.GetComponent<GinelliOvca>().voronoiPes == this.gameObject) velikostCelice++;
        }
        if (terrain.sheepList.Count > 0)
        {
            GCM /= terrain.sheepList.Count;
        }
        sensor.AddObservation((GCM - transform.position).normalized);  // 1 Vector3 = 3 values
        sensor.AddObservation((GCM - transform.position).magnitude);  // 1 float = 1 value
        sensor.AddObservation(velikostCelice);  // 1 float = 1 value

        sensor.AddObservation(terrain.sheepList.Count);  // 1 float = 1 value
        sensor.AddObservation(terrain.sheepardList.Count);  // 1 float = 1 value

        for (int i = 0; i < 5; i++)       // 5 Vector3, 5 float = 20 values
        {
            if (i >= terrain.sheepList.Count)
            { 
                sensor.AddObservation(razdaljeDoOvc[0].normalized);
                sensor.AddObservation(razdaljeDoOvc[0].magnitude);
            }
            else
            {
                sensor.AddObservation(razdaljeDoOvc[i].normalized);
                sensor.AddObservation(razdaljeDoOvc[i].magnitude);
            }
        }
        for (int i = 1; i < 3; i++)       // 2 Vector3, 2 float = 8 values
        {
            if (i >= terrain.sheepardList.Count)
            {
                sensor.AddObservation(razdaljeDoOvcarjev[0].normalized);
                sensor.AddObservation(razdaljeDoOvcarjev[0].magnitude);
            }
            else
            {
                sensor.AddObservation(razdaljeDoOvcarjev[i].normalized);
                sensor.AddObservation(razdaljeDoOvcarjev[i].magnitude);
            }
        }
        // Direction penguin is facing (1 Vector3 = 3 values)
        sensor.AddObservation(transform.forward);

        // staja glede na ovcarja, GCM, najblizje in najbolj oddaljene ovce)
        sensor.AddObservation((transform.position - staja).normalized);  // 1 Vector3 = 3 values
        sensor.AddObservation((transform.position - staja).magnitude);  // 1 float = 1 value
        sensor.AddObservation((GCM - staja).normalized);  // 1 Vector3 = 3 values
        sensor.AddObservation((GCM - staja).magnitude);  // 1 float = 1 value

        List<Vector3> razdaljeDoStaje = new List<Vector3>();
        foreach (GameObject o in terrain.sheepList) razdaljeDoStaje.Add(o.transform.position - staja);
        razdaljeDoStaje = razdaljeDoStaje.OrderBy(order => order.magnitude).ToList();  // urejen seznam razdalij do ovc
        sensor.AddObservation(razdaljeDoStaje[0].normalized);  // 1 Vector3 = 3 values
        sensor.AddObservation(razdaljeDoStaje[0].magnitude);  // 1 float = 1 value
        sensor.AddObservation(razdaljeDoStaje[terrain.sheepList.Count - 1].normalized);  // 1 Vector3 = 3 values
        sensor.AddObservation(razdaljeDoStaje[terrain.sheepList.Count - 1].magnitude);  // 1 float = 1 value

        // (3 + 1 + 1) + (1 + 1) + (20 + 8) + 3 + (3 + 1 + 3 + 1 + 3 + 1 + 3 + 1) = 54 total values
    }
}
