;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "ccm"
  OutFile "vsCCM.exe"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\ccm"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\ccm" ""

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING
  !define MUI_FINISHPAGE_SHOWREADME "$INSTDIR\Documentation\readme.doc"
  
;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Binaries" Required
  SectionIn RO
  SetOutPath "$INSTDIR\bin"
  
  File "..\bin\ccm.config"
  File "..\bin\CCM.exe"
  File "..\bin\CCMEngine.dll"
  File "..\bin\ccmUI.exe"
  File "..\bin\vsCCM.dll"

  SetOutPath "$INSTDIR\Documentation"
  File "readme.doc"

  SetOutPath $INSTDIR\sample
  File /r "sample\*.h"
  File /r "sample\*.cpp"
  File /r "sample\*.config"
  File /r "sample\*.bat"

  ;Store installation folder
  WriteRegStr HKCU "Software\ccm" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

; optional section
Section "Visual Studio Integration" VSIntegration

  SetOutPath "$INSTDIR\Integration"
  File "deployaddin.bat"
  File "..\bin\CCMEngine.dll"
  File "..\bin\vsCCM_2008.addin"
  File "..\bin\vsCCM_2010.addin"
  File "..\bin\vsCCM_2012.addin"
  File "..\bin\vsCCM.dll"
  File "..\bin\ccm.config"
  
  Exec $INSTDIR\Integration\deployaddin.bat
SectionEnd

; optional section
Section "Start Menu items" StartMenuItems
  CreateDirectory "$SMPROGRAMS\ccm"
  CreateDirectory "$SMPROGRAMS\ccm\Tools"
  CreateShortCut "$SMPROGRAMS\ccm\Cyclomatic Complexity Analyzer.lnk" "$INSTDIR\bin\ccmUI.exe" 

  CreateShortCut "$SMPROGRAMS\ccm\Tools\Getting started.lnk" "$INSTDIR\Documentation\readme.doc" 
  CreateShortCut "$SMPROGRAMS\ccm\Tools\Web site.lnk" "http://www.blunck.info/ccm.html" 
  CreateShortCut "$SMPROGRAMS\ccm\Tools\Sample.lnk" "$INSTDIR\sample\sample.bat" 

  SetOutPath "$INSTDIR\bin"
  CreateShortCut "$SMPROGRAMS\ccm\Tools\Command prompt.lnk" "cmd.exe" "$INSTDIR" 
SectionEnd


;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...
  Delete "$INSTDIR\Uninstall.exe"

  RMDir /r "$INSTDIR"
  RMDir /r "$SMPROGRAMS\ccm"

  DeleteRegKey /ifempty HKCU "Software\ccm"

SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_Required ${LANG_ENGLISH} "Required binaries will be installed."
  LangString DESC_VSIntegration ${LANG_ENGLISH} "Install Visual Studio 2008/2010 addin."
  LangString DESC_StartMenuItems ${LANG_ENGLISH} "Create start menu shortcuts."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${Required} $(DESC_Required)
    !insertmacro MUI_DESCRIPTION_TEXT ${VSIntegration} $(DESC_VSIntegration)
    !insertmacro MUI_DESCRIPTION_TEXT ${StartMenuItems} $(DESC_StartMenuItems)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

