import os
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.colors import TABLEAU_COLORS
from mpl_toolkits.mplot3d import axes3d


# TODO:
#  sledenje psom (izriši njihovo gibanje in koliko časa so bili v stanju zbiranja in koliko vodenja)
#  škatle z brki (min, 1-kvartil, mediana, 3-kvartil, max) za vse 4 različne funkcije
#  razlika med časom ko je pripeljal čez 80% in 100% (da se vidi ali jih hkrati pelje čez)
#  doprinos dodatnega psa (razlika v porabljenem času ipd. v primerjavi z enim psom manj


# folder = "Rezultati-novemberVoronoi/"
folder = "../iOvcar_Izidor/game/Rezultati/"  # mapa z rezultati

colors = list(TABLEAU_COLORS)  # barve za locevanje med stevili ovc in ovcarjev
markers = [".", "*", "o", "x", "+", "y", "v", "^"]
lines = ["-", "--", ":"]
maxT = 180

files = [file[:-4].split("-")[0].split("_") + file[:-4].split("-")[1].split("_") for file in os.listdir(folder)]
files = [(lastnosti[0], int(lastnosti[1]), lastnosti[2], int(lastnosti[3])) for lastnosti in files]
data = {}  # branje rezultatov
poskusi = []
bins = None
for gin0, novc0, vod0, novcar0 in files:
    casi = []
    with open(folder + f"{gin0}_{novc0}-{vod0}_{novcar0}.txt", "r") as file:
        for line in file.readlines()[2:]:
            if len(line) > 1:  # predelava casov v histograme
                casi.append(list(np.histogram([int(cas) for cas in line[:-1].split(",")], maxT, (0, maxT))[0]))
            else:
                casi.append([0] * maxT)
    poskusi.append(len(casi))
    data[(gin0, novc0, vod0, novcar0)] = np.array(casi)
print(f"\n\t{min(poskusi)}-{max(poskusi)} poskusov na kombinacijo")
plt.plot(sorted(poskusi, key=lambda x: -x))
plt.show()

# uporabljeni parametri
gins = sorted(list({p[0] for p in data.keys()}))
novcs = sorted(list({p[1] for p in data.keys()}))
vods = sorted(list({p[2] for p in data.keys()}))
novcars = sorted(list({p[3] for p in data.keys()}))


# izris grafa v odvisnosti od stevila ovc ali ovcarjev
def graf_OdOvc(f, xovc=True, title="", ylab="", up_bound=None):
    for gin in gins:
        for vod in vods:
            plt.figure()
            if xovc:  # izbira x osi
                for novcar in novcars:
                    plt.plot(np.array(novcs), np.array([f(gin, novc, vod, novcar)["mean"] for novc in novcs]),
                             "o-", color=colors[novcars.index(novcar)],
                             markersize=5, label=f"{novcar} " + vod + " ovčarjev")
            else:
                for novc in novcs:
                    plt.plot(np.array(novcars), np.array([f(gin, novc, vod, novcar)["mean"] for novcar in novcars]),
                             "o-", color=colors[novcs.index(novc)],
                             markersize=5, label=f"{novc} " + gin + " ovc")
            plt.ylabel(ylab)
            plt.xlabel(f"Število {gin + ' ovc' if xovc else vod + ' ovčarjev'}")
            plt.title(title + f", {gin + ' ovce' if not xovc else vod + ' ovčarji'}")
            plt.legend(loc="best")
            if up_bound:
                plt.ylim(-0.1, up_bound + 0.1)
    plt.show()


