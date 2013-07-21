CCM
===

## Overview

CCM is a tool that analyzes c, c++, c#, javascript and TypeScript code and reports back with cyclomatic complexity metric.

There are two parts included with the installer:

* CCM.exe; a command line executable that will analyze code bases and report back with cyclomatic complexity metrics.
* vsCCM; a Visual Studio 2008, 2010 and 2012 add-in that will add a new toolbar for integrating CCM with Visual Studio.

## Command line usage
To CCM.exe, simply use one of the two parameters:

* CCM.exe <path-to-config-file>
* CCM.exe <folder-path-to-analyze>

Note that if you want to override any default parameters, such as output format, number of metrics to be displayed, etc, you need to pass in a config file (see below).

# Configuration file
Below is an example of a configuration file.

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
   <numMetrics>30</numMetrics>
   <fileExtensions>
     <fileExtension>.cxx</fileExtension>
   </fileExtensions>
 </ccm>

* <exclude> element can be used to exclude files and/or folders from analysis.
* <analyze> element specified which folders to analyze. All paths in the <folder> element is relative to the location of the configuration file.
* <recursive> element tells CCM to traverse folders or not.
* <outputter> element tells CCM how to output the data. Valid values are 'XML', 'Tabbed', 'Text'.
* <suppressMethodSignatures> set to 'yes' and CCM will only print the name of the method and not the full signature.
* <numMetrics> tells CCM how many metrics that should be reported. Only the top x functions will be reported.
* <fileExtensions> can be used to add additional file extensions for analysis. Per default, these are included: .h, .cpp, .c, .hpp, .cs, .js and .ts 


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