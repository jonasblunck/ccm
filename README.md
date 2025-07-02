CCM
===

## Overview

CCM is a tool that analyzes c, c++, c#, javascript, TypeScript and PowerShell code and reports back with cyclomatic complexity metric. 

## Command line usage
To use CCM.exe, simply use one of the two modes for invocation:

* CCM.exe ```<path-to-config-file>```
* CCM.exe ```<folder-path-to-analyze>``` ```[/xml]``` ```[/ignorecases]``` ```[/threshold=5]``` ```[/nummetrics=10]```

### Arguments
```
   <path-to-config-file>    Path to configuration file. See structure of file below. 
                            This option gives the best control over how ccm behaves.
   <folder-path-to-analyze> Path to directory which ccm will analyze.
   /xml                     Output results as XML (this parameter only valid if a config file is not passed in).
   /ignorecases             Don't count each case in switch block as a branching point (only valid if config file is not passed in).
   /threshold=5             Ignore units with metrics lower than assigned value.
   /nummetrics=10           Only report top 10 metrics (see numMetrics element in configuration file)
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
* ```<fileExtensions>``` can be used to add additional file extensions for analysis. Per default, these are included: .h, .cpp, .c, .hpp, .cs, .js, .ts, .psm1 and .ps1

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
CCM is built in C#, targetting .NET 8, and as such runs on Windows, Mac and other platform supported by .NET Core.  

## Building the code

1. Install .NET 8 SDK and runtime
2. Build by running: 'dotnet build'
3. Run tests by running: 'dotnet test'

## Running the tool on Mac OS X 

### Different options
1. ./bin/Release/net8.0/ccm (this will display the help)
1. ./bin/Release/net8.0/ccm ./bin/ccm.config (will run based on the ccm.config file)
2. ./bin/Release/net8.0/ccm ./source (run the analyzer for the source folder)

## Running the tool on Windows 

### Different options
1. .\bin\Release\net8.0\CCM.exe (this will display the help)
2. .\bin\Release\net8.0\CCM.exe .\bin\ccm.config (will run based on the ccm.config file)
3. .\bin\Release\net8.0\CCM.exe .\source (run the analyzer for the source folder)

## Integration into build pipelines
CCM can easily be integrated into build pipelines by using the xml output option. Example:

```
$output = ./bin/Release/net8.0/ccm <path-to-analyze> /xml /threshold=11
$xml = [xml]($output)
$xml.ccm.metric | %{  
   Write-Error "$($_.unit) has complexity $($_.complexity) and is classified as '$($_.classification)'"
}
```