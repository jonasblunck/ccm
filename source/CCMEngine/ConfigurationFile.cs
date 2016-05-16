using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CCMEngine
{
    public class ConfigurationFile
    {
        private List<string> excludeFiles = new List<string>();
        private List<string> excludeClasses = new List<string>();
        private List<string> excludeFolders = new List<string>();
        private List<string> analyzeFolders = new List<string>();
        private List<string> excludeFunctions = new List<string>();
        private bool recursive = false;
        private int numMetrics = 30;
        private int threshold = 0;
        private string outputType = "Text"; // default to Text output
        private bool suppressMethodSignatures = false;
        private ParserSwitchBehavior switchBehavior = ParserSwitchBehavior.TraditionalInclude;

        public List<string> SupportedExtensions { get; private set; }

        public ConfigurationFile(XmlDocument doc)
        {
            this.SupportedExtensions = new List<string>();

            Parse(doc);
        }

        private void ParseSwitchStatementBehavior(XmlDocument doc)
        {
            XmlElement recursive = (XmlElement)doc.SelectSingleNode("/ccm/switchStatementBehavior");

            if (null != recursive)
            {
                string setting = recursive.InnerText;

                if (setting.Equals("TraditionalInclude", StringComparison.InvariantCultureIgnoreCase))
                    this.switchBehavior = ParserSwitchBehavior.TraditionalInclude;
                else if (setting.Equals("IgnoreCases", StringComparison.InvariantCultureIgnoreCase))
                    this.switchBehavior = ParserSwitchBehavior.IgnoreCases;
                else
                    throw new InvalidOperationException(string.Format("Unknown switchStatementBehavior: {0}", setting));
            }
        }

        private void ParseRecursiveSetting(XmlDocument doc)
        {
            XmlElement recursive = (XmlElement)doc.SelectSingleNode("/ccm/recursive");

            if (null != recursive)
            {
                string setting = recursive.InnerText;

                if (setting.ToLower().Equals("yes") || setting.ToLower().Equals("true") || setting.Equals("1"))
                    this.recursive = true;
            }
        }

        private void ParseOutputXML(XmlDocument doc)
        {
            XmlElement output = (XmlElement)doc.SelectSingleNode("/ccm/outputter");

            if (null != output)
            {
                this.outputType = output.InnerText;
            }
        }

        private void ParseNumMetrics(XmlDocument doc)
        {
            XmlElement metrics = (XmlElement)doc.SelectSingleNode("/ccm/numMetrics");

            if (null != metrics)
                this.numMetrics = int.Parse(metrics.InnerText);
        }

        private void ParseThreshold(XmlDocument doc)
        {
            XmlElement metrics = (XmlElement)doc.SelectSingleNode("/ccm/threshold");

            if (null != metrics)
                this.threshold = int.Parse(metrics.InnerText);
        }

        private void ParseExcludes(XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("/ccm/exclude");

            if (null != root)
            {
                XmlNodeList fileNodes = root.SelectNodes("file");

                foreach (XmlNode file in fileNodes)
                    this.excludeFiles.Add(((XmlElement)file).InnerText);

                XmlNodeList classNodes = root.SelectNodes("class");

                foreach (XmlNode classNode in classNodes)
                    this.excludeClasses.Add(((XmlElement)classNode).InnerText);

                XmlNodeList folderNodes = root.SelectNodes("folder");

                foreach (XmlNode folderNode in folderNodes)
                    this.excludeFolders.Add(((XmlElement)folderNode).InnerText);

                XmlNodeList functionNodes = root.SelectNodes("function");
                foreach (XmlNode functionNode in functionNodes)
                    this.excludeFunctions.Add(((XmlElement)functionNode).InnerText);
            }
        }

        private void ParseAnalyzeFolders(XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("/ccm/analyze");

            if (null != root)
            {
                XmlNodeList folderNodes = root.SelectNodes("folder");

                foreach (XmlNode folder in folderNodes)
                    this.analyzeFolders.Add(((XmlElement)folder).InnerText);
            }
        }

        private void ParseSuppressSignatureElement(XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("/ccm/suppressMethodSignatures");

            if (null != root)
            {
                if (root.InnerText.ToLower().Equals("yes") || root.InnerText.ToLower().Equals("1"))
                    this.suppressMethodSignatures = true;
            }

        }

        public static List<string> GetDefaultSupportedFileExtensions()
        {
            List<string> supportedExtension = new List<string>();
            supportedExtension.Add(".cpp");
            supportedExtension.Add(".cs");
            supportedExtension.Add(".h");
            supportedExtension.Add(".hpp");
            supportedExtension.Add(".c");
            supportedExtension.Add(".js");
            supportedExtension.Add(".ts");

            return supportedExtension;
        }

        private void ParseSupportedFileExtensions(XmlDocument doc)
        {
            ConfigurationFile.GetDefaultSupportedFileExtensions().ForEach(ext => this.SupportedExtensions.Add(ext));

            foreach (XmlElement e in doc.SelectNodes("/ccm/fileExtensions/fileExtension"))
            {
                this.SupportedExtensions.Add(e.InnerText);
            }
        }

        private void Parse(XmlDocument doc)
        {
            ParseExcludes(doc);
            ParseAnalyzeFolders(doc);
            ParseRecursiveSetting(doc);
            ParseNumMetrics(doc);
            ParseOutputXML(doc);
            ParseThreshold(doc);
            ParseSuppressSignatureElement(doc);
            ParseSupportedFileExtensions(doc);
            ParseSwitchStatementBehavior(doc);

            if (doc.SelectSingleNode("/ccm/outputXML") != null)
            {
                throw new InvalidOperationException("Configuration element 'outputXML' is invalid. You should now use '<outputter>Xml</outputter>' instead.");
            }

        }

        public List<string> AnalyzeFolders
        {
            get
            {
                return this.analyzeFolders;
            }
        }

        public List<string> ExcludeFolders
        {
            get
            {
                return this.excludeFolders;
            }
        }

        public List<String> ExcludeClasses
        {
            get
            {
                return this.excludeClasses;
            }
        }

        public List<string> ExcludeFiles
        {
            get
            {
                return this.excludeFiles;
            }

        }

        public List<string> ExcludeFunctions
        {
            get
            {
                return this.excludeFunctions;
            }
        }

        public int NumMetrics
        {
            get
            {
                return this.numMetrics;
            }
        }

        public int Threshold
        {
            get
            {
                return this.threshold;
            }
        }

        public bool RecursiveAnalyze
        {
            get
            {
                return this.recursive;
            }
        }

        public string OutputType
        {
            get
            {
                return this.outputType;
            }

        }

        public bool SuppressMethodSignatures
        {
            get { return this.suppressMethodSignatures; }
        }

        public ParserSwitchBehavior SwitchStatementBehavior
        {
            get { return this.switchBehavior; }
        }
    }
}
