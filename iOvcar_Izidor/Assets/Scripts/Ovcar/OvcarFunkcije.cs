﻿using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OvcarFunkcije : MonoBehaviour
{
    public Vector3 smer;
    Vector3 prejsnaSmer;
    List<GameObject> ovcarji;
    float v2;  // Hitrost premikanja v stanju iskanja ovc
    Vector3 cilj;   // pozicija staje
    public Vector3 tocka = new Vector3(0f, 0f, 0f);
    float hitrost = 0;
    float casDoNakljucnegaPremika;
    bool zacetekNakljucnegaPremika = false;
    Vector3 smerNakljucnegaPremika = new Vector3(0f, 0f, 0f);
    bool nakljucenPremik = false;   // ali sem v stanju nakljucnega premika, koliko ta traja, koliko casa je se do naslednjega, smer nakljucnega premika

    // Use this for initialization
    public void VoronoiStart()
    {
        v2 = StaticClass.vMax;   // hitrost v stanju teka
        casDoNakljucnegaPremika = Random.Range(0f, StaticClass.nakljucniDodatek) + StaticClass.casNakljucnegaPremika;   // nakjucen premik naj bo ob nakljucnem casu cez slabo minuto
        smer = transform.rotation.eulerAngles;
        prejsnaSmer = smer;
        ovcarji = transform.parent.GetComponent<Terrain>().sheepardList;
    }

    // Update is called once per frame
    public void VoronoiUpdate()
    {
        List<GameObject> ovce = transform.parent.GetComponent<Terrain>().sheepList;
        Vector3 center = transform.parent.GetComponent<Terrain>().center;
        cilj = center + new Vector3(175f, 0f, 0f);
        casDoNakljucnegaPremika -= Time.deltaTime;
        if (ovce.Count > 0)
        {
            Vector3 GCM = new Vector3(0f, 0f, 0f);   // skupno povprecje pozicij ovc
            foreach (GameObject ovca in ovce)
            {
                GCM += ovca.transform.position;
            }
            GCM /= ovce.Count;
            float fN = F(ovce.Count);   // najvecja dovoljena velikost crede (polmer)

            Vector3 pobeglaOvca = new Vector3(0f, 0f, 0f);   // lokacija pobegle ovce in njena razdalja do GCM
            float doPobegle = 0;
            float doCentra = 0;
            bool soZbrane = true;
            bool preblizu = false;
            int pobeglih = 0;
            foreach (GameObject ovca in ovce)
            {
                float doOvce = (transform.position - ovca.transform.position).magnitude;
                if (doOvce < StaticClass.da)
                {
                    preblizu = true;    // ce mora pes zaokroziti okrog kaksne ovce ker ji je preblizu
                    // print("preblizu ovci -> zaokrozi");
                }
                float razdalja = (GCM - ovca.transform.position).magnitude;
                if (razdalja > fN)
                {
                    pobeglih++;
                }
                // mera za dolocanje katero ovco je treba pripeljati blizje (razdalja do GCM bolj pomembna kot do ovcarja a tudi ta ni nepomembna)
                float mera = Mathf.Pow(razdalja, StaticClass.pomenRazdalje) / Mathf.Pow(doOvce, StaticClass.pomenDoOvce);
                if (ovca.GetComponent<GinelliOvca>().voronoiPes == this.gameObject && razdalja > fN && (cilj - ovca.transform.position).magnitude > (GCM - cilj).magnitude - StaticClass.dovoljenoSpredaj && mera > doPobegle)
                {
                    // pes vodi proti GCM le ovce v njegovi Voronoievi celici, ki so od cilja dlje kot gcm
                    // ko ima celico prazno ali nobena ovca v njegovi celici ni pobegla, vodi celo credo, kot da je zbrana
                    doCentra = razdalja;
                    doPobegle = mera;
                    pobeglaOvca = ovca.transform.position;
                }
            }
            if (doCentra > fN && Mathf.Pow(ovcarji.Count, StaticClass.pomenOvcarjev) * pobeglih > ovce.Count * StaticClass.dovoljenoZadaj)   // ce je pobeglih ovc vec kot 15% / #ovcarjev^2
            {
                soZbrane = false;
            }
            if (casDoNakljucnegaPremika < 0f)
            {
                casDoNakljucnegaPremika = Random.Range(0f, StaticClass.nakljucniDodatek) + StaticClass.casNakljucnegaPremika;
                nakljucenPremik = false;
                zacetekNakljucnegaPremika = true;
            }
            else if (casDoNakljucnegaPremika < StaticClass.trajanjeNakljucnegaPremika)
            {
                nakljucenPremik = true;
            }

            Vector3 Pc = new Vector3(0f, 0f, 0f);   // tocka za zbiranje ovc v credo ali nakljucni premik
            Vector3 Pd = new Vector3(0f, 0f, 0f);   // tocka za vodenje crede
            if (nakljucenPremik)
            {
                if (zacetekNakljucnegaPremika)
                {
                    Pc = new Vector3(Random.Range(-120f, 0f), 0f, Random.Range(-120f, 120f));
                    smerNakljucnegaPremika = Pc;
                    zacetekNakljucnegaPremika = false;
                }
                else
                {
                    Pc = smerNakljucnegaPremika;
                }
            }
            else if (!soZbrane)
            {
                // ovcar se postavi zadaj za pobeglo ovco glede na credo
                Pc = pobeglaOvca + (pobeglaOvca - GCM).normalized * StaticClass.dc - center;
            }
            else
            {
                // ovcar se postavi zadaj za credo glede na stajo
                Pd = GCM + (GCM - cilj).normalized * (fN + StaticClass.ra) - center;
            }
            // tocka znotraj ograje
            tocka = (center + new Vector3(Mathf.Max(-149.5f, Mathf.Min(149.5f, (Pc + Pd).x)), 0f, Mathf.Max(-149.5f, Mathf.Min(149.5f, (Pc + Pd).z)))) * 0.05f + tocka * 0.95f;

            float premikNazaj = (transform.position - cilj).magnitude - (tocka - cilj).magnitude;
            if (premikNazaj < 0)
            {
                smer = (transform.position - cilj) * Mathf.Sqrt(-premikNazaj) + (tocka - transform.position) * StaticClass.protiTockiNazaj;
            }
            else smer = tocka - transform.position;

            foreach (GameObject izbran in ovcarji)   // ovcarji naj se med seboj izogibajo in drzijo razdaljo
            {
                Vector3 razdalja = izbran.transform.position - transform.position;
                if (razdalja.magnitude > 1e-3f)
                {
                    smer -= razdalja.normalized * StaticClass.udobnaRazdalja / (razdalja.magnitude + 1f);
                }
            }
            smer = smer.normalized;

            // ne zaokrozi, ce je skoraj na tocki
            if ((transform.position - tocka).magnitude < StaticClass.blizuTocki ||
                (transform.position - GCM).magnitude > (GCM - tocka).magnitude * StaticClass.dljeCilju) { }  // ce sem blizu tocki ali dlje od GCM kot tocka ne zaokrozim
            else if (preblizu)
            {  // https://math.stackexchange.com/questions/274712/calculate-on-which-side-of-a-straight-line-is-a-given-point-located
                Vector3 rotLevo = new Vector3(Mathf.Cos(StaticClass.rotiraj) * smer.x - Mathf.Sin(StaticClass.rotiraj) * smer.z,
                    0f, Mathf.Cos(StaticClass.rotiraj) * smer.z + Mathf.Sin(StaticClass.rotiraj) * smer.x);
                Vector3 rotDesno = new Vector3(Mathf.Cos(-StaticClass.rotiraj) * smer.x - Mathf.Sin(-StaticClass.rotiraj) * smer.z,
                    0f, Mathf.Cos(-StaticClass.rotiraj) * smer.z + Mathf.Sin(-StaticClass.rotiraj) * smer.x);
                float dGCM = (GCM.x - transform.position.x) * smer.z - (GCM.z - transform.position.z) * smer.x;
                float dRotLevo = rotLevo.x * smer.z - rotLevo.z * smer.x;
                if (dGCM * dRotLevo < 0)  // GCM on the other side as rotation in left
                {
                    smer = rotLevo;  // zaokrozi okrog (izberi tisti vektor ki res zaokrozi
                }
                else { smer = rotDesno; }
            }

            Vector3 smerPsov = new Vector3(0f, 0f, 0f);  // ovcar naj si zeli, da gredo skupaj v povprecju proti cilju
            // bolj naj uposteva blizje ovcarje (tudi sebe)
            foreach (GameObject o in ovcarji)
            {
                Vector3 razdalja = transform.position - o.transform.position;
                float oProtiOvcarju = razdalja.normalized.x * o.GetComponent<OvcarFunkcije>().smer.normalized.x +
                    razdalja.normalized.z * o.GetComponent<OvcarFunkcije>().smer.normalized.z;
                // bolj kot gre o proti ovcarju, blize je ta stevilka 1, ce gre direktno stran pa bolj -1
                float ovcarProtiO = -razdalja.normalized.x * smer.normalized.x - razdalja.normalized.z * smer.normalized.z;
                // bolj kot gre drug proti meni, bolj naj jaz ne grem proti njemu, ce je blizu
                float factor = Mathf.Max(oProtiOvcarju, ovcarProtiO); // ce gre vsaj eden proti drugemu, si zeli iti stran
                smerPsov += razdalja.normalized / (razdalja.magnitude + 1f) * factor;
            }
            if (smerPsov.magnitude > 1e-7f) smerPsov = smerPsov.normalized;
            smer += smerPsov * StaticClass.pomenSmeriDrugih;

            if (smer.magnitude > 1e-7) smer = smer.normalized;

            float phi = StaticClass.e * Random.Range(-Mathf.PI / 3f, Mathf.PI / 3f);  // dodaj sum
            smer = (1f - StaticClass.e) * smer + StaticClass.e * new Vector3(Mathf.Cos(phi), 0f, Mathf.Sin(phi));
            if (smer.magnitude > 1e-7) smer = smer.normalized;
            smer = smer * 0.95f + prejsnaSmer * 0.05f;  // gladko gibanje
            prejsnaSmer = smer;

            // nsatavi hitrost glede na razdaljo do staje ali tocke
            if ((transform.position - cilj).magnitude < StaticClass.df || (transform.position - tocka).magnitude < StaticClass.d0)  // blizu cilja ali ovcam
            {
                hitrost = hitrost * 0.9f + StaticClass.v1 * 0.1f;
            }
            else
            {
                hitrost = hitrost * 0.9f + v2 * 0.1f;
            }
            smer = IzogibOgraji(- center + transform.position, smer);
            Vector3 step = transform.position + Time.deltaTime * hitrost * smer;
            Vector3 p = Vector3.MoveTowards(transform.position, step, hitrost);
            if (p.x > center.x  + 149f) { GetComponent<Rigidbody>().MovePosition(center + new Vector3(149f, 0f, p.z)); }  // da se ne zatakne v stajo, ker ne zna ven
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
        float r = 5f;
        if (Mathf.Abs(lokacija.z) > 150f - r && lokacija.z * smer.z > 0f && Mathf.Abs(smer.x) < 0.9f)
        {

            float kot = (Mathf.Abs(lokacija.z) - 150f - r) *
                Mathf.PI / 30f * (lokacija.z > 0 ? -1f : 1f) *
                    (smer.x > 0 ? -1f : 1f);
            smer = new Vector3(Mathf.Cos(kot) * smer.x - Mathf.Sin(kot) * smer.z,
                Mathf.Cos(kot) * smer.z + Mathf.Sin(kot) * smer.x);
        }
        else if (Mathf.Abs(lokacija.x) > 150f - r && lokacija.x * smer.x > 0f && Mathf.Abs(smer.z) < 0.9f)
        {
            float kot = (Mathf.Abs(lokacija.x) - 150f - r) *
                Mathf.PI / 30f * (lokacija.x > 0 ? -1f : 1f) *
                    (smer.z > 0 ? 1f : -1f);
            smer = new Vector3(Mathf.Cos(kot) * smer.x - Mathf.Sin(kot) * smer.z,
                Mathf.Cos(kot) * smer.z + Mathf.Sin(kot) * smer.x);
        }
        if (Mathf.Abs(lokacija.z) > 150f - r && Mathf.Abs(lokacija.x) > 150f - r && lokacija.x * smer.x > 0f && lokacija.z * smer.z > 0f)
        {
            float kotKot = Mathf.PI / 3f * ((lokacija.z * lokacija.x) > 0 ? 1f : -1f) *
                (Mathf.Abs(lokacija.z) < Mathf.Abs(lokacija.x) ? 1f : -1f);
            smer = new Vector3(Mathf.Cos(kotKot) * smer.x - Mathf.Sin(kotKot) * smer.z, 0f,
                Mathf.Cos(kotKot) * smer.z + Mathf.Sin(kotKot) * smer.x);
        }
        return smer.normalized;
    }

    float F(int N)   // najvecja dovoljena velikost crede (polmer) odvisna od stevila ovc
    {
        return StaticClass.ra * Mathf.Sqrt(N);
    }
}
