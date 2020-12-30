﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using TMPro;
using System;
using Random = UnityEngine.Random;
using System.IO;

public class Terrain : MonoBehaviour
{
    private TextMeshPro cumulativeRewardText;
    public List<GameObject> sheepList;
    public List<GameObject> sheepardList;
    int nOvc;
    int nOvcarjev;
    GinelliOvca.ModelGibanja modelGibanja;
    OvcarEnum.ObnasanjePsa obnasanjeOvcarja;
    public float timer = 0;

    public GameObject ovcaGO;
    public GameObject ovcarGO;
    public GameObject ovcarGOML;
    int score = 0;  // rezultat za izpis
    float maxCas = 180f;  // casovna omejitev simulacije
    public GameObject kameraGO;
    private GameObject kamera;
    public GameObject napisGO;
    public SimulationManeger sm;
    public Vector3 center;

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
        if (obnasanjeOvcarja == OvcarEnum.ObnasanjePsa.AI2)
        {
            maxCas = 480f;
        }
        if (sheepardList.Count == 0 || obnasanjeOvcarja != OvcarEnum.ObnasanjePsa.AI2)
        { foreach (GameObject o in sheepardList) Destroy(o);
            sheepardList = new List<GameObject>();
            for (int i = 0; i < nOvcarjev; i++) { AddDog(); } }
        else
        {
            foreach (GameObject o in sheepardList) o.transform.position = transform.position +
                    new Vector3(Random.Range(-45f, 45f) + 70f, 0f, Random.Range(-45f, 45f) + 70f);
        }
        for (int i = 0; i < nOvc; i++) { AddSheep(); }  // postavi ovce in pse na polje
    }

    void AddSheep()
    {
        float phi = Random.Range(0f, 360f);  // dodaj ovco nekam na sredino
        Vector2 loc = Random.insideUnitCircle * 20f;
        GameObject o = Instantiate(ovcaGO, transform.position + new Vector3(loc.x + 70f, 0f, loc.y + 70f), Quaternion.Euler(0f, phi, 0f), this.transform);
        o.GetComponent<GinelliOvca>().model = modelGibanja;
        o.transform.SetParent(this.transform);
        sheepList.Add(o);
    }

    void AddDog()
    {
        float phi = Random.Range(0f, 360f);  // kamorkoli dodaj psa
        GameObject o = Instantiate(obnasanjeOvcarja != OvcarEnum.ObnasanjePsa.AI2 ? ovcarGO : ovcarGOML, transform.position + new Vector3(Random.Range(-45f, 45f) + 70f, 0f, Random.Range(-45f, 45f) + 70f), Quaternion.Euler(0f, phi, 0f), this.transform);
        o.GetComponent<PremakniOvcarja>().obnasanjeOvcarja = obnasanjeOvcarja;
        sheepardList.Add(o);
    }

    public void RemoveSpecificSheep(GameObject sheepObject)
    {
        if (timer < maxCas) sm.DNA.casi.Add(timer);
        sheepList.Remove(sheepObject);
        Destroy(sheepObject);
        if (sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI2 && timer < maxCas && score > 2)
            foreach (GameObject oa in sheepardList)
                oa.GetComponent<OvcarAgent>().AddReward((maxCas - timer) / nOvc * 10f);
    }

    private void RemoveAllSheep()
    {
        if (sheepList != null)
        {
            for (int i = 0; i < sheepList.Count; i++)
            {
                if (sheepList[i] != null)
                {
                    Destroy(sheepList[i]);
                }
            }
        }
        sheepList = new List<GameObject>();
    }
    
    private void Start()
    {
        center = transform.position + new Vector3(70f, 0f, 70f);
        cumulativeRewardText = Instantiate(napisGO, transform.position + new Vector3(40f, 11f, 27f), Quaternion.Euler(25f, 0f, 0f), transform).GetComponent<TextMeshPro>();
        kamera = Instantiate(kameraGO, transform.position + new Vector3(0f, 100f, 0f), Quaternion.Euler(90f, 0f, 0f), transform);
        sm = new SimulationManeger();
        sm.SimulationStart();
        ResetTerrain();
    }

    private void Update()
    {
        cumulativeRewardText.fontSize = sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI2 ? 50f : 25f;
        cumulativeRewardText.fontStyle = sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI2 ? FontStyles.Bold : FontStyles.Normal;
        score = nOvc - sheepList.Count;
        // Update the cumulative reward text
        cumulativeRewardText.text = string.Format("{0:0}:{1:00}", Mathf.FloorToInt(timer / 60), Mathf.FloorToInt(timer % 60)) +
            (sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI2 ? " (" + (sheepardList[0].GetComponent<OvcarAgent>().CompletedEpisodes + 1) +
            ")\n:: " + sheepardList[0].GetComponent<OvcarAgent>().GetCumulativeReward().ToString("0.00") + " ::\nV staji: " + score + " / " + nOvc : "\n\n" +
            "Ovce: " + score + " / " + nOvc +
            "\nGeneracija: " + (sm.evolucija.generation == sm.evolucija.maxGeneracij + 1 ? "Final" : sm.evolucija.generation.ToString()) + ",\nposkus: " + (sm.osebek + 1) + " (" + (sm.DNA.ponovitev + 1) +
            ")\nMax. fitness v " + (sm.evolucija.generation - 1) + ". generaciji:\n" + sm.evolucija.maxFitness + "\n   Nad 1: " + sm.evolucija.steviloUspesnih);
        if (sheepList.Count == 0 || timer > maxCas)   // na koncu (vse ovce v staji ali konec casa) zapisi rezultate v datoteko v mapi Rezultati
        {
            ZapisiRezultate();
            if (sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI2)
                foreach (GameObject oa in sheepardList)
                {
                    if (sheepList.Count == 0)
                        oa.GetComponent<OvcarAgent>().AddReward(0f);   //  (maxCas - timer) * 100);
                    oa.GetComponent<OvcarAgent>().EndEpisode();
                }
            ResetTerrain();
        }
        else
        {
            timer += Time.deltaTime;
            Vector3 GCM = new Vector3(0f, 0f, 0f);
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
            razdalja = Mathf.Min(Mathf.Max(razdalja, 25f), 75f);
            kamera.transform.position = new Vector3(GCM.x, razdalja / Mathf.Tan(Mathf.PI / 12), GCM.z) * 0.05f + kamera.transform.position * 0.95f;
        }
    }

    public void ZapisiRezultate()
    {
        if (sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI2)
        {
            // Rezultate piši le ko se na koncu testira
        }
        else
        {
            string dirName = "Rezultati/Rezultati" + "-" + obnasanjeOvcarja.ToString();
            if (sm.evolucija.generation == sm.evolucija.maxGeneracij + 1 || sm.DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.Voronoi)
            {
                dirName += "-Final";
            }
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = dirName + "/" + modelGibanja.ToString() + "_" + nOvc + "-" + obnasanjeOvcarja.ToString() + "_" + nOvcarjev
                + ".txt";

            if (!File.Exists(fileName))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine("\t" + nOvc + modelGibanja.ToString() + " ovc\n\t" + nOvcarjev +
                    " " + obnasanjeOvcarja.ToString() + " ovcarjev");
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
                sm.DNA.GetFitness(maxCas, timer, (GCM - new Vector3(65f, 0f, 0f)).magnitude, nOvc);
                sw.WriteLine(sm.DNA.GenStr());
                sm.DNA.casi = new List<float>();
            }
        }
    }
}
