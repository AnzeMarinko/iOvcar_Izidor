import matplotlib.pyplot as plt
import numpy as np
from matplotlib._color_data import TABLEAU_COLORS

folderV = "Rezultati/Rezultati-Voronoi-Final/"


def prezivetvena(filename):
    data = filename.split(".")[0].split("-")
    (_, n1) = data[0].split("_")
    data = []
    with open(folderV + filename, "r") as f:
        for line in f.readlines()[2:-1]:
            if "Gen" not in line:
                d = [int(i) for i in line[:-1].split(",")[1:]]
                da = [d + [181] * int(n1)][:(int(n1))]
                data.append(list(np.histogram(da, 180, (0, 180))[0]))
    data = 1 - np.cumsum(np.array(data) / int(n1), 1)
    data1 = list(np.mean(np.array(data), 0) * 100)
    data2 = list(np.percentile(np.array(data), 97.5, axis=0) * 100)
    data3 = list(np.percentile(np.array(data), 2.5, axis=0) * 100)
    return data1, data2, data3


colors = list(TABLEAU_COLORS)  # barve za locevanje med stevili ovc in ovcarjev
lines = ["-", "--", ":"]
n1s = [25, 50, 75, 100]
n2s = list(range(1, 4))

plt.figure(figsize=(9, 10))
for k, gin in enumerate(["Ginelli", "Stroembom", "PopravljenStroembom"]):
    for i1, n1 in enumerate(n1s):
        for i2, n2 in enumerate(n2s):
            plt.subplot(len(n1s), 3, 3 * i1 + k + 1)
            vod = "Voronoi"
            podatki, spodaj, zgoraj = prezivetvena(f"{gin}_{n1}-{vod}_{n2}.txt")
            plt.plot(podatki, color=colors[i2],
                     label=f"Povprečje, {n2} {'pes' if n2 == 1 else 'psa' if n2 == 2 else 'psi'}")
            plt.fill_between([*range(180)], spodaj, zgoraj, color=colors[i2], alpha=0.15, label=f"95 % IZ, {n2} {'pes' if n2 == 1 else 'psa' if n2 == 2 else 'psi'}")
            plt.plot(spodaj, color=colors[i2], alpha=0.15)
            plt.plot(zgoraj, color=colors[i2], alpha=0.15)
            if k == 0:
                plt.ylabel(f"Delež ovc na pašniku\nod začetnih {n1} [%]")
            if i1 == 0:
                plt.title(f"{gin}")
        if k == 2 and i1 == 2:
            plt.legend(loc='best')
    plt.xlabel("Čas [s]")
plt.tight_layout()
plt.savefig("../MagistrskoDelo-pisanje/poglavja/grafi/prezivetvena-Voronoi.png", dpi=300)
plt.show()


def geni():
    podatki = []
    opt = {}
    with open("Rezultati/geni.txt", "r", encoding='utf-8') as f:
        for line in f.readlines():
            if "(" in line:
                text, meje = line[:-2].replace(",", ".").split(": (")
                m, M = meje.split(" - ")
                M, u = M.split(" ")
                podatki.append((text, int(m), float(M), u))
            elif len(line) > 3:
                gin, n1, vod, n2, gen = line.replace(",", ".").split(" ")
                g = [float(i) for i in gen.split(";")[:-1]]
                opt[(gin, int(n1), vod, int(n2))] = g
        return podatki, opt


data, geni = geni()
n1s = [5, 10, 25, 50, 75, 100]

plt.figure(figsize=(9, 9))
for g in range(4):
    d = data[g][2] - data[g][1]
    for k, gin in enumerate(["Ginelli", "Stroembom", "PopravljenStroembom"]):
        plt.subplot(4, 3, g * 3 + k+1)
        for i2, n2 in enumerate(range(1, 4)):
            g1 = [geni[(gin, n1, "AI1", n2)][g] * d + data[g][1] for n1 in n1s]
            plt.plot(n1s, g1, color=colors[i2], label=f"{n2} {'pes' if n2 == 1 else 'psa' if n2 == 2 else 'psi'}")
        gv = [geni[(gin, n1, "Voronoi", 1)][g] * d + data[g][1] for n1 in n1s]
        plt.plot(n1s, gv, '--', color="tab:red", label="Ročno razvit")
        if k == 0:
            ime = ["$v_{min}$", "$r_a$", "$d_c$", "$d_a$"][g]
            plt.ylabel(ime + ("" if len(data[g][3]) == 0 else f" [{data[g][3]}]"), size="large")
        if g == 3:
            plt.xlabel("Število ovc")
        plt.ylim((data[g][1], data[g][2]))
        if g == 2 and k == 0:
            plt.legend(loc='best')
        if g == 0:
            plt.title(gin.replace("S", " S"))
plt.tight_layout()
plt.savefig(f"../MagistrskoDelo-pisanje/poglavja/grafi/geni-1-4.pdf")
plt.show()
