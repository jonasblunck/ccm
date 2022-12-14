using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCMEngine;
using System.IO;
using System.Xml;
using CCM;

namespace CCMTests
{
  [TestClass]
  public class DriverTests
  {
    private static void AssertMetric(List<ccMetric> metrics, string function, int ccm, string file)
    {
      foreach (ccMetric metric in metrics)
      {
        if (metric.Unit == function)
        {
          Assert.AreEqual(ccm, metric.CCM);
          Assert.AreEqual(file, metric.Filename);
          return;
        }
      }

      Assert.Fail(string.Format("Could not found function '{0}'.", function));
    }

    [TestMethod]
    public void SingleStreamIsAnalyzed()
    {
      string code = "void Foo() {}";

      Driver driver = new Driver();

      driver.StartAnalyze(TestUtil.GetTextStream(code), "file1.h");
      driver.WaitForWorkThreadsToFinish();

      DriverTests.AssertMetric(driver.Metrics, "Foo()", 1, "file1.h");
    }

    [TestMethod]
    public void ExcludedFolderOnlyExcludesWhereNameOfFolderIsExactMatch()
    {
      string config = "<ccm>" +
                      "  <exclude>" +
                      "    <folder>Foo</folder> " +
                      "    <folder>Bar</folder> " +
                      "  </exclude>" +
                      "</ccm>";

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(config);

      ConfigurationFile file = new ConfigurationFile(doc);
      Driver driver = new Driver(file);

            /*
      Assert.IsTrue(driver.PathShouldBeExcluded("c:\\code\\Foo\\file.cpp"));
      Assert.IsTrue(driver.PathShouldBeExcluded("c:\\code\\bar\\file.cpp"));
      Assert.IsFalse(driver.PathShouldBeExcluded("c:\\code\\FooBar\\fileB.cs"));
      Assert.IsFalse(driver.PathShouldBeExcluded("c:\\code\\BarA\\fileB.cs"));
            */
    }

    [TestMethod]
    public void FullFolderPathIsExcludedOnExactMatch()
    {
      string config = "<ccm>" +
                      "  <exclude>" +
                      "    <folder>c:\\code</folder> " +
                      "  </exclude>" +
                      "</ccm>";

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(config);

      ConfigurationFile file = new ConfigurationFile(doc);
      Driver driver = new Driver(file);

      Assert.IsTrue(driver.PathShouldBeExcluded("c:\\code\\file.cpp"));
      Assert.IsTrue(driver.PathShouldBeExcluded("c:\\code\\sub\\file.cpp"));
      Assert.IsFalse(driver.PathShouldBeExcluded("c:\\temp\\fileB.cs"));
    }

    [TestMethod]
    public void ExcludedFunctionIsExcluded()
    {
      string config = "<ccm>" +
                      "  <exclude>" +
                      "    <function>Foo2</function> " +
                      "    <function>Bar2</function> " +
                      "  </exclude>" +
                      "</ccm>";

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(config);

      ConfigurationFile file = new ConfigurationFile(doc);

      string code1 = "void Foo() {}";
      string code2 = "void Bar() {}";
      string code3 = "void Foo2() {}";
      string code4 = "void Bar2() {}";

      Driver driver = new Driver(file);

      driver.StartAnalyze(TestUtil.GetTextStream(code1), "file1.h");
      driver.StartAnalyze(TestUtil.GetTextStream(code2), "file2.h");
      driver.StartAnalyze(TestUtil.GetTextStream(code3), "file3.h");
      driver.StartAnalyze(TestUtil.GetTextStream(code4), "file4.h");
      driver.WaitForWorkThreadsToFinish();

      DriverTests.AssertMetric(driver.Metrics, "Foo()", 1, "file1.h");
      DriverTests.AssertMetric(driver.Metrics, "Bar()", 1, "file2.h");

      Assert.AreEqual(2, driver.Metrics.Count);
    }

    [TestMethod]
    public void MultipleStreamsAnalyzed()
    {
      string code1 = "void Foo() {}";
      string code2 = "void Bar() {}";
      string code3 = "void Foo2() {}";
      string code4 = "void Bar2() {}";

      Driver driver = new Driver();

      driver.StartAnalyze(TestUtil.GetTextStream(code1), "file1.h");
      driver.StartAnalyze(TestUtil.GetTextStream(code2), "file2.cpp");
      driver.StartAnalyze(TestUtil.GetTextStream(code3), "file3.cs");
      driver.StartAnalyze(TestUtil.GetTextStream(code4), "file4.h");
      driver.WaitForWorkThreadsToFinish();

      DriverTests.AssertMetric(driver.Metrics, "Foo()", 1, "file1.h");
      DriverTests.AssertMetric(driver.Metrics, "Bar()", 1, "file2.cpp");
      DriverTests.AssertMetric(driver.Metrics, "Foo2()", 1, "file3.cs");
      DriverTests.AssertMetric(driver.Metrics, "Bar2()", 1, "file4.h");
      
    }

    [TestMethod]
    public void TestJSFileExtensionIsValidForAnalysis()
    {
      Assert.AreEqual(true, new Driver().IsValidFile("code.js"));
      Assert.AreEqual(true, new Driver().IsValidFile("code.JS"));

      Assert.AreEqual(false, new Driver().IsValidFile("//depot/code/js/file.html"));
    }

      
  }
}
