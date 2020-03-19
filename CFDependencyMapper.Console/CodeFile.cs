using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CFDependencyMapper.Console
{
    class CodeFile
    {
        public string FileName { get; private set; }
        private string _normalizedFileName;
        protected IFileSystem FileSystem { get; private set; }
        private Lazy<bool> _exists;
        public bool Exists
        {
            get
            {
                return _exists.Value;
            }
        }

        public CodeFile(string fileName, IFileSystem fileSystem)
        {
            FileName = fileName;
            _normalizedFileName = fileSystem.Path.NormalizePath(fileName);
            FileSystem = fileSystem;
            _exists = new Lazy<bool>(() => FileSystem.File.Exists(FileName));
        }

        public override bool Equals(object obj)
        {
            var castObj = (CodeFile)obj;
            return FileSystem.Path.ArePathsEqual(_normalizedFileName, castObj.FileName);
        }

        private string GetContents()
        {
            return FileSystem.File.ReadAllText(FileName);
        }

        private bool ExcludeComponent(string componentPath)
        {
            return componentPath.ToLower() == "query";
        }

        private List<ComponentReference> GetComponentReferences(string fileContents)
        {
            Regex createObjectRegex = new Regex(@"[cC][rR][eE][aA][tT][eE][oO][bB][jJ][eE][cC][tT]\s*\(\s*['""][cC][oO][mM][pP][oO][nN][eE][nN][tT]['""]\s*,\s*['""]([^'""]+)['""]\s*\)");
            MatchCollection createObjectMatches = createObjectRegex.Matches(fileContents);

            Regex newRegex = new Regex(@"[nN][eE][wW]\s([A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*)\s*\(");
            MatchCollection newMatches = newRegex.Matches(fileContents);

            return createObjectMatches
                .Concat(newMatches)
                .Select(x => x.Groups[1].Value)
                .Distinct()
                .Where(x => !ExcludeComponent(x))
                .Select(x => new ComponentReference(this, x))
                .ToList();
        }

        private List<IncludeReference> GetIncludedFiles(string fileContents)
        {
            Regex includeRegex = new Regex(@"[iI][nN][cC][lL][uU][dD][eE]\s['""]([^'""]+\.cf[mc])['""]|[cC][fF][iI][nN][cC][lL][uU][dD][eE]\s+[tT][eE][mM][pP][lL][aA][tT][eE]\s*=['""]([^'""]+\.cf[mc])['""]");
            MatchCollection matches = includeRegex.Matches(fileContents);

            return matches.Select(x => x.Groups[1].Success ? x.Groups[1].Value : x.Groups[2].Value)
                .Distinct()
                .Select(x => new IncludeReference(this, x))
                .ToList();
        }

        public List<CodeFile> GetReferences(FileSearcher fileSearcher)
        {
            var files = new List<CodeFile>();
            string content = GetContents();

            var componentRefs = GetComponentReferences(content);
            foreach (var comp in componentRefs)
            {
                string relativePath = comp.RelativePath.Replace('.', FileSystem.Path.DirectorySeparatorChar) + ".cfc";
                CodeFile foundFile = fileSearcher.LocateFile(relativePath);
                if (foundFile != null)
                {
                    files.Add(foundFile);
                }
            }

            var includeRefs = GetIncludedFiles(content);
            var currentDirectory = FileSystem.Path.GetDirectoryName(FileName);
            foreach (var include in includeRefs)
            {
                CodeFile foundFile = null;
                if (FileSystem.Path.IsPathRooted(include.RelativePath))
                {
                    foundFile = fileSearcher.LocateFile(include.RelativePath.Substring(1, include.RelativePath.Length - 1));
                }
                else
                {
                    string path = FileSystem.Path.Combine(currentDirectory, include.RelativePath);
                    if (FileSystem.File.Exists(path))
                    {
                        foundFile = new CodeFile(path, FileSystem);
                    }
                }

                if (foundFile != null)
                {
                    files.Add(foundFile);
                }
            }//foreach included reference

            //Find Application File
            var appFile = fileSearcher.GetApplicationFile(FileName);
            if (appFile != null)
            {
                files.Add(appFile);
            }

            return files;
        }

        public override int GetHashCode()
        {
            return _normalizedFileName.GetHashCode();
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
