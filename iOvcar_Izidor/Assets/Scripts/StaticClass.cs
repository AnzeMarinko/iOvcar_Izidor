using UnityEngine;

public static class StaticClass
{
    public static string modelName = "";  // vhodno ime modela za shranjevanje
    public static bool zgodovina = true;  // shranjevanje rezultatov
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
    public static string[] imena = {"Hitrost v stanju vodenja",  // imena parametrov (alelov)
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
    // parametri za Voronoi model
    readonly public static float[] rocniParametri = { 5 / 3f, 2 / 3f, 10 / 3f, 20 / 3f, 10 / 3f, 5 / 3f, 0.3f, 3f, 60f, 20f, 2f, 1f, 2f, 0.15f, 2f, 2f, 20f, 12f, 0.95f, Mathf.PI / 6f, 0.1f };
    public static float vMax = 7.5f / 3f;
    // meje za parametre za genetski algoritem
    readonly public static float[] zgornjeMeje = { vMax, 3f, 10f, 10f, 10f, 10f, 0.5f, 10f, 90f, 30f,  3f,  3f, 40f, 0.4f,  4f, 10f, 40f, 20f, 1f, 2f,  1f };
    readonly public static float[] spodnjeMeje = { 0f,   0f,  0f,  0f,  0f,  0f,   0f,  0f,  0f,  0f, -1f, -1f, -1f,   0f, -1f,  0f,  0f,  0f, 0f, 0f, -1f };
    // faktor za prostorske vrednosti
    public static float[] faktor = { 3f, 3f, 3f, 3f, 3f, 3f, 100f, 1f, 1f, 1f, 1f, 1f, 1f, 100f, 1f, 1f, 3f, 3f, 1f, 1f, 1f };
    public static string[] enote = { "m/s", "m", "m", "m", "m", "m", "%", "s", "s", "s", "", "", "", "%", "", "", "m", "m", "", "rad/s", "" };
    public static float[] rocniGen = new float[21];


    // izracunaj gen (vrednosti od 0 do 1) za Voronoi model
    public static void ComputeGen()
    {
        for (int i = 0; i < 21; i++) rocniGen[i] = (rocniParametri[i] - spodnjeMeje[i]) / (zgornjeMeje[i] - spodnjeMeje[i]);
    }

    // nastavi parametrom izbrane vrednosti
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

    // Nastavi parametrom vrednosti glede na gen
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
        pomenRazdalje = sm[i] + (zm[i] - sm[i]) * gen[i]; i++;
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
