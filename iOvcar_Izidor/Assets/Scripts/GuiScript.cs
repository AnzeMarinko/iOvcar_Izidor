using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

/* TODO:
 * 
 * 
 * za AI2 nakljucno nastavi stevilo ovcarjev in ovc v simulaciji
 * guiscript s tipkami, meniji in menjavo scene
 * optimalne gene mora tudi znati prebrati in iz njih ekstrapolirati za nove kombinacije
 * spreminjajoči AI1 - spreminjanje optimalnega gena glede na naučene kombinacije
 *   (ko se zmanjša št. ovc spremeni parametre),
 * na podlagi kombinacije izračunaj parametre (predvidi tudi za nenaučeno - več ovc (150 in 200) ipd.)
 * testiraj predvidene gene (in tudi za najbližjo kombinacijo) in preizkusi ali je dosti slabše kot, če ga naučiš za točno to kombinacijo
 *   s tem vidiš ali je model overfitan s parametri samo za določeno kombinacijo (ne sme biti pretirano počasnejši za več ovc)
 * drsniki za ročno nastavljanje poljubnih parametrov (gumb, ki ustavi igro in ti ponudi izbiro in brez
 *   beleženja rezultatov požene), zraven gumbi za izbiro kombinacije (obnašanje psa možno ročno, AI1-Opt,
 *   Voronoi, AI2-Opt (brez gena), naključen gen, osnoven - gre lahko poljubno pred čredo, zbira vse ...) in
 *   gumb za uporabo nastavitev
 *   Torej na izbiro učenje, testiranje (v tem primeru si zapisuj rezultate tudi za AI2) in zgolj prikaz (uporabljaj StaticClass)
 *   ko želi novo kombinacijo je ne prebere ampak ustavi čas in te vpraša za nastavitev nove kombinacije ali ponovitev prejšnje
 * 
 * Unity ML agents
 * Evalvacija posameznih pristopov (ročno razviti psi, umetna inteligenca).
 * Odstrani izris in pospeši, uredi kodo, datoteke, lastnosti projekta
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
    float pavzaOd = 0;
    float trajanjePavz = 0;

    public void Start()
    {
        scena = SceneManager.GetActiveScene().name;
        Time.timeScale = 10f;
        Time.maximumDeltaTime = 0.1f;
    }

    void Update()
    {
        cas = Time.realtimeSinceStartup - trajanjePavz;
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
        if (Input.GetKey(KeyCode.P)) Time.timeScale = Time.timeScale > 0 ? 0f : 10f;
        sm = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Terrain>().sm;
    }

    private void OnGUI()   // v zgornjem levem kotu napisi nekaj lastnosti simulacije in dodaj nekaj gumbov
    {
        if (GUI.Button(new Rect(3, 0, 60, 20), Time.timeScale > 0 ? "Premor" : "Naprej"))
        {
            // Time.timeScale = Time.timeScale > 0 ? 0f : 10f;
            if (Time.timeScale > 0)
            {
                trajanjePavz += Time.realtimeSinceStartup - pavzaOd;
            } else
            {
                pavzaOd = Time.realtimeSinceStartup;
            }
        }
        if (GUI.Button(new Rect(120, 0, 60, 20), "Izhod")) { Application.Quit(); }
        GUI.Box(new Rect(3, 20, 180, 90), "iOvcar IZIDOR v0.3\n" + string.Format("{0}h {1:00}' {2:00}''\n\n", Mathf.FloorToInt(cas / 3600), Mathf.FloorToInt((cas / 60) % 60), Mathf.FloorToInt(cas % 60)) +
            sm.DNA.nOvc + " " + sm.DNA.modelGibanja.ToString() + "\n" + sm.DNA.nOvcarjev + " " + sm.DNA.obnasanjePsa.ToString());
        if (GUI.Button(new Rect(3, 110, 180, 20), GetComponent<Camera>().depth > 0 ? "Vkolpi sprehodno kamero" : "Izklopi sprehodno kamero"))  // naslednja simulacija iz seznama
        { GetComponent<Camera>().depth *= -1; }
        if (GUI.Button(new Rect(3, 130, 105, 20), "Celoten zaslon"))  // naslednja simulacija iz seznama
        { Screen.fullScreen = !Screen.fullScreen; }

        if (Time.timeScale < 0.1f)
        {
            GUI.Box(new Rect(Screen.width / 3, Screen.height / 2 - 60, Screen.width / 3, 120), "Voronoi/AI1 (testiranje in učenje),\nučenje AI2," +
                "\ntestiranje AI2 ali\nročne nastavitve (drsniki)\n" + scena);  // gumbi kjer se potem zamenja scena in se pojavi nov meni v primeru z drsniki
        }
    }
}
