using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PremakniOvcarja : MonoBehaviour
{
    public OvcarEnum.ObnasanjePsa obnasanjeOvcarja;   // nacin vodenja ovcarja nastavljen v gui

    // pozeni pravi start glede na vodenje ovcarja
    void Start()
    {
        switch (obnasanjeOvcarja)
        {
            case OvcarEnum.ObnasanjePsa.Voronoi:
                {
                    GetComponent<OvcarFunkcije>().VoronoiStart();
                    break;
                }
            case OvcarEnum.ObnasanjePsa.AI1:
                {
                    StaticClass.ComputeParameters(transform.parent.GetComponent<Terrain>().sm.DNA.gen);
                    GetComponent<OvcarFunkcije>().VoronoiStart();
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
                    GetComponent<OvcarFunkcije>().VoronoiUpdate();
                    break;
                }
            case OvcarEnum.ObnasanjePsa.AI1:
                {
                    GetComponent<OvcarFunkcije>().VoronoiUpdate();
                    break;
                }
            default: break;
        }
    }
}
