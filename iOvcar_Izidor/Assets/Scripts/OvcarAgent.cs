using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;

public class OvcarAgent : Agent
{
    private Terrain terrain;
    new private Rigidbody rigidbody;
    private Vector3 staja;

    float exRazprsenost = 10f;
    float exRazdalja = 50f;
    float exBlizu = 30f;
    float minHitrost = 1f / 2f;
    float razdelitevHitrosti = 5f;
    float maxKot = 3f;
    float razdelitevKota = 3f;

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
        float forwardAmount = (actionBuffers.DiscreteActions[0] / razdelitevHitrosti * (1f - minHitrost) + minHitrost) * StaticClass.vMax;
        // Convert the second action to turning left or right
        float turn = (actionBuffers.DiscreteActions[1] / razdelitevKota) * 360f * maxKot *
            (actionBuffers.DiscreteActions[2] > 0.5f ? 1f : -1f);
        // Apply movement
        rigidbody.velocity = new Vector3(0f, 0f, 0f);
        transform.Rotate(transform.up * turn * Time.deltaTime);
        transform.forward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * Time.deltaTime);
        if (transform.position.y < 0f || transform.position.y > 0.05f)  // tudi pes naj ne leti, rije ali se vrti na raznju
        {
            transform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
        }

        Vector3 GCM = new Vector3(0f, 0f, 0f);
        float najblizja = 50f;
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
        AddReward(- Time.deltaTime / 100f);   // majhna nagrada za blizu vsaj kaksni ovci
        if (terrain.sheepList.Count > 0 && terrain.timer > 0.05f)
        {
            if (najblizja > 5f)
                AddReward(Mathf.Max(0f, exBlizu - najblizja) / 100f);
            else if (razprsenost > 5f)
                AddReward(Mathf.Max(0f, exRazprsenost - razprsenost / terrain.sheepList.Count) / 100f);
            else
                AddReward(Mathf.Max(0f, exRazdalja - (GCM - staja).magnitude) / 10f);
        }
        exRazprsenost = razprsenost / terrain.sheepList.Count;
        exRazdalja = (GCM - staja).magnitude;
        exBlizu = najblizja;
        // v terrain se izvedejo tudi nagrade za ovce ko pridejo v stajo
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Stroembom 10, AI1 1 Opt
        float[] zm = StaticClass.zgornjeMeje;
        float[] sm = StaticClass.spodnjeMeje;
        float[] gen = { 0.8709f, 0.5468f, 0.7486f, 0.3398f, 0.3559f, 0.3024f, 0.184f, 0.3836f, 0.2992f, 0.2832f, 0.7336f, 0.2673f,
            0.0587f, 0.4105f, 0.5105f, 0.562f, 0.0303f, 0.6609f, 0.9649f, 0.7463f, 0.9061f }; 

        int i = 0;
        float v1 = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float ra = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float dc = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float da = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float d0 = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float df = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float e = sm[i] + (zm[i] - sm[i]) * gen[i]; i += 4;
        float pomenRazdalje = sm[0] + (zm[0] - sm[0]) * gen[0];
        float pomenDoOvce = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float dovoljenoSpredaj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float dovoljenoZadaj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float pomenOvcarjev = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float protiTockiNazaj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float udobnaRazdalja = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float blizuTocki = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float dljeCilju = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float rotiraj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        float pomenSmeriDrugih = sm[i] + (zm[i] - sm[i]) * gen[i];

        Vector3 center = transform.parent.GetComponent<Terrain>().center;
        Vector3 smer = new Vector3(0f, 0f, 0f);
        Vector3 prejsnaSmer = transform.forward.normalized;
        float hitrost = 0f;
        if (terrain.sheepList.Count > 0)
        {
            Vector3 GCM = new Vector3(0f, 0f, 0f);   // skupno povprecje pozicij ovc
            foreach (GameObject ovca in terrain.sheepList)
            {
                GCM += ovca.transform.position;
            }
            GCM /= terrain.sheepList.Count;
            float fN = ra * Mathf.Sqrt(terrain.sheepList.Count);   // najvecja dovoljena velikost crede (polmer)

            Vector3 pobeglaOvca = new Vector3(0f, 0f, 0f);   // lokacija pobegle ovce in njena razdalja do GCM
            float doPobegle = 0;
            float doCentra = 0;
            bool soZbrane = true;
            bool preblizu = false;
            int pobeglih = 0;
            foreach (GameObject ovca in terrain.sheepList)
            {
                float doOvce = (transform.position - ovca.transform.position).magnitude;
                if (doOvce < da)
                {
                    preblizu = true;    // ce mora pes zaokroziti okrog kaksne ovce ker ji je preblizu
                    // print("preblizu ovci -> zaokrozi");
                }
                float razdalja = (GCM - ovca.transform.position).magnitude;
                if (razdalja > fN)
                {
                    pobeglih++;
                }
                // mera za dolocanje katero ovco je treba pripeljati blizje (razdalja do GCM bolj pomembna kot do ovcarja a tudi ta ni nepomembna)
                float mera = Mathf.Pow(razdalja, pomenRazdalje) / Mathf.Pow(doOvce, pomenDoOvce);
                if (ovca.GetComponent<GinelliOvca>().voronoiPes == this.gameObject && razdalja > fN && (staja - ovca.transform.position).magnitude > (GCM - staja).magnitude - dovoljenoSpredaj && mera > doPobegle)
                {
                    // pes vodi proti GCM le ovce v njegovi Voronoievi celici, ki so od cilja dlje kot gcm
                    // ko ima celico prazno ali nobena ovca v njegovi celici ni pobegla, vodi celo credo, kot da je zbrana
                    doCentra = razdalja;
                    doPobegle = mera;
                    pobeglaOvca = ovca.transform.position;
                }
            }
            if (doCentra > fN && Mathf.Pow(terrain.sheepardList.Count, pomenOvcarjev) * pobeglih > terrain.sheepList.Count * dovoljenoZadaj)   // ce je pobeglih ovc vec kot 15% / #ovcarjev^2
            {
                soZbrane = false;
            }

            Vector3 Pc = new Vector3(0f, 0f, 0f);   // tocka za zbiranje ovc v credo ali nakljucni premik
            Vector3 Pd = new Vector3(0f, 0f, 0f);   // tocka za vodenje crede
            if (!soZbrane)
            {
                // ovcar se postavi zadaj za pobeglo ovco glede na credo
                Pc = pobeglaOvca + (pobeglaOvca - GCM).normalized * dc - center;
            }
            else
            {
                // ovcar se postavi zadaj za credo glede na stajo
                Pd = GCM + (GCM - staja).normalized * (fN + ra) - center;
            }
            // tocka znotraj ograje
            Vector3 tocka = (center + new Vector3(Mathf.Max(-49.8f, Mathf.Min(49.8f, (Pc + Pd).x)), 0f, Mathf.Max(-49.8f, Mathf.Min(49.8f, (Pc + Pd).z))));

            float premikNazaj = (transform.position - staja).magnitude - (tocka - staja).magnitude;
            if (premikNazaj < 0)
            {
                smer = (transform.position - staja) * Mathf.Sqrt(-premikNazaj) + (tocka - transform.position) * protiTockiNazaj;
            }
            else smer = tocka - transform.position;

            foreach (GameObject izbran in terrain.sheepardList)   // ovcarji naj se med seboj izogibajo in drzijo razdaljo
            {
                Vector3 razdalja = izbran.transform.position - transform.position;
                if (razdalja.magnitude > 1e-3f)
                {
                    smer -= razdalja.normalized * udobnaRazdalja / (razdalja.magnitude + 1f);
                }
            }
            smer = smer.normalized;

            // ne zaokrozi, ce je skoraj na tocki
            if ((transform.position - tocka).magnitude < blizuTocki ||
                (transform.position - GCM).magnitude > (GCM - tocka).magnitude * dljeCilju) { }  // ce sem blizu tocki ali dlje od GCM kot tocka ne zaokrozim
            else if (preblizu)
            {  // https://math.stackexchange.com/questions/274712/calculate-on-which-side-of-a-straight-line-is-a-given-point-located
                Vector3 rotLevo = new Vector3(Mathf.Cos(rotiraj) * smer.x - Mathf.Sin(rotiraj) * smer.z,
                    0f, Mathf.Cos(rotiraj) * smer.z + Mathf.Sin(rotiraj) * smer.x);
                Vector3 rotDesno = new Vector3(Mathf.Cos(-rotiraj) * smer.x - Mathf.Sin(-rotiraj) * smer.z,
                    0f, Mathf.Cos(-rotiraj) * smer.z + Mathf.Sin(-rotiraj) * smer.x);
                float dGCM = (GCM.x - transform.position.x) * smer.z - (GCM.z - transform.position.z) * smer.x;
                float dRotLevo = rotLevo.x * smer.z - rotLevo.z * smer.x;
                if (dGCM * dRotLevo < 0)  // GCM on the other side as rotation in left
                {
                    smer = rotLevo;  // zaokrozi okrog (izberi tisti vektor ki res zaokrozi
                }
                else { smer = rotDesno; }
            }

            Vector3 smerPsov = new Vector3(0f, 0f, 0f);  // ovcar naj si zeli, da gredo skupaj v povprecju proti cilju
            // bolj naj uposteva blizje ovcarje (tudi sebe)
            foreach (GameObject o in terrain.sheepardList)
            {
                Vector3 razdalja = transform.position - o.transform.position;
                float oProtiOvcarju = razdalja.normalized.x * o.GetComponent<OvcarAgent>().rigidbody.velocity.normalized.x +
                    razdalja.normalized.z * o.GetComponent<OvcarAgent>().rigidbody.velocity.normalized.z;
                // bolj kot gre o proti ovcarju, blize je ta stevilka 1, ce gre direktno stran pa bolj -1
                float ovcarProtiO = -razdalja.normalized.x * smer.normalized.x - razdalja.normalized.z * smer.normalized.z;
                // bolj kot gre drug proti meni, bolj naj jaz ne grem proti njemu, ce je blizu
                float factor = Mathf.Max(oProtiOvcarju, ovcarProtiO); // ce gre vsaj eden proti drugemu, si zeli iti stran
                smerPsov += razdalja.normalized / (razdalja.magnitude + 1f) * factor;
            }
            if (smerPsov.magnitude > 1e-7f) smerPsov = smerPsov.normalized;
            smer += smerPsov * pomenSmeriDrugih;

            if (smer.magnitude > 1e-7) smer = smer.normalized;

            float phi = e * Random.Range(-Mathf.PI / 3f, Mathf.PI / 3f);  // dodaj sum
            smer = (1f - e) * smer + e * new Vector3(Mathf.Cos(phi), 0f, Mathf.Sin(phi));
            if (smer.magnitude > 1e-7) smer = smer.normalized;
            smer = smer * 0.05f + prejsnaSmer * 0.95f;
            smer = smer.normalized;

            // nsatavi hitrost glede na razdaljo do staje ali tocke
            if ((transform.position - staja).magnitude < df || (transform.position - tocka).magnitude < d0)  // blizu cilja ali ovcam
            {
                hitrost = 0f;
            }
            else
            {
                hitrost = razdelitevHitrosti - 1f;
            }
        }
        float kot = Mathf.FloorToInt((Vector3.Angle(prejsnaSmer, smer) / maxKot) * razdelitevKota);
        float stran = prejsnaSmer.x * smer.z - smer.x * prejsnaSmer.z < 0 ? 1f : 0f;
        float forwardAction;
        float turnAction;
        if (Input.GetKey(KeyCode.W))
        {
            // move forward
            forwardAction = 1;
        } else
        {
            forwardAction = hitrost;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            turnAction = 0f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            turnAction = 1f;
        }
        else
        {
            turnAction = kot;
        }
        // Put the actions into the array
        actionsOut.DiscreteActions.Array[0] = Mathf.FloorToInt(forwardAction);
        actionsOut.DiscreteActions.Array[1] = Mathf.FloorToInt(turnAction);
        actionsOut.DiscreteActions.Array[2] = Mathf.FloorToInt(stran);
    }

    public override void OnEpisodeBegin()
    {
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 center = transform.parent.GetComponent<Terrain>().center;
        Vector3 GCM = new Vector3(0f, 0f, 0f);   // skupno povprecje pozicij ovc
        foreach (GameObject ovca in terrain.sheepList)
        {
            GCM += ovca.transform.position / terrain.sheepList.Count;
        }
        Vector3 pobeglaOvca = new Vector3(1f, 0f, 0f);   // lokacija pobegle ovce in njena razdalja do GCM
        float doCentra = 0f;
        foreach (GameObject ovca in terrain.sheepList)
        {
            float doOvce = (transform.position - ovca.transform.position).magnitude;
            float razdalja = (GCM - ovca.transform.position).magnitude;
            if (ovca.GetComponent<GinelliOvca>().voronoiPes == this.gameObject && (GCM - staja).magnitude > doCentra)
            {
                doCentra = razdalja;
                pobeglaOvca = ovca.transform.position;
            }
        }
        Vector3 najblizjiPes = new Vector3(1f, 0f, 0f);
        float doNajblizjega = 10000f;
        Vector3 smerNajblizjega = new Vector3(1f, 0f, 0f);
        foreach (GameObject o in terrain.sheepardList)
        {
            Vector3 razdalja = transform.position - o.transform.position;
            if (razdalja.magnitude < doNajblizjega && razdalja.magnitude > 0f)
            {
                najblizjiPes = razdalja;
                doNajblizjega = razdalja.magnitude;
                smerNajblizjega = o.transform.forward;
            }
        }
        Vector3 Pc = pobeglaOvca + (pobeglaOvca - GCM).normalized - center;
        Vector3 Pd = GCM + (GCM - staja).normalized - center;
        float razprsenost = 0;
        foreach (GameObject o in terrain.sheepList)
        {
            razprsenost += (GCM - o.transform.position).magnitude / terrain.sheepList.Count;
        }
        float kot = kotSmeri(transform.forward);

        float[] obs = {
            kotSmeri(transform.forward),
            rigidbody.velocity.magnitude, 
            terrain.sheepList.Count, 
            terrain.sheepardList.Count,
            doCentra, 
            kotSmeri((pobeglaOvca - transform.position)) - kot,
            (pobeglaOvca - transform.position).magnitude,
            (transform.position - staja).magnitude,
            kotSmeri(transform.position - staja) - kot,
            kotSmeri(Pc - staja),
            (Pc - staja).magnitude, 
            kotSmeri(Pd - staja),
            (Pd - staja).magnitude,
            kotSmeri(Pc - transform.position) - kot, 
            (Pc - transform.position).magnitude,
            kotSmeri(Pd - transform.position) - kot, 
            (Pd - transform.position).magnitude,
            kotSmeri(GCM - transform.position) - kot,
            (GCM - transform.position).magnitude, 
            kotSmeri(Pd - GCM), 
            (Pd - GCM).magnitude,
            kotSmeri(Pc - GCM), 
            (Pc - GCM).magnitude,
            kotSmeri(najblizjiPes) - kot,
            doNajblizjega, 
            kotSmeri(smerNajblizjega) - kot,
            razprsenost
        };
        foreach (float observation in obs)
            sensor.AddObservation(observation);  // 1 float = 1 value
        // 27 float = 27 total values
    }

    float kotSmeri(Vector3 smer)
    {
        return Mathf.Atan2(smer.z, smer.x);
    }

}
