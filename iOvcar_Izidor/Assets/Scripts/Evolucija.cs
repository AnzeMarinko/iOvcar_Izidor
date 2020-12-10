using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Evolucija
{
    // ta datoteka naj nadzoruje evolucijo
    public readonly static int populationSize = 50;
    public static DNA[] population;
    public static int generation = 0;

    public static void Setup(GinelliOvca.ModelGibanja gin, int n1, OvcarEnum.ObnasanjePsa vod, int n2)
    { 
        population = new DNA[populationSize];
        for (int i = 0; i < populationSize; i++) population[i] = new DNA(1, gin, n1, vod, n2); generation = 1; }

    public static void Reproduce()
    {
        List<DNA> Order = new List<DNA>();
        foreach (DNA d in population)
        {
            Order.Add(d);
        }
        float maxFitness = 0;
        int uspesnih = 0;
        foreach (DNA d in population)
        {
            d.fits.Sort();
            maxFitness = Mathf.Max(d.fits[0], maxFitness);
            uspesnih += d.fitness > 1f ? 1 : 0;
        }
        StaticClass.steviloUspesnih = uspesnih;
        StaticClass.maxFitness = maxFitness;
        if (StaticClass.currentBest < maxFitness)
        {
            StaticClass.currentBest = maxFitness;
            StaticClass.bestGen = generation;
        }
        List<int> matingPool = new List<int>();
        List<DNA> Sorted = Order.OrderBy(order => order.fitness).ToList();
        if (generation == SimulationManeger.maxGeneracij)
        {
            population[0] = Sorted[populationSize-1];
            population[0].generacija += 1;
            population[0].ponovitev = 0;
            population[0].fitness = 0;
            generation++;
        } else
        {
            int i = 0;
            int k = 0;
            foreach (DNA d in Sorted)
            {
                for (int j = 0; j < 2 * Mathf.Pow(i, generation < SimulationManeger.maxGeneracij - SimulationManeger.zadnjeGeneracije ? 2 : 3) / populationSize; j++)
                {
                    matingPool.Add(i);
                    k++;
                }
                i++;
            }
            generation++;
            for (int j = 0; j < populationSize; j++)
            {
                int a = matingPool[Random.Range(0, k)];
                int b = matingPool[Random.Range(0, k)];
                if (a == b) b = matingPool[Random.Range(0, k)];
                DNA parentA = Sorted[a];
                DNA parentB = Sorted[b];

                DNA child = parentA.Crossover(parentB);
                child.Mutate();
                population[j] = child;
            }
        }
    }
}
