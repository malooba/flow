!include MUI2.nsh

Name "Flow"
OutFile "Flow.exe"

!include "Sections.nsh"
!include "logiclib.nsh"

RequestExecutionLevel admin

!define SERVICE_ACCOUNT ""
!define SERVICE_ACCOUNT_PASSWORD ""

var name
var description
var path
var tmp
var chkbox
var installAsServices

!macro stopAndRemoveService name description path
  !define UniqueID ${__LINE__}
  StrCpy $name "${name}"
  StrCpy $description "${description}"
  StrCpy $path "${path}"
  
  SimpleSC::ExistsService $name
  Pop $tmp
  IntCmp $tmp 0 +1 done${UniqueID} done${UniqueID}
  DetailPrint "Service $name already exists"
  
  SimpleSC::ServiceIsStopped $name
  Pop $tmp
  Pop $tmp
  IntCmp $tmp 0 +1 stopped${UniqueID} stopped${UniqueID}
  DetailPrint "Service $name is running"
  
  SimpleSC::StopService $name 1 30
  Pop $tmp
  DetailPrint "Service $name has been stopped"
  
stopped${UniqueID}:
  SimpleSC::RemoveService $name
  Pop $tmp
  DetailPrint "Service $name has been removed"
  
done${UniqueID}:
  !undef UniqueID
!macroend

!macro installService name description path
  StrCpy $name "${name}"
  StrCpy $description "${description}"
  StrCpy $path "${path}"
  ${If} $installAsServices == ${BST_CHECKED}
    SimpleSC::InstallService $name $description 16 2  $path "" "${SERVICE_ACCOUNT}" "${SERVICE_ACCOUNT_PASSWORD}"
    Pop $tmp
    DetailPrint "Service $name has been installed"
  ${EndIf}
!macroend

InstallDir $PROGRAMFILES\Malooba\Flow

ShowInstDetails show
ShowUnInstDetails show

!define MUI_ICON "Flow.ico"
!define MUI_UNICON "Flow.ico"

!insertmacro MUI_PAGE_DIRECTORY

#!insertmacro MUI_PAGE_COMPONENTS

page Custom InstallAsServicesDialogue

!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

# Debug or Release Build
!define BUILD "Debug"

Section 
  SectionIn RO
  SetOutPath "$instdir"
  WriteUninstaller "uninstall.exe"
SectionEnd

Section
  SectionIn RO
  !insertmacro stopAndRemoveService "FlowCore" "Flow Core" "$INSTDIR\FlowCore\FlowCore.exe"
  !insertmacro stopAndRemoveService "FlowDecider" "Flow Decider" "$INSTDIR\FlowDecider\FlowDecider.exe"
SectionEnd

Section "FlowCore" FlowCore
  SectionIn RO
  SetOutPath "$INSTDIR\FlowCore"
  File "FlowCore\bin\${BUILD}\FlowCore.exe"
  File "FlowCore\bin\${BUILD}\FlowCore.exe.config"
  File "FlowCore\bin\${BUILD}\FlowShared.dll"
  File "FlowCore\bin\${BUILD}\Nancy.dll"
  File "FlowCore\bin\${BUILD}\Nancy.Hosting.Self.dll"
  File "FlowCore\bin\${BUILD}\Newtonsoft.Json.dll"
  File "FlowCore\bin\${BUILD}\log4net.dll"
  
  StrCmp ${BUILD} "Release" release 0
  File "FlowCore\bin\${BUILD}\FlowCore.pdb"
  File "FlowCore\bin\${BUILD}\FlowShared.pdb"
  release:
  !insertmacro installService "FlowCore" "FlowCore" "$INSTDIR\FlowCore\FlowCore.exe"
SectionEnd

Section "FlowDecider" FlowDecider
  SectionIn RO
  SetOutPath "$INSTDIR\FlowDecider"
  File "FlowDecider\bin\${BUILD}\FlowDecider.exe"
  File "FlowDecider\bin\${BUILD}\FlowDecider.exe.config"
  File "FlowDecider\bin\${BUILD}\FlowShared.dll"
  File "FlowDecider\bin\${BUILD}\Newtonsoft.Json.dll"
  File "FlowDecider\bin\${BUILD}\log4net.dll"

  StrCmp ${BUILD} "Release" release 0
  File "FlowDecider\bin\${BUILD}\FlowDecider.pdb"
  File "FlowDecider\bin\${BUILD}\FlowShared.pdb"
  release:
  !insertmacro installService "FlowDecider" "Flow Decider" "$INSTDIR\FlowDecider\FlowDecider.exe"
SectionEnd

Section
  SectionIn RO
  SetOutPath "$INSTDIR"
  File "Flow.ps1"
  FileOpen $0 "$InstDir\processes.txt" w
  ${If} $installAsServices == ${BST_CHECKED}
    FileWrite $0 "#SERVICES$\r$\n"
  ${Else}
    FileWrite $0 "#EXECUTABLES$\r$\n"
  ${EndIf}
  FileWrite $0 "# Comment lines to prevent installed modules from starting/stopping$\r$\n"
  
  ${If} $installAsServices == ${BST_CHECKED}
    FileWrite $0 "FlowCore$\r$\n"
  ${Else}
    FileWrite $0 "$InstDir\FlowCore\FlowCore.exe$\r$\n"
  ${EndIf}

  ${If} $installAsServices == ${BST_CHECKED}
    FileWrite $0 "FlowDecider$\r$\n"
  ${Else}
    FileWrite $0 "$InstDir\FlowDecider\FlowDecider.exe$\r$\n"
  ${EndIf}

  FileClose $0
  SetOutPath "$INSTDIR\FlowWorkflow"
  File "Flow.ico"
  SetOutPath "$INSTDIR"
  CreateShortcut "Start.lnk" "Powershell.exe" `-command "&{Set-Location '$INSTDIR'; & '$INSTDIR\Flow.ps1' -start}"` "$INSTDIR\FlowWorkflow\Flow.ico" 0
  CreateShortcut "Stop.lnk" "Powershell.exe" `-command "&{Set-Location '$INSTDIR'; & '$INSTDIR\Flow.ps1' -stop}"` "$INSTDIR\FlowWorkflow\Flow.ico" 0
  CreateShortcut "Restart.lnk" "Powershell.exe" `-command "&{Set-Location '$INSTDIR'; & '$INSTDIR\Flow.ps1' -restart}"` "$INSTDIR\FlowWorkflow\Flow.ico" 0
SectionEnd

Section "Uninstall"
  SectionIn RO
  !insertmacro stopAndRemoveService "FlowCore" "Flow Core" "$INSTDIR\FlowCore\FlowCore.exe"
  !insertmacro stopAndRemoveService "FlowDecider" "Flow Decider" "$INSTDIR\FlowDecider\FlowDecider.exe"
  RMDir /r /REBOOTOK "$INSTDIR"
  Delete "$INSTDIR\uninstall.exe"
SectionEnd

Function InstallAsServicesDialogue
  Push $R0
  Push $R1
  StrCpy $installAsServices ${BST_CHECKED}
  
  nsDialogs::Create 1018
  Pop $tmp

  ${NSD_CreateCheckBox} 0 12% 40% 6% "Install as services"
  Pop $chkbox
  ${NSD_SetState} $chkbox $installAsServices
  ${NSD_OnClick} $chkbox SetInstallAsServices

  nsDialogs::Show
  Pop $R1
  Pop $R0
FunctionEnd

Function SetInstallAsServices
  ${NSD_GetState} $chkbox $installAsServices
FunctionEnd
