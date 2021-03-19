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
    data = list((1 - np.cumsum(np.mean(np.array(data), 0) / int(n1))) * 100)
    return data


colors = list(TABLEAU_COLORS)  # barve za locevanje med stevili ovc in ovcarjev
lines = ["-", "--", ":"]

plt.figure(figsize=(12, 4))
for k, gin in enumerate(["Ginelli", "Stroembom", "PopravljenStroembom"]):
    plt.subplot(1, 3, k+1)
    for i1, n1 in enumerate([25, 50, 75, 100]):
        for i2, n2 in enumerate(range(1, 4)):
            vod = "Voronoi"
            podatki = prezivetvena(f"{gin}_{n1}-{vod}_{n2}.txt")
            plt.plot(podatki, linestyle=lines[i2], color=colors[i1],
                     label=f"{n1} ovc, {n2} {'pes' if n2 == 1 else 'psa' if n2 == 2 else 'psi'}")
    if k == 2:
        plt.legend(loc='best')
    if k == 0:
        plt.ylabel("Delež ovc na pašniku [%]")
    plt.xlabel("Čas [s]")
    plt.title(f"{gin}")
plt.tight_layout()
plt.savefig("../../MagistrskoDelo-pisanje/poglavja/grafi/prezivetvena-Voronoi.pdf")


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
            plt.title(f"{gin} ovc")
plt.tight_layout()
plt.savefig(f"../../MagistrskoDelo-pisanje/poglavja/grafi/geni-1-4.pdf")
plt.show()
