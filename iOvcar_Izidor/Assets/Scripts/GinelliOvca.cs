using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// lastnosti gibanja ovc, kadar so po Ginelliju
public class GinelliOvca : MonoBehaviour
{
    public bool hoja = true;   // nacin gibanja (hoja, tek in ustavljanje en trenutek po tem, ko gre iz teka v stanje)
    public bool tek = false;
    public bool ustavljanje = false;
    public bool Ginelli = true;   // ali se giblje po Ginelliju ali Stroembomu
    public GameObject voronoiPes;   // vsaka ovca ima izracunano, kateri pes ji je najblizje (v cigavi voronoiei celici je)
    public bool umira = false;   // ze prestopila prag staje
}
