using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCM;
using CCMEngine;

namespace CCMTests
{
  [TestClass]
  public class FileAnalyzerTests
  {
    private SortedListener listener = null;

    [TestInitialize]
    public void Setup()
    {
      this.listener = new SortedListener(100, new List<string>(), 0); 
    }

    private void Analyze(string code, string filename)
    {
      StreamReader r = new StreamReader(new MemoryStream(Encoding.Default.GetBytes(code)));
      new FileAnalyzer(r, this.listener, null, true, filename).Analyze();
    }

    [TestMethod]
    public void TestMethodOnFirstLineReturnsLineNumberOne()
    {
      string code = "void Foo(){}";

      Analyze(code, "one.cpp");

      Assert.AreEqual(1, this.listener.Metrics.Count);
      Assert.AreEqual(1, this.listener.Metrics[0].StartLineNumber);
      Assert.AreEqual(1, this.listener.Metrics[0].EndLineNumber);
    }

    [TestMethod]
    public void TestEndLineNumberIsSameAssEndOfFunction()
    {
      string code = "void Foo() \r\n" +
                    "{ \r\n " +
                    "}";

      Analyze(code, "two.cpp");
      Assert.AreEqual(1, this.listener.Metrics.Count);
      Assert.AreEqual(2, this.listener.Metrics[0].StartLineNumber);
      Assert.AreEqual(3, this.listener.Metrics[0].EndLineNumber);
    }

    [TestMethod]
    public void TestJavascriptFileGetsJSParserAsFunctionStream()
    {
      string filename = "test.js";
      var langParser = LookAheadLangParser.CreateCppParser(new StreamReader(new MemoryStream()));

      IFunctionStream functionStream = FileAnalyzer.CreateFunctionStream(langParser, filename, false);

      Assert.IsNotNull(functionStream);
      Assert.IsTrue(functionStream is JSParser);
    }

    [TestMethod]
    public void TestCSharpFileGetsCCCParserAsFunctionStream()
    {
      string filename = "myCode.cs";
      var langParser = LookAheadLangParser.CreateCppParser(new StreamReader(new MemoryStream()));

      IFunctionStream functionStream = FileAnalyzer.CreateFunctionStream(langParser, filename, false);

      Assert.IsNotNull(functionStream);
      Assert.IsTrue(functionStream is CCCParser);
    }
  }
}
