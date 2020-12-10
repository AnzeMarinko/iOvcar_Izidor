using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OvcarVoronoi : MonoBehaviour
{
    readonly float v1 = (5-1)/3f;  // Hitrost premikanja v stanju vodenja ovc
    readonly float ra = 2 / 3f;   // Faktor za dovoljeno velikost crede
    readonly float dc = 10 / 3f;  // Razdalja za zbiranje ovc v credo
    readonly float da = 20 / 3f;   // Razdalja za zaznavo ovc na poti
    readonly float d0 = 10 / 3f;   // Razdalja za upocasnitve v blizini ovc
    readonly float df = 5 / 3f;   // Razdalja za upocasnitev v blizini cilja
    readonly float e = 0.3f;  // Relativna moc suma
    readonly float trajanjeNakljucnegaPremika = 3f;
    readonly float casNakljucnegaPremika = 60f;
    readonly float nakljucniDodatek = 20f;
    readonly float pomenRazdalje = 2f;
    readonly float pomenDoOvce = 1f;
    readonly float dovoljenoSpredaj = 2f;
    readonly float dovoljenoZadaj = 0.15f;
    readonly float pomenOvcarjev = 2f;
    readonly float protiTockiNazaj = 2f;
    readonly float udobnaRazdalja = 20f;
    readonly float blizuTocki = 12f;
    readonly float dljeCilju = 0.95f;
    readonly float rotiraj = Mathf.PI / 6f;
    readonly float pomenSmeriDrugih = 0.1f;

    // Use this for initialization
    public void VoronoiStart()
    {
        ComputeGen();
        GetComponent<OvcarFunkcije>().SetParameters(v1, ra, dc, da, d0, df, e, trajanjeNakljucnegaPremika, casNakljucnegaPremika, nakljucniDodatek,
            pomenRazdalje, pomenDoOvce, dovoljenoSpredaj, dovoljenoZadaj, pomenOvcarjev, protiTockiNazaj, udobnaRazdalja, blizuTocki, dljeCilju, rotiraj, pomenSmeriDrugih);
        GetComponent<OvcarFunkcije>().VoronoiStart();
    }

    // Update is called once per frame
    public void VoronoiUpdate()
    {
        GetComponent<OvcarFunkcije>().VoronoiUpdate();
    }

    public void ComputeGen()
    {
        float[] par = {v1, ra, dc, da, d0, df, e, trajanjeNakljucnegaPremika, casNakljucnegaPremika, nakljucniDodatek,
            pomenRazdalje, pomenDoOvce, dovoljenoSpredaj, dovoljenoZadaj, pomenOvcarjev, protiTockiNazaj, udobnaRazdalja, blizuTocki, dljeCilju, rotiraj, pomenSmeriDrugih };
        float[] zm = StaticClass.zgornjeMeje;
        float[] sm = StaticClass.spodnjeMeje;

        float[] gen = new float[par.Length];
        for (int i = 0; i < par.Length; i++) gen[i] = (par[i] - sm[i]) / (zm[i] - sm[i]);
        StaticClass.rocniGen = gen;
    }
}
