import matplotlib.pyplot as plt
import matplotlib.lines as mlines
import networkx as nx
import json
import sys
import math

if (len(sys.argv) < 4):
    print("Usage: py " + sys.argv[0] + " {dataFile} {seed} {graphTitle}")
    exit()

_,dataFile,seed,title = sys.argv

seed = int(seed)

graph = nx.DiGraph()

#Parse File
with open(dataFile) as f:
    data = json.load(f)

for fileName, references in data.items():
    for reference in references:
        graph.add_edge(fileName, reference)


#Classify the files by type and set node colors
print("Adding color...")
cfmColor = "#77A6B6"
componentColor = "#4F7CAC"
webServiceColor = "#593F62"
applicationColor = "#49A078"

colors = []
for node in graph.nodes():
    color = "white"
    if (node.lower().endswith("application.cfm")):
        color = applicationColor
    elif (node.lower().endswith(".cfm")):
        color = cfmColor
    elif ("webservices" in node.lower()):
        color = webServiceColor
    elif (node.lower().endswith(".cfc")):
        color = componentColor
    else:
        print("   Outlier: " + node)
    colors.append(color)

#Get the top n percentile of nodes, based on how many times they are referenced. We will give these labels.
print("Finding most important nodes...")
centrality = nx.algorithms.centrality.in_degree_centrality(graph)
sortedNodes = sorted(centrality.items(), key=lambda x: x[1], reverse=True)
percentage = int(math.ceil(len(sortedNodes) * 0.05))
labels = {}
for i in range(0, percentage):
    labels[sortedNodes[i][0]] = sortedNodes[i][0].split("\\")[-1]


#Execute positioning algorithm
print("Positioning nodes...")
k = 4 / math.sqrt(graph.number_of_nodes()) #The ideal distance apart. 1/sqrt(n) is the default
pos = nx.spring_layout(graph,
                        k=k,
                        seed=seed)


# Draw Graph
print("Drawing network...")

nodeSize = 75
nodeShape = "D"

plt.figure(figsize=(50, 50))
plt.title(title, fontsize=50)

#Build Legend
legendProps = {
    "marker": nodeShape,
    "markersize": 30,
    "linestyle": ""
}
appLegendEntry = mlines.Line2D([], [],
                                color=applicationColor,
                                label="Application Files",
                                **legendProps)
compLegendEntry = mlines.Line2D([], [],
                                color=componentColor,
                                label="Components",
                                **legendProps)
wsLegendEntry = mlines.Line2D([], [],
                                color=webServiceColor,
                                label="Web Services",
                                **legendProps)
webLegendEntry = mlines.Line2D([], [],
                                color=cfmColor,
                                label="Web Pages",
                                **legendProps)
plt.legend(handles=[appLegendEntry, compLegendEntry, wsLegendEntry, webLegendEntry],
            fontsize=30)

#Draw the graph
nx.draw_networkx_edges(graph,
                    pos=pos,
                    alpha=0.3,
                    width=0.2,
                    arrowsize=7,
                    linewidths=0,
                    node_size=nodeSize,
                    node_shape=nodeShape)
nx.draw_networkx_nodes(graph,
                pos=pos,
                node_size=nodeSize,
                node_shape=nodeShape,
                node_color=colors
                )
nx.draw_networkx_labels(graph,
                        pos=pos,
                        labels=labels)
plt.savefig("graph.png")
#plt.show()