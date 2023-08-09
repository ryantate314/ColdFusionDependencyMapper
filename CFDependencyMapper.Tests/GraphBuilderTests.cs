using CFDependencyMapper.Console;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace CFDependencyMapper.Tests
{
    public class GraphBuilderTests
    {
        private MockFileSystem _fileSystem;
        private GraphBuilder _graphBuilder;

        [SetUp]
        public void SetUp()
        {
            _fileSystem = new MockFileSystem();
            _graphBuilder = new GraphBuilder(_fileSystem);
        }

        [Test]
        public void TestBuildGraph_ReferencesFileNotInRoot_IsIncludedInGraph()
        {
            string index_cfm = @"
<cfscript>
    foo = new Bar();
</cfscript>
";
            string bar_cfc = @"
<cfscript>
    fiz = new Fiz();
</cfscript>
";
            _fileSystem.AddFile(@"C:\web_root\index.cfm", new MockFileData(index_cfm));
            _fileSystem.AddFile(@"C:\components\bar.cfc", new MockFileData(bar_cfc));
            _fileSystem.AddFile(@"C:\components\fiz.cfc", MockFileData.NullObject);

            _graphBuilder.AddReferenceDirectory(@"C:\components");
            _graphBuilder.AddRootDirectory(@"C:\web_root");

            var graph = _graphBuilder.Build();

            Assert.AreEqual(3, graph.Nodes.Count);
        }
    }
}
