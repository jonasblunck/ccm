CCM is a tool that analyzes c, c++, c#, javascript and TypeScript code and reports back with cyclomatic complexity metric.

This is the source code for the ccm tool available here: http://www.blunck.se/ccm.html

To build installer, follow these steps:

1. Open ccm.sln in Visual Studio 2012
2. Build release
3. Run unit tests
4. Right click on install\setup.nsi and choose 'Compile NSIS Script'
5. vsCCM.exe installer is now available in the install folder.

NOTE that you need NSIS (http://nsis.sourceforge.net) to be able to build the installer.