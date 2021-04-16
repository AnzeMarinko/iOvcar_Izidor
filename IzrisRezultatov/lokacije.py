import os
import matplotlib.pyplot as plt

folder = "Rezultati/lokacije/"


def lokacije(filename):
    data = filename.split("-")[:2]
    (gin, n1) = data[0].split("_")
    (vod, n2) = data[1].split("_")
    xs0 = []
    ys0 = []
    xs1 = []
    ys1 = []
    with open(folder + filename, "r") as f:
        for line in f.readlines():
            if len(line) > 2:
                d = [float(i) for i in line[:-2].replace(",", ".").split(";")]
                if d[1] == 0:
                    if d[0] == 0:
                        xs0.append(d[2:])
                    else:
                        xs1.append(d[2:])
                else:
                    if d[0] == 0:
                        ys0.append(d[2:])
                    else:
                        ys1.append(d[2:])
        return gin, n1, vod, n2, xs0, ys0, xs1, ys1


plt.figure(figsize=(10, 10), tight_layout=True)
for k, file in enumerate(os.listdir(folder)[:12]):
    ginf, n1f, vodf, n2f, xs0f, ys0f, xs1f, ys1f = lokacije(file)
    print(file)
    plt.subplot(4, 3, k+1)
    for i, x in enumerate(xs0f):
        plt.plot(x[2:], ys0f[i][2:], color="tab:blue", alpha=0.4)
    for i, x in enumerate(xs1f):
        plt.plot(x[2:], ys1f[i][2:], color="tab:red")
    plt.plot([151, 151], [-80, 80], color="tab:brown")
    plt.plot([151, 151], [-50, 50], color="tab:orange", linewidth=4)
    plt.title(n1f + " " + ginf + " ovc, " + n2f + " ovčar" + ("" if int(n2f) == 1 else "ja" if int(n2f) == 2 else "ji"))
    plt.axis('equal')
    plt.axis('off')
plt.savefig("../MagistrskoDelo-pisanje/poglavja/grafi/lokacijeAI1.png", dpi=300)
plt.show()