# izris grafa v odvisnosti od stevila ovc ali ovcarjev
def graf3D(f, title="", zlab=""):
    for stat in ["mean", "std"]:
        i = 0
        plt.figure()
        for gin in gins:
            for vod in vods:
                i += 1
                ax = plt.subplot((len(gins)*len(vods) + 1) // 2, 2, i, projection="3d")
                x, y = np.meshgrid(novcars, novcs)
                z = np.array([[f(gin, novc, vod, novcar)[stat] for novcar in novcars] for novc in novcs])
                ax.plot_surface(x, y, z)
                ax.set_ylabel(f"Število {gin} ovc")
                ax.set_xlabel(f"Število {vod} ovčarjev")
                ax.set_zlabel(zlab)
                plt.title(title + f", {stat}")
    plt.show()


# izris grafa v odvisnosti od casa
def cas3D(f, title="", zlab=""):
    for gin in gins:
        for vod in vods:
            plt.figure()
            i = 0
            for novcar in novcars:
                i += 1
                ax = plt.subplot((len(novcars) + 2) // 3, 3, i, projection="3d")
                t, y = np.meshgrid(range(maxT), novcs)
                z = np.array([f(gin, novc, vod, novcar)["mean"] for novc in novcs])
                ax.plot_surface(t, y, z)
                ax.set_ylabel(f"Število {gin} ovc")
                ax.set_xlabel(f"Čas [s]")
                ax.set_zlabel(zlab)
                plt.title(title + f", {novcar} {vod}")
            plt.figure()
            i = 0
            for novc in novcs:
                i += 1
                ax = plt.subplot((len(novcs) + 2) // 3, 3, i, projection="3d")
                t, y = np.meshgrid(range(maxT), novcars)
                z = np.array([f(gin, novc, vod, novcar)["mean"] for novcar in novcars])
                ax.plot_surface(t, y, z)
                ax.set_ylabel(f"Število {vod} ovčarjev")
                ax.set_xlabel(f"Čas [s]")
                ax.set_zlabel(zlab)
                plt.title(title + f", {novc} {gin}")
            plt.show()


# izris grafa v odvisnosti od casa
def graf_OdCasa(f, title="", ylab="", up_bound=None):
    for gin in gins:
        for vod in vods:
            plt.figure()
            for novc in novcs:
                for novcar in novcars:
                    plt.plot(f(gin, novc, vod, novcar)["mean"],
                             markers[novcs.index(novc)] + "-", color=colors[novcars.index(novcar)],
                             markersize=2, label=f"{novc} " + gin + f" ovc, {novcar} " + vod + " ovčarjev")
            plt.ylabel(ylab)
            plt.xlabel(f"Čas [s]")
            plt.title(title + f", {gin}-{vod}")
            plt.legend(novcars)
            if up_bound:
                plt.ylim(-0.1, up_bound + 0.1)
    plt.show()


def prop(d):
    ax = 0
    return {"mean": np.mean(d, ax), "std": np.std(d, axis=ax),
            "q0": np.min(d, ax), "q1": np.quantile(d, 0.25, axis=ax),
            "q2": np.median(d, ax), "q3": np.quantile(d, 0.75, axis=ax),
            "q4": np.max(d, ax)}

# =================================================================================


# funkcije za izris
def delez_ovc(g, n1, v, n2):
    d = 1 - np.cumsum(data[(g, n1, v, n2)], 1) / n1
    return prop(d)


# cas3D(delez_ovc, "Povp. % ovc na travniku")
graf_OdCasa(delez_ovc, "Povprečen delež ovc na travniku", up_bound=1)


def cas_simulacije(p):
    def aux(g, n1, v, n2):
        d = data[(g, n1, v, n2)]
        n = round(n1 * p)
        d = np.where(np.sum(d, 1) >= n, np.argmax(np.where(np.cumsum(d, 1) >= n, 1, 0), 1), maxT)
        return prop(d)
    return aux


graf3D(cas_simulacije(1), f"Povp. čas ({100} % ovc)")
for delez_ovc in [0.8, 1]:
    graf_OdOvc(cas_simulacije(delez_ovc), True,
               f"Povp. čas simulacije do pripeljanih {delez_ovc * 100} % ovc", up_bound=maxT)
    graf_OdOvc(cas_simulacije(delez_ovc), False,
               f"Povp. čas simulacije do pripeljanih {delez_ovc * 100} % ovc", up_bound=maxT)


def uspesnost(p):
    def aux(g, n1, v, n2):
        d = data[(g, n1, v, n2)]
        d = np.sum(d[:, :round(p*d.shape[1])], 1) / n1
        return prop(d)
    return aux


graf3D(uspesnost(1), f"Povp. uspešnost v {round(maxT)} s")
for delez_casa in [0.333, 0.5, 1]:
    graf_OdOvc(uspesnost(delez_casa), True, f"Povprečna uspešnost v {round(delez_casa * maxT)} s", up_bound=1)
    graf_OdOvc(uspesnost(delez_casa), False, f"Povprečna uspešnost v {round(delez_casa * maxT)} s", up_bound=1)


def uspeh_simulacije(g, n1, v, n2):
    d = np.where(np.sum(data[(g, n1, v, n2)], 1) == n1, 1, 0)
    return prop(d)


graf3D(uspeh_simulacije, "Povp. % uspešnih simulacij")
graf_OdOvc(uspeh_simulacije, True, "Povprečen delež uspešnih simulacij", up_bound=1)
graf_OdOvc(uspeh_simulacije, False, "Povprečen delež uspešnih simulacij", up_bound=1)

