CCM
===

## Overview

CCM is a tool that analyzes c, c++, c#, javascript and TypeScript code and reports back with cyclomatic complexity metric.

There are two parts included with the installer:

* CCM.exe; a command line executable that will analyze code bases and report back with cyclomatic complexity metrics.
* vsCCM; a Visual Studio 2008, 2010 and 2012 add-in that will add a new toolbar for integrating CCM with Visual Studio.

## Command line usage
To use CCM.exe, simply use one of the two modes for invocation:

* CCM.exe ```<path-to-config-file>```
* CCM.exe ```<folder-path-to-analyze>``` ```[/xml]``` ```[/ignorecases]``` ```[/threshold=5]``` ```[/nummetrics=10]``` ```[v]``` 

### Arguments
```
   <path-to-config-file>    Path to configuration file. See structure of file below. 
                            This option gives the best control over how ccm behaves.
   <folder-path-to-analyze> Path to directory which ccm will analyze.
   /xml                     Output results as XML (this parameter only valid if a config file is not passed in).
   /ignorecases             Don't count each case in switch block as a branching point (only valid if config file is not passed in).
   /threshold=5             Ignore units with metrics lower than assigned value.
   /nummetrics=10           Only report top 10 metrics (see numMetrics element in configuration file)
   /v                       Print the version number of ccm.exe.
```

### Configuration file
Below is an example of a configuration file.

```
 <ccm>
    <exclude>
     <file>myfile.cpp</file>
     <folder>myfolder</folder>
    </exclude>
    <analyze>
      <folder>..\..\code</folder>
   </analyze>
   <recursive>yes</recursive>
   <outputter>XML</outputter>
   <suppressMethodSignatures>yes</suppressMethodSignatures>
   <switchStatementBehavior>IgnoreCases</switchStatementBehavior>
   <numMetrics>30</numMetrics>
   <threshold>6</threshold>
   <fileExtensions>
     <fileExtension>.cxx</fileExtension>
   </fileExtensions>
 </ccm>
```

* ```<exclude>``` element can be used to exclude files and/or folders from analysis.
* ```<analyze>``` element specified which folders to analyze. All paths in the ```<folder>``` element is relative to the location of the configuration file.
* ```<recursive>``` element tells CCM to traverse folders or not.
* ```<outputter>``` element tells CCM how to output the data. Valid values are 'XML', 'Tabbed', 'Text', 'CSV'
* ```<suppressMethodSignatures>``` set to 'yes' and CCM will only print the name of the method and not the full signature.
* ```<switchStatementBehavior>``` set to 'IgnoreCases' and CCM will not count each case statement in switch blocks as a branching point.
* ```<numMetrics>``` tells CCM how many metrics that should be reported. Only the top x functions will be reported.
* ```<threshold>``` tells CCM to ignore units with a complexity less than configured value.
* ```<fileExtensions>``` can be used to add additional file extensions for analysis. Per default, these are included: .h, .cpp, .c, .hpp, .cs, .js and .ts 

### Example output
Below is example output from the Text outputter (can be contolled in the ```<outputter>``` element in the config file).
```
Driver::HandleDirectory(string basePath,string path) : 7 - simple, without much risk (\Driver.cs@line 141)
Driver::IsValidFile(string filename) : 6 - simple, without much risk (\Driver.cs@line 84)
Program::CreateConfigurationFromArgs(string [ ] args) : 6 - simple, without much risk (\Program.cs@line 71)
Driver::PathShouldBeExcluded(string path) : 5 - simple, without much risk (\Driver.cs@line 109)
XmlOutputter::Output(List<ccMetric>metrics,List<ErrorInfo>errors,bool verbose) : 5 - simple, without much risk (\XmlOutputter.cs@line 17)
ConsoleOutputter::Output(List<ccMetric>metrics,List<ErrorInfo>errors,bool verbose) : 4 - simple, without much risk (\ConsoleOutputter.cs@line 12)
Driver::AnalyzeFilestream(object context) : 4 - simple, without much risk (\Driver.cs@line 47)
TabbedOutputter::Output(List<ccMetric>metrics,List<ErrorInfo>errors,bool verbose) : 4 - simple, without much risk (\TabbedOutputter.cs@line 12)
Program::OutputterFactory(string outputType) : 3 - simple, without much risk (\Program.cs@line 60)
Program::ValidateArgs(string [ ] args) : 2 - simple, without much risk (\Program.cs@line 15)
Program::LoadConfiguration(string [ ] args) : 2 - simple, without much risk (\Program.cs@line 97)
Program::Main(string [ ] args) : 2 - simple, without much risk (\Program.cs@line 122)
```

## Platforms
CCM is built in C#, targetting .NET 3.5 and as such runs on Windows. The commandline version, CCM.exe, can run on Linux distributions using the mono framework (http://www.monoproject.org).

## Building the code

To build installer, follow these steps:

1. Open ccm.sln in Visual Studio 2012
2. Build release
3. Run unit tests (optional)
4. Right click on install\setup.nsi and choose 'Compile NSIS Script'
5. vsCCM.exe installer is now available in the install folder.

NOTE that you need NSIS (http://nsis.sourceforge.net) to be able to build the installer.
