using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OvcarAI1 : MonoBehaviour
{
    float v1, ra, dc, da, d0, df, e;
    float trajanjeNakljucnegaPremika, casNakljucnegaPremika, nakljucniDodatek, pomenRazdalje, pomenDoOvce, dovoljenoSpredaj, dovoljenoZadaj;
    float pomenOvcarjev, protiTockiNazaj, udobnaRazdalja, blizuTocki, dljeCilju, rotiraj, pomenSmeriDrugih;

    public void AI1Start()
    {
        Evolucija.DNA();
        ComputeParameters(Evolucija.gen);
        GetComponent<OvcarFunkcije>().SetParameters(v1, ra, dc, da, d0, df, e, trajanjeNakljucnegaPremika, casNakljucnegaPremika, nakljucniDodatek,
            pomenRazdalje, pomenDoOvce, dovoljenoSpredaj, dovoljenoZadaj, pomenOvcarjev, protiTockiNazaj, udobnaRazdalja, blizuTocki, dljeCilju, rotiraj, pomenSmeriDrugih);
        GetComponent<OvcarFunkcije>().VoronoiStart();
    }

    // Update is called once per frame
    public void AI1Update()
    {
        GetComponent<OvcarFunkcije>().VoronoiUpdate();
    }

    public void ComputeParameters(float[] gen)
    {
        float v2 = GetComponent<PremakniOvcarja>().vMax;

        v1 = v2 * gen[0];                 // [0, v2]
        ra = 1 / 3f + 4 / 3f * gen[1];   // Faktor za dovoljeno velikost crede          [1/3, 5/3]
        dc = 1 + 4 * gen[2];  // Razdalja za zbiranje ovc v credo            [1, 5]
        da = 1 + 14 * gen[3];   // Razdalja za zaznavo ovc na poti            [1, 15]
        d0 = 1 + 4 * gen[4];   // Razdalja za upocasnitve v blizini ovc      [1, 5]
        df = 1 + 4 * gen[5];   // Razdalja za upocasnitev v blizini cilja     [1, 5]
        e = 0.01f + 0.5f * gen[6];  // Relativna moc suma            [0.01, 0.5]
        trajanjeNakljucnegaPremika = 5 * gen[7];         // [0, 5]
        casNakljucnegaPremika = 10 + 80 * gen[8];             // [10, 90]
        nakljucniDodatek = 30 * gen[9];                  // [0, 30]
        pomenRazdalje = -1 + 4 * gen[10];                      // [-1, 3]
        pomenDoOvce = -1 + 4 * gen[11];                        // [-1, 3]
        dovoljenoSpredaj = 40 * gen[12];                   // [0, 40]
        dovoljenoZadaj = 0.3f * gen[13];                  // [0, 0.3]
        pomenOvcarjev = 3 * gen[14];                      // [0, 3]
        protiTockiNazaj = 5 * gen[15];                    // [0, 5]
        udobnaRazdalja = 5 + 35 * gen[16];                    // [5, 40]
        blizuTocki = 30 * gen[17];                        // [0, 30]
        dljeCilju = 2 * gen[18];                       // [0, 2]
        rotiraj = Mathf.PI / 2f * gen[19];                 // [0, Mathf.PI / 2]
        pomenSmeriDrugih = -1 + 2 * gen[20];                 // [-1, 1]
    }
}
