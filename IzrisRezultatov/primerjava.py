import os
import matplotlib.pyplot as plt
import numpy as np
from matplotlib._color_data import TABLEAU_COLORS

n1s = [1, 5, 10, 25, 50, 75, 100, 125, 150, 200]
n2s = [1, 2, 3, 4, 5]
gins = ["Ginelli", "Stroembom", "PopravljenStroembom"]
vods = ["Voronoi", "AI1", "AI2"]
maxT = 180

results = {}
for gin in gins:
    for n1 in n1s:
        for vod in vods:
            for n2 in n2s:
                if "2" in vod and (n2 > 1 or "Str" in gin):
                    continue
                file = f"Rezultati/Rezultati-{vod}-Final/{gin}_{n1}-{vod}_{n2}.txt"
                konec = []
                with open(file, "r") as f:
                    for line in f.readlines()[4:-1]:
                        if "Gen" not in line:
                            d = [int(i) for i in line[:-1].split(",")[1:]]
                            konec.append([len([1 for i in d if i <= 60]) / n1 * 100,
                                          100 if len(d) == n1 else 0,
                                          len(d) / n1 * 100,
                                          d[-1] if len(d) == n1 else maxT])
                results[(gin, n1, vod, n2)] = list(np.mean(np.array(konec), 0))
lastnost = [("Povp. delež ovc v staji v 1 min [%]", "povp60s"),
            ("Delež simulacij zaključenih v 3 min [%]", "zakljucene"),
            ("Povp. delež ovc v staji v 3 min [%]", "povp180s"),
            ("Povp. čas simulacije [s]", "cas")]

colors = list(TABLEAU_COLORS)  # barve za locevanje med stevili ovc in ovcarjev
lines = ["-", "--", "-.", ":", "-"]


def draw(i, ai2=False):
    plt.figure(figsize=(10 if ai2 else 10, 4))
    for k, gin in enumerate(range(4) if ai2 else gins):
        gin = "Ginelli" if ai2 else gin
        plt.subplot(1, 4 if ai2 else 3, k+1)
        for v, vod in enumerate(vods):
            for i2, n2 in enumerate(n2s[:1] if ai2 else n2s):
                if "2" in vod and (n2 > 1 or "Str" in gin):
                    continue
                podatki = [results[(gin, n1, vod, n2)][k if ai2 else i] for n1 in n1s]
                plt.plot(n1s, podatki, "." + lines[i2], markersize=5, linewidth=np.sqrt(1 + i2), color=colors[v],
                         label=f"{n2} {'pes' if n2 == 1 else 'psa' if n2 == 2 else 'psi' if n2 < 5 else 'psov'} - {vod}")
        if k == (0 if ai2 else 2):
            plt.legend(loc='best')
        if ai2 or k == 0:
            plt.ylabel(lastnost[k if ai2 else i][0])
        plt.xlabel("Število ovc na začetku simulacije")
        plt.ylim((0, 180 if i == 3 or (k==3 and ai2) else 100))
        plt.title(f"{gin}")
    plt.tight_layout()
    plt.savefig(f"../../MagistrskoDelo-pisanje/poglavja/grafi/{lastnost[i][1] if not ai2 else '1-Ginelli'}.pdf")


draw(0, True)
for i in range(len(lastnost)):
    draw(i)
