import numpy as np
import os
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import axes3d


class Poskus:
    def __init__(self, filename):
        self.filename = filename
        self.final, self.ovce, self.novc, self.ovcarji, self.novcarjev = None, None, None, None, None
        self.file2prop()
        self.casi, self.geni, self.fitness = None, None, None
        self.preberi()

    def file2prop(self):
        file = self.filename.split("/")[-1]
        prop = file[:-4].split("-")[0].split("_") + file[:-4].split("-")[1].split("_")
        self.final = "Final" in self.filename
        self.ovce = prop[0]
        self.novc = int(prop[1])
        self.ovcarji = prop[2]
        self.novcarjev = int(prop[3])

    def preberi(self):
        self.casi = {}
        self.fitness = {}
        self.geni = {}
        with open(self.filename, "r") as g:
            lines = g.readlines()[2:]
            for i in range(len(lines) // 2):
                param = lines[i * 2][:-1].split(" ")
                cas = lines[i * 2 + 1][:-1].split(",")[1:]
                if len(param) == 6:
                    generacija = int(param[1][:-1])
                    self.fitness[generacija] = self.fitness.get(generacija, []) + [float(param[3][:-1].replace(',', '.'))]
                    self.geni[generacija] = self.geni.get(generacija, []) + [tuple(float(p.replace(',', '.')) for p in param[5].split(";")[1:])]
                else:
                    generacija = "OptGen"
                    self.fitness[generacija] = self.fitness.get(generacija, []) + [float(param[1][:-1].replace(',', '.'))]
                    self.geni[generacija] = self.geni.get(generacija, []) + [tuple(float(p.replace(',', '.')) for p in param[3].split(";")[1:])]
                self.casi[generacija] = self.casi.get(generacija, []) + [list(np.histogram([int(t) for t in cas], maxT, (0, maxT))[0])]


folder = "Rezultati/"  # "../iOvcar_Izidor/iOvcar_IZIDOR/Rezultati/"  # mapa z rezultati
maxT = 240
files = [folder + subfolder + "/" + file for subfolder in os.listdir(folder)
         if ".txt" not in subfolder for file in os.listdir(folder + subfolder)]
data = {(d.final, d.ovce, d.novc, d.ovcarji, d.novcarjev): (d.casi, d.geni, d.fitness) for d in
        [Poskus(file) for file in files]}
# uporabljeni parametri
gins = sorted(list({p[1] for p in data.keys()}))
novcs = sorted(list({r[2] for r in data.keys()}))
vods = sorted(list({o[3] for o in data.keys()}))
novcars = sorted(list({p[4] for p in data.keys()}))


for i in range(len(data[(True, "Ginelli", 5, "AI1", 1)][1]["OptGen"][0])):
    x, y = np.meshgrid(novcars, novcs)
    z = np.array([[data.get((True, "Ginelli", n1, "AI1", n2),
                            (0, {"OptGen": [[0] * (i+1)]}))[1]["OptGen"][0][i] for n2 in novcars] for n1 in novcs])
    ax = plt.subplot(1, 1, 1, projection="3d")
    ax.plot_surface(x, y, z)
    ax.set_ylabel(f"Število ovc")
    ax.set_xlabel(f"Število ovčarjev")
    ax.set_zlabel(i)
    plt.show()
