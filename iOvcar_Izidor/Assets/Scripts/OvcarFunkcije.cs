using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OvcarFunkcije : MonoBehaviour
{
    GameObject ovcar;
    public Vector3 smer;
    Vector3 prejsnaSmer;
    GameObject[] ovcarji;
    float v2;  // Hitrost premikanja v stanju iskanja ovc
    Vector3 cilj;   // pozicija staje
    GameObject tocka;  // pika ki se premika kamor je namenjen pes
    public GameObject pika;
    float hitrost = 0;
    float casDoNakljucnegaPremika;
    bool zacetekNakljucnegaPremika = false;
    Vector3 smerNakljucnegaPremika = new Vector3(0f, 0f, 0f);
    bool nakljucenPremik = false;   // ali sem v stanju nakljucnega premika, koliko ta traja, koliko casa je se do naslednjega, smer nakljucnega premika

    float v1, ra, dc, da, d0, df, e;
    float trajanjeNakljucnegaPremika, casNakljucnegaPremika, nakljucniDodatek, pomenRazdalje, pomenDoOvce, dovoljenoSpredaj, dovoljenoZadaj;
    float pomenOvcarjev, protiTockiNazaj, udobnaRazdalja, blizuTocki, dljeCilju, rotiraj, pomenSmeriDrugih;

    // Use this for initialization
    public void VoronoiStart()
    {
        ovcar = GetComponent<Rigidbody>().gameObject;
        v2 = ovcar.GetComponent<PremakniOvcarja>().vMax;   // hitrost v stanju teka
        casDoNakljucnegaPremika = Random.Range(0f, nakljucniDodatek) + casNakljucnegaPremika;   // nakjucen premik naj bo ob nakljucnem casu cez slabo minuto
        smer = ovcar.transform.rotation.eulerAngles;
        prejsnaSmer = smer;
        ovcarji = GameObject.FindGameObjectsWithTag("Ovcar");
        cilj = GameObject.FindGameObjectWithTag("Finish").transform.position;
        tocka = Instantiate(pika, new Vector3(Random.Range(-45f, 45f), -0.5f, Random.Range(-45f, 45f)), Quaternion.identity);
    }

    // Update is called once per frame
    public void VoronoiUpdate()
    {
        GameObject[] ovce = GameObject.FindGameObjectsWithTag("Ovca");
        casDoNakljucnegaPremika -= Time.deltaTime;
        if (ovce.Length > 0)
        {
            Vector3 GCM = new Vector3(0f, 0f, 0f);   // skupno povprecje pozicij ovc
            foreach (GameObject ovca in ovce)
            {
                GCM += ovca.transform.position;
            }
            GCM /= ovce.Length;
            float fN = F(ovce.Length);   // najvecja dovoljena velikost crede (polmer)

            Vector3 pobeglaOvca = new Vector3(0f, 0f, 0f);   // lokacija pobegle ovce in njena razdalja do GCM
            float doPobegle = 0;
            float doCentra = 0;
            bool soZbrane = true;
            bool preblizu = false;
            int pobeglih = 0;
            foreach (GameObject ovca in ovce)
            {
                float doOvce = (ovcar.transform.position - ovca.transform.position).magnitude;
                if (doOvce < da)
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
                float mera = Mathf.Pow(razdalja, pomenRazdalje) / Mathf.Pow(doOvce, pomenDoOvce);
                if (ovca.GetComponent<GinelliOvca>().voronoiPes == ovcar && razdalja > fN && (cilj - ovca.transform.position).magnitude > (GCM - cilj).magnitude - dovoljenoSpredaj && mera > doPobegle)
                {
                    // pes vodi proti GCM le ovce v njegovi Voronoievi celici, ki so od cilja dlje kot gcm
                    // ko ima celico prazno ali nobena ovca v njegovi celici ni pobegla, vodi celo credo, kot da je zbrana
                    doCentra = razdalja;
                    doPobegle = mera;
                    pobeglaOvca = ovca.transform.position;
                }
            }
            if (doCentra > fN && Mathf.Pow(ovcarji.Length, pomenOvcarjev) * pobeglih > ovce.Length * dovoljenoZadaj)   // ce je pobeglih ovc vec kot 15% / #ovcarjev^2
            {
                soZbrane = false;
            }
            if (casDoNakljucnegaPremika < 0f)
            {
                casDoNakljucnegaPremika = Random.Range(0f, nakljucniDodatek) + casNakljucnegaPremika;
                nakljucenPremik = false;
                zacetekNakljucnegaPremika = true;
            }
            else if (casDoNakljucnegaPremika < trajanjeNakljucnegaPremika)
            {
                nakljucenPremik = true;
            }

            Vector3 Pc = new Vector3(0f, 0f, 0f);   // tocka za zbiranje ovc v credo ali nakljucni premik
            Vector3 Pd = new Vector3(0f, 0f, 0f);   // tocka za vodenje crede
            if (nakljucenPremik)
            {
                if (zacetekNakljucnegaPremika)
                {
                    Pc = new Vector3(Random.Range(-48f, 48f), -0.5f, Random.Range(-48f, 48f));
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
                Pc = pobeglaOvca + (pobeglaOvca - GCM).normalized * dc;
            }
            else
            {
                // ovcar se postavi zadaj za credo glede na stajo
                Pd = GCM + (GCM - cilj).normalized * (fN + ra);
            }
            // tocka znotraj ograje
            tocka.transform.position = new Vector3(Mathf.Max(-49.8f, Mathf.Min(49.8f, (Pc + Pd).x)), 0f, Mathf.Max(-49.8f, Mathf.Min(49.8f, (Pc + Pd).z))) * 0.05f + tocka.transform.position * 0.95f;

            float premikNazaj = (ovcar.transform.position - cilj).magnitude - (tocka.transform.position - cilj).magnitude;
            if (premikNazaj < 0)
            {
                smer = (ovcar.transform.position - cilj) * Mathf.Sqrt(-premikNazaj) + (tocka.transform.position - ovcar.transform.position) * protiTockiNazaj;
            }
            else smer = tocka.transform.position - ovcar.transform.position;

            foreach (GameObject izbran in ovcarji)   // ovcarji naj se med seboj izogibajo in drzijo razdaljo
            {
                Vector3 razdalja = izbran.transform.position - ovcar.transform.position;
                if (razdalja.magnitude > 1e-3f)
                {
                    smer -= razdalja.normalized * udobnaRazdalja / (razdalja.magnitude + 1f);
                }
            }
            smer = smer.normalized;

            // ne zaokrozi, ce je skoraj na tocki
            if ((ovcar.transform.position - tocka.transform.position).magnitude < blizuTocki ||
                (ovcar.transform.position - GCM).magnitude > (GCM - tocka.transform.position).magnitude * dljeCilju) { }  // ce sem blizu tocki ali dlje od GCM kot tocka ne zaokrozim
            else if (preblizu)
            {  // https://math.stackexchange.com/questions/274712/calculate-on-which-side-of-a-straight-line-is-a-given-point-located
                Vector3 rotLevo = new Vector3(Mathf.Cos(rotiraj) * smer.x - Mathf.Sin(rotiraj) * smer.z,
                    0f, Mathf.Cos(rotiraj) * smer.z + Mathf.Sin(rotiraj) * smer.x);
                Vector3 rotDesno = new Vector3(Mathf.Cos(-rotiraj) * smer.x - Mathf.Sin(-rotiraj) * smer.z,
                    0f, Mathf.Cos(-rotiraj) * smer.z + Mathf.Sin(-rotiraj) * smer.x);
                float dGCM = (GCM.x - ovcar.transform.position.x) * smer.z - (GCM.z - ovcar.transform.position.z) * smer.x;
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
                Vector3 razdalja = ovcar.transform.position - o.transform.position;
                float oProtiOvcarju = razdalja.normalized.x * o.GetComponent<OvcarFunkcije>().smer.normalized.x +
                    razdalja.normalized.z * o.GetComponent<OvcarFunkcije>().smer.normalized.z;
                // bolj kot gre o proti ovcarju, blize je ta stevilka 1, ce gre direktno stran pa bolj -1
                float ovcarProtiO = -razdalja.normalized.x * smer.normalized.x - razdalja.normalized.z * smer.normalized.z;
                // bolj kot gre drug proti meni, bolj naj jaz ne grem proti njemu, ce je blizu
                float factor = Mathf.Max(oProtiOvcarju, ovcarProtiO); // ce gre vsaj eden proti drugemu, si zeli iti stran
                smerPsov += razdalja.normalized / (razdalja.magnitude + 1f) * factor;
            }
            if (smerPsov.magnitude > 1e-7f) smerPsov = smerPsov.normalized;
            smer += smerPsov * pomenSmeriDrugih;

            if (smer.magnitude > 1e-7) smer = smer.normalized;

            float phi = e * Random.Range(-Mathf.PI / 3f, Mathf.PI / 3f);  // dodaj sum
            smer = (1f - e) * smer + e * new Vector3(Mathf.Cos(phi), 0f, Mathf.Sin(phi));
            if (smer.magnitude > 1e-7) smer = smer.normalized;
            smer = smer * 0.95f + prejsnaSmer * 0.05f;  // gladko gibanje
            prejsnaSmer = smer;

            // nsatavi hitrost glede na razdaljo do staje ali tocke
            if ((ovcar.transform.position - cilj).magnitude < df || (ovcar.transform.position - tocka.transform.position).magnitude < d0)  // blizu cilja ali ovcam
            {
                hitrost = hitrost * 0.9f + v1 * 0.1f;
            }
            else
            {
                hitrost = hitrost * 0.9f + v2 * 0.1f;
            }
            smer = IzogibOgraji(transform.position, smer);
            Vector3 step = ovcar.transform.position + Time.deltaTime * hitrost * smer;
            Vector3 p = Vector3.MoveTowards(transform.position, step, hitrost);
            if (p.x > 49f) { GetComponent<Rigidbody>().MovePosition(new Vector3(49f, 0f, p.z)); }  // da se ne zatakne v stajo, ker ne zna ven
            else { GetComponent<Rigidbody>().MovePosition(p); }
            ovcar.transform.LookAt(p + smer);
        }
        if (ovcar.transform.position.y < 0f || ovcar.transform.position.y > 0.03f)  // tudi pes naj ne leti, rije ali se vrti na raznju
        {
            ovcar.transform.position = new Vector3(ovcar.transform.position.x, 0f, ovcar.transform.position.z);
            ovcar.transform.forward = new Vector3(ovcar.transform.forward.x, 0f, ovcar.transform.forward.z);
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

    float F(int N)   // najvecja dovoljena velikost crede (polmer) odvisna od stevila ovc
    {
        return ra * Mathf.Sqrt(N);
    }

    public void SetParameters(float p1, float p2, float p3, float p4, float p5, float p6, float p7, float p8, float p9, float p10,
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
}
