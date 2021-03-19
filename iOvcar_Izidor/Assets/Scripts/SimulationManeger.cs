using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationManeger
{
    public int steviloPonovitev = 2;   // stevilo iteracij za vsako nastavitev in vse mozne nastavitve
    public int[] nOvc1 = { 25, 75 };
    public int[] nOvcarjev1 = { 1, 2, 3 };
    public GinelliOvca.ModelGibanja[] modelGibanja1 = { GinelliOvca.ModelGibanja.Ginelli, GinelliOvca.ModelGibanja.Stroembom, GinelliOvca.ModelGibanja.PopravljenStroembom };
    public List<OvcarEnum.ObnasanjePsa> obnasanjeOvcarja = new List<OvcarEnum.ObnasanjePsa>();

    public DNA DNA;
    public List<DNA> kombinacije = new List<DNA>();
    public int osebek = 0;
    public Evolucija evolucija;
    bool zacetek = true;
    bool testiranje = true;   // brez ucenja

    public SimulationManeger()
    {
        StaticClass.ComputeGen();
        zacetek = true;
        if (SceneManager.GetActiveScene().name == "testScene")   // nastavi seznam modelov za testiranje
        {
            // obnasanjeOvcarja.Add(OvcarEnum.ObnasanjePsa.Voronoi);
            obnasanjeOvcarja.Add(OvcarEnum.ObnasanjePsa.AI1);
            // obnasanjeOvcarja.Add(OvcarEnum.ObnasanjePsa.AI2);
        }
        else if (SceneManager.GetActiveScene().name == "trainingScene")
        {
            obnasanjeOvcarja.Add(OvcarEnum.ObnasanjePsa.AI2);
        }
    }

    public void SimulationStart()
    {
        if (zacetek)  // nastavi parametre
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
        if (DNA.obnasanjePsa != OvcarEnum.ObnasanjePsa.AI2 || testiranje)  // zamenjaj parametre
        {
            if (!testiranje) ZapisiGen(DNA.modelGibanja, DNA.nOvc, DNA.obnasanjePsa, DNA.nOvcarjev, DNA.gen);
            VrniKombinacijo();
            if (0 == kombinacije.ToArray().Length) Application.Quit();
            evolucija = new Evolucija(kombinacije[0].modelGibanja, kombinacije[0].nOvc, kombinacije[0].obnasanjePsa, kombinacije[0].nOvcarjev);
            DNA = evolucija.population[0];
            osebek = 0;
        }
    }

    public void SimulationUpdate()
    {
        if (testiranje)
        {
            if (DNA.ponovitev == steviloPonovitev) ZamenjajKombinacijo();
            else
            {
                osebek = 0;
                DNA.fitness = 0;
            }
            if (DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.Voronoi) for (int i = 0; i < 21; i++) DNA.gen[i] = StaticClass.rocniGen[i];
            else if (DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.AI1)
            {
                string[] geni = optGen.opt.Split('\n');
                float najblizje = 1000f;
                float[] genf = new float[21];
                foreach (string line in geni)
                {
                    string[] kombinacija = line.Split(' ');
                    float razdalja = (kombinacija[0].Contains(DNA.modelGibanja.ToString()) ? 1f : 100f) *
                            (Mathf.Abs(int.Parse(kombinacija[1]) - DNA.nOvc) + 1f) *
                            (kombinacija[2].Contains(DNA.nOvcarjev.ToString()) ? 1f : 100f) *
                            (Mathf.Abs(int.Parse(kombinacija[3]) - DNA.nOvcarjev) + 1f);
                    if (razdalja < najblizje)
                    {
                        najblizje = razdalja;
                        string[] gen = kombinacija[4].Split(';');
                        for (int i = 0; i < 21; i++) genf[i] = float.Parse(gen[i]);
                    }
                }
                for (int i = 0; i < 21; i++) DNA.gen[i] = genf[i];
            }
        }
        else if (DNA.obnasanjePsa == OvcarEnum.ObnasanjePsa.Voronoi)  // naslednji poskus (glede na trenutno stevilko poskusa, model psa, generacijo ...)
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
                if (DNA.ponovitev == 3) { osebek++; DNA = evolucija.population[osebek]; }
            }
            else if (osebek < evolucija.populationSize - 1)
            {
                osebek++; DNA = evolucija.population[osebek]; 
            } else
            { evolucija.Reproduce(); DNA = evolucija.population[0]; osebek = 0; }
        }
        else
        {
            // ucenje AI2  (dodaj se moznost testiranja 100x)
            DNA.nOvc = (int)Mathf.Pow(Random.Range(1f, 10f), 2f);
            DNA.nOvcarjev = 1;
            DNA.modelGibanja = GinelliOvca.ModelGibanja.Ginelli;
        }
    }

    public void VrniKombinacijo()  // skrbi za jemanje novih genov in delanje novih generacij
    {
        if (!Directory.Exists("Rezultati")) Directory.CreateDirectory("Rezultati");
        kombinacije = new List<DNA>();
        kombinacije.Add(new DNA(1, GinelliOvca.ModelGibanja.Ginelli, (int)Random.Range(40f, 60f), OvcarEnum.ObnasanjePsa.AI1, (int)Random.Range(1, 4), 0));
        if (kombinacije.Count == 0)
            foreach (int n2 in nOvcarjev1)
                foreach (int n1 in nOvc1)
                    foreach (GinelliOvca.ModelGibanja gin in modelGibanja1)
                        foreach (OvcarEnum.ObnasanjePsa vod in obnasanjeOvcarja)
                        {
                            string name = "/" + gin.ToString() + "_" + n1 + "-" + vod.ToString() + "_" + n2 + ".txt";
                            if (!(vod == OvcarEnum.ObnasanjePsa.AI2 && !(n2 == 1 && gin == GinelliOvca.ModelGibanja.Ginelli))  && !File.Exists("Rezultati/Rezultati-" + vod.ToString() + name) && !File.Exists("Rezultati/Rezultati-" + vod.ToString() + "-Final" + name))
                                kombinacije.Add(new DNA(1, gin, n1, vod, n2, 0));
                        }
    }

    void ZapisiGen(GinelliOvca.ModelGibanja gin, int n1, OvcarEnum.ObnasanjePsa vod, int n2, float[] gen)
    {
        if (vod != OvcarEnum.ObnasanjePsa.AI2)  // zapisi gen po zadnji generaciji
        {
            string fileName = "Rezultati/geni.txt";
            if (!File.Exists(fileName))
            {
                // glava z imeni in mejami parametrov
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    float[] zm = StaticClass.zgornjeMeje;
                    float[] sm = StaticClass.spodnjeMeje;
                    string glava = "";
                    for (int i = 0; i < StaticClass.imena.Length; i++) glava += "" + StaticClass.imena[i] + ": (" + (sm[i] * StaticClass.faktor[i]) + " - " + (zm[i] * StaticClass.faktor[i]) + " " + StaticClass.enote[i] + ")\n";
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
