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
        ComputeParameters(transform.parent.GetComponent<Terrain>().sm.DNA.gen);
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
        float[] zm = StaticClass.zgornjeMeje;
        float[] sm = StaticClass.spodnjeMeje;

        int i = 0;
        v1 = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        ra = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        dc = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        da = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        d0 = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        df = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        e = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        trajanjeNakljucnegaPremika = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        casNakljucnegaPremika = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        nakljucniDodatek = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        pomenRazdalje = sm[i] + (zm[i] - sm[i]) * gen[i];
        pomenDoOvce = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        dovoljenoSpredaj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        dovoljenoZadaj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        pomenOvcarjev = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        protiTockiNazaj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        udobnaRazdalja = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        blizuTocki = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        dljeCilju = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        rotiraj = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
        pomenSmeriDrugih = sm[i] + (zm[i] - sm[i]) * gen[i];
    }
}
