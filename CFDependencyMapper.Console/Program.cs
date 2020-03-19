using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CFDependencyMapper.Tests")]
namespace CFDependencyMapper.Console
{
    class Program
    {

        private Graph _graph;
        private Queue<Node> _filesToProcess;
        private HashSet<Node> _processedNodes;
        

        static void Main(string[] args)
        {
            var p = new Program();
            p.Run();
        }

        private Program()
        {
            _graph = new Graph();
            _filesToProcess = new Queue<Node>();
            _processedNodes = new HashSet<Node>();
        }

        private void Run()
        {
            var fileSystem = new FileSystem();
            
            string webRoot = "";
            do
            {
                System.Console.Write("Enter the Web Root directory: ");
                webRoot = System.Console.ReadLine();
                if (!fileSystem.Directory.Exists(webRoot))
                {
                    System.Console.WriteLine("Directory does not exist.");
                }
            } while (!fileSystem.Directory.Exists(webRoot));

            var searcher = new FileSearcher(fileSystem, webRoot);

            string input = "";
            do
            {
                System.Console.Write("Enter a root directory, or q to quit: ");
                input = System.Console.ReadLine();

                if (input != "q")
                {
                    if (fileSystem.Directory.Exists(input))
                    {
                        searcher.AddRootDirectory(input);
                    }
                    else
                    {
                        System.Console.WriteLine("Directory does not exist.");
                    }
                }

            } while (input != "q");

            IEnumerable<CodeFile> files = searcher.GetAllFiles();
            foreach (CodeFile file in files)
            {
                var node = new Node(file);
                _graph.Nodes.Add(node);
                _filesToProcess.Enqueue(node);
            }

            while (_filesToProcess.Count > 0)
            {
                Node node = _filesToProcess.Dequeue();
                if (!_processedNodes.Contains(node) && node.CodeFile.Exists)
                {
                    var references = node.CodeFile.GetReferences(searcher);
                    foreach (var reference in references)
                    {
                        var refNode = new Node(reference);
                        Node foundNode;
                        if (_graph.Nodes.TryGetValue(refNode, out foundNode))
                        {
                            node.Edges.Add(foundNode);
                        }
                        else
                        {
                            System.Console.Error.WriteLine("Unknown file " + refNode.CodeFile.FileName);
                        }

                    }
                    _processedNodes.Add(node);
                }
            }

            System.Console.WriteLine("Found " + _graph.Nodes.Count + " nodes.");

            System.Console.Write("Enter output file: ");
            string outputFile = System.Console.ReadLine();
            string data = _graph.ToJson();
            fileSystem.File.WriteAllText(outputFile, data);

            System.Console.WriteLine("Press Enter to Exit...");
            System.Console.ReadKey();
        }
    }
}
