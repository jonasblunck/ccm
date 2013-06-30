@ECHO OFF
SET BINPATH=%~dp0\..\bin\
SET ANALYZE_FOLDER=%~dp0\code


ECHO ----
ECHO - Running sample analysis on ChainHook library code
ECHO ----

"%BINPATH%\ccm.exe" "%ANALYZE_FOLDER%"

PAUSE