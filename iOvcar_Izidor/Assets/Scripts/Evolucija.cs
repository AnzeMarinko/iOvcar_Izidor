using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Evolucija
{
    // guiscript naj tu kaj spremeni, kadar v AI1 (npr. sem naj pove case in se tu izracina fitness ipd.)
    // ta datoteka naj nadzoruje evolucijo
    public static float[] gen;
    static bool dolocen = false;
    public static void DNA()
    {
        if (!dolocen)
        {
            gen = new float[21];
            for (int i = 0; i < 21; i++) gen[i] = Random.value;
            dolocen = true;
        }
    }
}
