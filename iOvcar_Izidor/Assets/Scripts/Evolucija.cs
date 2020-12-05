using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Evolucija
{
    // guiscript naj tu kaj spremeni, kadar v AI1 (npr. sem naj pove case in se tu izracina fitness ipd.)
    // ta datoteka naj nadzoruje evolucijo
    public static int populationSize = 50;
    public static DNA[] population;
    public static int generation = 0;

    public static void Setup(GinelliOvca.ModelGibanja gin, int n1, OvcarEnum.ObnasanjePsa vod, int n2)
    { population = new DNA[populationSize];
        for (int i = 0; i < populationSize; i++) population[i] = new DNA(1, gin, n1, vod, n2); generation = 1; }

    public static void Reproduce()
    {
        List<DNA> Order = new List<DNA>();
        foreach (DNA d in population)
        {
            Order.Add(d);
        }
        List<DNA> matingPool = new List<DNA>();
        List<DNA> Sorted = Order.OrderBy(order => order.fitness).ToList();
        int i = 0;
        foreach (DNA d in Sorted)
        {
            for (int j = 0; j < 2 * i * i / populationSize; j++)
            {
                matingPool.Add(Sorted[i-1]);
            }
            i++;
        }
        generation++;
        for (int j=0; j<populationSize; j++)
        {
            int a = Random.Range(0, i);
            int b = Random.Range(0, i);
            if (a == b) b = Random.Range(0, i);
            DNA parentA = matingPool[a];
            DNA parentB = matingPool[b];

            DNA child = parentA.Crossover(parentB);
            child.mutate();
            population[j] = child;
        }
    }
}
