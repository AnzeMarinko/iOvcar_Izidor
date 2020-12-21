using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationManeger
{
    public int steviloPonovitev = 40;   // stevilo iteracij za vsako nastavitev in vse mozne nastavitve
    public int[] nOvc1 = { 25, 5, 10, 25, 50, 75, 100 };
    public int[] nOvcarjev1 = { 1, 2, 3, 4, 5 };
    public GinelliOvca.ModelGibanja[] modelGibanja1 = { GinelliOvca.ModelGibanja.Ginelli, GinelliOvca.ModelGibanja.PopravljenStroembom, GinelliOvca.ModelGibanja.Stroembom };
    public List<OvcarEnum.ObnasanjePsa> obnasanjeOvcarja = new List<OvcarEnum.ObnasanjePsa>();  //, OvcarEnum.ObnasanjePsa.AI2 };

    public DNA DNA;
    public List<DNA> kombinacije = new List<DNA>();
    public int osebek = 0;
    public Evolucija evolucija;
    bool zacetek;

    public SimulationManeger()
    {
        zacetek = true;
        if (SceneManager.GetActiveScene().name == "testScene")
        {
            obnasanjeOvcarja.Add(OvcarEnum.ObnasanjePsa.AI2); 
            obnasanjeOvcarja.Add(OvcarEnum.ObnasanjePsa.Voronoi);
            obnasanjeOvcarja.Add(OvcarEnum.ObnasanjePsa.AI1);
        }
        else if (SceneManager.GetActiveScene().name == "trainingScene")
        {
            obnasanjeOvcarja.Add(OvcarEnum.ObnasanjePsa.AI2);
        }
        else
        {
            // vprasaj za novo kombinacijo
        }
    }

    public void SimulationStart()
    {
        if (zacetek)
        {
            VrniKombinacijo();
            if (0 == kombinacije.ToArray().Length) Application.Quit();
            evolucija = new Evolucija(kombinacije[0].modelGibanja, kombinacije[0].nOvc, kombinacije[0].obnasanjePsa, kombinacije[0].nOvcarjev);
            DNA = evolucija.population[0];
            osebek = -1;
            zacetek = false;
        }
    }

    void ZamenjajKombinacijo()
    {
        if (DNA.obnasanjePsa != OvcarEnum.ObnasanjePsa.AI2)
        {
            ZapisiGen(DNA.modelGibanja, DNA.nOvc, DNA.obnasanjePsa, DNA.nOvcarjev, DNA.gen);
            VrniKombinacijo();
            if (0 == kombinacije.ToArray().Length) Application.Quit();
            evolucija = new Evolucija(kombinacije[0].modelGibanja, kombinacije[0].nOvc, kombinacije[0].obnasanjePsa, kombinacije[0].nOvcarjev);
            DNA = evolucija.population[0];
            osebek = 0;
        }
    }

    public void SimulationUpdate()
    {
        if (DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.Voronoi)
        {
            if (DNA.ponovitev == steviloPonovitev) ZamenjajKombinacijo();
            else
            {
                osebek = 0;
                DNA.fitness = 0;
            }
        } else if (DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI1)
        {
            if (DNA.generacija == evolucija.maxGeneracij + 1)
            {
                if (DNA.ponovitev == steviloPonovitev) ZamenjajKombinacijo();
                else osebek++;
            } else if (DNA.generacija > evolucija.maxGeneracij - evolucija.zadnjeGeneracije &&
                DNA.generacija <= evolucija.maxGeneracij && (osebek < evolucija.populationSize - 1 || DNA.ponovitev < 3))
            {
                if (DNA.ponovitev == 5) { osebek++; DNA = evolucija.population[osebek]; }
            }
            else if (osebek < evolucija.populationSize - 1)
            {
                osebek++; DNA = evolucija.population[osebek]; 
            } else
            { evolucija.Reproduce(); DNA = evolucija.population[0]; osebek = 0; }
        }
    }

    public void VrniKombinacijo()  // skrbi za jemanje novih genov in delanje novih generacij
    {
        if (!Directory.Exists("Rezultati")) Directory.CreateDirectory("Rezultati");
        kombinacije = new List<DNA>();
        foreach (int n2 in nOvcarjev1)
            foreach (int n1 in nOvc1)
                foreach (GinelliOvca.ModelGibanja gin in modelGibanja1)
                    foreach (OvcarEnum.ObnasanjePsa vod in obnasanjeOvcarja)
                    {
                        string name = "/" + gin.ToString() + "_" + n1 + "-" + vod.ToString() + "_" + n2 + ".txt";
                        if (!File.Exists("Rezultati/Rezultati" + "-" + vod.ToString() + name) &&
                            !File.Exists("Rezultati/Rezultati" + "-" + vod.ToString() + "-Final" + name))
                            kombinacije.Add(new DNA(1, gin, n1, vod, n2, 0));
                    }
    }

    void ZapisiGen(GinelliOvca.ModelGibanja gin, int n1, OvcarEnum.ObnasanjePsa vod, int n2, float[] gen)
    {
        if (vod != OvcarEnum.ObnasanjePsa.AI2)
        {
            string fileName = "Rezultati/geni.txt";
            if (!File.Exists(fileName))
            {
                // glava z imeni in mejami parametrov
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    string[] imena = {"Hitrost v stanju vodenja",
                        "Faktor za dovoljeno velikost črede",
                        "Razdalja za zbiranje",
                        "Razdalja za zaznavo ovc na poti",
                        "Razdalja za upočasnitve v bližini ovc",
                        "Razdalja za upočasnitev v bližini cilja",
                        "Relativna moč šuma",
                        "Trajanje nakljucnega premika",
                        "Fiksen čas do naključnega premika",
                        "Razpon naključnega dodatnega časa",
                        "Pomen oddaljenosti pobegle ovce od črede",
                        "Pomen oddaljenosti pobegle ovce od ovčarja",
                        "Ovčar bližje cilju kot čreda",
                        "Največji delež ignoriranih ovc",
                        "Vpliv števila ovčarjev na delež ignoriranih ovc",
                        "Odpor pred pred stanjem bližje cilju kot točka",
                        "Udobna razdalja med ovčarji",
                        "Razdalja za upočasnitev v bližini točke",
                        "Odpor pred pred stanjem bližje čredi kot točka",
                        "Zaokroževanje blizu ovc",
                        "Pomen smeri drugih ovčarjev" };
                    float[] zm = StaticClass.zgornjeMeje;
                    float[] sm = StaticClass.spodnjeMeje;
                    string glava = "";
                    for (int i = 0; i < imena.Length; i++) glava += "" + imena[i] + ": (" + sm[i] + "-" + zm[i] + ")\n";
                    sw.WriteLine(glava);
                }
            }
            using (StreamWriter sw = File.AppendText(fileName))
            {
                string vrstica = "" + gin.ToString() + " " + n1 + " " + vod.ToString() + " " + n2 + " ";
                foreach (float g in gen) vrstica += (Mathf.RoundToInt(g * 10000f) / 10000f) + ";";
                sw.WriteLine(vrstica);
            }
        }
    }
}
