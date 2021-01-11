using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class OvcarAgent : Agent
{
    private Terrain terrain;
    private Vector3 staja;

    float exRazprsenost = 10f;
    float exRazdalja = 50f;
    float exBlizu = 30f;
    int razdelitevHitrosti = 3;
    int razdelitevKota = 3;

    public override void Initialize()
    {
        base.Initialize();
        terrain = GetComponentInParent<Terrain>();
        staja = terrain.center + new Vector3(60f, 0f, 0f);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)  // izvedi akcijo in daj nagrado
    {
        // Convert the first action to forward movement
        float forwardAmount = (1f - actionBuffers.DiscreteActions[0] / (razdelitevHitrosti - 1f) / 3f) * StaticClass.vMax;
        // Convert the second action to turning left or right
        float turn = actionBuffers.DiscreteActions[1] / (razdelitevKota - 1f) * 72f;
        float side = actionBuffers.DiscreteActions[2] > 0.5f ? 1f : -1f;
        // Apply movement
        GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        transform.Rotate(transform.up * turn * side * Time.deltaTime);
        transform.forward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * forwardAmount * Time.deltaTime);
        if (transform.position.y < 0f || transform.position.y > 0.05f)  // tudi pes naj ne leti, rije ali se vrti na raznju
        {
            transform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
        }

        Vector3 GCM = new Vector3(0f, 0f, 0f);
        float najblizja = 200f;
        foreach (GameObject o in terrain.sheepList)
        {
            najblizja = Mathf.Min(najblizja, (o.transform.position - transform.position).magnitude);
            GCM += o.transform.position;
        }
        if (terrain.sheepList.Count > 0) GCM /= terrain.sheepList.Count;
        float razprsenost = 0;
        foreach (GameObject o in terrain.sheepList)
        {
            razprsenost += (GCM - o.transform.position).magnitude;
        }
        AddReward(- Time.deltaTime / 10f);   // majhna nagrada za blizu vsaj kaksni ovci
        if (terrain.sheepList.Count > 0 && terrain.timer > 0.05f)
        {
            if (najblizja > 10f)
                AddReward((exBlizu - najblizja) / (exBlizu - najblizja > 0f ? 1000f : 200f));
            else if (razprsenost > 5f)
                AddReward((exRazprsenost - razprsenost / terrain.sheepList.Count) / 1000f);
            else
                AddReward((exRazdalja - (GCM - staja).magnitude) / 10f);
        }
        exRazprsenost = razprsenost / terrain.sheepList.Count;
        exRazdalja = (GCM - staja).magnitude;
        exBlizu = najblizja;
        // v terrain se izvedejo tudi nagrade za ovce ko pridejo v stajo
    }

    public override void Heuristic(in ActionBuffers actionsOut)   // glede na okolico sam oceni, katera akcija bi bila dobra
    {
        int hitrost = 0;  // izracunaj na poenostavljen voronoi model
        Vector3 smer = new Vector3(0f, 0f, 0f);
        float[] obs = Observe();
        float[] zm = StaticClass.zgornjeMeje;
        float[] sm = StaticClass.spodnjeMeje;

        float d0 = sm[5] + (zm[5] - sm[5]) * 0.3024f;
        float df = sm[6] + (zm[6] - sm[6]) * 0.184f;
        Vector3 center = transform.parent.GetComponent<Terrain>().center;
        if (obs[0] > 0)
        {
            bool soZbrane = obs[1] < 0.7f;
            Vector3 tocka = center + new Vector3(!soZbrane ? obs[8] : obs[10], 0f, !soZbrane ? obs[9] : obs[11]);
            smer = tocka - transform.position;
            if (smer.magnitude > 1e-7) smer = smer.normalized;
            smer = smer * 0.05f + transform.forward.normalized * 0.95f;
            smer = smer.normalized;

            // nastavi hitrost glede na razdaljo do staje ali tocke
            if (obs[7] < df || obs[2] < d0 + 3f)  // blizu cilja ali ovcam
            {
                hitrost = razdelitevHitrosti - 1;
            }
            else
            {
                hitrost = 0;
            }
        }
        float kot = Mathf.Min(1f, Vector3.Angle(transform.forward.normalized, smer) / 3f) * (razdelitevKota - 1f);
        int forwardAction;
        int turnAction = 1;
        int sideAction;
        if (Input.GetKey(KeyCode.W))
        {
            // move forward
            forwardAction = 0;
        } else
        {
            forwardAction = hitrost;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            sideAction = 0;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            sideAction = 1;
        }
        else
        {
            turnAction = Mathf.FloorToInt(kot);
            sideAction = transform.forward.normalized.x * smer.z - smer.x * transform.forward.normalized.z < 0 ? 1 : 0;
        }
        // Put the actions into the array
        actionsOut.DiscreteActions.Array[0] = forwardAction;
        actionsOut.DiscreteActions.Array[1] = turnAction;
        actionsOut.DiscreteActions.Array[2] = sideAction;
    }

    public override void OnEpisodeBegin()
    {
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float[] obs = Observe();
        for (int observation=0; observation<obs.Length; observation++)
            if (observation < 8 || observation > 11) sensor.AddObservation(obs[observation]);  // 1 float = 1 value
        // 16 float = 16 total values
    }

    float kotSmeri(Vector3 smer)
    {
        return Mathf.Atan2(smer.z, smer.x);
    }

    float[] Observe()  // izracunaj razne informacije, ki jih bo potem poznal
    {
        Vector3 center = transform.parent.GetComponent<Terrain>().center;
        Vector3 GCM = new Vector3(0f, 0f, 0f);   // skupno povprecje pozicij ovc
        foreach (GameObject ovca in terrain.sheepList)
        {
            GCM += ovca.transform.position / terrain.sheepList.Count;
        }
        Vector3 pobeglaOvca = new Vector3(1f, 0f, 0f);   // lokacija pobegle ovce in njena razdalja do GCM
        float doCentra = 0f;
        float doNajblizje = 1000f;
        foreach (GameObject ovca in terrain.sheepList)
        {
            float doOvce = (transform.position - ovca.transform.position).magnitude;
            float razdalja = (GCM - ovca.transform.position).magnitude;
            if (doOvce < doNajblizje)
            {
                doNajblizje = doOvce;
            }
            if (ovca.GetComponent<GinelliOvca>().voronoiPes == this.gameObject && (GCM - staja).magnitude > doCentra)
            {
                doCentra = razdalja;
                pobeglaOvca = ovca.transform.position;
            }
        }
        Vector3 najblizjiPes = new Vector3(1f, 0f, 0f);
        float doNajblizjega = 10000f;
        foreach (GameObject o in terrain.sheepardList)
        {
            Vector3 razdalja = transform.position - o.transform.position;
            if (razdalja.magnitude < doNajblizjega && razdalja.magnitude > 0f)
            {
                najblizjiPes = razdalja;
                doNajblizjega = razdalja.magnitude;
            }
        }
        Vector3 Pc = pobeglaOvca + (pobeglaOvca - GCM).normalized * 3f - center;
        Vector3 Pd = GCM + (GCM - staja).normalized * 3f - center;
        float kot = kotSmeri(transform.forward);

        float[] obs = {
            terrain.sheepList.Count,
            doCentra,
            doNajblizje,
            kotSmeri(transform.forward) - kot,
            kotSmeri((pobeglaOvca - transform.position)) - kot,
            (pobeglaOvca - transform.position).magnitude,
            kotSmeri(transform.position - staja) - kot,
            (transform.position - staja).magnitude,
            Pc.x, Pc.z, Pd.x, Pd.z,  // 8-11
            kotSmeri(Pc - transform.position) - kot,
            (Pc - transform.position).magnitude,
            kotSmeri(Pd - transform.position) - kot,
            (Pd - transform.position).magnitude,
            kotSmeri(GCM - transform.position) - kot,
            (GCM - transform.position).magnitude,
            kotSmeri(najblizjiPes) - kot,
            doNajblizjega
        };
        return obs;
    }
}
