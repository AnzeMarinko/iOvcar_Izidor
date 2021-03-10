﻿using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/* TODO:
 * 
 * grafi za Voronoi in AI1
 * 
 * za AI2 nakljucno nastavi stevilo ovcarjev in ovc v simulaciji (mozno spreminjanje stevila ovcarjev in modela ovcarjev)
 * najprej se nauci AI2 iz demonstracij iz opt gena
 * 
 * uredi kodo, pomoč, datoteke, lastnosti projekta
 * 
 *      Dodatne ideje kot predlogi za nadaljnje delo:
 * Ovire ali drugačna oblika polja ali npr. voda kjer ovce nočejo hoditi
 * Fizikalna/teoretična študija kjer ovce smatramo kot delce (particle system) ter skušamo primerjati vedenje z elementarnimi 
 *        fizikalnimi pojavi v naravi (npr. https://journals.aps.org/prl/abstract/10.1103/PhysRevLett.110.228701). N -> \infty
*/

public class GuiScript : MonoBehaviour
{
    float cas = 0;
    Terrain terrain;
    float pavzaOd = 0;
    float trajanjePavz = 0;
    public Canvas canvas;  // uporabniski vmesnik, sliderji, besedila ...
    public Slider sliderGen1, sliderGen2, sliderGen3, sliderGen4, sliderGen5, sliderGen6,
            sliderGen7, sliderGen8, sliderGen9, sliderGen10, sliderGen11, sliderGen12, sliderGen13, sliderGen14,
            sliderGen15, sliderGen16, sliderGen17, sliderGen18, sliderGen19, sliderGen20, sliderGen21, nSOvc, nSOvcarjev;
    public Text textValGen1, textValGen2, textValGen3, textValGen4, textValGen5, textValGen6,
            textValGen7, textValGen8, textValGen9, textValGen10, textValGen11, textValGen12, textValGen13, textValGen14,
            textValGen15, textValGen16, textValGen17, textValGen18, textValGen19, textValGen20, textValGen21, nOvc, nOvcarjev;
    public Text textGen1, textGen2, textGen3, textGen4, textGen5, textGen6,
            textGen7, textGen8, textGen9, textGen10, textGen11, textGen12, textGen13, textGen14,
            textGen15, textGen16, textGen17, textGen18, textGen19, textGen20, textGen21;
    Slider[] sliders = new Slider[21];
    Text[] texts = new Text[21];
    Text[] imena = new Text[21];
    public Dropdown modelOvc;
    public InputField imeModela;
    public Toggle zgodovina;
    public Toggle MLAgents;
    public Canvas pomoc;
    public Text pomocText;


    public void Start()
    {
        Time.timeScale = 20f;
        Time.maximumDeltaTime = 1f / Time.timeScale / 2f;
        canvas.enabled = false;
        sliders = new Slider[21] { sliderGen1, sliderGen2, sliderGen3, sliderGen4, sliderGen5, sliderGen6,
            sliderGen7, sliderGen8, sliderGen9, sliderGen10, sliderGen11, sliderGen12, sliderGen13, sliderGen14,
            sliderGen15, sliderGen16, sliderGen17, sliderGen18, sliderGen19, sliderGen20, sliderGen21 };
        texts = new Text[21] { textValGen1, textValGen2, textValGen3, textValGen4, textValGen5, textValGen6,
            textValGen7, textValGen8, textValGen9, textValGen10, textValGen11, textValGen12, textValGen13, textValGen14,
            textValGen15, textValGen16, textValGen17, textValGen18, textValGen19, textValGen20, textValGen21 };
        imena = new Text[21] { textGen1, textGen2, textGen3, textGen4, textGen5, textGen6,
            textGen7, textGen8, textGen9, textGen10, textGen11, textGen12, textGen13, textGen14,
            textGen15, textGen16, textGen17, textGen18, textGen19, textGen20, textGen21 };
        for (int i = 0; i < 21; i++) imena[i].text = StaticClass.imena[i];
        terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Terrain>();
        terrain.sm = new SimulationManeger();
        terrain.sm.SimulationStart();
        StaticClass.zgodovina = zgodovina.isOn;
        StaticClass.ComputeGen();
        pomoc.enabled = false;
    }

