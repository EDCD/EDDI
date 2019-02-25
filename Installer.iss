; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "EDDI"
#define MyAppVersion "3.3.7-a2"
#define MyAppPublisher "Elite Dangerous Community Developers (EDCD)"
#define MyAppURL "https://github.com/EDCD/EDDI/"
#define MyAppExeName "EDDI.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AllowUNCPath=no
AppId={{830C0324-30D8-423C-B5B4-D7EE8D007A79}
AppName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppVerName={#MyAppName} {#MyAppVersion}
AppVersion={#MyAppVersion}
DefaultDirName={reg:HKCU\Software\VoiceAttack.com\VoiceAttack,InstallPath|{pf32}\VoiceAttack}\Apps\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableDirPage=no
DisableWelcomePage=no
LicenseFile="{#SourcePath}\LicenseFile.txt"
OutputBaseFilename={#MyAppName}-{#MyAppVersion}
OutputDir="{#SourcePath}\bin\Installer"
SolidCompression=yes
SourceDir="{#SourcePath}\bin\Release"
UninstallDisplayIcon={app}\{#MyAppExeName}
UsePreviousTasks=no
WizardImageFile={#SourcePath}\graphics\logo.bmp
WizardSmallImageFile={#SourcePath}\graphics\logo.bmp

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "EDDI.exe"; DestDir: "{app}"; Flags: ignoreversion

Source: "x86\*.*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs replacesameversion
Source: "*.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

Source: "*.resources.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs replacesameversion

Source: "eddi.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "eddi.*.json"; DestDir: "{app}"; Flags: ignoreversion

Source: "ChangeLog.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "Help.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "Variables.md"; DestDir: "{app}"; Flags: ignoreversion

Source: "EDDI.vap"; DestDir: "{app}"; Flags: ignoreversion

Source: "EddiDataProviderService.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "EDDI.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "EddiVoiceAttackResponder.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "EddiGalnetMonitor.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "System.Data.SQLite.dll.config"; DestDir: "{app}"; Flags: ignoreversion

; Remove outdated files
[InstallDelete]
Type: files; Name: "{app}\Eddi.exe"
Type: files; Name: "{app}\EDDI.ico"
Type: files; Name: "{app}\EddiNetLogMonitor.dll"
Type: files; Name: "{app}\Newtonsoft.Json.xml"
Type: files; Name: "{app}\CommonMark.xml"
Type: files; Name: "{app}\Exceptionless.Wpf.xml"
Type: files; Name: "{app}\Exceptionless.xml"
Type: files; Name: "{app}\MathNet.Numerics.xml"
Type: files; Name: "{app}\System.Data.SQLite.xml"
Type: files; Name: "{app}\SimpleFeedReader.xml"
Type: files; Name: "{app}\CSCore.xml"
Type: files; Name: "{app}\RestSharp.xml"
Type: files; Name: "{app}\EntityFramework.SqlServer.xml"
Type: files; Name: "{app}\EntityFramework.xml"
Type: files; Name: "{userappdata}\EDDI\credentials.json"

; Remove sensitive data on uninstall
[UninstallDelete]
Type: files; Name: "{userappdata}\EDDI\CompanionAPI.json"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[ThirdParty]
UseRelativePaths=True

[Messages]
SelectDirBrowseLabel=To continue, click Next. If this is not your VoiceAttack installation location, or you would like to put the EDDI files in a different location, click Browse.

[Registry]
Root: "HKLM"; Subkey: "Software\Classes\eddi"; ValueType: string; ValueData: "EDDI URL Protocol"; Flags: uninsdeletekey
Root: "HKLM"; Subkey: "Software\Classes\eddi"; ValueType: string; ValueName: "URL Protocol"; Flags: uninsdeletekey
Root: "HKLM"; Subkey: "Software\Classes\eddi\Default Icon"; ValueType: string; ValueData: "{app}\{#MyAppExeName},0"; Flags: uninsdeletekey
Root: "HKLM"; Subkey: "Software\Classes\eddi\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey
Root: "HKLM"; Subkey: "Software\Classes\eddi\shell\open\ddeexec"; ValueType: string; ValueData: "%1"; Flags: uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\eddi"; ValueType: string; ValueData: "EDDI URL Protocol"; Flags: uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\eddi"; ValueType: string; ValueName: "URL Protocol"; Flags: uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\eddi\Default Icon"; ValueType: string; ValueData: "{app}\{#MyAppExeName},0"; Flags: uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\eddi\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\eddi\shell\open\ddeexec"; ValueType: string; ValueData: "%1"; Flags: uninsdeletekey
