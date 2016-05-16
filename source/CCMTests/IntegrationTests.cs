using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCMEngine;

namespace CCMTests
{
  [TestClass]
  public class IntegrationTests
  {
    private static AnalyzerCollector Analyze(string filename)
    {
      return Analyze(filename, false);
    }

    private static AnalyzerCollector Analyze(string filename, bool suppressMethodSignature)
    {
      //TODO: start using SortedListener instead of AnalyzerCollector (remove this one)
      AnalyzerCollector collector = new AnalyzerCollector();

      StreamReader stream = new StreamReader(filename);

      try
      {
        FileAnalyzer analyzer =
          new FileAnalyzer(stream, collector, null, suppressMethodSignature, filename);

        analyzer.Analyze();
      }
      finally
      {
        stream.Dispose();
      }

      return collector;
    }

    private static bool FunctionIsInCollection(string function, Dictionary<string, int> collection)
    {
      Dictionary<string, int>.Enumerator e = collection.GetEnumerator();

      while (e.MoveNext())
      {
        if (e.Current.Key.Contains(function))
          return true;
      }

      return false;
    }

    private static int GetComplexityFromCollection(string function, Dictionary<string, int> collection)
    {
      Dictionary<string, int>.Enumerator e = collection.GetEnumerator();

      while (e.MoveNext())
      {
        if (e.Current.Key.Contains(function))
          return e.Current.Value;
      }

      return -1;
    }
    
