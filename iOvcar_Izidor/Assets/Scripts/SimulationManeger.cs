using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SimulationManeger
{
    public static int steviloPonovitev1 = 5;   // stevilo iteracij za vsako nastavitev in vse mozne nastavitve
    public static int[] nOvc1 = { 25, 50, 75, 100, 125, 150 };
    public static int[] nOvcarjev1 = { 1, 2, 3, 4, 5 };
    public static GinelliOvca.ModelGibanja[] modelGibanja1 = { GinelliOvca.ModelGibanja.Ginelli, GinelliOvca.ModelGibanja.PopravljenStroembom, GinelliOvca.ModelGibanja.Stroembom };
    public static OvcarEnum.ObnasanjePsa obnasanjeOvcarja = OvcarEnum.ObnasanjePsa.AI1;

    static int maxGeneracij = 20;

    public static DNA DNA;
    public static DNA[] kombinacije;
    static int kombinacija;
    public static int osebek = 0;

    // Start is called before the first frame update
    public static void SimulationStart()
    {
        // na koncu simulacije nastavi v GuiScript da se spet zacne in preberi novo kombinacijo
        if (StaticClass.zacetek)
        {
            if (obnasanjeOvcarja == OvcarEnum.ObnasanjePsa.Voronoi) Evolucija.populationSize = 10;
            VrniKombinacijo();
            StaticClass.zacetek = false;
            kombinacija = 0;
            StaticClass.kombinacija = kombinacije[kombinacija];
            Evolucija.Setup(StaticClass.kombinacija.modelGibanja, StaticClass.kombinacija.nOvc,
                StaticClass.kombinacija.obnasanjePsa, StaticClass.kombinacija.nOvcarjev);
        }
    }

    // Update is called once per frame
    public static void SimulationUpdate()
    {
        if (osebek < Evolucija.populationSize)
        {  }
        else if (StaticClass.kombinacija.obnasanjePsa == OvcarEnum.ObnasanjePsa.Voronoi ||
            Evolucija.generation == maxGeneracij )
        {
            kombinacija++;
            if (kombinacija == kombinacije.Length) Application.Quit();
            StaticClass.kombinacija = kombinacije[kombinacija];
            Evolucija.Setup(StaticClass.kombinacija.modelGibanja, StaticClass.kombinacija.nOvc,
                StaticClass.kombinacija.obnasanjePsa, StaticClass.kombinacija.nOvcarjev);
            osebek = 0;
        }
        else { Evolucija.Reproduce(); osebek = 0; }

        DNA = Evolucija.population[osebek];
        osebek++;
    }

    public static void VrniKombinacijo()  // skrbi za jemanje novih genov in delanje novih generacij
    {
        kombinacije = new DNA[nOvc1.Length * nOvcarjev1.Length * modelGibanja1.Length];
        int j = 0;
        foreach (int n1 in nOvc1)
            foreach (GinelliOvca.ModelGibanja gin in modelGibanja1)
                foreach (int n2 in nOvcarjev1)
                {
                    kombinacije[j] = new DNA(1, gin, n1, obnasanjeOvcarja, n2);
                    j++;
                }
        kombinacija = -1;
    }
}
