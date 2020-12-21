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
    readonly float[] turnSpeed = { -120f, -40f, -10f, 0f, 10f, 40f, 120f };

    private Terrain terrain;
    new private Rigidbody rigidbody;
    private Vector3 staja;

    float exRazprsenost = 10f;
    float exRazdalja = 50f;

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
        rigidbody.velocity = new Vector3(0f, 0f, 0f);
        transform.forward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * Time.deltaTime);
        if (transform.position.y < 0f || transform.position.y > 0.05f)  // tudi pes naj ne leti, rije ali se vrti na raznju
        {
            transform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
        }
        transform.Rotate(transform.up * turn * Time.deltaTime);

        Vector3 GCM = new Vector3(0f, 0f, 0f);
        foreach (GameObject o in terrain.sheepList)
        {
            GCM += o.transform.position;
        }
        float razprsenost = 0;
        if (terrain.sheepList.Count > 0) GCM /= terrain.sheepList.Count;
        foreach (GameObject o in terrain.sheepList)
        {
            razprsenost += (GCM - o.transform.position).magnitude;
        }
        AddReward(- Time.deltaTime / 100f);   // majhna nagrada za blizu vsaj kaksni ovci
        if (terrain.sheepList.Count > 0 && terrain.timer > 1f)
        {
            if (razprsenost > 3f * terrain.sheepList.Count) AddReward((exRazprsenost - razprsenost / terrain.sheepList.Count) / 100f);
            if (razprsenost < 5f * terrain.sheepList.Count) AddReward((exRazdalja - Mathf.Pow((GCM - staja).magnitude, 2f)) / 100f);
        }
        exRazprsenost = razprsenost / terrain.sheepList.Count;
        exRazdalja = Mathf.Pow((GCM - staja).magnitude, 2);
        // v terrain se izvedejo tudi nagrade za ovce ko pridejo v stajo
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        int turnAction = 3;
        if (Input.GetKey(KeyCode.W))
        {
            // move forward
            forwardAction = 2;
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
        Vector3 GCM = new Vector3(0f, 0f, 0f);
        Vector3 najdljeStaji = new Vector3(0f, 0f, 0f);
        foreach (GameObject o in terrain.sheepList)
        {
            GCM += o.transform.position;
        }
        if (terrain.sheepList.Count > 0)
        {
            GCM /= terrain.sheepList.Count;
        }
        float razprsenostL2 = 0;
        foreach (GameObject o in terrain.sheepList)
        {
            razprsenostL2 += Mathf.Pow((GCM - o.transform.position).magnitude, 2);
        }
        if (terrain.sheepList.Count > 0)
        {
            razprsenostL2 /= terrain.sheepList.Count;
        }

        float kot = kotSmeri(transform.forward);
        sensor.AddObservation(Mathf.FloorToInt((kotSmeri(GCM - transform.position)-kot) % 360f));  // 1 float = 1 value
        sensor.AddObservation(Mathf.FloorToInt((GCM - transform.position).magnitude));  // 1 float = 1 value
        sensor.AddObservation(Mathf.FloorToInt((kotSmeri(GCM - staja)-kot) % 360f));  // 1 float = 1 value
        sensor.AddObservation(Mathf.FloorToInt((GCM - staja).magnitude));  // 1 float = 1 value
        sensor.AddObservation(Mathf.FloorToInt(razprsenostL2));  // 1 float = 1 value
        sensor.AddObservation(Mathf.FloorToInt(terrain.sheepList.Count));  // 1 float = 1 value
        sensor.AddObservation(Mathf.FloorToInt((kotSmeri(rigidbody.velocity)-kot) % 360f));  // 1 float = 1 value
        sensor.AddObservation(Mathf.FloorToInt(rigidbody.velocity.magnitude));  // 1 float = 1 value

        List<Vector3> razdaljeDoGCM = new List<Vector3>();
        foreach (GameObject o in terrain.sheepList) razdaljeDoGCM.Add(o.transform.position - GCM);
        razdaljeDoGCM = razdaljeDoGCM.OrderBy(order => - order.magnitude).ToList();  // urejen seznam razdalij ovc do crede padajoc
        List<Vector3> razdaljeDoOvc = new List<Vector3>();
        foreach (GameObject o in terrain.sheepList) razdaljeDoOvc.Add(o.transform.position - transform.position);
        razdaljeDoOvc = razdaljeDoOvc.OrderBy(order => order.magnitude).ToList();  // urejen seznam razdalij ovc do crede padajoc
        List<GameObject> razdaljeDoOvcarjev = new List<GameObject>();
        foreach (GameObject o in terrain.sheepardList) razdaljeDoOvcarjev.Add(o);
        razdaljeDoOvcarjev = razdaljeDoOvcarjev.OrderBy(order => (order.transform.position - transform.position).magnitude).ToList();  // urejen seznam razdalij do ovcarjev

        for (int i = 0; i < 3; i++)       // 6 float = 6 values
        {
            sensor.AddObservation(Mathf.FloorToInt((kotSmeri(razdaljeDoGCM[i >= terrain.sheepList.Count ? 0 : i])-kot) % 360f));
            sensor.AddObservation(Mathf.FloorToInt(razdaljeDoGCM[i >= terrain.sheepList.Count ? 0 : i].magnitude));
        }
        for (int i = 0; i < 3; i++)       // 9 float = 9 values
        {
            Vector3 smer = razdaljeDoOvcarjev[i >= terrain.sheepardList.Count ? 0 : i].transform.position - transform.position;
            sensor.AddObservation(Mathf.FloorToInt((kotSmeri(smer)-kot) % 360f));
            sensor.AddObservation(Mathf.FloorToInt(smer.magnitude));
            sensor.AddObservation(Mathf.FloorToInt((kotSmeri(razdaljeDoOvcarjev[i >= terrain.sheepardList.Count ? 0 : i].transform.forward) - kot) % 360f));
        }

        // 23 float = 23 total values
    }

    float kotSmeri(Vector3 smer)
    {
        return Mathf.Atan2(smer.z, smer.x);
    }

}
