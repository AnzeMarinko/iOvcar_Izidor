﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* TODO:
 * 
 * tipka za menjavo scene, tipke za nastavljanje genov in kombinacij (tudi tipka za pripravo testov in genetskega algoritma brez ustavljanja)
 * drsniki za ročno nastavljanje poljubnih parametrov (gumb, ki ustavi igro in ti ponudi izbiro in brez
 *   beleženja rezultatov požene), zraven gumbi za izbiro kombinacije (obnašanje psa možno ročno, AI1-Opt,
 *   Voronoi, AI2-Opt (brez gena), naključen gen, osnoven - gre lahko poljubno pred čredo, zbira vse ...) in
 *   gumb za uporabo nastavitev, gen lahko vmes spreminjaš
 *   Torej na izbiro učenje, testiranje (v tem primeru si zapisuj rezultate tudi za AI2) in zgolj prikaz (uporabljaj StaticClass)
 *   ko želi novo kombinacijo je ne prebere ampak ustavi čas in te vpraša za nastavitev nove kombinacije ali ponovitev prejšnje
 * guiscript s tipkami, meniji in menjavo scene
 * 
 * za AI2 nakljucno nastavi stevilo ovcarjev in ovc v simulaciji
 * optimalne gene mora tudi znati prebrati in iz njih interpolira za nove kombinacije
 * testiraj predvidene gene (in tudi za najbližjo kombinacijo) in preizkusi ali je dosti slabše kot, če ga naučiš za točno to kombinacijo
 *   s tem vidiš ali je model overfitan s parametri samo za določeno kombinacijo (ne sme biti pretirano počasnejši za več ovc)
 * 
 * 
 * uredi kodo, datoteke, lastnosti projekta
 * 
 *      Dodatne ideje kot predlogi za nadaljnje delo:
 * Ovire ali drugačna oblika polja ali npr. voda kjer ovce nočejo hoditi
 * Fizikalna/teoretična študija kjer ovce smatramo kot delce (particle system) ter skušamo primerjati vedenje z elementarnimi 
 *        fizikalnimi pojavi v naravi (npr. https://journals.aps.org/prl/abstract/10.1103/PhysRevLett.110.228701). N -> \infty
*/

public class GuiScript : MonoBehaviour
{
    string scena;
    float cas = 0;
    SimulationManeger sm;
    Terrain terrain;
    float pavzaOd = 0;
    float trajanjePavz = 0;
    public Canvas canvas;
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


    public void Start()
    {
        scena = SceneManager.GetActiveScene().name;
        Time.timeScale = 50f;
        Time.maximumDeltaTime = 0.02f;
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
        sm = terrain.sm;
        for (int i = 0; i < 21; i++) sliders[i].value = sm.DNA.gen[i];
        for (int i = 0; i < 21; i++) SetGen(i);
        nSOvc.value = sm.DNA.nOvc;
        nSOvcarjev.value = sm.DNA.nOvcarjev;
        nOvc.text = "" + sm.DNA.nOvc;
        nOvcarjev.text = "" + sm.DNA.nOvcarjev;
    }

    void Update()
    {
        cas = Time.realtimeSinceStartup - trajanjePavz;
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
        if (Input.GetKey(KeyCode.P)) Time.timeScale = Time.timeScale > 0 ? 0f : 10f;
    }

    public void ClickUporabi()
    {
        terrain.ResetTerrain();
        GinelliOvca.ModelGibanja[] modelGibanja1 = { GinelliOvca.ModelGibanja.Ginelli, GinelliOvca.ModelGibanja.Stroembom, GinelliOvca.ModelGibanja.PopravljenStroembom };
        sm.DNA = new DNA(0, modelGibanja1[modelOvc.value], (int)nSOvc.value, OvcarEnum.ObnasanjePsa.Voronoi, (int)nSOvcarjev.value, 100);
        Time.timeScale = 10f;
        canvas.enabled = !canvas.enabled;
    }

    public void SetNumbers()
    {
        sm.DNA.nOvc = (int) nSOvc.value;
        sm.DNA.nOvcarjev = (int) nSOvcarjev.value;
        nOvc.text = "" + sm.DNA.nOvc;
        nOvcarjev.text = "" + sm.DNA.nOvcarjev;
    }

    public void SetGen(int i)
    {
        float gen = sliders[i].value;
        sm.DNA.gen[i] = gen;
        StaticClass.ComputeParameters(sm.DNA.gen);
        float val = StaticClass.spodnjeMeje[i] + (StaticClass.zgornjeMeje[i] - StaticClass.spodnjeMeje[i]) * gen;
        texts[i].text = (Mathf.FloorToInt(val * 100 * StaticClass.faktor[i]) / 100f).ToString() + " " + StaticClass.enote[i];
    }

    public void SetRandomGen()
    {
        for (int i = 0; i < 21; i++) sliders[i].value = Random.value;
        for (int i = 0; i < 21; i++) SetGen(i);
    }

    private void OnGUI()   // v zgornjem levem kotu napisi nekaj lastnosti simulacije in dodaj nekaj gumbov
    {
        if (GUI.Button(new Rect(3, 0, 60, 20), Time.timeScale > 0 ? "Meni" : "Nadaljuj"))
        {
            canvas.enabled = !canvas.enabled;
            Time.timeScale = Time.timeScale > 0 ? 0f : 10f;
            if (Time.timeScale > 0)
            {
                trajanjePavz += Time.realtimeSinceStartup - pavzaOd;
            } else
            {
                pavzaOd = Time.realtimeSinceStartup;
            }
        }
        if (GUI.Button(new Rect(120, 0, 60, 20), "Izhod")) { Application.Quit(); }
        if (Time.timeScale > 0)
        {
            GUI.Box(new Rect(3, 20, 180, 90), "iOvcar IZIDOR v0.3" + scena + "\n" + string.Format("{0}h {1:00}' {2:00}''\n\n", Mathf.FloorToInt(cas / 3600), Mathf.FloorToInt((cas / 60) % 60), Mathf.FloorToInt(cas % 60)) +
            sm.DNA.nOvc + " " + sm.DNA.modelGibanja.ToString() + "\n" + sm.DNA.nOvcarjev + " " + sm.DNA.obnasanjePsa.ToString());
            if (GUI.Button(new Rect(3, 110, 180, 20), GetComponent<Camera>().depth > 0 ? "Vkolpi sprehodno kamero" : "Izklopi sprehodno kamero"))  // naslednja simulacija iz seznama
            { GetComponent<Camera>().depth *= -1; }
            if (GUI.Button(new Rect(3, 130, 105, 20), "Celoten zaslon"))  // naslednja simulacija iz seznama
            { Screen.fullScreen = !Screen.fullScreen; }
        }
    }
}
