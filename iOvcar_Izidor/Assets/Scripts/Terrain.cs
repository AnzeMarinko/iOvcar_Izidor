using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using System.IO;

public class Terrain : MonoBehaviour
{
    private TextMeshPro cumulativeRewardText;
    public List<GameObject> sheepList;
    public List<GameObject> sheepardList = new List<GameObject>();
    public int nOvc;
    int nOvcarjev;
    public GinelliOvca.ModelGibanja modelGibanja;
    OvcarEnum.ObnasanjePsa obnasanjeOvcarja;
    public float timer = 0;

    public GameObject ovcaGO;
    public GameObject ovcarGOML;
    public GameObject ovcarGO;
    int score = 0;  // rezultat za izpis
    public float maxCas = 180f;  // casovna omejitev simulacije
    public GameObject kameraGO;
    private GameObject kamera;
    public GameObject napisGO;
    public SimulationManeger sm;
    public Vector3 center;
    List<List<float>> xs = new List<List<float>>();
    List<List<float>> zs = new List<List<float>>();
    public bool odZgorajPogled = false;
    public int snemaniOvcar = 0;

    public void ResetTerrain()
    {
        sm.SimulationUpdate();
        nOvc = sm.DNA.nOvc;
        nOvcarjev = sm.DNA.nOvcarjev;
        modelGibanja = sm.DNA.modelGibanja;
        obnasanjeOvcarja = sm.DNA.obnasanjePsa;
        score = 0;
        timer = 0;
        RemoveAllSheep();
        maxCas = 180f;  //  obnasanjeOvcarja == OvcarEnum.ObnasanjePsa.AI2 ? 300f : 180f;  // vec casa za ucenje
        int i = 0;
        foreach (GameObject o in sheepardList)
        {
            if (i < 0)    // nOvcarjev)
                o.transform.position = center + new Vector3(Random.Range(-120f, 120f), 0f, Random.Range(-120f, 120f));
            else { Destroy(o); sheepardList.Remove(o); }
            i++;
        }
        for (int j = i; j < nOvcarjev; j++) { AddDog(); }
        for (int j = 0; j < nOvc; j++) { AddSheep(); }  // postavi ovce in pse na polje
        xs = new List<List<float>>();
        zs = new List<List<float>>();
    }

    void AddSheep()
    {
        float phi = Random.Range(0f, 360f);  // dodaj ovco nekam na sredino
        Vector2 loc = Random.insideUnitCircle * 50f;
        GameObject o = Instantiate(ovcaGO, center + new Vector3(loc.x, 0f, loc.y), Quaternion.Euler(0f, phi, 0f), this.transform);
        o.GetComponent<GinelliOvca>().model = modelGibanja;
        o.transform.SetParent(this.transform);
        sheepList.Add(o);
    }

    void AddDog()
    {
        float phi = Random.Range(0f, 360f);  // kamorkoli dodaj psa
        GameObject o = Instantiate(sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI2 ? ovcarGOML : ovcarGO, center + new Vector3(Random.Range(-120f, 120f), 0f, Random.Range(-120f, 120f)), Quaternion.Euler(0f, phi, 0f), this.transform);
        o.GetComponent<PremakniOvcarja>().obnasanjeOvcarja = obnasanjeOvcarja;
        sheepardList.Add(o);
    }

    public void RemoveSpecificSheep(GameObject sheepObject)
    {
        if (timer < maxCas) sm.DNA.casi.Add(timer);
        sheepList.Remove(sheepObject);
        if (StaticClass.zgodovina)
        {
            xs.Add(sheepObject.GetComponent<ObnasanjeOvce>().xs);
            zs.Add(sheepObject.GetComponent<ObnasanjeOvce>().zs);
        }
        Destroy(sheepObject);
    }

