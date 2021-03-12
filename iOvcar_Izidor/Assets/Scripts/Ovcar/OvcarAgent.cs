using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.IO;

public class OvcarAgent : Agent
{
    private Terrain terrain;
    private Vector3 staja;
    public Vector3 smer;
    Vector3 prejsnaSmer;
    float v2;  // Hitrost premikanja v stanju iskanja ovc
    Vector3 cilj;   // pozicija staje
    public Vector3 tocka = new Vector3(0f, 0f, 0f);
    float hitrost = 0;
    float casDoNakljucnegaPremika;
    bool zacetekNakljucnegaPremika = false;
    Vector3 smerNakljucnegaPremika = new Vector3(0f, 0f, 0f);
    bool nakljucenPremik = false;   // ali sem v stanju nakljucnega premika, koliko ta traja, koliko casa je se do naslednjega, smer nakljucnega premika
    public float v1 = 5f;  // Hitrost premikanja v stanju vodenja ovc
    public float ra = 2;   // Faktor za dovoljeno velikost crede
    public float dc = 10;  // Razdalja za zbiranje ovc v credo
    public float da = 20;   // Razdalja za zaznavo ovc na poti
    public float d0 = 10;   // Razdalja za upocasnitve v blizini ovc
    public float df = 5;   // Razdalja za upocasnitev v blizini cilja
    public float e = 0.3f;  // Relativna moc suma
    public float trajanjeNakljucnegaPremika = 3f;
    public float casNakljucnegaPremika = 60f;
    public float nakljucniDodatek = 20f;
    public float pomenRazdalje = 2f;
    public float pomenDoOvce = 1f;
    public float dovoljenoSpredaj = 2f;
    public float dovoljenoZadaj = 0.15f;
    public float pomenOvcarjev = 2f;
    public float protiTockiNazaj = 2f;
    public float udobnaRazdalja = 20f;
    public float blizuTocki = 12f;
    public float dljeCilju = 0.95f;
    public float rotiraj = Mathf.PI / 6f;
    public float pomenSmeriDrugih = 0.1f;
    public float exGCM = 1000f;
    int pobeglih = 0;

    public override void Initialize()
    {
        base.Initialize();
        terrain = GetComponentInParent<Terrain>();
        staja = terrain.center + new Vector3(175f, 0f, 0f);
        v2 = StaticClass.vMax;   // hitrost v stanju teka
        casDoNakljucnegaPremika = Random.Range(0f, nakljucniDodatek) + casNakljucnegaPremika;   // nakjucen premik naj bo ob nakljucnem casu cez slabo minuto
        smer = transform.rotation.eulerAngles;
        prejsnaSmer = smer;
        StaticClass.ComputeGen();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)  // izvedi akcijo in daj nagrado
    {
        float[] gen = new float[21];
        for(int i=0; i<21; i++) gen[i] = Mathf.Clamp(actionBuffers.ContinuousActions[i], -1f, 1f) / 2f + 0.5f;
        ComputeOwnParameters(gen);
        VoronoiUpdate();

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
        AddReward(- Time.deltaTime / terrain.maxCas);
        if (exGCM - (GCM - staja).magnitude > Time.deltaTime * v2 / 3)
        {
            exGCM = (GCM - staja).magnitude;
            AddReward(Time.deltaTime / terrain.maxCas);
        }
        // v terrain se izvedejo tudi nagrade za ovce ko pridejo v stajo
    }

    public override void Heuristic(in ActionBuffers actionsOut)   // glede na okolico sam oceni, katera akcija bi bila dobra
    {
        // Put the actions into the array
        // Optimal gen from genetic
        GinelliOvca.ModelGibanja gin = terrain.sm.DNA.modelGibanja;
        int n1 = terrain.sm.DNA.nOvc;
        OvcarEnum.ObnasanjePsa vod = OvcarEnum.ObnasanjePsa.AI1;
        int n2 = terrain.sm.DNA.nOvcarjev;
        string[] geni = optGen.opt.Split('\n');
        float najblizje = 1000f;
        float[] genf = new float[21];
        foreach (string line in geni)
        {
            string[] kombinacija = line.Split(' ');
            float razdalja = (kombinacija[0].Contains(gin.ToString()) ? 1f : 100f) *
                (Mathf.Abs(int.Parse(kombinacija[1]) - n1) + 1f) *
                (kombinacija[2].Contains(vod.ToString()) ? 1f : 100f) *
                (Mathf.Abs(int.Parse(kombinacija[3]) - n2) + 1f);
            if (razdalja < najblizje)
            {
                najblizje = razdalja;
                string[] gen = kombinacija[4].Split(';');
                for (int i = 0; i < 21; i++) genf[i] = (float.Parse(gen[i]) - 0.5f) * 2f;
            }
        }
        for (int i = 0; i < 21; i++) actionsOut.ContinuousActions.Array[i] = genf[i];
    }

