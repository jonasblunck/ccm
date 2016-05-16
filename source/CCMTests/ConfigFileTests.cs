using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using CCMEngine;

namespace CCMTests
{
    [TestClass]
    public class ConfigFileTests
    {
        [TestMethod]
        public void FileExclude()
        {
            string config = "<ccm> " +
                            " <exclude>" +
                            "   <file>file1.ext</file> " +
                            " </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual("file1.ext", file.ExcludeFiles[0]);
        }

        [TestMethod]
        public void ClassExclude()
        {
            string config = "<ccm>" +
                            "  <exclude>" +
                            "    <class>CMyClass</class> " +
                            "  </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual("CMyClass", file.ExcludeClasses[0]);
        }

        [TestMethod]
        public void ExcludeFunctions()
        {
            string config = "<ccm>" +
                            "  <exclude>" +
                            "    <function>Foo::Bar</function> " +
                            "    <function>CreateInstance</function> " +
                            "  </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual("Foo::Bar", file.ExcludeFunctions[0]);
            Assert.AreEqual("CreateInstance", file.ExcludeFunctions[1]);
        }

        [TestMethod]
        public void FolderExclude()
        {
            string config = "<ccm>" +
                            " <exclude>" +
                            "  <folder>Dir1</folder> " +
                            " </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual("Dir1", file.ExcludeFolders[0]);
        }

        [TestMethod]
        public void Combined()
        {
            string config = "<ccm>" +
                            " <exclude>" +
                            "  <folder>Dir1</folder> " +
                            "  <class>CFoo</class> " +
                            "  <class>CBar</class> " +
                            "  <file>file.txt</file> " +
                            " </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual("Dir1", file.ExcludeFolders[0]);
            Assert.AreEqual("CFoo", file.ExcludeClasses[0]);
            Assert.AreEqual("CBar", file.ExcludeClasses[1]);
            Assert.AreEqual("file.txt", file.ExcludeFiles[0]);
        }

        [TestMethod]
        public void MultipleClassExcludes()
        {
            string config = "<ccm>" +
                            " <exclude>" +
                            "  <class>CMyClass</class> " +
                            "  <class>CFoo</class> " +
                            " </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual("CMyClass", file.ExcludeClasses[0]);
            Assert.AreEqual("CFoo", file.ExcludeClasses[1]);
        }

        [TestMethod]
        public void AnalyzeFolder()
        {
            string config = "<ccm>" +
                            " <analyze>" +
                            "  <folder>folderA</folder> " +
                            "  <folder>folderB</folder> " +
                            " </analyze>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual("folderA", file.AnalyzeFolders[0]);
            Assert.AreEqual("folderB", file.AnalyzeFolders[1]);
        }

        [TestMethod]
        public void RecursiveSetting()
        {
            string config = "<ccm>" +
                            " <recursive>no</recursive>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual(false, file.RecursiveAnalyze);
        }

        [TestMethod]
        public void RecursiveSettingYes()
        {
            string config = "<ccm>" +
                            " <recursive>yes</recursive>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual(true, file.RecursiveAnalyze);
        }

        [TestMethod]
        public void MetricsCount()
        {
            string config = "<ccm>" +
                            " <numMetrics>20</numMetrics>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual(20, file.NumMetrics);
        }

        [TestMethod]
        public void OutputXml()
        {
            string config = "<ccm>" +
                            " <outputter>XML</outputter>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual(CCM.CCMOutputter.XmlOutputType, file.OutputType);
        }

        [TestMethod]
        public void TestReadThreshold()
        {
            string config = "<ccm>" +
                            " <threshold>11</threshold>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual(11, file.Threshold);
        }

        [TestMethod]
        public void MultipleFileExcludes()
        {
            string config = "<ccm>" +
                            " <exclude>" +
                            "  <file>file1.ext</file> " +
                            "  <file>file2.ext</file> " +
                            " </exclude>" +
                            "</ccm>";


            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual("file1.ext", file.ExcludeFiles[0]);
            Assert.AreEqual("file2.ext", file.ExcludeFiles[1]);
        }

        [TestMethod]
        public void TestNoSuppressElementReturnShouldNotSuppressMethodNames()
        {
            string config = "<ccm>" +
                            " <exclude>" +
                            " </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);
            Assert.AreEqual(false, file.SuppressMethodSignatures);
        }

        [TestMethod]
        public void TestConfigFileWithEmptySuppressMethodSignatureElementDoesNotSuppress()
        {
            string config = "<ccm>" +
                            "<suppressMethodSignatures />" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);
            Assert.AreEqual(false, file.SuppressMethodSignatures);
        }

        [TestMethod]
        public void TestConfigFileWithSuppressYesElementDoesSuppress()
        {
            string config = "<ccm>" +
                            "<suppressMethodSignatures>Yes</suppressMethodSignatures>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);
            Assert.AreEqual(true, file.SuppressMethodSignatures);
        }

        [TestMethod]
        public void TestConfigFileThatHasNoFileExtensionGetsDefaultCsAndCpp()
        {
            string config = "<ccm>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual(7, file.SupportedExtensions.Count);
            Assert.AreEqual(".cpp", file.SupportedExtensions[0]);
            Assert.AreEqual(".cs", file.SupportedExtensions[1]);
            Assert.AreEqual(".h", file.SupportedExtensions[2]);
            Assert.AreEqual(".hpp", file.SupportedExtensions[3]);
            Assert.AreEqual(".c", file.SupportedExtensions[4]);
            Assert.AreEqual(".js", file.SupportedExtensions[5]);
            Assert.AreEqual(".ts", file.SupportedExtensions[6]);
        }

        [TestMethod]
        public void TestConfigFileThatAddsHppExtensionIsRecognizedAsSupported()
        {
            string config = "<ccm>" +
                            "<fileExtensions>" +
                            "<fileExtension>.hpp</fileExtension>" +
                            "<fileExtension>.cxx</fileExtension>" +
                            "</fileExtensions>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual(9, file.SupportedExtensions.Count);
            Assert.AreEqual(".hpp", file.SupportedExtensions[7]);
            Assert.AreEqual(".cxx", file.SupportedExtensions[8]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestThrowsIfOutputToXmlElementIsAvailable()
        {
            string config = "<ccm>" +
                            " <outputXML>No</outputXML>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);
        }

        [TestMethod]
        public void TestDefaultSwitchBehaviorIsTraditional()
        {
            string config = "<ccm> " +
                            " <exclude>" +
                            "   <file>file1.ext</file> " +
                            " </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual(ParserSwitchBehavior.TraditionalInclude, file.SwitchStatementBehavior);
        }

        [TestMethod]
        public void TestSwitchBehaviorIgnoreCaseCanbeRead()
        {
            string config = "<ccm> " +
                            " <switchStatementBehavior>IgnoreCases</switchStatementBehavior> " +
                            " <exclude>" +
                            "   <file>file1.ext</file> " +
                            " </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);

            Assert.AreEqual(ParserSwitchBehavior.IgnoreCases, file.SwitchStatementBehavior);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestThrowsOnIllegalSwitchStatementBehavior()
        {
            string config = "<ccm> " +
                            " <switchStatementBehavior>Merge</switchStatementBehavior> " +
                            " <exclude>" +
                            "   <file>file1.ext</file> " +
                            " </exclude>" +
                            "</ccm>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);

            ConfigurationFile file = new ConfigurationFile(doc);
        }


    }
}
