using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

/* TODO:
 * 
 * genetski algoritmi, Unity ML agents / Markov brains, Reinforcement learning (tabela stanj in potez)
 * Evalvacija posameznih pristopov (ročno razviti psi, umetna inteligenca, morda tudi najosnovnejši model brez upoštevanja GCM ipd.).
 * Odstrani izris in pospeši
 * 
 *      Dodatne ideje, če bomo slučajno imeli preveč časa:
 * Ovire ali drugačna oblika polja ali npr. voda kjer ovce nočejo hoditi
 * Fizikalna/teoretična študija kjer ovce smatramo kot delce (particle system) ter skušamo primerjati vedenje z elementarnimi 
 *        fizikalnimi pojavi v naravi (npr. https://journals.aps.org/prl/abstract/10.1103/PhysRevLett.110.228701). N -> \infty
*/

public class GuiScript : MonoBehaviour
{
    readonly float pospesitev = 7;   // hitrost predvajanja, ce je mogoce

    int nOvc;
    int nOvcarjev;
    GinelliOvca.ModelGibanja modelGibanja;
    OvcarEnum.ObnasanjePsa obnasanjeOvcarja;

    public GameObject ovcaGO;
    public GameObject ovcarGO;
    GameObject[] ovce;
    int score = 0;  // rezultat za izpis
    float timer;
    List<float> casi = new List<float>();   // casi prihodov v stajo
    readonly float maxCas = 180f;  // casovna omejitev simulacije
    bool konec = false;  // cas za izpis navodil na koncu in konec simulacije

    public GameObject kameraGO;
    public GameObject kamera;

    // Start is called before the first frame update
    public void Start()
    {
        kamera = Instantiate(kameraGO, new Vector3(0f, 100f, 0f), Quaternion.Euler(90f, 0f, 0f));
        kamera.GetComponent<Camera>().depth = StaticClass.kamera;
        SimulationManeger.SimulationStart();
        SimulationManeger.SimulationUpdate();
        nOvc = StaticClass.kombinacija.nOvc;
        nOvcarjev = StaticClass.kombinacija.nOvcarjev;
        modelGibanja = StaticClass.kombinacija.modelGibanja;
        obnasanjeOvcarja = StaticClass.kombinacija.obnasanjePsa;
        score = 0;
        timer = 0;
        for (int i = 0; i < nOvc; i++) { AddSheep(); }  // postavi ovce in pse na polje
        for (int i = 0; i < nOvcarjev; i++) { AddDog(); }
        // GetComponent<AudioSource>().Play();   // zaigraj zvok na zacetku simulacije
        casi = new List<float>();
        Time.timeScale = pospesitev;
    }

    void AddSheep()
    {
        float phi = Random.Range(0f, 360f);  // dodaj ovco nekam na sredino
        Vector2 loc = Random.insideUnitCircle * 20;
        GameObject o = Instantiate(ovcaGO, new Vector3(loc.x, -0.5f, loc.y), Quaternion.Euler(0f, phi, 0f));
        o.GetComponent<GinelliOvca>().model = modelGibanja;
    }

