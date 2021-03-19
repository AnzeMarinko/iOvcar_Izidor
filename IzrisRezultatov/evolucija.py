import os
import matplotlib.pyplot as plt
import numpy as np

folderG = "Rezultati/Rezultati-AI1/"
folderV = "Rezultati/Rezultati-Voronoi-Final/"


def evolucija(filename):
    data = filename.split(".")[0].split("-")
    (gin, n1) = data[0].split("_")
    (vod, n2) = data[1].split("_")
    data = []
    auxdata = None
    with open(folderG + filename, "r") as f:
        for line in f.readlines()[2:-1]:
            if "Generacija" in line:
                d = line[:-1].split(", ")
                d = [i.split(" ")[1].replace(",", ".") for i in d]
                generacija = int(d[0])
                fit = float(d[-2])
                gen = tuple(float(i) for i in d[-1].split(";")[1:])
                auxdata = [generacija, [fit], gen, None]
            elif auxdata:
                d = [int(i) for i in line[:-1].split(",")[1:]]
                da = [d + [180] * int(n1)][:(int(n1))]
                auxdata[-1] = (np.max(d) if len(d) > 0 else 180, np.mean(da), len(d) / int(n1))
                data.append(tuple(auxdata))
                auxdata = None
    dataV = []
    auxdataV = None
    with open(folderV + filename.replace("AI1", "Voronoi"), "r") as f:
        for line in f.readlines()[2:-1]:
            if "Gen" in line:
                d = line[:-1].split(", ")
                d = [i.split(" ")[1].replace(",", ".") for i in d]
                fit = float(d[0])
                gen = [float(i) for i in d[1].split(";")[1:]]
                auxdataV = [fit] + gen
            elif auxdataV:
                d = [int(i) for i in line[:-1].split(",")[1:]]
                da = [d + [180] * int(n1)][:(int(n1))]
                auxdataV += [np.max(d) if len(d) > 0 else 180, np.mean(da), len(d) / int(n1)]
                dataV.append([auxdataV])
                auxdataV = None
    vm = np.median(np.array(dataV)[:, 0, :], 0)
    return gin, int(n1), vod, int(n2), data, vm


def draw(x, y):
    M, m1, m0, s1, s2 = [], [], [], [], []
    plt.plot([max(x)-2.5, max(x)-2.5], [0, 180], color="tab:orange", alpha=0.5)
    for g in set(x):
        f = y[np.argwhere(x == g)]
        M.append(np.max(f))
        m1.append(np.median(f))
        m0.append(np.min(f))
        s1.append(np.percentile(f, 15))
        s2.append(np.percentile(f, 85))
        plt.plot([g] * (len(f) // 2), f[:len(f) // 2], "o", color="tab:blue", markersize=0.5, alpha=0.3)
    ax = sorted(list(set(x)))
    plt.fill_between(ax, s1, s2, color='royalblue', alpha=0.2)
    plt.fill_between(ax, M, m0, color='royalblue', alpha=0.1)
    plt.plot(ax, M, color="tab:red", alpha=0.9)
    plt.plot(ax, m0, color="tab:red", alpha=0.9)
    plt.plot(ax, m1, color="tab:blue")


podatki = sorted([evolucija(file) for file in os.listdir(folderG) if ("25" in file or "50" in file or "100" in file)
                  and "Popravljen" not in file])


def figs(i, j, ylim, title, v):
    j = min(len(podatki[0][4][i])-1, j)
    for k, (ginf, n1f, vodf, n2f, data, vm) in enumerate(podatki):
        if k % 12 == 0:
            plt.figure(figsize=(10, 7))
        plt.subplot(3, 4, k % 12+1)
        generacije = np.array([g[0] for g in data])
        fits = np.array([g[i][j] for g in data])   # [1] ... fits, [2][i] ... gen i, [3][i] ... time stat
        draw(generacije, fits)
        plt.plot(generacije, generacije*0 + vm[v], color="tab:green", alpha=0.7, linewidth=2)
        plt.ylim((0, ylim))   # (0, 120) if fits
        if n1f == 25:
            plt.title(str(n2f) + " ovčar" + ("" if n2f == 1 else "ja" if n2f == 2 else "ji"))
        if n2f == 1:
            plt.ylabel(str(n1f) + " " + ginf + " ovc", size='large')
        if k % 12 == 11:
            plt.tight_layout()
            plt.savefig("../../MagistrskoDelo-pisanje/poglavja/grafi/" + ginf + "-evolucija-" + title + ".pdf")


figs(1, 0, 110, "Fitness", 0)
figs(2, 0, 1, "Gen1", 1)
figs(2, 1, 1, "Gen2", 2)
figs(3, 0, 180, "MaxT", -3)
figs(3, 1, 180, "MeanT", -2)
figs(3, 2, 1, "Uspeh", -1)