    [TestMethod]
    [DeploymentItem("IntegrationTests\\ReportedCppErrors.cpp")]
    public void ReportedCppErrors()
    {
      AnalyzerCollector collector = IntegrationTests.Analyze ("ReportedCppErrors.cpp");

      Assert.AreEqual (4, collector.Collection.Count);
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("DllMain", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("test::testing", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("GetXMLStuff", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("AssertWinFile", collector.Collection));
  
      Assert.AreEqual(6, IntegrationTests.GetComplexityFromCollection("DllMain", collector.Collection));
      Assert.AreEqual(4, IntegrationTests.GetComplexityFromCollection("test::testing", collector.Collection));
      Assert.AreEqual(1, IntegrationTests.GetComplexityFromCollection("GetXMLStuff", collector.Collection));
      Assert.AreEqual(2, IntegrationTests.GetComplexityFromCollection("AssertWinFile", collector.Collection));
    }

    [TestMethod]
    [DeploymentItem("IntegrationTests\\CxxMocks.h")]
    public void CxxMocksTests()
    {
      AnalyzerCollector collector = IntegrationTests.Analyze ("CxxMocks.h");
      Assert.AreEqual (9, collector.Collection.Count);

      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("Mock0::AndReturn", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("Mock0<void>::Func", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("Mock1::Expect", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("Mock1::AndReturn", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("Mock1::Func", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("Mock1<void,_Arg1>::Expect", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("Mock1<void,_Arg1>::Func", collector.Collection));
    }

    [TestMethod]
    [DeploymentItem("IntegrationTests\\IOPropertyValue.cpp")]
    public void IOTest()
    {
      AnalyzerCollector collector = IntegrationTests.Analyze("IOPropertyValue.cpp");

      Assert.AreEqual(17, collector.Collection.Count);
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOElecTypeValue::IsValid", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOElecTypeValue::Set", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOElecTypeValue::Get", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOElecTypeValue::Pack", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOCompoundValue::IOCompoundValue", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOCompoundValue::operator ==", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOCompoundValue::Set", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOCompoundValue::Get", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOCompoundValue::IsValid", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOCompoundValue::ResetValue", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("IOCompoundValue::SetType", collector.Collection));

      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("OutboundConvertibleCompoundValue::OutboundConvertibleCompoundValue", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("OutboundConvertibleCompoundValue::Pack", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("DigitalOverrideValue::Unpack", collector.Collection));
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("DigitalOverrideValue::Get", collector.Collection));
    }

    [TestMethod]
    [DeploymentItem("IntegrationTests\\log.cs")]
    public void TrendLogTest()
    {
      AnalyzerCollector collector = IntegrationTests.Analyze("log.cs");

      Assert.AreEqual(1, collector.Collection.Count);
      Assert.AreEqual(true, IntegrationTests.FunctionIsInCollection("Log::Write", collector.Collection));
      Assert.AreEqual(1, IntegrationTests.GetComplexityFromCollection("Log::Write", collector.Collection));
     
      Assert.AreEqual(15, collector.Metrics["Log::Write(string text)"].StartLineNumber);
      Assert.AreEqual(17, collector.Metrics["Log::Write(string text)"].EndLineNumber);
    }

    [TestMethod]
    [DeploymentItem("IntegrationTests\\valian.c")]
    public void TestSuppressMethodSignature()
    {
      {
        AnalyzerCollector collector = IntegrationTests.Analyze("valian.c");

        Assert.AreEqual(3, collector.Collection["InstrumentWndProc(WPARAM wParam)"]);
        Assert.AreEqual(5, collector.Collection["InstrumentWndProc2(WPARAM wParam)"]);
      }
      {
        AnalyzerCollector collector = IntegrationTests.Analyze("valian.c", true);

        Assert.AreEqual(3, collector.Collection["InstrumentWndProc"]);
        Assert.AreEqual(5, collector.Collection["InstrumentWndProc2"]);
      }
    }

    [TestMethod]
    [DeploymentItem("IntegrationTests\\valian.c")]
    public void ValianTest()
    {
      AnalyzerCollector collector = IntegrationTests.Analyze("valian.c");

      Assert.AreEqual(3, collector.Collection.Count);
      Assert.IsTrue(IntegrationTests.FunctionIsInCollection("SmpEditFrameProc", collector.Collection));
      Assert.IsTrue(IntegrationTests.FunctionIsInCollection("InstrumentWndProc", collector.Collection));
      Assert.IsTrue(IntegrationTests.FunctionIsInCollection("InstrumentWndProc2", collector.Collection));

      Assert.AreEqual(3, IntegrationTests.GetComplexityFromCollection("InstrumentWndProc", collector.Collection));
      Assert.AreEqual(5, IntegrationTests.GetComplexityFromCollection("InstrumentWndProc2", collector.Collection));
      Assert.AreEqual(22, IntegrationTests.GetComplexityFromCollection("SmpEditFrameProc", collector.Collection));
    }

    private static ccMetric GetByName(List<ccMetric> metrics, string nameOfFunction)
    {
      foreach (ccMetric m in metrics)
        if (m.Unit == nameOfFunction)
          return m;

      return null;
    }

    [TestMethod]
    [DeploymentItem("IntegrationTests\\examples.js")]
    public void TestJavascriptFileContainsAllFunctions()
    {
      string filename = "examples.js"; // this is deployed through local.testsettings
      SortedListener listener = new SortedListener(10, new List<string>(), 0);
      using (StreamReader stream = new StreamReader(filename))
      {
        FileAnalyzer analyzer = new FileAnalyzer(stream, listener, null, false, filename);
        analyzer.Analyze();

        Assert.AreEqual(10, listener.Metrics.Count);

        Assert.AreEqual(6, IntegrationTests.GetByName(listener.Metrics, "gcd(segmentA,segmentB)").CCM);
        Assert.AreEqual(2, IntegrationTests.GetByName(listener.Metrics, "function(monkey)").CCM);
        Assert.AreEqual(1, IntegrationTests.GetByName(listener.Metrics, "localFunction()").CCM);
        Assert.AreEqual(1, IntegrationTests.GetByName(listener.Metrics, "Some.Foo(args)").CCM);
        Assert.AreEqual(1, IntegrationTests.GetByName(listener.Metrics, "functionWithColon()").CCM);
        Assert.AreEqual(2, IntegrationTests.GetByName(listener.Metrics, "localFunction1()").CCM);
        Assert.AreEqual(4, IntegrationTests.GetByName(listener.Metrics, "outerFunction1()").CCM);
        Assert.AreEqual(1, IntegrationTests.GetByName(listener.Metrics, "localFunction2()").CCM);
        Assert.AreEqual(1, IntegrationTests.GetByName(listener.Metrics, "Monkey::feedMonkey()").CCM);
        Assert.AreEqual(1, IntegrationTests.GetByName(listener.Metrics, "afterMonkeyFeed()").CCM);

      }
    }

    [TestMethod]
    [DeploymentItem("IntegrationTests\\TypeScript.ts")]
    public void TestTypescriptFileIsCorrectlyParsed()
    {
      string filename = "TypeScript.ts"; // this is deployed through local.testsettings
      SortedListener listener = new SortedListener(10, new List<string>(), 0);
      using (StreamReader stream = new StreamReader(filename))
      {
        FileAnalyzer analyzer = new FileAnalyzer(stream, listener, null, false, filename);
        analyzer.Analyze();

        Assert.AreEqual(3, listener.Metrics.Count);
        
        Assert.IsNotNull(IntegrationTests.GetByName(listener.Metrics, "Sayings::Greeter::greet()"));
        Assert.IsNotNull(IntegrationTests.GetByName(listener.Metrics, "Sayings::Greeter::constructor(message:string)"));
        Assert.IsNotNull(IntegrationTests.GetByName(listener.Metrics, "button.onclick()"));
      }
    }

    [TestMethod]
    [DeploymentItem("IntegrationTests\\cstylefuncs.c")]
    public void TestCStyleFuncDecl()
    {
      string filename = "cstylefuncs.c"; // this is deployed through local.testsettings
      SortedListener listener = new SortedListener(10, new List<string>(), 0);
      using (StreamReader stream = new StreamReader(filename))
      {
        FileAnalyzer analyzer = new FileAnalyzer(stream, listener, null, true, filename);
        analyzer.Analyze();

        Assert.AreEqual(3, listener.Metrics.Count);

        Assert.IsNotNull(IntegrationTests.GetByName(listener.Metrics, "added"));
        Assert.IsNotNull(IntegrationTests.GetByName(listener.Metrics, "new_cfg_record"));
        Assert.IsNotNull(IntegrationTests.GetByName(listener.Metrics, "edit_cfg_record"));
      }
    }

    [TestMethod]
    [DeploymentItem("IntegrationTests\\FileWithTabAfterEndIf.c")]
    public void TestFileWithTabAfterEndif()
    {
      string filename = "FileWithTabAfterEndIf.c"; // this is deployed through local.testsettings
      SortedListener listener = new SortedListener(10, new List<string>(), 0);
      using (StreamReader stream = new StreamReader(filename))
      {
        FileAnalyzer analyzer = new FileAnalyzer(stream, listener, null, true, filename);
        analyzer.Analyze();

        Assert.AreEqual(1, listener.Metrics.Count);

        Assert.IsNotNull(IntegrationTests.GetByName(listener.Metrics, "Foo"));
      }
    }
  }
}
