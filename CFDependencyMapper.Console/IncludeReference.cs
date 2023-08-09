using System;
using System.Collections.Generic;
using System.Text;

namespace CFDependencyMapper.Console
{
    public class IncludeReference
    {
        public CodeFile SourceFile { get; private set; }
        public string RelativePath { get; private set; }

        public IncludeReference(CodeFile sourceFile, string relativePath)
        {
            SourceFile = sourceFile;
            RelativePath = relativePath;
        }
    }
}
