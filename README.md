## Overview

CCM is a tool that analyzes c, c++, c#, javascript and TypeScript code and reports back with cyclomatic complexity metric.

There are two parts included with the installer:

* CCM.exe; a command line executable that will analyze code bases and report back with cyclomatic complexity metrics.
* vsCCM; a Visual Studio 2008, 2010 and 2012 add-in that will add a new toolbar for integrating CCM with Visual Studio.

## Command line usage
To analyze 



## Building the code

To build installer, follow these steps:

1. Open ccm.sln in Visual Studio 2012
2. Build release
3. Run unit tests (optional)
4. Right click on install\setup.nsi and choose 'Compile NSIS Script'
5. vsCCM.exe installer is now available in the install folder.

NOTE that you need NSIS (http://nsis.sourceforge.net) to be able to build the installer.