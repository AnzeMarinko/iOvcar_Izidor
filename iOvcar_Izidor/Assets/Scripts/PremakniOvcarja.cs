using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PremakniOvcarja : MonoBehaviour
{
    public OvcarEnum.ObnasanjePsa obnasanjeOvcarja;   // nacin vodenja ovcarja nastavljen v gui
    public float vMax = 7.5f / 3f;  // Najvisja hitrost premikanja ovcarjev (torej GetComponent<Rigidbody>().velocity.magnitude < 15)

    // pozeni pravi start glede na vodenje ovcarja
    void Start()
    {
        switch (obnasanjeOvcarja)
        {
            case OvcarEnum.ObnasanjePsa.Voronoi:
                {
                    GetComponent<OvcarVoronoi>().VoronoiStart();
                    break;
                }
            case OvcarEnum.ObnasanjePsa.AI1:
                {
                    GetComponent<OvcarAI1>().AI1Start();
                    break;
                }
            case OvcarEnum.ObnasanjePsa.AI2:
                {
                    GetComponent<OvcarAI2>().AI2Start();
                    break;
                }
            default: break;
        }
    }

    // pozeni pravi update glede na vodenje ovcarja
    void FixedUpdate()
    {
        switch (obnasanjeOvcarja)
        {
            case OvcarEnum.ObnasanjePsa.Voronoi:
                {
                    GetComponent<OvcarVoronoi>().VoronoiUpdate();
                    break;
                }
            case OvcarEnum.ObnasanjePsa.AI1:
                {
                    GetComponent<OvcarAI1>().AI1Update();
                    break;
                }
            case OvcarEnum.ObnasanjePsa.AI2:
                {
                    GetComponent<OvcarAI2>().AI2Update();
                    break;
                }
            default: break;
        }
    }
}
