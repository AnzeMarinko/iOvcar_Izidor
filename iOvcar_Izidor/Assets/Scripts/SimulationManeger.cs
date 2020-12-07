using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SimulationManeger
{
    public static int steviloPonovitev = 50;   // stevilo iteracij za vsako nastavitev in vse mozne nastavitve
    public static int[] nOvc1 = { 25, 50, 75, 100, 125, 150 };
    public static int[] nOvcarjev1 = { 1, 2, 3, 4, 5 };
    public static GinelliOvca.ModelGibanja[] modelGibanja1 = { GinelliOvca.ModelGibanja.Ginelli, GinelliOvca.ModelGibanja.PopravljenStroembom, GinelliOvca.ModelGibanja.Stroembom };
    public static OvcarEnum.ObnasanjePsa[] obnasanjeOvcarja = { OvcarEnum.ObnasanjePsa.AI1, OvcarEnum.ObnasanjePsa.Voronoi };

    public static int maxGeneracij = 24;
    public static int zadnjeGeneracije = 3;  // stevilo generacij, ko se izvaja po tri poskuse in se vzame minimal fitness ali mean fitness
    
    public static DNA DNA;
    public static List<DNA> kombinacije = new List<DNA>();
    public static int osebek = 0;

    public static void SimulationStart()
    {
        if (StaticClass.zacetek)
        {
            VrniKombinacijo();
            if (0 == kombinacije.ToArray().Length) Application.Quit();
            StaticClass.kombinacija = kombinacije[0];
            Evolucija.Setup(StaticClass.kombinacija.modelGibanja, StaticClass.kombinacija.nOvc,
                StaticClass.kombinacija.obnasanjePsa, StaticClass.kombinacija.nOvcarjev);
            StaticClass.bestGen = 0;
            StaticClass.maxFitness = 0;
            DNA = Evolucija.population[0];
            osebek = -1;
            StaticClass.zacetek = false;
        }
    }

    static void ZamenjajKombinacijo()
    {
        VrniKombinacijo();
        if (0 == kombinacije.ToArray().Length) Application.Quit();
        StaticClass.kombinacija = kombinacije[0];
        Evolucija.Setup(StaticClass.kombinacija.modelGibanja, StaticClass.kombinacija.nOvc,
            StaticClass.kombinacija.obnasanjePsa, StaticClass.kombinacija.nOvcarjev);
        StaticClass.bestGen = 0;
        StaticClass.maxFitness = 0;
        DNA = Evolucija.population[0];
        osebek = 0;
    }

    // Update is called once per frame
    public static void SimulationUpdate()
    {
        if (StaticClass.kombinacija.obnasanjePsa == OvcarEnum.ObnasanjePsa.Voronoi)
        {
            if (DNA.ponovitev == steviloPonovitev) ZamenjajKombinacijo();
            else
            {
                osebek = 0;
                DNA.fitness = 0;
            }
        } else if (StaticClass.kombinacija.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI1)
        {
            if (Evolucija.generation == maxGeneracij + 1)
            {
                if (DNA.ponovitev == steviloPonovitev) ZamenjajKombinacijo();
                else osebek++;
            } else if (Evolucija.generation > maxGeneracij - zadnjeGeneracije &&
                Evolucija.generation <= maxGeneracij && (osebek < Evolucija.populationSize - 1 || DNA.ponovitev < 3))
            {
                if (DNA.ponovitev == 3) { osebek++; DNA = Evolucija.population[osebek]; }
            }
            else if (osebek < Evolucija.populationSize - 1)
            {
                osebek++; DNA = Evolucija.population[osebek]; 
            } else
            { Evolucija.Reproduce(); DNA = Evolucija.population[0]; osebek = 0; }
        }
    }

    public static void VrniKombinacijo()  // skrbi za jemanje novih genov in delanje novih generacij
    {
        kombinacije = new List<DNA>();
        foreach (OvcarEnum.ObnasanjePsa vod in obnasanjeOvcarja)
            foreach (int n1 in nOvc1)
                foreach (GinelliOvca.ModelGibanja gin in modelGibanja1)
                    foreach (int n2 in nOvcarjev1)
                        if (!File.Exists("Rezultati" + "-" + vod.ToString() + "/"
                                    + gin.ToString() + "_" + n1 + "-" + vod.ToString() + "_" + n2 + ".txt"))
                                kombinacije.Add(new DNA(1, gin, n1, vod, n2));
    }
}