    private void RemoveAllSheep()
    {
        if (sheepList != null)
        {
            for (int i = 0; i < sheepList.Count; i++)
            {
                if (sheepList[i] != null)
                {
                    if (StaticClass.zgodovina)
                    {
                        xs.Add(sheepList[i].GetComponent<ObnasanjeOvce>().xs);
                        zs.Add(sheepList[i].GetComponent<ObnasanjeOvce>().zs);
                    }
                    Destroy(sheepList[i]);
                }
            }
        }
        sheepList = new List<GameObject>();
    }

    private void Start()
    {
        center = transform.position + new Vector3(70f, 0f, 70f);
        cumulativeRewardText = Instantiate(napisGO, transform.position + new Vector3(-5f, 30f, -65f), Quaternion.Euler(25f, 0f, 0f), transform).GetComponent<TextMeshPro>();
        kamera = Instantiate(kameraGO, transform.position + new Vector3(0f, 100f, 0f), Quaternion.Euler(90f, 0f, 0f), transform);
        sm = new SimulationManeger();
        sm.SimulationStart();
        ResetTerrain();
    }

    private void Update()
    {
        cumulativeRewardText.fontSize = 100f;
        cumulativeRewardText.fontStyle = FontStyles.Bold;
        score = nOvc - sheepList.Count;
        // Update the cumulative reward text
        cumulativeRewardText.text = Time.timeScale > 0 ? string.Format("{0:0}:{1:00}", Mathf.FloorToInt(timer / 60), Mathf.FloorToInt(timer % 60)) +
            " (" + (sm.DNA.ponovitev + 1) + ")\nV staji: " + score + " / " + nOvc : "";
        if (sheepList.Count == 0 || timer > maxCas)   // na koncu (vse ovce v staji ali konec casa) zapisi rezultate v datoteko v mapi Rezultati
        {
            if (sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI2)
                foreach (GameObject oa in sheepardList)
                {
                    if (sheepList.Count == 0)
                        oa.GetComponent<OvcarAgent>().AddReward(Mathf.Pow((maxCas - timer) / maxCas, 2f) * 4f);
                    oa.GetComponent<OvcarAgent>().EndEpisode();
                    oa.GetComponent<OvcarAgent>().exGCM = 1000f;
                }
            RemoveAllSheep();
            ZapisiRezultate();
            ResetTerrain();
        }
        else
        {
            timer += Time.deltaTime;
            if (odZgorajPogled)
            {
                Vector3 GCM = new Vector3(0f, 0f, 0f);  // premikaj sprehodno kamero s credo
                foreach (GameObject ovca in sheepList)
                {
                    GCM += ovca.transform.position;
                }
                GCM /= sheepList.Count;
                float razdalja = 0f;
                foreach (GameObject ovca in sheepList)
                {
                    razdalja += (ovca.transform.position - GCM).magnitude;
                }
                razdalja /= sheepList.Count;
                razdalja *= 8;
                razdalja = Mathf.Min(Mathf.Max(razdalja, 75f), 240f);
                kamera.transform.position = new Vector3(GCM.x, razdalja / Mathf.Tan(Mathf.PI / 12), GCM.z) * 0.05f + kamera.transform.position * 0.95f;
                kamera.transform.rotation = Quaternion.Euler(90f * 0.3f + kamera.transform.rotation.eulerAngles.x * 0.7f, 0f * 0.05f + kamera.transform.rotation.eulerAngles.y * 0.95f, 0f);
            } else if (sheepardList.Count > 0 && sheepList.Count > 0 && Time.timeScale > 0)
            {
                if (snemaniOvcar >= sheepardList.Count)
                    snemaniOvcar = 0;
                Vector3 GCM = new Vector3(0f, 0f, 0f);  // premikaj sprehodno kamero s credo
                foreach (GameObject ovca in sheepList)
                {
                    GCM += ovca.transform.position;
                }
                GCM /= sheepList.Count;
                Vector3 lokacija = sheepardList[snemaniOvcar].transform.position;
                Vector3 smer = (GCM - lokacija).normalized;
                lokacija = lokacija - new Vector3(smer.x, -20f / 70f, smer.z) * 70f;
                smer = smer * 0.2f + kamera.transform.forward * 0.8f;
                kamera.transform.position = lokacija * 0.3f + kamera.transform.position * 0.7f;
                kamera.transform.rotation = Quaternion.Euler(10f * 0.3f + kamera.transform.rotation.eulerAngles.x * 0.7f,
                    Mathf.Atan2(smer.x, smer.z) * 180f / Mathf.PI, 0f);
            }
        }
    }

