using System.Collections.Generic;
using UnityEngine;

public class ObnasanjeOvce : MonoBehaviour
{
    Vector2 smer;      // njena smer
    readonly float rd = 15f/3f;    // Obmocje zaznavanja sosednjih agentov in ograje
    readonly float rs = 30f/3f;    // Obmocje zaznavanja ovcarja
    GinelliOvca.ModelGibanja modelGibanja;  // gibanje po popravljenem Stroembomu
    public Terrain terrain;

    // po Stroembomovem modelu:
    float speed = 5f/3f;  // Hitrost premikanja
    readonly float ra = 2f/3f;     // Obmocje preprecevanja trkov
    readonly float ps = 1f;     // Relativna moc odbojne sile od ovcarja
    readonly float pa = 2f;     // Relativna moc odbijanja med agenti
    readonly float c = 1.05f;   // Relativna moc privlacne sile v credo
    readonly float h = 0.5f;    // Relativna moc nadaljevanja v isti smeri
    readonly float e = 0.3f;    // Relativna moc suma
    readonly float dodajSum = 0.05f;   // Verjetnost za nastanek suma

    // po Ginellijevem modelu:
    readonly float v1 = 2.5f / 3f;   // Hitrost premikanja v stanju hoje
    readonly float v2 = 5f / 3f;     // Hitrost premikanja v stanju teka
    readonly float t01 = 70f;   // Spontani casovni prehod iz mirovanja v hojo
    readonly float t10 = 16f;   // Spontani casovni prehod iz hoje v mirovanje
    float t012;        // Spontani casovni prehod v tek
    float t20;         // Spontani casovni prehod iz teka v mirovanje
    readonly float dR = 31.6f / 3f;  // Karakteristična dolzinska utez za prestop v tek
    readonly float dS = 2.1f / 3f;   // Karakteristicna dolzinska utez za prestop iz teka
    readonly float re = 1f / 3f;     // Ravnovesna razdalja za izracun pprivlacno/odbojne sile
    readonly float r0 = 1f / 3f;     // Interakcijska razdalja med hojo
    readonly float alpha = 15f; // Utez efekta oponasanja
    readonly float beta = 1.8f; // Relativna moc privlacno/odbojne sile
    readonly float delta = 4f;  // Ojacevalni eksponent
    readonly float eta = 0.13f; // Razpon suma

