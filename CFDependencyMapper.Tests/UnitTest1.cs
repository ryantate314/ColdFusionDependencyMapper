using CFDependencyMapper.Console;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace CFDependencyMapper.Tests
{
    public class Tests
    {

        private MockFileSystem _fileSystem;

        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
        }

        [Test]
        public void TestDetectApplicationFile()
        {
            string fileName = "C:/Temp/test.cfm";
            string applicationFileName = "C:/Temp/application.cfm";
            _fileSystem.AddFile(applicationFileName, MockFileData.NullObject);
            var expectedAppFile = new CodeFile(applicationFileName, _fileSystem);
            var fileSearcher = new FileSearcher(_fileSystem, "C:/Temp");

            CodeFile foundAppFile = fileSearcher.GetApplicationFile(fileName);

            Assert.IsTrue(expectedAppFile.Equals(foundAppFile));
        }

        [Test]
        public void TestDetectApplicationFile_Inherited()
        {
            string fileName = "C:/Temp/Sub/test.cfm";
            string applicationFileName = "C:/Temp/application.cfm";
            _fileSystem.AddFile(applicationFileName, MockFileData.NullObject);
            var expectedAppFile = new CodeFile(applicationFileName, _fileSystem);
            var fileSearcher = new FileSearcher(_fileSystem, "C:/Temp");
            fileSearcher.AddRootDirectory("C:/Temp");

            CodeFile foundAppFile = fileSearcher.GetApplicationFile(fileName);

            Assert.IsTrue(expectedAppFile.Equals(foundAppFile));
        }

        [Test]
        public void TestDetectApplicationFile_None()
        {
            string fileName = "C:/Temp/Sub/test.cfm";
            var fileSearcher = new FileSearcher(_fileSystem, "C:/Temp");

            CodeFile foundAppFile = fileSearcher.GetApplicationFile(fileName);

            Assert.IsNull(foundAppFile);
        }

        [Test]
        public void TestCodeFileEquals()
        {
            var file1 = new CodeFile("C:/Temp/Test.txt", _fileSystem);
            var file2 = new CodeFile("C:/Temp/Test2.txt", _fileSystem);
            Assert.IsFalse(file1.Equals(file2));
        }

        [Test]
        public void TestFindReferences_CreateObject()
        {
            string fileName = "C:/WebRoot/Test.cfm";
            string testContent =
@"<cfset variables.foo = createObject(""component"", ""CFC.Foo"") />
<cfset variables.bar=CREATEOBJECT(""component"",""CFC.Bar"")/>
<cfset variables.cat = createObject('component', 'CFC.Cat').init() />";
            _fileSystem.AddFile(fileName, new MockFileData(testContent));

            const string fooFile = "C:/Components/CFC/Foo.cfc";
            const string barFile = "C:/Components/CFC/Bar.cfc";
            const string catFile = "C:/Components/CFC/Cat.cfc";
            _fileSystem.AddFile(fooFile, MockFileData.NullObject);
            _fileSystem.AddFile(barFile, MockFileData.NullObject);
            _fileSystem.AddFile(catFile, MockFileData.NullObject);

            var fileSearcher = new FileSearcher(_fileSystem, "C:/WebRoot");
            fileSearcher.AddRootDirectory("C:/Components");

            var file = new CodeFile(fileName, _fileSystem);

            List<CodeFile> references = file.GetReferences(fileSearcher);

            var fooCodeFile = new CodeFile(fooFile, _fileSystem);
            var barCodeFile = new CodeFile(barFile, _fileSystem);
            var catCodeFile = new CodeFile(catFile, _fileSystem);

            Assert.AreEqual(3, references.Count);
            Assert.IsTrue(fooCodeFile.Equals(references[0]));
            Assert.IsTrue(barCodeFile.Equals(references[1]));
            Assert.IsTrue(catCodeFile.Equals(references[2]));
        }

        [Test]
        public void TestFindReferences_New()
        {
            string fileName = "C:/Test.cfm";
            string testContent =
@"<cfset variables.foo = new CFC.Foo() />
<cfset variables.bar=new CFC.Bar(apple,
                                pear,
                                banana.getName(first, second, third))/>
<cfscript>
    variables.cat = new Cat();
</cfscript>";
            _fileSystem.AddFile(fileName, new MockFileData(testContent));

            const string fooFile = "C:/Components/CFC/Foo.cfc";
            const string barFile = "C:/Components/CFC/Bar.cfc";
            const string catFile = "C:/Components/Cat.cfc";
            _fileSystem.AddFile(fooFile, MockFileData.NullObject);
            _fileSystem.AddFile(barFile, MockFileData.NullObject);
            _fileSystem.AddFile(catFile, MockFileData.NullObject);

            var fileSearcher = new FileSearcher(_fileSystem, "C:/WebRoot");
            fileSearcher.AddRootDirectory("C:/Components");

            var file = new CodeFile(fileName, _fileSystem);

            List<CodeFile> references = file.GetReferences(fileSearcher);

            var fooCodeFile = new CodeFile(fooFile, _fileSystem);
            var barCodeFile = new CodeFile(barFile, _fileSystem);
            var catCodeFile = new CodeFile(catFile, _fileSystem);

            Assert.AreEqual(3, references.Count);
            Assert.IsTrue(fooCodeFile.Equals(references[0]));
            Assert.IsTrue(barCodeFile.Equals(references[1]));
            Assert.IsTrue(catCodeFile.Equals(references[2]));
        }

        [Test]
        public void TestFindReferences_Include()
        {
            string fileName = "C:/WebRoot/Sub/Test.cfm";
            string testContent =
@"<cfscript>
    include ""/template1.cfm"";
    include 'template2.cfm';
</cfscript>
<cfinclude template=""\template3.cfm"" />
<cfinclude template='template4.cfm' />
";
            _fileSystem.AddFile(fileName, new MockFileData(testContent));

            const string template1 = "C:/WebRoot/template1.cfm";
            const string template2 = "C:/WebRoot/Sub/template2.cfm";
            const string template3 = "C:/WebRoot/template3.cfm";
            const string template4 = "C:/WebRoot/Sub/template4.cfm";
            _fileSystem.AddFile(template1, MockFileData.NullObject);
            _fileSystem.AddFile(template2, MockFileData.NullObject);
            _fileSystem.AddFile(template3, MockFileData.NullObject);
            _fileSystem.AddFile(template4, MockFileData.NullObject);

            var fileSearcher = new FileSearcher(_fileSystem, "C:/WebRoot");
            fileSearcher.AddRootDirectory("C:/Components");

            var file = new CodeFile(fileName, _fileSystem);

            List<CodeFile> references = file.GetReferences(fileSearcher);

            var temp1File = new CodeFile(template1, _fileSystem);
            var temp2File = new CodeFile(template2, _fileSystem);
            var temp3File = new CodeFile(template3, _fileSystem);
            var temp4File = new CodeFile(template4, _fileSystem);

            Assert.AreEqual(4, references.Count);
            Assert.IsTrue(temp1File.Equals(references[0]));
            Assert.IsTrue(temp2File.Equals(references[1]));
            Assert.IsTrue(temp3File.Equals(references[2]));
            Assert.IsTrue(temp4File.Equals(references[3])); 
        }

        [Test]
        public void TestLocateFile_CFC()
        {
            var fileSearcher = new FileSearcher(_fileSystem, "C:/Test");

            _fileSystem.AddFile("C:/Test/CFC/Foo.cfc", MockFileData.NullObject);

            CodeFile codeFile = fileSearcher.LocateFile("CFC/Foo.cfc");

            var expectedFile = new CodeFile("C:/Test/CFC/Foo.cfc", _fileSystem);
            Assert.IsTrue(expectedFile.Equals(codeFile));
        }

        [Test]
        public void TestNormalizePath()
        {
            _fileSystem.AddDirectory("C:/Temp");
            _fileSystem.AddDirectory("C:/Temp/sub");
            _fileSystem.AddDirectory("C:/Temp/sub/foo");

            Assert.AreEqual("C:\\test.cfm", _fileSystem.Path.NormalizePath("test.cfm"));
            Assert.AreEqual("C:\\test\\sub", _fileSystem.Path.NormalizePath("C:/test/sub"));
            Assert.AreEqual("C:\\test\\sub\\test.cfm", _fileSystem.Path.NormalizePath("C:/test/sub/foo/../test.cfm"));
        }
    }
}