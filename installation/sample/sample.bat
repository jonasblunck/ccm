@ECHO OFF
SET BINPATH=%~dp0\..\bin\
SET CONFIG_FILE=%~dp0\sample.config

ECHO ----
ECHO - Configuration file (sample.config) looks like this:
ECHO ----

TYPE "%CONFIG_FILE%"

ECHO ----
ECHO - Running sample analysis on ChainHook library code
ECHO ----

"%BINPATH%\ccm.exe" "%CONFIG_FILE%"

PAUSE