    public override void OnEpisodeBegin()
    {
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 GCM = new Vector3(0f, 0f, 0f);   // skupno povprecje pozicij ovc
        foreach (GameObject ovca in terrain.sheepList)
        {
            GCM += ovca.transform.position / terrain.sheepList.Count;
        }
        sensor.AddObservation(Mathf.Min(terrain.sheepList.Count / 150f, 1f));
        sensor.AddObservation(pobeglih / (terrain.sheepList.Count + 1f));
        sensor.AddObservation(Mathf.Min(terrain.sheepardList.Count / 7f, 1f));
        sensor.AddObservation(terrain.modelGibanja == GinelliOvca.ModelGibanja.Ginelli ? 1f : 0f);
        sensor.AddObservation(terrain.modelGibanja == GinelliOvca.ModelGibanja.Stroembom ? 1f : 0f);
        sensor.AddObservation(Mathf.Min((GCM.x - staja.x) / 150f, 1f));
        sensor.AddObservation(Mathf.Min((GCM.z - staja.z) / 150f, 1f));
        sensor.AddObservation(Mathf.Min((transform.position.x - staja.x) / 150f, 1f));
        sensor.AddObservation(Mathf.Min((transform.position.z - staja.z) / 150f, 1f));
        // 9 float = 9 total values
    }
    public void ComputeOwnParameters(float[] gen)
    {
        float[] zm = StaticClass.zgornjeMeje;
        float[] sm = StaticClass.spodnjeMeje;

        int i = 0;
        v1 = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        ra = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        dc = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        da = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        d0 = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        df = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        e = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        trajanjeNakljucnegaPremika = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        casNakljucnegaPremika = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        nakljucniDodatek = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        pomenRazdalje = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        pomenDoOvce = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        dovoljenoSpredaj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        dovoljenoZadaj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        pomenOvcarjev = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        protiTockiNazaj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        udobnaRazdalja = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        blizuTocki = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        dljeCilju = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        rotiraj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        pomenSmeriDrugih = sm[i] + (zm[i] - sm[i]) * gen[i];
    }