    void Update()
    {
        cas = Time.realtimeSinceStartup - trajanjePavz;
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
        if (Input.GetKey(KeyCode.P)) Time.timeScale = Time.timeScale > 0 ? 0f : 20f;
    }

    public void ClickUporabi()
    {
        StaticClass.modelName = (imeModela.text.Length > 0 ? "-" : "") + imeModela.text + "manual-";
        GinelliOvca.ModelGibanja[] modelGibanja1 = { GinelliOvca.ModelGibanja.Ginelli, GinelliOvca.ModelGibanja.Stroembom, GinelliOvca.ModelGibanja.PopravljenStroembom };
        float[] gen = terrain.sm.DNA.gen;
        terrain.sm.DNA = new DNA(0, modelGibanja1[modelOvc.value], (int)nSOvc.value,
            MLAgents.isOn ? OvcarEnum.ObnasanjePsa.AI2 : OvcarEnum.ObnasanjePsa.Voronoi, (int)nSOvcarjev.value, 50);
        terrain.sm.DNA.gen = gen;
        terrain.ResetTerrain();
        Time.timeScale = 20f;
        canvas.enabled = !canvas.enabled;
        pomoc.enabled = false;
    }

    public void SetNumbers()  // nastavi stevilo ovc in ovcarjev
    {
        terrain.sm.DNA.nOvc = (int) nSOvc.value;
        terrain.sm.DNA.nOvcarjev = (int) nSOvcarjev.value;
        nOvc.text = "" + terrain.sm.DNA.nOvc;
        nOvcarjev.text = "" + terrain.sm.DNA.nOvcarjev;
    }

    public void SetGen(int i)
    {
        float gen = sliders[i].value;
        terrain.sm.DNA.gen[i] = gen;
        StaticClass.ComputeParameters(terrain.sm.DNA.gen);
        float val = StaticClass.spodnjeMeje[i] + (StaticClass.zgornjeMeje[i] - StaticClass.spodnjeMeje[i]) * gen;
        texts[i].text = (Mathf.RoundToInt(val * 100 * StaticClass.faktor[i]) / 100f).ToString() + " " + StaticClass.enote[i];
    }

    public void SetRandomGen()
    {
        for (int i = 0; i < 21; i++) sliders[i].value = Random.value;
        for (int i = 0; i < 21; i++) SetGen(i);
    }

    public void SetVoronoiGen()
    {
        StaticClass.ComputeGen();
        for (int i = 0; i < 21; i++) sliders[i].value = StaticClass.rocniGen[i];
        for (int i = 0; i < 21; i++) SetGen(i);
    }

    public void SetOptimalGen()  // preberi iz datoteke najboljsi gen po evoluciji
    {
        GinelliOvca.ModelGibanja[] modelGibanja1 = { GinelliOvca.ModelGibanja.Ginelli, GinelliOvca.ModelGibanja.Stroembom, GinelliOvca.ModelGibanja.PopravljenStroembom };
        GinelliOvca.ModelGibanja gin = modelGibanja1[modelOvc.value];
        int n1 = (int)nSOvc.value;
        OvcarEnum.ObnasanjePsa vod = OvcarEnum.ObnasanjePsa.AI1;
        int n2 = (int)nSOvcarjev.value;
        string fileName = "Rezultati/geni.txt";
        if (File.Exists(fileName))
        {
            string[] geni = File.ReadAllLines(fileName);
            float najblizje = 1000f;
            foreach(string line in geni)
            {
                if (line.Length < 80) continue;
                string[] kombinacija = line.Split(' ');
                float razdalja = ( kombinacija[0].Contains(gin.ToString()) ? 1f : 100f) *
                    (Mathf.Abs(int.Parse(kombinacija[1]) - n1) + 1f) *
                    (kombinacija[2].Contains(vod.ToString()) ? 1f : 100f) *
                    (Mathf.Abs(int.Parse(kombinacija[3]) - n2) + 1f);
                if (razdalja < najblizje)
                {
                    najblizje = razdalja;
                    string[] gen = kombinacija[4].Split(';');
                    for (int i = 0; i < 21; i++) sliders[i].value = float.Parse(gen[i]);
                    for (int i = 0; i < 21; i++) SetGen(i);
                }
            }
        }
    }