    // Use this for initialisation
    void Start()
    {
        smer = new Vector3(Mathf.Cos(transform.rotation.eulerAngles.y * 180f / Mathf.PI),
            Mathf.Sin(transform.rotation.eulerAngles.y * 180f / Mathf.PI));
        modelGibanja = GetComponent<GinelliOvca>().model;
        GetComponent<GinelliOvca>().voronoiPes = GameObject.FindGameObjectWithTag("Ovcar");
        terrain = transform.parent.GetComponent<Terrain>();
        GetComponent<GinelliOvca>().voronoiPes = terrain.sheepardList[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (GameObject ovcar in terrain.sheepardList)   // posodobitev, kateri pes je najblizje
        {
            if ((ovcar.transform.position - transform.position).magnitude < (GetComponent<GinelliOvca>().voronoiPes.transform.position - transform.position).magnitude)
            {
                GetComponent<GinelliOvca>().voronoiPes = ovcar;
            }
        }
        switch (modelGibanja)
        {
            case GinelliOvca.ModelGibanja.Ginelli:
            {
                // po Ginelliju
                Vector2 position = new Vector2(transform.position.x, transform.position.z);  // pozicija izbrane ovce v Vector2
                Vector2 vsotaSmeri = new Vector2(0f, 0f);
                t012 = terrain.sheepList.Count;  // stopnja preskoka iz stanja ali hoje v tek
                t20 = terrain.sheepList.Count;   // stopnja preskoka iz teka v stanje
                int stojece = 0;
                int hodijo = 0;
                int tecejo = 0;
                int ustavljane = 0;
                float l1 = 0;  // povprecna razdalja do ovc v teku
                float l2 = 0;  // povprecna razdalja do ustavljenih ovc

                // prestej sosednje ovce glede na stanja in izracunaj novo stanje
                foreach (GameObject o in terrain.sheepList)
                {
                    Vector2 point = new Vector2(o.transform.position.x, o.transform.position.z);
                    float razdalja = (point - position).magnitude;
                    if (razdalja < rd && razdalja > 1e-4f)
                    {
                        if (o.GetComponent<GinelliOvca>().tek)
                        {
                            tecejo++;
                            l1 += razdalja;
                        }
                        else if (o.GetComponent<GinelliOvca>().hoja)
                        {
                            hodijo++;
                        }
                        else { stojece++; }
                        if (o.GetComponent<GinelliOvca>().ustavljanje)
                        {
                            ustavljane++;
                            l2 += razdalja;
                        }
                    }
                }
                if (tecejo > 0) l1 /= tecejo;
                if (ustavljane > 0) l2 /= ustavljane;

                float x = Random.Range(0f, 10001f) / 10000f;   // preskok med stanjem in hojo
                if (!GetComponent<GinelliOvca>().tek)
                {
                    if (GetComponent<GinelliOvca>().hoja)
                    {
                        float p10 = (1 + alpha * stojece) / t10;
                        p10 = 1 - Mathf.Exp(-p10);
                        if (x < p10) GetComponent<GinelliOvca>().hoja = false;
                    }
                    else
                    {
                        float p01 = (1 + alpha * hodijo) / t01;
                        p01 = 1 - Mathf.Exp(-p01);
                        if (x < p01)
                        {
                            GetComponent<GinelliOvca>().hoja = true;
                        }
                        GetComponent<GinelliOvca>().ustavljanje = false;
                    }
                }

                x = Random.Range(0f, 10001f) / 10000f;   // preskok v tek ali iz teka
                if (!GetComponent<GinelliOvca>().tek
                    && (Mathf.Max(Mathf.Abs(- terrain.center.x + transform.position.x), Mathf.Abs(- terrain.center.z + transform.position.z)) < 48f
                    || (- terrain.center.x + transform.position.x > 45f && Mathf.Abs(- terrain.center.x + transform.position.x) < 20f)))
                {
                    float p012 = Mathf.Pow(l1 * (1 + alpha * tecejo) / dR, delta) / t012;
                    p012 = 1 - Mathf.Exp(-p012);
                    if (x < p012 * 100)
                    {
                        GetComponent<GinelliOvca>().tek = true;
                        GetComponent<GinelliOvca>().hoja = false;
                    }
                }
                else
                {
                    float p20 = (l2 < 1e-4f) ? 1f : Mathf.Pow(dS * (1 + alpha * ustavljane) / l2, delta) / t20;
                    p20 = (l2 < 1e-4f) ? 1f : 1 - Mathf.Exp(-p20);
                    if (x < p20)
                    {
                        GetComponent<GinelliOvca>().tek = false;
                        GetComponent<GinelliOvca>().ustavljanje = true;
                    }
                }

                Vector2 beg = new Vector2(0f, 0f);   // smer bega stran od ovcarjev in preskok v tek
                foreach (GameObject ovcar in terrain.sheepardList)
                {
                    Vector2 razdalja = position - new Vector2(ovcar.transform.position.x, ovcar.transform.position.z);
                    if (razdalja.magnitude < rs)
                    {
                        beg += razdalja.normalized * Mathf.Pow((razdalja.magnitude - rs) / rs, 2);
                        GetComponent<GinelliOvca>().tek = true;
                        GetComponent<GinelliOvca>().hoja = false;
                        GetComponent<GinelliOvca>().ustavljanje = false;
                    }
                }

                // izracunaj hitrost in smer
                if (GetComponent<GinelliOvca>().tek)
                {
                    speed = speed * 0.8f + v2 * 0.2f;
                    foreach (GameObject o in terrain.sheepList)
                    {
                        Vector2 razdalja = new Vector2(o.transform.position.x, o.transform.position.z) - position;
                        if (razdalja.magnitude < rd)  // smer teka poravnaj s smerjo teka bliznjih ovc
                        {
                            if (o.GetComponent<GinelliOvca>().tek)
                            {
                                vsotaSmeri.x += delta * o.transform.forward.x;
                                vsotaSmeri.y += delta * o.transform.forward.z;
                            }
                        }
                        // cohesion/repulsion force
                        float d_ij = razdalja.magnitude;   // bliznjim ovcam bodi na primerni razdalji
                        float f_ij = Mathf.Min(1, (d_ij - re) / re);
                        vsotaSmeri += beta * f_ij * razdalja.normalized;
                    }
                    smer = smer * 0.9f + 0.1f * vsotaSmeri.normalized;
                    if (beg.magnitude > 1e-4f) smer += beg.normalized;
                    smer = smer.normalized;
                    smer = IzogibOgraji(position, smer);
                    Vector2 step = position + Time.deltaTime * speed * smer;
                    Vector3 p = Vector3.MoveTowards(transform.position, new Vector3(step.x, 0f, step.y), speed);
                    GetComponent<Rigidbody>().MovePosition(p);
                    transform.LookAt(p + new Vector3(smer.x, 0f, smer.y));
                }
                else if (GetComponent<GinelliOvca>().hoja)
                {
                        speed = speed * 0.8f + v1 * 0.2f;
                    foreach (GameObject o in terrain.sheepList)   // smer hoje poravnaj s smerjo hoje bliznjih ovc
                    {
                        if ((position - new Vector2(o.transform.position.x, o.transform.position.z)).magnitude < r0)
                        {
                            vsotaSmeri.x += o.transform.forward.x;
                            vsotaSmeri.y += o.transform.forward.z;
                        }
                    }
                    if (vsotaSmeri.magnitude < 1e-3f) vsotaSmeri += new Vector2(transform.forward.x, transform.forward.z);
                    float angle = Mathf.Atan2(vsotaSmeri.y, vsotaSmeri.x);
                    float phi = Random.Range(-eta * Mathf.PI, eta * Mathf.PI);  // dodaj sum
                    angle += phi;
                    vsotaSmeri = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    smer = smer * 0.9f + 0.1f * vsotaSmeri.normalized;
                    smer = smer.normalized;
                    smer = IzogibOgraji(position, smer);
                    Vector2 step = position + Time.deltaTime * speed * smer;
                    Vector3 p = Vector3.MoveTowards(transform.position, new Vector3(step.x, 0f, step.y), speed);
                    GetComponent<Rigidbody>().MovePosition(p);
                    transform.LookAt(p + new Vector3(smer.x, 0f, smer.y));
                }
                break;
            }
            default:  // Stroembom
            {
                Vector2 position = new Vector2(transform.position.x, transform.position.z);   // pozicija ovce v Vector2
                List<Vector2> points = new List<Vector2>();    // seznam lokacij vseh ovc
                foreach (GameObject o in terrain.sheepList)
                {
                    Vector2 point = new Vector2(o.transform.position.x, o.transform.position.z);
                    points.Add(new Vector2(point.x, point.y));
                }
                Vector2 Ra = new Vector2(0f, 0f);   // izogibanje trkom med agenti
                Vector2 LCM = new Vector2(0f, 0f);  // lokalno povprecje
                int steviloOvcBlizu = 0;
                foreach (Vector2 o in points)
                {
                    float mag = (position - o).magnitude;  // razdalja med ovcama
                    if (mag < ra && mag > 1e-3f)
                        Ra += (position - o).normalized;
                    if (mag < rd)
                    {
                        LCM += o;
                        steviloOvcBlizu++;
                    }
                }
                Vector2 Rs = new Vector2(0f, 0f);   // izogibanje ovcarjem
                foreach (GameObject ovcar in terrain.sheepardList)
                {
                    Vector2 razdalja = position - new Vector2(ovcar.transform.position.x, ovcar.transform.position.z);
                    if (razdalja.magnitude < rs)
                    {
                        Rs += razdalja.normalized * Mathf.Pow((razdalja.magnitude - rs) / rs, 2);
                    }
                }
                LCM /= steviloOvcBlizu;
                Vector2 Ci = LCM - position;   // Zdruzevanje agentov v credo
                if (Rs.magnitude > 1e-4) { Rs = Rs.normalized; }   // normalizacija vseh izracunanih vektorjev
                if (Ra.magnitude > 1e-4) { Ra = Ra.normalized; }
                if (Ci.magnitude > 1e-4) { Ci = Ci.normalized; }
                Vector3 staraSmer = smer;
                smer = h * smer + pa * Ra + c * Ci + ps * Rs;   // posodobitev smeri z upostevanjem tudi prejsnje
                if (Random.Range(0f, 10001f) / 10000f < dodajSum)   // dodajanje suma
                {
                    float phi = Random.Range(0f, 2f * Mathf.PI);
                    smer += new Vector2(e * Mathf.Cos(phi), e * Mathf.Sin(phi));  // sum
                }
                smer = smer.normalized;
                switch (modelGibanja)
                {
                    case GinelliOvca.ModelGibanja.PopravljenStroembom:
                        {
                            speed = (Rs.magnitude > 1e-4 ? v2 : v2 / 5f) * 0.01f + 0.99f * speed;
                            float kot = Mathf.PI / (120f * Mathf.Sqrt(speed / v2));
                            if (Vector3.Dot(smer, staraSmer) < Mathf.Cos(kot))
                            {
                                Vector3 smer3 = new Vector3(smer.x, smer.y);
                                Vector3 os = Vector3.Cross(smer3, staraSmer);
                                smer3 = Quaternion.AngleAxis(-Mathf.Rad2Deg * kot, os) * staraSmer;
                                smer = new Vector2(smer3.x, smer3.y);
                            }
                            break;
                        }
                    default: break;
                }
                smer = IzogibOgraji(position, smer);   // izogibanje ograji
                Vector2 step = position + Time.deltaTime * speed * smer;
                Vector3 p = Vector3.MoveTowards(transform.position, new Vector3(step.x, 0f, step.y), speed);  // izracun naslednje tocke
                GetComponent<Rigidbody>().MovePosition(p);
                transform.LookAt(p + new Vector3(smer.x, 0f, smer.y));  // smer gledanja
                break;
            }
        }
        if (transform.position.y < 0f || transform.position.y > 0.1f)   // ovca ne sme leteti ali riti pod zemljo
        {
            transform.position = new Vector3(transform.position.x, 0.02f, transform.position.z);
            transform.forward = new Vector3(transform.forward.x, 0.02f, transform.forward.z);  // spotoma ji nastavi se obrnjenost tako da stoji na nogah
        }
        if (GetComponent<Rigidbody>().velocity.magnitude > 5f / 3f)   // ce gre prehitro jo upocasni, da je ne izstreli
        {
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * 5f / 3f;
        }
        if (transform.position.x > terrain.center.x + 51f)
        {
            if (!GetComponent<GinelliOvca>().umira)
            { terrain.RemoveSpecificSheep(this.gameObject); GetComponent<GinelliOvca>().umira = true; }
        }
    }    

    Vector2 IzogibOgraji(Vector2 lokacija, Vector2 smer)
    {
        lokacija.x -= terrain.center.x;
        lokacija.y -= terrain.center.z;
        // ce sem ograji blizje kot r se ji izognem, da ne grem proti njej prevec direktno
        float r = 5f;
        if (Mathf.Abs(lokacija.y) > 50f - r && lokacija.y * smer.y > 0f && Mathf.Abs(smer.x) < 0.9f) {
            
            float kot = (Mathf.Abs(lokacija.y) - 50f - r) *
                Mathf.PI / 90f * (lokacija.y > 0 ? -1f : 1f) *
                    (smer.x > 0 ? -1f : 1f);
            smer = new Vector2(Mathf.Cos(kot) * smer.x - Mathf.Sin(kot) * smer.y,
                Mathf.Cos(kot) * smer.y + Mathf.Sin(kot) * smer.x);
        }
        if (lokacija.x > 50f - r && Mathf.Abs(lokacija.y) + lokacija.x < 60f) { }   // normalno kjer je vhod v stajo
        else if (Mathf.Abs(lokacija.x) > 50f - r && lokacija.x * smer.x > 0f && Mathf.Abs(smer.y) < 0.9f)
        {
            float kot = (Mathf.Abs(lokacija.x) - 50f - r) *
                Mathf.PI / 90f * (lokacija.x > 0 ? -1f : 1f) *
                    (smer.y > 0 ? 1f : -1f);
            smer = new Vector2(Mathf.Cos(kot) * smer.x - Mathf.Sin(kot) * smer.y,
                Mathf.Cos(kot) * smer.y + Mathf.Sin(kot) * smer.x);
        }
        if (Mathf.Abs(lokacija.y) > 50f - r && Mathf.Abs(lokacija.x) > 50f - r && lokacija.x * smer.x > 0f && lokacija.y * smer.y > 0f)
        {
            float kotKot = Mathf.PI / 6f * ((lokacija.y * lokacija.x) > 0 ? 1f : -1f) *
                (Mathf.Abs(lokacija.y) < Mathf.Abs(lokacija.x) ? 1f : -1f);
            smer = new Vector2(Mathf.Cos(kotKot) * smer.x - Mathf.Sin(kotKot) * smer.y,
                Mathf.Cos(kotKot) * smer.y + Mathf.Sin(kotKot) * smer.x);
        }
        return smer.normalized;
    }
}