    void AddDog()
    {
        float phi = Random.Range(0f, 360f);  // kamorkoli dodaj psa
        GameObject o = Instantiate(ovcarGO, new Vector3(Random.Range(-45f, 45f), -0.5f, Random.Range(-45f, 45f)), Quaternion.Euler(0f, phi, 0f));
        o.GetComponent<PremakniOvcarja>().obnasanjeOvcarja = obnasanjeOvcarja;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))  // ce pritisnjen Escape zaustavi program
        {
            Application.Quit();
        }
        ovce = GameObject.FindGameObjectsWithTag("Ovca");  // izracunaj rezultat
        
        score = nOvc - ovce.Length;
        if (ovce.Length == 0 || timer > maxCas)   // na koncu (vse ovce v staji ali konec casa) zapisi rezultate v datoteko v mapi Rezultati
        {
            string dirName = "Rezultati";
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = dirName + "/" + modelGibanja.ToString() + "_" + nOvc + "-" + obnasanjeOvcarja.ToString() + "_" + nOvcarjev
                + "_" + StaticClass.zapStPrograma + ".txt";

            if (!File.Exists(fileName))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine("\t" + nOvc + modelGibanja.ToString() + " ovc\n\t" + nOvcarjev +
                    " " + obnasanjeOvcarja.ToString() + " ovcarjev");
                }
            }

            // This text is always added, making the file longer over time
            using (StreamWriter sw = File.AppendText(fileName))
            //    Appends text at the end of an existing file
            {
                foreach (float c in casi) SimulationManeger.DNA.casi.Add(c);
                print(SimulationManeger.DNA.GetFitness(maxCas, timer));
                sw.WriteLine(SimulationManeger.DNA.GenStr());
            }
            Time.timeScale = 0;
            SceneManager.LoadScene(0);
        }
        else 
        {
            timer += Time.deltaTime;
            Vector3 GCM = new Vector3(0f, 0f, 0f);
            foreach (GameObject ovca in ovce)
            {
                GCM += ovca.transform.position;
            }
            GCM /= ovce.Length;
            float razdalja = 0f;
            foreach (GameObject ovca in ovce)
            {
                razdalja += (ovca.transform.position - GCM).magnitude;
            }
            razdalja /= ovce.Length;
            razdalja *= 8;
            razdalja = Mathf.Min(Mathf.Max(razdalja, 25f), 75f);
            kamera.transform.position = new Vector3(GCM.x, razdalja / Mathf.Tan(Mathf.PI / 12), GCM.z) * 0.05f + kamera.transform.position * 0.95f;
        }
    }

    private void OnGUI()   // v zgornjem levem kotu napisi nekaj lastnosti simulacije in dodaj nekaj gumbov
    {
        ovce = GameObject.FindGameObjectsWithTag("Ovca");
        GUI.Label(new Rect(3, 0, 100, 20), string.Format("{0:00}:{1:00}", Mathf.FloorToInt(timer / 60), Mathf.FloorToInt(timer % 60)));
        if (GUI.Button(new Rect(150, 0, 50, 20), "Izhod"))
        { Application.Quit(); }
        GUI.Box(new Rect(3, 20, 200, 120),    "Ovce v staji: " + score + "\nOvce na pašniku: " + ovce.Length +
            "\nModel gibanja ovc: " + StaticClass.kombinacija.modelGibanja.ToString() + "\nStevilo ovcarjev: " + StaticClass.kombinacija.nOvcarjev + "\nModel vodenja ovcarjev: " + StaticClass.kombinacija.obnasanjePsa.ToString() +
            "\nGeneracija: " + Evolucija.generation + ", poskus: " + SimulationManeger.osebek);
        if (GUI.Button(new Rect(130, 140, 85, 20), "Naprej!"))  // naslednja simulacija iz seznama
        { SceneManager.LoadScene(0); }
        if (GUI.Button(new Rect(3, 160, 180, 20), kamera.GetComponent<Camera>().depth < 0 ? "Vkolpi sprehodno kamero" : "Izklopi sprehodno kamero"))  // naslednja simulacija iz seznama
        { kamera.GetComponent<Camera>().depth *= -1; StaticClass.kamera = kamera.GetComponent<Camera>().depth; }
        if (GUI.Button(new Rect(3, 140, 105, 20), "Celoten zaslon"))  // naslednja simulacija iz seznama
        { Screen.fullScreen = !Screen.fullScreen; }
        if (konec)   // na koncu eno minuto predvajaj spodnja navodila
        {
            GUI.Box(new Rect(Screen.width / 3, Screen.height / 2 - 60, Screen.width / 3, 120), "Bravo, vse simulacije so zaključene!\n" +
                                                "Za ponoven zagon izbriši datoteko 'kombinacije.txt'\n" +
                                                "in program bo začel od začetka.\n" +
                                                "Ob tem bo ohranil rezultate, ki se nahajajo v mapi\n" +
                                                "'Rezultati'." +
                                                "\n\n\tProgram se bo zdaj samodejno ugasnil.");
            if (timer > 60f) Application.Quit();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ovca"))  // ko ovca pride v stajo dodaj njen cas in jo po treh sekundah odstrani
        {
            Destroy(other.gameObject, 3f);
            if (!other.GetComponent<GinelliOvca>().umira)
            { casi.Add(timer); other.GetComponent<GinelliOvca>().umira = true; }
        }
    }
}
