using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OvcarAI2 : MonoBehaviour
{
    /*
     * 
     * vse potrebno za drugega pametnega psa (glej voronoi kodo, da ne pozabiš na izogib ograji in uporabne funkcije)
     *
     */

    public void AI2Start()
    {
        // ML ovcar naj bo nov prefab ki ima namesto obnašanjeOvcarja kar OvcarAgent
    }

    // Update is called once per frame
    public void AI2Update()
    {
        
    }

    public Vector3 smer;
    Vector3 prejsnaSmer;
    float v2;  // Hitrost premikanja v stanju iskanja ovc
    Vector3 cilj;   // pozicija staje
    float hitrost = 0;
    List<GameObject> ovcarji;

    public void VoronoiStart()
    {
        v2 = StaticClass.vMax;   // hitrost v stanju teka
        smer = transform.rotation.eulerAngles;
        prejsnaSmer = smer;
        ovcarji = transform.parent.GetComponent<Terrain>().sheepardList;
    }

    // Update is called once per frame
    public void VoronoiUpdate()
    {
        List<GameObject> ovce = transform.parent.GetComponent<Terrain>().sheepList;
        Vector3 center = transform.parent.GetComponent<Terrain>().center;
        cilj = center + new Vector3(60f, 0f, 0f);
        if (ovce.Count > 0)
        {
            if (smer.magnitude > 1e-7) smer = smer.normalized;
            smer = smer * 0.95f + prejsnaSmer * 0.05f;  // gladko gibanje
            prejsnaSmer = smer;
            smer = IzogibOgraji(-center + transform.position, smer);
            Vector3 step = transform.position + Time.deltaTime * hitrost * smer;
            Vector3 p = Vector3.MoveTowards(transform.position, step, hitrost);
            if (p.x > center.x + 49f) { GetComponent<Rigidbody>().MovePosition(center + new Vector3(49f, 0f, p.z)); }  // da se ne zatakne v stajo, ker ne zna ven
            else { GetComponent<Rigidbody>().MovePosition(p); }
            transform.LookAt(p + smer);
        }
        if (transform.position.y < 0f || transform.position.y > 0.05f)  // tudi pes naj ne leti, rije ali se vrti na raznju
        {
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.forward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        }

    }

    Vector3 IzogibOgraji(Vector3 lokacija, Vector3 smer)
    {
        // ce sem ograji blizje kot r se ji izognem, da ne grem proti njej prevec direktno
        float r = 2f;
        if (Mathf.Abs(lokacija.z) > 50f - r && lokacija.z * smer.z > 0f && Mathf.Abs(smer.x) < 0.9f)
        {

            float kot = (Mathf.Abs(lokacija.z) - 50f - r) *
                Mathf.PI / 30f * (lokacija.z > 0 ? -1f : 1f) *
                    (smer.x > 0 ? -1f : 1f);
            smer = new Vector3(Mathf.Cos(kot) * smer.x - Mathf.Sin(kot) * smer.z,
                Mathf.Cos(kot) * smer.z + Mathf.Sin(kot) * smer.x);
        }
        else if (Mathf.Abs(lokacija.x) > 50f - r && lokacija.x * smer.x > 0f && Mathf.Abs(smer.z) < 0.9f)
        {
            float kot = (Mathf.Abs(lokacija.x) - 50f - r) *
                Mathf.PI / 30f * (lokacija.x > 0 ? -1f : 1f) *
                    (smer.z > 0 ? 1f : -1f);
            smer = new Vector3(Mathf.Cos(kot) * smer.x - Mathf.Sin(kot) * smer.z,
                Mathf.Cos(kot) * smer.z + Mathf.Sin(kot) * smer.x);
        }
        if (Mathf.Abs(lokacija.z) > 50f - r && Mathf.Abs(lokacija.x) > 50f - r && lokacija.x * smer.x > 0f && lokacija.z * smer.z > 0f)
        {
            float kotKot = Mathf.PI / 3f * ((lokacija.z * lokacija.x) > 0 ? 1f : -1f) *
                (Mathf.Abs(lokacija.z) < Mathf.Abs(lokacija.x) ? 1f : -1f);
            smer = new Vector3(Mathf.Cos(kotKot) * smer.x - Mathf.Sin(kotKot) * smer.z, 0f,
                Mathf.Cos(kotKot) * smer.z + Mathf.Sin(kotKot) * smer.x);
        }
        return smer.normalized;
    }
}
