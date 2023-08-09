using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace CFDependencyMapper.Console
{

    public class FileSearcher
    {
        private readonly IFileSystem _fileSystem;
        private List<string> _rootDirectories;
        private List<string> _referenceDirectories;

        public FileSearcher(IFileSystem system)
        {
            _fileSystem = system;
            _rootDirectories = new List<string>();
            _referenceDirectories = new List<string>();
        }

        public FileSearcher(IFileSystem system, string webRoot)
        {
            _fileSystem = system;
            _rootDirectories = new List<string>()
            {
                webRoot
            };
            _referenceDirectories = new List<string>();
        }

        public void AddRootDirectory(string directory)
        {
            _rootDirectories.Add(directory);
        }

        public void AddRootDirectories(IEnumerable<string> directories)
        {
            _rootDirectories.AddRange(directories);
        }

        public void AddReferenceDirectories(List<string> referenceDirectories)
        {
            _referenceDirectories.AddRange(referenceDirectories);
        }

        public void AddReferenceDirectory(string directory)
        {
            _referenceDirectories.Add(directory);
        }

        private bool IsRootDirectory(string directory)
        {
            return _rootDirectories.Any(x => _fileSystem.Path.ArePathsEqual(directory, x))
                || _referenceDirectories.Any(x => _fileSystem.Path.ArePathsEqual(directory, x));
        }

        /// <summary>
        /// Climb up the directory tree until one of the following: you find an application file, you reach a configured root directory, or you reach the root of the drive.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public CodeFile GetApplicationFile(string fileName)
        {
            string directory = fileName;
            CodeFile file = null;

            do
            {
                directory = _fileSystem.Path.GetDirectoryName(directory);

                string applicationFile = _fileSystem.Path.Combine(directory, "application.cfm");
                if (_fileSystem.File.Exists(applicationFile))
                {
                    file = new CodeFile(applicationFile, _fileSystem);
                    break;
                }
            } while (_fileSystem.Path.GetDirectoryName(directory) != null && !IsRootDirectory(directory));

            return file;
        }

        public CodeFile LocateFile(string relativePath)
        {
            CodeFile file = null;

            foreach (string rootDir in _rootDirectories.Concat(_referenceDirectories))
            {
                string path = _fileSystem.Path.Combine(rootDir, relativePath);
                if (_fileSystem.File.Exists(path))
                {
                    file = new CodeFile(path, _fileSystem);
                }
            }

            return file;
        }

        private bool IgnoreDirectory(string path)
        {
            path = path.ToLower();
            //TODO have the user enter these fields
            return path.EndsWith(".git") || path.EndsWith("testbox") || path.EndsWith("testing") || path.EndsWith("testappentry");
        }

        public IEnumerable<CodeFile> GetRootFiles()
        {
            Queue<string> queue = new Queue<string>();
            foreach (string dir in _rootDirectories)
            {
                queue.Enqueue(dir);
            }
            
            while (queue.Count > 0)
            {
                string path = queue.Dequeue();
                try
                {
                    foreach (string subDir in _fileSystem.Directory.GetDirectories(path))
                    {
                        if (!IgnoreDirectory(subDir))
                        {
                            queue.Enqueue(subDir);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO log error
                }
                string[] files = null;
                try
                {
                    files = _fileSystem.Directory.GetFiles(path)
                        .Where(x => x.ToLower().EndsWith(".cfc") || x.ToLower().EndsWith(".cfm"))
                        .ToArray();
                }
                catch (Exception ex)
                {
                    //TODO log error
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return new CodeFile(files[i], _fileSystem);
                    }
                }
            }//End while files left
        }
    }
}
