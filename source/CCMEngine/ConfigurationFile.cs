using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;

namespace CCMEngine
{
    public class ConfigurationFile
    {
        public ConfigurationFile(XmlDocument doc)
        {
            Parse(doc);
        }

        private void ParseSwitchStatementBehavior(XmlDocument doc)
        {
            XmlElement recursive = (XmlElement)doc.SelectSingleNode("/ccm/switchStatementBehavior");

            if (null != recursive)
            {
                string setting = recursive.InnerText;

                if (setting.Equals("TraditionalInclude", StringComparison.InvariantCultureIgnoreCase))
                    this.SwitchStatementBehavior = ParserSwitchBehavior.TraditionalInclude;
                else if (setting.Equals("IgnoreCases", StringComparison.InvariantCultureIgnoreCase))
                    this.SwitchStatementBehavior = ParserSwitchBehavior.IgnoreCases;
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
                    this.RecursiveAnalyze = true;
            }
        }

        private void ParseOutputXML(XmlDocument doc)
        {
            XmlElement output = (XmlElement)doc.SelectSingleNode("/ccm/outputter");

            if (null != output)
            {
                this.OutputType = output.InnerText;
            }
        }

        private void ParseNumMetrics(XmlDocument doc)
        {
            XmlElement metrics = (XmlElement)doc.SelectSingleNode("/ccm/numMetrics");

            if (null != metrics)
                this.NumMetrics = int.Parse(metrics.InnerText);
        }

        private void ParseThreshold(XmlDocument doc)
        {
            XmlElement metrics = (XmlElement)doc.SelectSingleNode("/ccm/threshold");

            if (null != metrics)
                this.Threshold = int.Parse(metrics.InnerText);
        }

        private void ParseExcludes(XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("/ccm/exclude");

            if (null != root)
            {
                XmlNodeList fileNodes = root.SelectNodes("file");

                foreach (XmlNode file in fileNodes)
                    this.ExcludeFiles.Add(((XmlElement)file).InnerText);

                XmlNodeList classNodes = root.SelectNodes("class");

                foreach (XmlNode classNode in classNodes)
                    this.ExcludeClasses.Add(((XmlElement)classNode).InnerText);

                XmlNodeList folderNodes = root.SelectNodes("folder");

                foreach (XmlNode folderNode in folderNodes)
                    this.ExcludeFolders.Add(((XmlElement)folderNode).InnerText);

                XmlNodeList functionNodes = root.SelectNodes("function");
                foreach (XmlNode functionNode in functionNodes)
                    this.ExcludeFunctions.Add(((XmlElement)functionNode).InnerText);
            }
        }

        private void ParseAnalyzeFolders(XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("/ccm/analyze");

            if (null != root)
            {
                XmlNodeList folderNodes = root.SelectNodes("folder");

                foreach (XmlNode folder in folderNodes)
                    this.AnalyzeFolders.Add(((XmlElement)folder).InnerText);
            }
        }

        private void ParseSuppressSignatureElement(XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("/ccm/suppressMethodSignatures");

            if (null != root)
            {
                if (root.InnerText.ToLower().Equals("yes") || root.InnerText.ToLower().Equals("1"))
                    this.SuppressMethodSignatures = true;
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
            supportedExtension.Add(".ps1");
            supportedExtension.Add(".psm1");

            return supportedExtension;
        }

        private void ParseSupportedFileExtensions(XmlDocument doc)
        {
            foreach (XmlElement e in doc.SelectNodes("/ccm/fileExtensions/fileExtension"))
            {
                this.SupportedExtensions.Add(e.InnerText);
            }

            if (this.SupportedExtensions.Count() == 0)
            {
                // no extensions supported, so use the default ones
                ConfigurationFile.GetDefaultSupportedFileExtensions().ForEach(ext => this.SupportedExtensions.Add(ext));
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

        public List<string> AnalyzeFolders { get; private set; } = new List<string>();
        public List<string> ExcludeFolders { get; private set; } = new List<string>();
        public List<String> ExcludeClasses { get; private set; } = new List<string>();
        public List<string> ExcludeFiles { get; private set; } = new List<string>();
        public List<string> ExcludeFunctions { get; private set; } = new List<string>();
        public int NumMetrics { get; private set; } = 30;
        public int Threshold { get; private set; }
        public bool RecursiveAnalyze { get; private set; } = false;
        public string OutputType { get; private set; } = "Text";
        public bool SuppressMethodSignatures { get; private set; } = false;
        public ParserSwitchBehavior SwitchStatementBehavior { get; private set; } = ParserSwitchBehavior.IgnoreCases;
        public List<string> SupportedExtensions { get; private set; } = new List<string>();

    }
}
