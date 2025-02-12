﻿using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


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
    public readonly float speedup = 20f;


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
        GetComponent<Camera>().depth = 1;
        terrain.odZgorajPogled = true;
        terrain.snemaniOvcar = 0;
        StaticClass.zgodovina = false;
    }

    void Update()
    {
        cas = Time.realtimeSinceStartup - trajanjePavz;
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
        if (Input.GetKey(KeyCode.P)) Time.timeScale = Time.timeScale > 0 ? 0f : speedup;
    }

    public void ClickUporabi()
    {
        StaticClass.modelName = (imeModela.text.Length > 0 ? "-" : "") + imeModela.text + "manual-";
        GinelliOvca.ModelGibanja[] modelGibanja1 = { GinelliOvca.ModelGibanja.Ginelli, GinelliOvca.ModelGibanja.Stroembom, GinelliOvca.ModelGibanja.PopravljenStroembom };
        float[] gen = terrain.sm.DNA.gen;
        terrain.sm.DNA = new DNA(0, modelGibanja1[modelOvc.value], (int)nSOvc.value,
            MLAgents.isOn ? OvcarEnum.ObnasanjePsa.AI2 : OvcarEnum.ObnasanjePsa.Voronoi, (int)nSOvcarjev.value, 50);
        terrain.sm.DNA.gen = gen;
        Time.timeScale = 20f;
        canvas.enabled = !canvas.enabled;
        pomoc.enabled = false;
        terrain.ResetTerrain();
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
        string[] geni = optGen.opt.Split('\n');
        float najblizje = 1000f;
        foreach (string line in geni)
        {
            string[] kombinacija = line.Split(' ');
            float razdalja = (kombinacija[0].Contains(gin.ToString()) ? 1f : 100f) *
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

    public void Hist()
    {
        StaticClass.zgodovina = zgodovina.isOn;
    }

    public void MLA()
    {
        foreach (Slider s in sliders) s.enabled = !s.enabled;
        nSOvcarjev.value = 1;
        modelOvc.value = 0;
        nSOvcarjev.enabled = !nSOvcarjev.enabled;
        modelOvc.enabled = !modelOvc.enabled;
    }

    public void Help(int text)  // besedilo v oknu z dodatnim ali pomocjo
    {
        pomoc.enabled = true;
        string[] texts = new string[2] {
            // dodatno:
            "<b>Dodatno</b>\n\n" +
            "Program je nastal v študijskem letu 2020/2021 za potrebe magistrskega dela.\n" +
            "Študijski program:     <i><b>Interdisciplinarni študij Računalništvo in matematika</b></i>\n" +
            "                           <i>Fakulteta za matematiko in fiziko, Univerza v Ljubljani</i>\n" +
            "                           <i>Fakulteta za računalništvo in informatiko, Univerza v Ljubljani</i>\n" +
            "Naslov magistrskega dela:   <i><b>Vodenje psa ovčarja s pomočjo umetne inteligence.</b></i>" +
            "\nAvtor:              <i><b>Anže Marinko</b></i>" +
            "\nMentor:             <i>izr. prof. Iztok Lebar Bajec</i>" +
            "\nSomentor:          <i>doc. dr. Jure Demšar</i>\n\n" +
            "Program je namenjen izrisu simulacij, nastavljanju parametrov in testiranju modelov psov ovčarjev.",
            // pomoc:
            "<b>Pomoč</b>\n\nV meniju izberite želene nastavitve in pritisnite gumb <b>Prični znova</b>. V kolikor ste" +
            " potrdili shranjevanje rezultatov, se bodo ti shranili v mapo <i>Rezultati</i> zraven programa.\n" +
            "V meniju, ki vam je na voljo med samo simulacijo, lahko tudi spreminjate pogled." };
        if (text < 2) pomocText.text = texts[text];
        else pomoc.enabled = false;
    }

    private void OnGUI()   // v zgornjem levem kotu napisi nekaj lastnosti simulacije in dodaj nekaj gumbov
    {
        if (GUI.Button(new Rect(3, 0, 60, 20), Time.timeScale > 0 ? "Meni" : "Nadaljuj"))
        {
            canvas.enabled = !canvas.enabled;
            Time.timeScale = Time.timeScale > 0 ? 0f : speedup;
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
            // Update the cumulative reward text
            GUI.Box(new Rect(3, 20, 180, 100), "iOvcar IZIDOR\n" + string.Format("{0}h {1:00}' {2:00}''\n\n", Mathf.FloorToInt(cas / 3600), Mathf.FloorToInt((cas / 60) % 60), Mathf.FloorToInt(cas % 60)) +
                (GetComponent<Camera>().depth < 0 ? string.Format("{0:0}:{1:00}", Mathf.FloorToInt(terrain.timer / 60), Mathf.FloorToInt(terrain.timer % 60)) + " (" + (terrain.sm.DNA.ponovitev + 1) +
                ")\nV staji: " + (terrain.nOvc - terrain.sheepList.Count) + " / " : "") + terrain.nOvc + " " + terrain.sm.DNA.modelGibanja.ToString() + "\n" + terrain.sm.DNA.nOvcarjev + " " + terrain.sm.DNA.obnasanjePsa.ToString());
            if (GUI.Button(new Rect(3, 120, 180, 20), GetComponent<Camera>().depth > 0 ? "Vkolpi sprehodno kamero" : "Izklopi sprehodno kamero"))  // naslednja simulacija iz seznama
            { GetComponent<Camera>().depth *= -1; }
            if (GetComponent<Camera>().depth < 0)
            {
                if (GUI.Button(new Rect(3, 160, 180, 20), "Menjaj premični pogled"))  // naslednja simulacija iz seznama
                { terrain.odZgorajPogled = !terrain.odZgorajPogled; }
                if (!terrain.odZgorajPogled)
                {
                    if (GUI.Button(new Rect(3, 180, 180, 20), "Menjaj ovčarja"))  // naslednja simulacija iz seznama
                    { terrain.snemaniOvcar++; }
                }
            }
            if (GUI.Button(new Rect(3, 140, 180, 20), "Celoten zaslon"))  // naslednja simulacija iz seznama
            { Screen.fullScreen = !Screen.fullScreen; }
        }
    }
}
