@ECHO OFF
SET ROOT=%~dp0
DEL  /Q %ROOT%bin\*.dll

SET PATH=%PATH%;%~dp0
SET VS = "%PROGRAMFILES%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe"
SET VSX86 = "%PROGRAMFILES(x86)%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe"

IF EXIST "%VS%" (
 START %VS% %ROOT%CCM.sln
) ELSE (
 START %VSX86% %ROOT%CCM.sln
)