    public void VoronoiUpdate()
    {
        Vector3 center = transform.parent.GetComponent<Terrain>().center;
        cilj = center + new Vector3(175f, 0f, 0f);
        casDoNakljucnegaPremika -= Time.deltaTime;
        if (terrain.sheepList.Count > 0)
        {
            Vector3 GCM = new Vector3(0f, 0f, 0f);   // skupno povprecje pozicij ovc
            foreach (GameObject ovca in terrain.sheepList)
            {
                GCM += ovca.transform.position;
            }
            GCM /= terrain.sheepList.Count;
            float fN = F(terrain.sheepList.Count);   // najvecja dovoljena velikost crede (polmer)

            Vector3 pobeglaOvca = new Vector3(0f, 0f, 0f);   // lokacija pobegle ovce in njena razdalja do GCM
            float doPobegle = 0;
            float doCentra = 0;
            bool soZbrane = true;
            bool preblizu = false;
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
                if (ovca.GetComponent<GinelliOvca>().voronoiPes == this.gameObject && razdalja > fN && (cilj - ovca.transform.position).magnitude > (GCM - cilj).magnitude - dovoljenoSpredaj && mera > doPobegle)
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
            if (casDoNakljucnegaPremika < 0f)
            {
                casDoNakljucnegaPremika = Random.Range(0f, nakljucniDodatek) + casNakljucnegaPremika;
                nakljucenPremik = false;
                zacetekNakljucnegaPremika = true;
            }
            else if (casDoNakljucnegaPremika < trajanjeNakljucnegaPremika)
            {
                nakljucenPremik = true;
            }

            Vector3 Pc = new Vector3(0f, 0f, 0f);   // tocka za zbiranje ovc v credo ali nakljucni premik
            Vector3 Pd = new Vector3(0f, 0f, 0f);   // tocka za vodenje crede
            if (nakljucenPremik)
            {
                if (zacetekNakljucnegaPremika)
                {
                    Pc = new Vector3(Random.Range(-120f, 0f), 0f, Random.Range(-120f, 120f));
                    smerNakljucnegaPremika = Pc;
                    zacetekNakljucnegaPremika = false;
                }
                else
                {
                    Pc = smerNakljucnegaPremika;
                }
            }
            else if (!soZbrane)
            {
                // ovcar se postavi zadaj za pobeglo ovco glede na credo
                Pc = pobeglaOvca + (pobeglaOvca - GCM).normalized * dc - center;
            }
            else
            {
                // ovcar se postavi zadaj za credo glede na stajo
                Pd = GCM + (GCM - cilj).normalized * (fN + ra) - center;
            }
            // tocka znotraj ograje
            tocka = (center + new Vector3(Mathf.Max(-149.5f, Mathf.Min(149.5f, (Pc + Pd).x)), 0f, Mathf.Max(-149.5f, Mathf.Min(149.5f, (Pc + Pd).z)))) * 0.05f + tocka * 0.95f;

            float premikNazaj = (transform.position - cilj).magnitude - (tocka - cilj).magnitude;
            if (premikNazaj < 0)
            {
                smer = (transform.position - cilj) * Mathf.Sqrt(-premikNazaj) + (tocka - transform.position) * protiTockiNazaj;
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
                if (razdalja.magnitude > 0.001f)
                {
                    float oProtiOvcarju = razdalja.normalized.x * o.GetComponent<OvcarAgent>().smer.normalized.x +
                        razdalja.normalized.z * o.GetComponent<OvcarAgent>().smer.normalized.z;
                    // bolj kot gre o proti ovcarju, blize je ta stevilka 1, ce gre direktno stran pa bolj -1
                    float ovcarProtiO = -razdalja.normalized.x * smer.normalized.x - razdalja.normalized.z * smer.normalized.z;
                    // bolj kot gre drug proti meni, bolj naj jaz ne grem proti njemu, ce je blizu
                    float factor = Mathf.Max(oProtiOvcarju, ovcarProtiO); // ce gre vsaj eden proti drugemu, si zeli iti stran
                    smerPsov += razdalja.normalized / (razdalja.magnitude + 1f) * factor;
                }
            }
            if (smerPsov.magnitude > 1e-7f) smerPsov = smerPsov.normalized;
            smer += smerPsov * pomenSmeriDrugih;

            if (smer.magnitude > 1e-7) smer = smer.normalized;

            float phi = e * Random.Range(-Mathf.PI / 3f, Mathf.PI / 3f);  // dodaj sum
            smer = (1f - e) * smer + e * new Vector3(Mathf.Cos(phi), 0f, Mathf.Sin(phi));
            if (smer.magnitude > 1e-7) smer = smer.normalized;
            smer = smer * 0.95f + prejsnaSmer * 0.05f;  // gladko gibanje
            prejsnaSmer = smer;

            // nsatavi hitrost glede na razdaljo do staje ali tocke
            if ((transform.position - cilj).magnitude < df || (transform.position - tocka).magnitude < d0)  // blizu cilja ali ovcam
            {
                hitrost = hitrost * 0.9f + v1 * 0.1f;
            }
            else
            {
                hitrost = hitrost * 0.9f + v2 * 0.1f;
            }
            smer = IzogibOgraji(-center + transform.position, smer);
            Vector3 step = transform.position + Time.deltaTime * hitrost * smer;
            Vector3 p = Vector3.MoveTowards(transform.position, step, hitrost);
            if (p.x > center.x + 149f) { GetComponent<Rigidbody>().MovePosition(center + new Vector3(149f, 0f, p.z)); }  // da se ne zatakne v stajo, ker ne zna ven
            else { GetComponent<Rigidbody>().MovePosition(p); }
            transform.LookAt(p + smer);
        }
        if (transform.position.y < 0f || transform.position.y > 0.05f)  // tudi pes naj ne leti, rije ali se vrti na raznju
        {
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.forward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        }
    }
    Vector3 IzogibOgraji(Vector3 lokacija, Vector3 smer)
    {
        // ce sem ograji blizje kot r se ji izognem, da ne grem proti njej prevec direktno
        float r = 5f;
        if (Mathf.Abs(lokacija.z) > 150f - r && lokacija.z * smer.z > 0f && Mathf.Abs(smer.x) < 0.9f)
        {

            float kot = (Mathf.Abs(lokacija.z) - 150f - r) *
                Mathf.PI / 30f * (lokacija.z > 0 ? -1f : 1f) *
                    (smer.x > 0 ? -1f : 1f);
            smer = new Vector3(Mathf.Cos(kot) * smer.x - Mathf.Sin(kot) * smer.z,
                Mathf.Cos(kot) * smer.z + Mathf.Sin(kot) * smer.x);
        }
        else if (Mathf.Abs(lokacija.x) > 150f - r && lokacija.x * smer.x > 0f && Mathf.Abs(smer.z) < 0.9f)
        {
            float kot = (Mathf.Abs(lokacija.x) - 150f - r) *
                Mathf.PI / 30f * (lokacija.x > 0 ? -1f : 1f) *
                    (smer.z > 0 ? 1f : -1f);
            smer = new Vector3(Mathf.Cos(kot) * smer.x - Mathf.Sin(kot) * smer.z,
                Mathf.Cos(kot) * smer.z + Mathf.Sin(kot) * smer.x);
        }
        if (Mathf.Abs(lokacija.z) > 150f - r && Mathf.Abs(lokacija.x) > 150f - r && lokacija.x * smer.x > 0f && lokacija.z * smer.z > 0f)
        {
            float kotKot = Mathf.PI / 3f * ((lokacija.z * lokacija.x) > 0 ? 1f : -1f) *
                (Mathf.Abs(lokacija.z) < Mathf.Abs(lokacija.x) ? 1f : -1f);
            smer = new Vector3(Mathf.Cos(kotKot) * smer.x - Mathf.Sin(kotKot) * smer.z, 0f,
                Mathf.Cos(kotKot) * smer.z + Mathf.Sin(kotKot) * smer.x);
        }
        return smer.normalized;
    }

    float F(int N)   // najvecja dovoljena velikost crede (polmer) odvisna od stevila ovc
    {
        return ra * Mathf.Sqrt(N);
    }
}
