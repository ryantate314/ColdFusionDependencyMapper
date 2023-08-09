import matplotlib.pyplot as plt
import matplotlib.lines as mlines
import networkx as nx
import json
import math
import argparse


def getFileLabel(name, redact):
    label = name.split("\\")[-1] #Get local file name

    if (redact):
        label = ""
    
    return label


applicationsColor = "#4F7CAC"
adminColor = "#FF8360"
scheduledJobsColor = "#C1666B"
componentColor = "#4A9950"
webServiceColor = "#D4B483"
miscColor = "#878787"
# Color the nodes based on the submodule
def getColor(name):
    color = miscColor
    if ("\\applications\\" in node.lower()):
        color = applicationsColor
    elif ("\\admin\\" in node.lower()):
        color = adminColor
    elif ("\\scheduledjobs\\" in node.lower()):
        color = scheduledJobsColor
    elif ("\\webservices\\" in node.lower()):
        color = webServiceColor
    elif (node.lower().endswith(".cfc")):
        color = componentColor
    else:
        print("   Outlier Color: " + node)
    return color

parser = argparse.ArgumentParser()
parser.add_argument("dataFile")
parser.add_argument("title")
parser.add_argument("--seed", "-s", type=int, help="Controls the node positions.")
parser.add_argument("--redact", action="store_true", default=False, help="Omit file name labels.")
parser.add_argument("--distance", "-d", type=float, default=4, help="Multiplier to control the ideal distance between nodes in the graph.")
parser.add_argument("--labelPercentile", "-l", type=float, default=0.03, help="The top n percentile of nodes which display the file name on the graph.")
args = parser.parse_args()

graph = nx.DiGraph()

#Parse File
with open(args.dataFile) as f:
    data = json.load(f)

for fileName, references in data.items():
    for reference in references:
        graph.add_edge(fileName, reference)


#Classify the files by type and set node colors
print("Adding color...")
colors = []
for node in graph.nodes():
    colors.append(getColor(node))

#Get the top n percentile of nodes, based on how many times they are referenced. We will give these labels.
print("Finding most important nodes...")
centrality = nx.algorithms.centrality.in_degree_centrality(graph)
sortedNodes = sorted(centrality.items(), key=lambda x: x[1], reverse=True)
percentage = int(math.ceil(len(sortedNodes) * args.labelPercentile))
labels = {}
for i in range(0, percentage):
    labels[sortedNodes[i][0]] = getFileLabel(sortedNodes[i][0], args.redact)


#Execute positioning algorithm
print("Positioning nodes...")
k = args.distance / math.sqrt(graph.number_of_nodes()) #The ideal distance apart. 1/sqrt(n) is the default
pos = nx.spring_layout(graph,
                       k=k,
                       seed=args.seed)


# Draw Graph
print("Drawing network...")

nodeSize = 75
nodeShape = "D"

plt.figure(figsize=(50, 50))
plt.title(args.title, fontsize=50)

#Build Legend
legendProps = {
    "marker":  nodeShape,
    "markersize": 30,
    "linestyle": ""
}
appLegendEntry = mlines.Line2D([], [],
                                color=applicationsColor,
                                label="App Module Web Pages",
                                **legendProps)
adminLegendEntry = mlines.Line2D([], [],
                                color=adminColor,
                                label="Admin Web Pages",
                                **legendProps)
compLegendEntry = mlines.Line2D([], [],
                                color=componentColor,
                                label="Class Files",
                                **legendProps)
wsLegendEntry = mlines.Line2D([], [],
                                color=webServiceColor,
                                label="Web Services",
                                **legendProps)
sjLegendEntry = mlines.Line2D([], [],
                                color=scheduledJobsColor,
                                label="Scheduled Jobs",
                                **legendProps)
webLegendEntry = mlines.Line2D([], [],
                                color=miscColor,
                                label="Misc Web Pages",
                                **legendProps)
plt.legend(handles=[appLegendEntry, adminLegendEntry, compLegendEntry, wsLegendEntry, sjLegendEntry, webLegendEntry],
            fontsize=30)

#Draw the graph
nx.draw_networkx_edges(graph,
                    pos=pos,
                    alpha=0.3,
                    width=0.2,
                    arrowsize=7,
                    # linewidths=0,
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