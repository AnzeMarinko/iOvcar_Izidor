using System.Collections;
using System.Collections.Generic;
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

    public DNA(int gene, GinelliOvca.ModelGibanja gin, int n1, OvcarEnum.ObnasanjePsa vod, int n2)
    {
        generacija = gene;
        nOvc = n1;
        nOvcarjev = n2;
        modelGibanja = gin;
        obnasanjePsa = vod;
        for (int i = 0; i < 21; i++) gen[i] = Random.Range(0f, 1f);
        fitness = 0;
        casi = new List<float>();
}

    public float GetFitness(float maxCas, float timer)
    {
        foreach (float cas in casi)
        {
            fitness += Mathf.Pow((maxCas - cas) / maxCas * 2, casi.ToArray().Length == nOvc ? 2 : 1);
        }
        fitness *= Mathf.Pow((maxCas - timer) / maxCas * 2, 2);
        return fitness;
    }
                

    public string GenStr()
    {
        string gN = "" + generacija + " " + fitness;
        if (obnasanjePsa == OvcarEnum.ObnasanjePsa.Voronoi) gen = StaticClass.rocniGen;
        foreach (float g in gen) gN += " " + Mathf.RoundToInt(g * 10000);
        gN += "\n";
        foreach (float cas in casi) gN += "," + Mathf.FloorToInt(cas);
        return gN;
    }

    public DNA Crossover(DNA partner)
    {
        DNA child = new DNA(generacija + 1, modelGibanja, nOvc, obnasanjePsa, nOvcarjev);

        for (int i = 0; i < gen.Length; i++)
        {
            child.gen[i] = Random.Range(0f, 1f) > 0.5f ? gen[i] : partner.gen[i];
        }
        return child;
    }

    public void mutate()
    {
        float mutationRate = 0.01f;
        for (int i = 0; i < gen.Length; i++)
        {
            if (Random.Range(0f, 1f) < mutationRate)
            {
                gen[i] = Random.Range(0f, 1f);
            }
        }
    }
}
