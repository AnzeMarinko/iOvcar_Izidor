using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Evolucija
{
    // ta datoteka naj nadzoruje evolucijo
    public readonly int populationSize = 50;
    public int maxGeneracij = 33;
    public int zadnjeGeneracije = 3;  // stevilo generacij, ko se izvaja po tri poskuse in se vzame minimal fitness ali mean fitness

    public float maxFitness = 0;
    public float currentBest = 0;
    public int steviloUspesnih = 0;
    public int bestGen = 1;
    public DNA[] population;
    public int generation;

    public Evolucija(GinelliOvca.ModelGibanja gin, int n1, OvcarEnum.ObnasanjePsa vod, int n2)
    { 
        population = new DNA[populationSize];
        for (int i = 0; i < populationSize; i++) population[i] = new DNA(1, gin, n1, vod, n2, maxGeneracij);
        generation = 1;
    }

    public void Reproduce()  // razmnozevanje odvisno od uspesnosti
    {
        List<DNA> Order = new List<DNA>();
        foreach (DNA d in population)
        {
            Order.Add(d);
        }
        currentBest = 0;
        steviloUspesnih = 0;
        foreach (DNA d in population)
        {
            d.fits.Sort();
            currentBest = Mathf.Max(d.fits[0], currentBest);
            steviloUspesnih += d.fitness > 1f ? 1 : 0;
        }
        maxFitness = currentBest;
        List<int> matingPool = new List<int>();
        List<DNA> Sorted = Order.OrderBy(order => order.fitness).ToList();
        if (generation == maxGeneracij)
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
                for (int j = 0; j < 2 * Mathf.Pow(i, generation < maxGeneracij - zadnjeGeneracije ? 2 : 3) / populationSize; j++)
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