    public void ZapisiRezultate()
    {
        if (StaticClass.zgodovina && timer > 10f)
        {
            string dirName = "Rezultati/Rezultati-" + StaticClass.modelName + obnasanjeOvcarja.ToString();
            if (sm.evolucija.generation == sm.evolucija.maxGeneracij + 1 || sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.Voronoi)
            {
                dirName += "-Final";
            }
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = dirName + "/" + StaticClass.modelName + modelGibanja.ToString() + "_" + nOvc + "-" + obnasanjeOvcarja.ToString() + "_" + nOvcarjev
                + ".txt";
            if (!Directory.Exists("Rezultati/lokacije/")) Directory.CreateDirectory("Rezultati/lokacije/");
            string lokacije = "Rezultati/lokacije/" + StaticClass.modelName + modelGibanja.ToString() + "_" + nOvc + "-" + obnasanjeOvcarja.ToString() + "_" + nOvcarjev + "-" + sm.DNA.ponovitev
                  + ".txt";
            if (!File.Exists(lokacije) && sm.DNA.ponovitev < 2)
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(lokacije))
                {
                    int preskakuj = 4;
                    string lok = "";
                    foreach (GameObject o in sheepardList)
                    {
                        lok = "1;0;";
                        for (int i = 0; i < o.GetComponent<PremakniOvcarja>().xs.Count; i += preskakuj + 1) lok += (Mathf.FloorToInt(o.GetComponent<PremakniOvcarja>().xs[i] * 100f) / 100f) + ";";
                        sw.WriteLine(lok);
                        lok = "1;1;";
                        for (int i = 0; i < o.GetComponent<PremakniOvcarja>().zs.Count; i += preskakuj + 1) lok += (Mathf.FloorToInt(o.GetComponent<PremakniOvcarja>().zs[i] * 100f) / 100f) + ";";
                        sw.WriteLine(lok);
                    }
                    for (int o = 0; o < sm.DNA.nOvc; o++)
                    {
                        lok = "0;0;";
                        for (int i = 0; i < xs[o].Count; i += preskakuj + 1) lok += (Mathf.FloorToInt(xs[o][i] * 100f) / 100f) + ";";
                        sw.WriteLine(lok);
                        lok = "0;1;";
                        for (int i = 0; i < zs[o].Count; i += preskakuj + 1) lok += (Mathf.FloorToInt(zs[o][i] * 100f) / 100f) + ";";
                        sw.WriteLine(lok);
                    }
                }
            }
            if (!File.Exists(fileName))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine("\t" + nOvc + modelGibanja.ToString() + " ovc\n\t" + nOvcarjev +
                    " " + StaticClass.modelName + obnasanjeOvcarja.ToString() + " ovcarjev");
                }
            }

            // This text is always added, making the file longer over time
            using (StreamWriter sw = File.AppendText(fileName))
            //    Appends text at the end of an existing file
            {
                Vector3 GCM = new Vector3(0f, 0f, 0f);
                foreach (GameObject ovca in sheepList)
                {
                    GCM += ovca.transform.position;
                }
                if (sheepList.Count > 0) GCM /= sheepList.Count; else GCM = new Vector3(175f, 0f, 0f);
                GCM -= center;
                sm.DNA.GetFitness(maxCas, timer, (GCM - new Vector3(175f, 0f, 0f)).magnitude, nOvc);
                sw.WriteLine(sm.DNA.GenStr());
                sm.DNA.casi = new List<float>();
            }
        }
    }
}
