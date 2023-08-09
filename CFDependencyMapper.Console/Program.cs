using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CFDependencyMapper.Tests")]
namespace CFDependencyMapper.Console
{
    class Program
    {
        private IFileSystem _fileSystem;

        static void Main(string[] args)
        {
            var p = new Program();
            p.Run();
        }

        private Program()
        {
            _fileSystem = new FileSystem();
        }

        private void Run()
        {
            List<string> rootPaths = GetPaths("Enter root paths to search from");

            List<string> referencePaths = GetPaths("Enter reference paths (e.g. components");

            var builder = new GraphBuilder(_fileSystem);
            foreach (string path in rootPaths)
                builder.AddRootDirectory(path);
            foreach (string path in referencePaths)
                builder.AddReferenceDirectory(path);

            Graph graph = builder.Build();

            System.Console.WriteLine("Found " + graph.Nodes.Count + " nodes.");

            System.Console.Write("Enter output file: ");
            string outputFile = System.Console.ReadLine();
            string data = graph.ToJson();
            _fileSystem.File.WriteAllText(outputFile, data);

            System.Console.WriteLine("Press Enter to Exit...");
            System.Console.ReadKey();
        }

        private List<string> GetPaths(string prompt)
        {
            var paths = new List<string>();

            string input;
            do
            {
                System.Console.Write($"{prompt} (or q to continue): ");
                input = System.Console.ReadLine();

                if (input != "q")
                {
                    if (_fileSystem.Directory.Exists(input))
                    {
                        paths.Add(input);
                    }
                    else
                    {
                        System.Console.WriteLine("Directory does not exist.");
                    }
                }

            } while (input != "q");

            return paths;
        }
    }
}
