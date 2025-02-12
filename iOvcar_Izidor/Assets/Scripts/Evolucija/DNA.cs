﻿using System.Collections.Generic;
using UnityEngine;

public class DNA
{
    public int nOvc;
    public int nOvcarjev;
    public GinelliOvca.ModelGibanja modelGibanja;
    public OvcarEnum.ObnasanjePsa obnasanjePsa;
    public float[] gen = new float[21];
    public float fitness;
    public List<float> casi;
    public int generacija;
    public int ponovitev = 0;
    float minFit = 10000f;
    public List<float> fits = new List<float>();
    public int maxGeneracij;

    public DNA(int gene, GinelliOvca.ModelGibanja gin, int n1, OvcarEnum.ObnasanjePsa vod, int n2, int mG)
    {
        generacija = gene;
        nOvc = n1;
        nOvcarjev = n2;
        modelGibanja = gin;
        obnasanjePsa = vod;
        for (int i = 0; i < 21; i++) gen[i] = Random.Range(0f, 10001f) / 10000f;
        if (vod == OvcarEnum.ObnasanjePsa.Voronoi) for (int i=0; i<21; i++) gen[i] = StaticClass.rocniGen[i];
        fitness = 0;
        casi = new List<float>();
        maxGeneracij = mG;
    }

    public float GetFitness(float maxCas, float timer, float GCM, int ovce)
    {
        fitness = 0;
        if (timer < maxCas)
        foreach (float cas in casi)
        {
            fitness += (maxCas - cas) / maxCas;
        }
        fitness *= Mathf.Pow((maxCas - timer) / maxCas, 2f) / nOvc * 210f + 1e-4f;
        fitness += 1f / (GCM * ovce + 1f + ovce);
        fits.Add(fitness);
        float mean = 0f;
        foreach (float fit in fits) mean += fit;
        mean /= (ponovitev + 1);
        fitness = (ponovitev > 0 && obnasanjePsa != OvcarEnum.ObnasanjePsa.Voronoi && generacija != maxGeneracij + 1)
            ? Mathf.Min(minFit, fitness) : fitness;
        minFit = fitness;
        if (ponovitev > 0 && obnasanjePsa != OvcarEnum.ObnasanjePsa.Voronoi && generacija != maxGeneracij + 1) fitness *= mean;
        ponovitev++;
        return fitness;
    }
                

    public string GenStr()
    {
        string gN;
        if (generacija < maxGeneracij + 1 && obnasanjePsa == OvcarEnum.ObnasanjePsa.AI1)
        {
            gN = "Generacija " + generacija + ", Fitness " + fits[ponovitev - 1] + ", Gen ";
        } else
        {
            gN = "Fitness " + fits[ponovitev-1] + ", Gen ";
        }
        foreach (float g in gen) gN += ";" + (Mathf.RoundToInt(g * 10000f) / 10000f);
        gN += "\n";
        foreach (float cas in casi) gN += "," + Mathf.FloorToInt(cas);
        return gN;
    }

    public DNA Crossover(DNA partner)  // krizanje dveh genov
    {
        DNA child = new DNA(generacija + 1, modelGibanja, nOvc, obnasanjePsa, nOvcarjev, maxGeneracij);
        for (int i = 0; i < gen.Length; i++)
        {
            child.gen[i] = Random.Range(0f, 10001f) / 10000f > 0.5f ? gen[i] : partner.gen[i];
        }
        return child;
    }

    public void Mutate()  // mutacija gena
    {
        float mutationRate = 0.01f;
        for (int i = 0; i < gen.Length; i++)
        {
            if (Random.Range(0f, 10001f) / 10000f < mutationRate)
            {
                gen[i] = Random.Range(Mathf.Max((gen[i] - 1 / generacija) * 10000f, 0f),
                    Mathf.Min((gen[i] + 1 / generacija) * 10000f, 10001f)) / 10000f;
            }
        }
    }
}
