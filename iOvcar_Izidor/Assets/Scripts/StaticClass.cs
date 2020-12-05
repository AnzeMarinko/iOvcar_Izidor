using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticClass
{
    public static int zapStPrograma = 0;
    public static string datoteka = "kombinacije";
    public static float kamera = 1;
    public static float vMax = 7.5f / 3f;
    public static bool zacetek = true;
    public static DNA kombinacija;

    /*
    float v1 = (5 - 1) / 3f;  // Hitrost premikanja v stanju vodenja ovc
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
    */
    public static float[] zgornjeMeje = { vMax, 1, 4, 10, 5, 3, 0.5f, 5, 90, 30, 3, 3, 40, 0.5f, 3, 5, 40, 30, 2, Mathf.PI / 3, 1 };
    public static float[] spodnjeMeje = { 0, 1 / 3, 1, 0, 0, 0, 0.01f, 0, 10, 0, -1, -1, -10, 0, 0, 0, 5, 0, 0, 0, -1 };
    public static float[] rocniGen = new float[21];
}
