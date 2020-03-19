using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;

namespace CFDependencyMapper.Console
{
    static class IFileSystemExtensions
    {
        public static bool ArePathsEqual(this IPath path, string a, string b)
        {
            return String.Compare(
                path.GetFullPath(a).TrimEnd(path.DirectorySeparatorChar),
                path.GetFullPath(b).TrimEnd(path.DirectorySeparatorChar),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static string NormalizePath(this IPath path, string p)
        {
            return path.GetFullPath(p)
                .TrimEnd(path.DirectorySeparatorChar)
                .ToLower();
        }
    }
}