    public void Hist()
    {
        StaticClass.zgodovina = zgodovina.isOn;
    }

    public void MLA()
    {
        foreach (Slider s in sliders) s.enabled = !s.enabled;
    }

    public void Help(int text)  // besedilo v oknu z dodatnim ali pomocjo
    {
        pomoc.enabled = true;
        string[] texts = new string[2] {
            // dodatno:
            "<b>Dodatno</b>\n" +
            "\nProgram je nastal v  študijskem letu 2020/2021 za potrebe magistrskega dela." +
            "\nAvtor:    <i>Anže Marinko</i>" +
            "\nVeč informacij o magistrski nalogi.",
            // pomoc:
            "<b>Pomoč</b>\n\n Informacije o programu." };
        if (text < 2) pomocText.text = texts[text];
        else pomoc.enabled = false;
    }

    private void OnGUI()   // v zgornjem levem kotu napisi nekaj lastnosti simulacije in dodaj nekaj gumbov
    {
        if (GUI.Button(new Rect(3, 0, 60, 20), Time.timeScale > 0 ? "Meni" : "Nadaljuj"))
        {
            canvas.enabled = !canvas.enabled;
            Time.timeScale = Time.timeScale > 0 ? 0f : 20f;
            if (Time.timeScale > 0)
            {
                trajanjePavz += Time.realtimeSinceStartup - pavzaOd;
            } else
            {
                pavzaOd = Time.realtimeSinceStartup;
                for (int i = 0; i < 21; i++) sliders[i].value = terrain.sm.DNA.gen[i];
                for (int i = 0; i < 21; i++) SetGen(i);
                nSOvc.value = terrain.sm.DNA.nOvc * 1;
                nSOvcarjev.value = terrain.sm.DNA.nOvcarjev * 1;
                nOvc.text = "" + terrain.sm.DNA.nOvc;
                nOvcarjev.text = "" + terrain.sm.DNA.nOvcarjev;
                modelOvc.value = GinelliOvca.ModelGibanja.Ginelli == terrain.sm.DNA.modelGibanja ? 0 : GinelliOvca.ModelGibanja.Stroembom == terrain.sm.DNA.modelGibanja ? 1 : 2;
            }
        }
        if (GUI.Button(new Rect(120, 0, 60, 20), "Izhod")) { Application.Quit(); }
        if (Time.timeScale > 0)
        {
            print(terrain.sm.DNA.nOvc);
            GUI.Box(new Rect(3, 20, 180, 90), "iOvcar IZIDOR\n" + string.Format("{0}h {1:00}' {2:00}''\n\n", Mathf.FloorToInt(cas / 3600), Mathf.FloorToInt((cas / 60) % 60), Mathf.FloorToInt(cas % 60)) +
            terrain.sm.DNA.nOvc + " " + terrain.sm.DNA.modelGibanja.ToString() + "\n" + terrain.sm.DNA.nOvcarjev + " " + terrain.sm.DNA.obnasanjePsa.ToString());
            if (GUI.Button(new Rect(3, 110, 180, 20), GetComponent<Camera>().depth > 0 ? "Vkolpi sprehodno kamero" : "Izklopi sprehodno kamero"))  // naslednja simulacija iz seznama
            { GetComponent<Camera>().depth *= -1; }
            if (GUI.Button(new Rect(3, 130, 105, 20), "Celoten zaslon"))  // naslednja simulacija iz seznama
            { Screen.fullScreen = !Screen.fullScreen; }
        }
    }
}
