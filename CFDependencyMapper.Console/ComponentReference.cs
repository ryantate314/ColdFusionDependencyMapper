using System;
using System.Collections.Generic;
using System.Text;

namespace CFDependencyMapper.Console
{
    class ComponentReference
    {
        public CodeFile SourceFile { get; private set; }
        public string RelativePath { get; private set; }

        public ComponentReference(CodeFile sourceFile, string relativePath)
        {
            SourceFile = sourceFile;
            RelativePath = relativePath;
        }
    }
}
