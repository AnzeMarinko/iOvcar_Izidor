import numpy as np
import os


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
                    self.fitness[generacija] = self.fitness.get(generacija, []) + [float(param[3][:-1])]
                    self.geni[generacija] = self.geni.get(generacija, []) + [tuple(float(p) for p in param[5].split(";")[1:])]
                else:
                    generacija = "OptGen"
                    self.fitness[generacija] = self.fitness.get(generacija, []) + [float(param[1][:-1])]
                    self.geni[generacija] = self.geni.get(generacija, []) + [tuple(float(p) for p in param[3].split(";")[1:])]
                if len(cas) > 1:  # predelava casov v histograme
                    self.casi[generacija] = self.casi.get(generacija, []) + [list(np.histogram([int(t) for t in cas], maxT, (0, maxT))[0])]
                else:
                    self.casi[generacija] = self.casi.get(generacija, []) + [[0] * maxT]


folder = "../iOvcar_Izidor/iOvcar-IZIDOR/Rezultati/"  # mapa z rezultati
maxT = 180
files = [folder + subfolder + "/" + file for subfolder in os.listdir(folder) for file in os.listdir(folder + subfolder)]
data = [Poskus(file) for file in files]

# uporabljeni parametri
prop = [(poskus.ovce, poskus.novc, poskus.ovcarji, poskus.novcarjev) for poskus in data]
gins = sorted(list({p[0] for p in prop}))
novcs = sorted(list({r[1] for r in prop}))
vods = sorted(list({o[2] for o in prop}))
novcars = sorted(list({p[3] for p in prop}))
