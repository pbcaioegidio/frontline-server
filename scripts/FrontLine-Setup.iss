; FrontLine — instalador Windows (Inno Setup 6)
; Compilado por scripts\pack-player-setup.ps1 (defines Fl*).

#ifndef FlSource
  #define FlSource "C:\Users\pbcai\AppData\Local\Temp\FrontLine-Setup-stage"
#endif
#ifndef FlOutDir
  #define FlOutDir "C:\Users\pbcai\Downloads\source\dist"
#endif
#ifndef FlOutName
  #define FlOutName "Instalador-FrontLine"
#endif
#ifndef FlVersion
  #define FlVersion "1.0.0"
#endif
#ifndef FlMode
  #define FlMode "Full"
#endif
#ifndef FlIcon
  #define FlIcon "C:\Users\pbcai\Downloads\source\docs\frontline-setup.ico"
#endif

#define MyAppName "FrontLine"
#define MyAppPublisher "FrontLine"
#define MyAppURL "https://frontline.local"
#define MyAppExeName "FLLauncher.exe"
#define MySetupTitle "Instalador FrontLine"

[Setup]
AppId={{A8F3C2E1-9B47-4D6A-8E21-7C4B91F0D2A6}
AppName={#MyAppName}
AppVersion={#FlVersion}
AppVerName={#MySetupTitle} {#FlVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppCopyright=Copyright (C) 2026 FrontLine
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#FlOutDir}
OutputBaseFilename={#FlOutName}
SetupIconFile={#FlIcon}
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
; Metadados (Propriedades → Detalhes)
VersionInfoVersion={#FlVersion}.0
VersionInfoProductVersion={#FlVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright=Copyright (C) 2026 FrontLine
VersionInfoDescription={#MySetupTitle}
VersionInfoProductName={#MyAppName}
VersionInfoOriginalFileName={#FlOutName}.exe
VersionInfoTextVersion={#FlVersion}
; Assinatura digital: só se existir certificado (ver pack-player-setup.ps1 -Sign)
#ifdef FlSign
  SignTool=FrontLineSign
  SignedUninstaller=yes
#endif
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
WizardSizePercent=120
AllowNoIcons=yes
MinVersion=10.0
DiskSpanning=no
CloseApplications=yes
RestartApplications=no
UsePreviousAppDir=yes
DirExistsWarning=auto
DisableDirPage=no
DisableReadyMemo=no
ShowLanguageDialog=no

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de trabalho"; GroupDescription: "Atalhos:"; Flags: checkedonce

[Files]
Source: "{#FlSource}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\Configurar (FLConfig)"; Filename: "{app}\FLConfig.exe"; WorkingDir: "{app}"; Check: FileExists(ExpandConstant('{app}\FLConfig.exe'))
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir {#MyAppName} agora"; Flags: nowait postinstall skipifsilent; WorkingDir: "{app}"

[UninstallDelete]
Type: filesandordirs; Name: "{app}\CEF\Cache"
Type: filesandordirs; Name: "{app}\Evidence"
Type: files; Name: "{app}\*.log"
