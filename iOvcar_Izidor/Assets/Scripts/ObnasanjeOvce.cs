using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObnasanjeOvce : MonoBehaviour
{
    GameObject ovca;   // izbrana ovca
    Vector2 smer;      // njena smer
    GameObject[] ovcarji;   // seznam ovcarjev
    readonly float rd = 15/3f;    // Obmocje zaznavanja sosednjih agentov in ograje
    readonly float rs = 30/3f;    // Obmocje zaznavanja ovcarja
    bool byGinelli;   // gibanje po Ginelliju ali Stroembomu

    // po Stroembomovem modelu:
    readonly float speed = 5/3f;  // Hitrost premikanja
    readonly float ra = 2/3f;     // Obmocje preprecevanja trkov
    readonly float ps = 1f;     // Relativna moc odbojne sile od ovcarja
    readonly float pa = 2f;     // Relativna moc odbijanja med agenti
    readonly float c = 1.05f;   // Relativna moc privlacne sile v credo
    readonly float h = 0.5f;    // Relativna moc nadaljevanja v isti smeri
    readonly float e = 0.3f;    // Relativna moc suma
    readonly float dodajSum = 0.05f;   // Verjetnost za nastanek suma

    // po Ginellijevem modelu:
    readonly float v1 = 2.5f / 3f;   // Hitrost premikanja v stanju hoje
    readonly float v2 = 5 / 3f;     // Hitrost premikanja v stanju teka
    readonly float t01 = 70f;   // Spontani casovni prehod iz mirovanja v hojo
    readonly float t10 = 16f;   // Spontani casovni prehod iz hoje v mirovanje
    float t012;        // Spontani casovni prehod v tek
    float t20;         // Spontani casovni prehod iz teka v mirovanje
    readonly float dR = 31.6f / 3f;  // Karakteristična dolzinska utez za prestop v tek
    readonly float dS = 2.1f / 3f;   // Karakteristicna dolzinska utez za prestop iz teka
    readonly float re = 1 / 3f;     // Ravnovesna razdalja za izracun pprivlacno/odbojne sile
    readonly float r0 = 1 / 3f;     // Interakcijska razdalja med hojo
    readonly float alpha = 15f; // Utez efekta oponasanja
    readonly float beta = 1.8f; // Relativna moc privlacno/odbojne sile
    readonly float delta = 4f;  // Ojacevalni eksponent
    readonly float eta = 0.13f; // Razpon suma

    // Use this for initialisation
    void Start()
    {
        ovca = GetComponent<Rigidbody>().gameObject;    // nastavitev zacetnih vrednosti
        smer = new Vector3(Mathf.Cos(ovca.transform.rotation.eulerAngles.y * 180f / Mathf.PI),
            Mathf.Sin(ovca.transform.rotation.eulerAngles.y * 180f / Mathf.PI)); ;
        ovcarji = GameObject.FindGameObjectsWithTag("Ovcar");
        byGinelli = ovca.GetComponent<GinelliOvca>().Ginelli;
        ovca.GetComponent<GinelliOvca>().voronoiPes = ovcarji[0];
        foreach (GameObject ovcar in ovcarji)   // izbira najblizjega psa
        {
            if ((ovcar.transform.position - ovca.transform.position).magnitude < (ovca.GetComponent<GinelliOvca>().voronoiPes.transform.position - ovca.transform.position).magnitude)
            {
                ovca.GetComponent<GinelliOvca>().voronoiPes = ovcar;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (GameObject ovcar in ovcarji)   // posodobitev, kateri pes je najblizje
        {
            if ((ovcar.transform.position - ovca.transform.position).magnitude < (ovca.GetComponent<GinelliOvca>().voronoiPes.transform.position - ovca.transform.position).magnitude)
            {
                ovca.GetComponent<GinelliOvca>().voronoiPes = ovcar;
            }
        }
        GameObject[] ovce = GameObject.FindGameObjectsWithTag("Ovca");
        if (!byGinelli)   // po Stroembomu
        {
            Vector2 position = new Vector2(ovca.transform.position.x, ovca.transform.position.z);   // pozicija ovce v Vector2
            List<Vector2> points = new List<Vector2>();    // seznam lokacij vseh ovc
            foreach (GameObject o in ovce)
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
            foreach (GameObject ovcar in ovcarji)
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
            smer = h * smer + pa * Ra + c * Ci + ps * Rs;   // posodobitev smeri z upostevanjem tudi prejsnje
            if (Random.value < dodajSum)   // dodajanje suma
            {
                float phi = Random.Range(0f, 2f * Mathf.PI);
                smer += new Vector2(e * Mathf.Cos(phi), e * Mathf.Sin(phi));  // sum
            }
            smer = smer.normalized;
            smer = IzogibOgraji(position, smer);   // izogibanje ograji
            Vector2 step = position + Time.deltaTime * speed * smer;
            Vector3 p = Vector3.MoveTowards(transform.position, new Vector3(step.x, 0f, step.y), speed);  // izracun naslednje tocke
            GetComponent<Rigidbody>().MovePosition(p);
            ovca.transform.LookAt(p + new Vector3(smer.x, 0f, smer.y));  // smer gledanja
        } 
        else
        {
            // po Ginelliju
            Vector2 position = new Vector2(ovca.transform.position.x, ovca.transform.position.z);  // pozicija izbrane ovce v Vector2
            Vector2 vsotaSmeri = new Vector2(0f, 0f);
            float speed;
            t012 = ovce.Length;  // stopnja preskoka iz stanja ali hoje v tek
            t20 = ovce.Length;   // stopnja preskoka iz teka v stanje
            int stojece = 0;
            int hodijo = 0;
            int tecejo = 0;
            int ustavljane = 0;
            float l1 = 0;  // povprecna razdalja do ovc v teku
            float l2 = 0;  // povprecna razdalja do ustavljenih ovc

            // prestej sosednje ovce glede na stanja in izracunaj novo stanje
            foreach (GameObject o in ovce)
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

            float x = Random.Range(.0f, 1.0f);   // preskok med stanjem in hojo
            if (!ovca.GetComponent<GinelliOvca>().tek)
            {
                if (ovca.GetComponent<GinelliOvca>().hoja)
                {
                    float p10 = (1 + alpha * stojece) / t10;
                    p10 = 1 - Mathf.Exp(-p10);
                    if (x < p10) ovca.GetComponent<GinelliOvca>().hoja = false;
                }
                else
                {
                    float p01 = (1 + alpha * hodijo) / t01;
                    p01 = 1 - Mathf.Exp(-p01);
                    if (x < p01)
                    {
                        ovca.GetComponent<GinelliOvca>().hoja = true;
                    }
                    ovca.GetComponent<GinelliOvca>().ustavljanje = false;
                }
            }

            x = Random.Range(.0f, 1.0f);   // preskok v tek ali iz teka
            if (!ovca.GetComponent<GinelliOvca>().tek
                && (Mathf.Max(Mathf.Abs(ovca.transform.position.x), Mathf.Abs(ovca.transform.position.z)) < 48f
                || (ovca.transform.position.x > 45f && Mathf.Abs(ovca.transform.position.x) < 20f)))
            {
                float p012 = Mathf.Pow(l1 * (1 + alpha * tecejo) / dR, delta) / t012;
                p012 = 1 - Mathf.Exp(-p012);
                if (x < p012 * 100)
                {
                    ovca.GetComponent<GinelliOvca>().tek = true;
                    ovca.GetComponent<GinelliOvca>().hoja = false;
                }
            } else {
                float p20 = (l2 < 1e-4f) ? 1f : Mathf.Pow(dS * (1 + alpha * ustavljane) / l2, delta) / t20;
                p20 = (l2 < 1e-4f) ? 1f : 1 - Mathf.Exp(-p20);
                if (x < p20)
                {
                    ovca.GetComponent<GinelliOvca>().tek = false;
                    ovca.GetComponent<GinelliOvca>().ustavljanje = true;
                }
            }

            Vector2 beg = new Vector2(0f, 0f);   // smer bega stran od ovcarjev in preskok v tek
            foreach (GameObject ovcar in ovcarji)
            {
                Vector2 razdalja = position - new Vector2(ovcar.transform.position.x, ovcar.transform.position.z);
                if (razdalja.magnitude < rs)
                {
                    beg += razdalja.normalized * Mathf.Pow((razdalja.magnitude - rs) / rs, 2);
                    ovca.GetComponent<GinelliOvca>().tek = true;
                    ovca.GetComponent<GinelliOvca>().hoja = false;
                    ovca.GetComponent<GinelliOvca>().ustavljanje = false;
                }
            }

            // izracunaj hitrost in smer
            if (ovca.GetComponent<GinelliOvca>().tek)
            {
                speed = v2;
                foreach (GameObject o in ovce)
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
                smer = smer * 0.95f + 0.05f * vsotaSmeri.normalized;
                if (beg.magnitude > 1e-4f) smer += beg.normalized;
                smer = smer.normalized;
                smer = IzogibOgraji(position, smer);
                Vector2 step = position + Time.deltaTime * speed * smer;
                Vector3 p = Vector3.MoveTowards(transform.position, new Vector3(step.x, 0f, step.y), speed);
                GetComponent<Rigidbody>().MovePosition(p);
                ovca.transform.LookAt(p + new Vector3(smer.x, 0f, smer.y));
            }
            else if (ovca.GetComponent<GinelliOvca>().hoja)
            {
                speed = v1;
                foreach (GameObject o in ovce)   // smer hoje poravnaj s smerjo hoje bliznjih ovc
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
                smer = smer * 0.95f + 0.05f * vsotaSmeri.normalized;
                smer = smer.normalized;
                smer = IzogibOgraji(position, smer);
                Vector2 step = position + Time.deltaTime * speed * smer;
                Vector3 p = Vector3.MoveTowards(transform.position, new Vector3(step.x, 0f, step.y), speed);
                GetComponent<Rigidbody>().MovePosition(p);
                ovca.transform.LookAt(p + new Vector3(smer.x, 0f, smer.y));
            }
        }
        if (ovca.transform.position.y < -0.05f || ovca.transform.position.y > 0.1f)   // ovca ne sme leteti ali riti pod zemljo
        {
            ovca.transform.position = new Vector3(ovca.transform.position.x, 0f, ovca.transform.position.z);
            ovca.transform.forward = new Vector3(ovca.transform.forward.x, 0f, ovca.transform.forward.z);  // spotoma ji nastavi se obrnjenost tako da stoji na nogah
        }
        if (ovca.GetComponent<Rigidbody>().velocity.magnitude > 5)   // ce gre prehitro jo upocasni, da je ne izstreli
        {
            ovca.GetComponent<Rigidbody>().velocity = ovca.GetComponent<Rigidbody>().velocity.normalized * 5;
        }
    }    

    Vector2 IzogibOgraji(Vector2 lokacija, Vector2 smer)
    {
        // ce sem ograji blizje kot r se ji izognem, da ne grem proti njej prevec direktno
        float r = 4f;
        if (Mathf.Abs(lokacija.y) > 50f - r && lokacija.y * smer.y > 0f && Mathf.Abs(smer.x) < 0.9f) {
            smer.y = 0.3f * smer.y - 0.1f * Random.Range((smer.y > 0) ? 0f : -1f, (smer.y > 0) ? 1f : 0f);
            smer.x -= (smer.x > 0) ? 0.2f : -0.2f;
        }
        if (lokacija.x > 50f - r && Mathf.Abs(lokacija.y) < 20f) { }   // normalno kjer je vhod v stajo
        else if (Mathf.Abs(lokacija.x) > 50f - r && lokacija.x * smer.x > 0f && Mathf.Abs(smer.y) < 0.9f)
        {
            smer.x = 0.3f * smer.x - 0.1f * Random.Range((smer.x > 0) ? 0f : -1f, (smer.x > 0) ? 1f : 0f);
            smer.y -= (smer.y > 0) ? 0.2f : -0.2f;
        }
        if (Mathf.Abs(lokacija.y) > 50f - r && Mathf.Abs(lokacija.x) > 50f - r && lokacija.x * smer.x > 0f && lokacija.y * smer.y > 0f)
        {
            if (Mathf.Abs(smer.x) < Mathf.Abs(smer.y))
            {
                smer.x = -0.8f * smer.x + 0.1f * Random.Range((smer.x > 0) ? 0f : -1f, (smer.x > 0) ? 1f : 0f);
                smer.y -= (smer.y > 0) ? 0.3f : -0.3f;
            } else { smer.y = -0.8f * smer.y + 0.1f * Random.Range((smer.y > 0) ? 0f : -1f, (smer.y > 0) ? 1f : 0f); ; smer.x -= (smer.x > 0) ? 0.3f : -0.3f; }
        }
        return smer.normalized;
    }
}