import numpy as np
import os


folder = "../iOvcar_Izidor/iOvcar-IZIDOR/Rezultati/Rezultati-Voronoi/"  # mapa z rezultati
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
print(sorted(poskusi, key=lambda x: -x))

# uporabljeni parametri
gins = sorted(list({p[0] for p in data.keys()}))
novcs = sorted(list({p[1] for p in data.keys()}))
vods = sorted(list({p[2] for p in data.keys()}))
novcars = sorted(list({p[3] for p in data.keys()}))
