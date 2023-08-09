using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;

namespace CFDependencyMapper.Console
{
    public class GraphBuilder
    {
        private readonly IFileSystem _fileSystem;
        private List<string> _referenceDirectories;
        private List<string> _rootDirectories;

        public GraphBuilder(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            _referenceDirectories = new List<string>();
            _rootDirectories = new List<string>();
        }

        public GraphBuilder AddReferenceDirectory(string path)
        {
            _referenceDirectories.Add(path);

            return this;
        }

        public GraphBuilder AddRootDirectory(string path)
        {
            _rootDirectories.Add(path);

            return this;
        }

        public Graph Build()
        {
            var rootSearcher = new FileSearcher(_fileSystem);
            rootSearcher.AddRootDirectories(_rootDirectories);
            rootSearcher.AddReferenceDirectories(_referenceDirectories);

            var filesToProcess = new Queue<Node>();
            var processedNodes = new HashSet<Node>();

            var graph = new Graph();

            IEnumerable<CodeFile> files = rootSearcher.GetRootFiles();
            foreach (CodeFile file in files)
            {
                var node = new Node(file);
                graph.Nodes.Add(node);
                filesToProcess.Enqueue(node);
            }

            // Begin building reference tree
            while (filesToProcess.Count > 0)
            {
                Node node = filesToProcess.Dequeue();
                if (!processedNodes.Contains(node) && node.CodeFile.Exists)
                {
                    List<CodeFile> references = node.CodeFile.GetReferences(rootSearcher);
                    foreach (var reference in references)
                    {
                        // Construct a Node object to search from the list, but actually use the found one instead to update by reference
                        var refNode = new Node(reference);
                        if (!graph.Nodes.TryGetValue(refNode, out Node foundNode))
                        {
                            // The file being referenced is in a reference folder, not a root folder.
                            graph.Nodes.Add(refNode);
                            filesToProcess.Enqueue(refNode);
                            foundNode = refNode;
                        }
                        node.Edges.Add(foundNode);
                    }
                    processedNodes.Add(node);
                }
            }

            return graph;
        }
    }
}
