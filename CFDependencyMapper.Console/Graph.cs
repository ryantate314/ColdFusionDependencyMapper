using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CFDependencyMapper.Console
{
    class Graph
    {
        public HashSet<Node> Nodes { get; private set; }

        public Graph()
        {
            Nodes = new HashSet<Node>();
        }


        public string ToJson()
        {
            var dict = new Dictionary<string, List<string>>();
            foreach (var node in Nodes)
            {
                dict[node.CodeFile.FileName] = node.Edges.Select(x => x.CodeFile.FileName).ToList();
            }
            return JsonConvert.SerializeObject(dict);
        }
    }

    class Node
    {
        public CodeFile CodeFile { get; private set; }
        public HashSet<Node> Edges { get; private set; }

        public Node(CodeFile file)
        {
            CodeFile = file;
            Edges = new HashSet<Node>();
        }

        public override bool Equals(object obj)
        {
            var node = (Node)obj;
            return CodeFile.Equals(node.CodeFile);
        }

        public override int GetHashCode()
        {
            return CodeFile.GetHashCode();
        }

        public override string ToString()
        {
            return CodeFile.ToString();
        }


    }
}
