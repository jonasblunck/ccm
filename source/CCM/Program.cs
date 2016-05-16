using System;
using System.Collections.Generic;
using System.Text;
using CCMEngine;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Linq;

namespace CCM
{
    public class Program
    {
        static private void ValidateArgs(string[] args)
        {
            if (args.Length == 0)
                throw new Exception("Argument error.");
        }

        static private void PrintUsage()
        {
            string usage =
              "\r\nccm will analyze you code base and provide cyclomatic complexity metrics. \r\n" +
              "Supported languages are c/c++ (.c, .cpp, .h, .hpp), c# (.cs) and javascript (.js)\r\n" +
              "\r\n\r\nUsage:\r\n" +
              "  ccm [config-file] \r\n" +
              "  ccm [path-to-analyze] [/xml] [/v] [/ignorecases] [/threshold=5] [/nummetrics=10] \r\n\r\n" +
              "    config-file        Path to configuration file (see below for structure of file).\r\n" +
              "                       Using a configuration file provides more control, such as analyzing multiple folders,\r\n" +
              "                       excluding folders and files and controlling number of metrics outputted.\r\n" +
              "    path-to-analyze    Provide a path to source code for analysis.\r\n" +
              "                       This will be analyzed recursively and 30 worst metrics outputted.\r\n" +
              "    xml                Add /xml if you want output in xml.\r\n" +
              "    v                  Add /v if you want ccm-version to be printed to console.\r\n" +
              "    ignorecases        Don't count each case in a switch as additional branch.\r\n" +
              "    threshold          Don't report metrics less than the threshold.\r\n" +
              "    nummetrics         Report only top <X> metrics (see numMetrics element in config file).\r\n" +
              " \r\n" +
              "  When ccm operates on a configuration file, all other parameters are ignored. \r\n" +
              "  Structure of configuration file should be:\r\n\r\n" +
              "  <ccm> \r\n " +
              "    <exclude>\r\n" +
              "      <file>myfile.cpp</file>\r\n" +
              "      <folder>myfolder</folder>\r\n " +
              "    </exclude>\r\n " +
              "    <analyze>\r\n " +
              "      <folder>..\\..\\code</folder>\r\n" +
              "    </analyze>\r\n" +
              "    <recursive>yes</recursive>\r\n" +
              "    <outputter>XML|Tabbed|Text|CSV</outputter>\r\n" +
              "    <numMetrics>30</numMetrics>\r\n" +
              "    <threshold>5</threshold>\r\n" +
              "    <switchStatementBehavior>TraditionalInclude|IgnoreCases</switchStatementBehavior>\r\n" +
              "    <fileExtensions>\r\n" +
              "      <fileExtension>.cxx</fileExtension>\r\n" +
              "    </fileExtensions>\r\n" +
              "  </ccm>\r\n" +
              " \r\n" +
              " \r\n";

            Console.WriteLine(usage);
        }

        static CCMOutputter OutputterFactory(string outputType)
        {
            if (outputType.Equals(CCMOutputter.TabbedOutputType, StringComparison.OrdinalIgnoreCase))
                return new TabbedOutputter();

            if (outputType.Equals(CCMOutputter.XmlOutputType, StringComparison.OrdinalIgnoreCase))
                return new XmlOutputter();

            if (outputType.Equals(CCMOutputter.CSVOutputType, StringComparison.OrdinalIgnoreCase))
                return new CSVOutputter();

            return new ConsoleOutputter();
        }

        public static XmlDocument CreateConfigurationFromArgs(string[] args)
        {
            int numMetrics = 30;

            StringBuilder sb = new StringBuilder();
            sb.Append("<ccm><analyze><folder>");
            sb.Append(args[0]);

            sb.Append("</folder></analyze><recursive>yes</recursive>");

            foreach (string arg in args.Where(a => a != args.First()))
            {
                if (arg.Equals("/xml"))
                    sb.Append(string.Format("<outputter>{0}</outputter>", CCMOutputter.XmlOutputType));
                else if (arg.Equals("/tabbedoutput", StringComparison.OrdinalIgnoreCase))
                    sb.Append(string.Format("<outputter>{0}</outputter>", CCMOutputter.TabbedOutputType));
                else if (arg.Contains("/threshold="))
                {
                    int threshold = int.Parse(arg.Split('=').Last());
                    sb.Append(string.Format("<threshold>{0}</threshold>", threshold));
                }
                else if (arg.Contains("/nummetrics="))
                    numMetrics = int.Parse(arg.Split('=').Last());
                else if (arg.Equals("/ignorecases"))
                    sb.Append("<switchStatementBehavior>IgnoreCases</switchStatementBehavior>");
                else if (arg.Equals("/version") || arg.Equals("/v"))
                    Console.WriteLine("ccm version is: {0}.", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                else
                    throw new ArgumentException(string.Format("Unknown parameter: {0}", arg));
            }

            sb.Append(string.Format("<numMetrics>{0}</numMetrics>", numMetrics));
            sb.Append("</ccm>");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sb.ToString());

            return doc;
        }

        public static XmlDocument LoadConfiguration(string[] args)
        {
            XmlDocument doc = new XmlDocument();

            if (File.Exists(args[0]))
            {
                //
                // set current path to the location of the configuration file
                //
                string configurationFile = Path.GetFullPath(args[0]);
                string filename = Path.GetFileName(configurationFile);
                string path = configurationFile.Substring(0, configurationFile.Length - filename.Length);

                Environment.CurrentDirectory = path;
                doc.Load(configurationFile);
            }
            else
            {
                doc = CreateConfigurationFromArgs(args);
            }

            return doc;

        }

        static void Main(string[] args)
        {
            try
            {
                Program.ValidateArgs(args);

                XmlDocument doc = Program.LoadConfiguration(args);
                ConfigurationFile config = new ConfigurationFile(doc);

                Driver driver = new Driver(config);
                driver.Drive();

                OutputterFactory(config.OutputType).Output(driver.Metrics, driver.Errors, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                PrintUsage();
            }

        }
    }
}
