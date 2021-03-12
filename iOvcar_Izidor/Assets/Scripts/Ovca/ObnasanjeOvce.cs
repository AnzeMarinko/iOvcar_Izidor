using System.Collections.Generic;
using UnityEngine;

public class ObnasanjeOvce : MonoBehaviour
{
    Vector2 smer;      // njena smer
    readonly float rd = 15f;    // Obmocje zaznavanja sosednjih agentov in ograje
    readonly float rs = 30f;    // Obmocje zaznavanja ovcarja
    GinelliOvca.ModelGibanja modelGibanja;  // gibanje po popravljenem Stroembomu
    public Terrain terrain;

    // po Stroembomovem modelu:
    float speed = 5f;  // Hitrost premikanja
    readonly float ra = 2f;     // Obmocje preprecevanja trkov
    readonly float ps = 1f;     // Relativna moc odbojne sile od ovcarja
    readonly float pa = 2f;     // Relativna moc odbijanja med agenti
    readonly float c = 1.05f;   // Relativna moc privlacne sile v credo
    readonly float h = 0.5f;    // Relativna moc nadaljevanja v isti smeri
    readonly float e = 0.3f;    // Relativna moc suma
    readonly float dodajSum = 0.05f;   // Verjetnost za nastanek suma

    // po Ginellijevem modelu:
    readonly float v1 = 0.5f;   // Hitrost premikanja v stanju hoje
    readonly float v2 = 5f;     // Hitrost premikanja v stanju teka
    readonly float t01 = 70f;   // Spontani casovni prehod iz mirovanja v hojo
    readonly float t10 = 16f;   // Spontani casovni prehod iz hoje v mirovanje
    float t012;        // Spontani casovni prehod v tek
    float t20;         // Spontani casovni prehod iz teka v mirovanje
    readonly float dR = 31.6f;  // Karakteristična dolzinska utez za prestop v tek
    readonly float dS = 2.1f;   // Karakteristicna dolzinska utez za prestop iz teka
    readonly float re = 1f;     // Ravnovesna razdalja za izracun pprivlacno/odbojne sile
    readonly float r0 = 1f;     // Interakcijska razdalja med hojo
    readonly float alpha = 15f; // Utez efekta oponasanja
    readonly float beta = 0.8f; // Relativna moc privlacno/odbojne sile
    readonly float delta = 4f;  // Ojacevalni eksponent
    readonly float eta = 0.13f; // Razpon suma
    public List<float> xs = new List<float>();
    public List<float> zs = new List<float>();

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
        xs.Add(transform.position.x);
        zs.Add(transform.position.z); 
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
                        float razdalja = (point - position).sqrMagnitude;
                        if (razdalja < rd * rd && razdalja > 1e-4f)
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
                        && (Mathf.Max(Mathf.Abs(-terrain.center.x + transform.position.x), Mathf.Abs(-terrain.center.z + transform.position.z)) < 148f
                        || (-terrain.center.x + transform.position.x > 148f && Mathf.Abs(-terrain.center.z + transform.position.z) < 53f)))
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
                        if (razdalja.sqrMagnitude < rs * rs)
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
                        speed = v2;
                        foreach (GameObject o in terrain.sheepList)
                        {
                            Vector2 razdalja = new Vector2(o.transform.position.x, o.transform.position.z) - position;
                            if (razdalja.sqrMagnitude < rd * rd)  // smer teka poravnaj s smerjo teka bliznjih ovc
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
                        smer = vsotaSmeri.normalized;
                        if (beg.magnitude > 1e-4f) smer += beg.normalized * 0.1f;
                        smer = smer.normalized;
                        smer = IzogibOgraji(position - new Vector2(terrain.center.x, terrain.center.z), smer);
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
                            if ((position - new Vector2(o.transform.position.x, o.transform.position.z)).sqrMagnitude < r0 * r0)
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
                        smer = IzogibOgraji(position - new Vector2(terrain.center.x, terrain.center.z), smer);
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
                                speed = (Rs.magnitude > 1e-4 ? v2 : v2 / 10f) * 0.3f + 0.7f * speed;
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
                    smer = IzogibOgraji(position - new Vector2(terrain.center.x, terrain.center.z), smer);   // izogibanje ograji
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
        if (Mathf.Max(Mathf.Abs(transform.position.x - terrain.center.x), Mathf.Abs(transform.position.z - terrain.center.z)) > 149f &&
            (transform.position.x - terrain.center.x < 0f || Mathf.Abs(transform.position.z - terrain.center.z) > 50f))
        {
            transform.position = new Vector3(Mathf.Max(Mathf.Min(transform.position.x - terrain.center.x, 149f), -149f) + terrain.center.x,
                0.02f, Mathf.Max(Mathf.Min(transform.position.z - terrain.center.z, 149f), -149f) + terrain.center.z);
        }
        if (GetComponent<Rigidbody>().velocity.magnitude > speed)   // ce gre prehitro jo upocasni, da je ne izstreli
        {
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * speed;
        }
        if (transform.position.x - terrain.center.x > 151f)
        {
            if (!GetComponent<GinelliOvca>().umira)
            { terrain.RemoveSpecificSheep(this.gameObject); GetComponent<GinelliOvca>().umira = true; }
        }
    }

    Vector2 IzogibOgraji(Vector2 lokacija, Vector2 smer)
    {
        if (lokacija.x > 0f && Mathf.Abs(lokacija.y) < 50f) return smer;
        List<Vector3> colliders = new List<Vector3>();
        colliders.Add(new Vector2(-150f, lokacija.y));
        colliders.Add(new Vector3(lokacija.x, -150f));
        colliders.Add(new Vector3(150f, lokacija.y));
        colliders.Add(new Vector3(lokacija.x, 150f));

        float r_f = 20.0f;
        float r_f2 = r_f * r_f;
        float gamma = 0.1f;

        foreach (Vector2 closestPoint in colliders)
        {
            // get dist
            Vector2 e_ij = closestPoint - lokacija;
            if (e_ij.sqrMagnitude < r_f2)
            {
                smer += gamma * (-Mathf.Max(0f, (r_f - e_ij.magnitude) / r_f) * e_ij.normalized);
            }
        }
        return smer.normalized;
    }
}
