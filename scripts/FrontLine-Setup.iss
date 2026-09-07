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
; Sempre Program Files (todos os usuarios). UAC uma vez na instalacao — nao mostra escolha.
; Depois do install, libera escrita em {app} para Users (Update sem admin).
PrivilegesRequired=admin
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

[Messages]
; Textos do assistente (PT-BR)
WelcomeLabel1=Bem-vindo ao instalador do FrontLine
WelcomeLabel2=Isto instala o jogo e o launcher em Program Files.%n%nO instalador pede administrador só agora. Depois o Update do launcher funciona sem pedir admin de novo.%n%nClique em Avancar para continuar.
FinishedHeadingLabel=FrontLine instalado
FinishedLabel=Pronto. Abra o FrontLine pelo atalho e faca login.%n%nSe o servidor pedir atualizacao, use Update no launcher (sem precisar de administrador).
ClickFinish=Clique em Concluir para sair do instalador.

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de trabalho"; GroupDescription: "Atalhos:"; Flags: checkedonce

; Pasta do app + staging de patch: Users podem modificar (Update sem UAC)
[Dirs]
Name: "{app}"; Permissions: users-modify
Name: "{app}\_DownloadPatchFiles"; Permissions: users-modify

[Files]
Source: "{#FlSource}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\Configurar (FLConfig)"; Filename: "{app}\FLConfig.exe"; WorkingDir: "{app}"; Check: FileExists(ExpandConstant('{app}\FLConfig.exe'))
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
; SID S-1-5-32-545 = BUILTIN\Users (funciona em Windows PT-BR). (OI)(CI)M = modificar + herança.
Filename: "{sys}\icacls.exe"; Parameters: """{app}"" /grant *S-1-5-32-545:(OI)(CI)M /T"; StatusMsg: "Liberando pasta para Update sem administrador..."; Flags: runhidden waituntilterminated
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir {#MyAppName} agora"; Flags: nowait postinstall skipifsilent; WorkingDir: "{app}"

[UninstallDelete]
Type: filesandordirs; Name: "{app}\CEF\Cache"
Type: filesandordirs; Name: "{app}\Evidence"
Type: filesandordirs; Name: "{app}\_DownloadPatchFiles"
Type: files; Name: "{app}\*.log"
