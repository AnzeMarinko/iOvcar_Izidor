using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OvcarVoronoi : MonoBehaviour
{
    float v1 = (5-1)/3f;  // Hitrost premikanja v stanju vodenja ovc
    float ra = 2 / 3f;   // Faktor za dovoljeno velikost crede
    float dc = 10 / 3f;  // Razdalja za zbiranje ovc v credo
    float da = 20 / 3f;   // Razdalja za zaznavo ovc na poti
    float d0 = 10 / 3f;   // Razdalja za upocasnitve v blizini ovc
    float df = 5 / 3f;   // Razdalja za upocasnitev v blizini cilja
    float e = 0.3f;  // Relativna moc suma
    float trajanjeNakljucnegaPremika = 3f;
    float casNakljucnegaPremika = 60f;
    float nakljucniDodatek = 20f;
    float pomenRazdalje = 2f;
    float pomenDoOvce = 1f;
    float dovoljenoSpredaj = 2f;
    float dovoljenoZadaj = 0.15f;
    float pomenOvcarjev = 2f;
    float protiTockiNazaj = 2f;
    float udobnaRazdalja = 20f;
    float blizuTocki = 12f;
    float dljeCilju = 0.95f;
    float rotiraj = Mathf.PI / 6f;
    float pomenSmeriDrugih = 0.1f;

    // Use this for initialization
    public void VoronoiStart()
    {
        GetComponent<OvcarFunkcije>().SetParameters(v1, ra, dc, da, d0, df, e, trajanjeNakljucnegaPremika, casNakljucnegaPremika, nakljucniDodatek,
            pomenRazdalje, pomenDoOvce, dovoljenoSpredaj, dovoljenoZadaj, pomenOvcarjev, protiTockiNazaj, udobnaRazdalja, blizuTocki, dljeCilju, rotiraj, pomenSmeriDrugih);
        GetComponent<OvcarFunkcije>().VoronoiStart();
    }

    // Update is called once per frame
    public void VoronoiUpdate()
    {
        GetComponent<OvcarFunkcije>().VoronoiUpdate();
    }
}
