﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticClass
{
    public static float v1 = (5 - 1) / 3f;  // Hitrost premikanja v stanju vodenja ovc
    public static float ra = 2 / 3f;   // Faktor za dovoljeno velikost crede
    public static float dc = 10 / 3f;  // Razdalja za zbiranje ovc v credo
    public static float da = 20 / 3f;   // Razdalja za zaznavo ovc na poti
    public static float d0 = 10 / 3f;   // Razdalja za upocasnitve v blizini ovc
    public static float df = 5 / 3f;   // Razdalja za upocasnitev v blizini cilja
    public static float e = 0.3f;  // Relativna moc suma
    public static float trajanjeNakljucnegaPremika = 3f;
    public static float casNakljucnegaPremika = 60f;
    public static float nakljucniDodatek = 20f;
    public static float pomenRazdalje = 2f;
    public static float pomenDoOvce = 1f;
    public static float dovoljenoSpredaj = 2f;
    public static float dovoljenoZadaj = 0.15f;
    public static float pomenOvcarjev = 2f;
    public static float protiTockiNazaj = 2f;
    public static float udobnaRazdalja = 20f;
    public static float blizuTocki = 12f;
    public static float dljeCilju = 0.95f;
    public static float rotiraj = Mathf.PI / 6f;
    public static float pomenSmeriDrugih = 0.1f;
    public static string[] imena = {"Hitrost v stanju vodenja",
                        "Faktor za dovoljeno velikost črede",
                        "Razdalja za zbiranje",
                        "Razdalja za zaznavo ovc na poti",
                        "Razdalja za upočasnitve v bližini ovc",
                        "Razdalja za upočasnitev v bližini cilja",
                        "Relativna moč šuma",
                        "Trajanje nakljucnega premika",
                        "Fiksen čas do naključnega premika",
                        "Razpon naključnega dodatnega časa",
                        "Pomen oddaljenosti pobegle ovce od črede",
                        "Pomen oddaljenosti pobegle ovce od ovčarja",
                        "Ovčar bližje cilju kot čreda",
                        "Največji delež ignoriranih ovc",
                        "Vpliv števila ovčarjev na delež ignoriranih ovc",
                        "Odpor pred stanjem bližje cilju kot točka",
                        "Udobna razdalja med ovčarji",
                        "Razdalja za upočasnitev v bližini točke",
                        "Odpor pred pred stanjem bližje čredi kot točka",
                        "Zaokroževanje blizu ovc",
                        "Pomen smeri drugih ovčarjev" };


    public static float vMax = 7.5f / 3f;
    public static float[] zgornjeMeje = { vMax, 1f, 4f, 10f, 5f, 3f, 0.5f, 5f, 90f, 30f, 3f, 3f, 40f, 0.5f, 3f, 5f, 40f, 30f, 2f, Mathf.PI / 3f, 1f };
    public static float[] spodnjeMeje = { 0f, 1f / 3f, 1f, 0f, 0f, 0f, 0.01f, 0f, 10f, 0f, -1f, -1f, -10f, 0f, 0f, 0f, 5f, 0f, 0f, 0f, -1f };
    public static float[] faktor = { 3f, 1f, 3f, 3f, 3f, 3f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 3f, 3f, 1f, 1f, 1f };
    public static string[] enote = { "m/s", "", "m", "m", "m", "m", "", "s", "s", "s", "", "", "", "", "", "", "m", "m", "", "rad/s", "" };
    public static float[] rocniGen = new float[21];

    public static void ComputeGen()
    {
        float[] par = {v1, ra, dc, da, d0, df, e, trajanjeNakljucnegaPremika, casNakljucnegaPremika, nakljucniDodatek,
            pomenRazdalje, pomenDoOvce, dovoljenoSpredaj, dovoljenoZadaj, pomenOvcarjev, protiTockiNazaj, udobnaRazdalja, blizuTocki, dljeCilju, rotiraj, pomenSmeriDrugih };
        float[] gen = new float[par.Length];
        for (int i = 0; i < par.Length; i++) gen[i] = (par[i] - spodnjeMeje[i]) / (zgornjeMeje[i] - spodnjeMeje[i]);
        rocniGen = gen;
    }

    public static void SetParameters(float p1, float p2, float p3, float p4, float p5, float p6, float p7, float p8, float p9, float p10,
        float p11, float p12, float p13, float p14, float p15, float p16, float p17, float p18, float p19, float p20, float p21)
    {
        v1 = p1;
        ra = p2;
        dc = p3;
        da = p4;
        d0 = p5;
        df = p6;
        e = p7;
        trajanjeNakljucnegaPremika = p8;
        casNakljucnegaPremika = p9;
        nakljucniDodatek = p10;
        pomenRazdalje = p11;
        pomenDoOvce = p12;
        dovoljenoSpredaj = p13;
        dovoljenoZadaj = p14;
        pomenOvcarjev = p15;
        protiTockiNazaj = p16;
        udobnaRazdalja = p17;
        blizuTocki = p18;
        dljeCilju = p19;
        rotiraj = p20;
        pomenSmeriDrugih = p21;
    }

    public static void ComputeParameters(float[] gen)
    {
        float[] zm = zgornjeMeje;
        float[] sm = spodnjeMeje;

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
