; =====================================================================
;  CoBu installer (Inno Setup 6.3 or newer)
;
;  Build:  powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
;          (publishes the app, then compiles this script into dist\CoBu-Setup-<version>.exe)
;
;  What it does:
;    * checks for the .NET 10 Desktop Runtime and, if it is missing, downloads it from
;      Microsoft (signature verified) and installs it - or sends the user to the download page
;    * optional checkboxes: autostart with Windows (silent, straight to the tray),
;      desktop shortcut
;    * per-user install (no admin rights needed, the app keeps its history next to the exe)
;    * dark "frosted glass" wizard in the colors of the app
; =====================================================================

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\dist\publish"
#endif

#define AppName   "CoBu"
#define AppExe    "CoBu.exe"
#define RunKey    "Software\Microsoft\Windows\CurrentVersion\Run"

[Setup]
AppId={{8E3B5A52-4C1D-4F2A-9B7E-C0B0A7E1A5D3}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=CoBu
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
; Install mode is chosen at startup: "just for me" (default, no admin rights, {autopf} = %LocalAppData%\Programs)
; or "for all users" (UAC prompt, {autopf} = C:\Program Files). In both cases CoBu keeps its history (CoBu_Storage)
; right next to CoBu.exe; in the all-users mode that folder is made writable for ordinary users (see [Dirs]).
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
; The autostart entry is written to HKCU on purpose (the app's own Settings toggle uses the same key)
UsedUserAreasWarning=no
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\dist
OutputBaseFilename=CoBu-Setup
SetupIconFile=..\src\CoBu\app.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
VersionInfoVersion={#AppVersion}
VersionInfoProductName={#AppName}
VersionInfoDescription={#AppName} Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
WizardImageFile=assets\wizard-large.bmp,assets\wizard-large-125.bmp,assets\wizard-large-150.bmp,assets\wizard-large-200.bmp
WizardSmallImageFile=assets\wizard-small.bmp,assets\wizard-small-125.bmp,assets\wizard-small-150.bmp,assets\wizard-small-200.bmp
SetupMutex=CoBuSetupMutex_2026
CloseApplications=no
RestartApplications=no

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
russian.WelcomeLabel1=Добро пожаловать в CoBu
russian.WelcomeLabel2=CoBu — лёгкий менеджер буфера обмена: сохраняет всё, что вы копируете (текст, ссылки, картинки), и возвращает в один клик.%n%nМастер установит CoBu на этот компьютер. Для установки в свой профиль права администратора не нужны, для «Program Files» Windows спросит подтверждение.
russian.FinishedHeadingLabel=CoBu установлен
english.WelcomeLabel1=Welcome to CoBu
english.WelcomeLabel2=CoBu is a lightweight clipboard manager: it keeps everything you copy (text, links, images) and brings it back in one click.%n%nThis wizard installs CoBu on your computer. Installing into your own profile needs no administrator rights; "Program Files" asks Windows for confirmation.
english.FinishedHeadingLabel=CoBu is installed

[CustomMessages]
russian.TaskStartup=Запускать CoBu вместе с Windows (тихо, в трей)
russian.TaskRuntimeGroup=Компоненты:
russian.TaskDotNet=Скачать и установить .NET Desktop Runtime 10 (нужен для запуска CoBu, сейчас не найден)
russian.DotNetDownloadTitle=Загрузка .NET Desktop Runtime 10
russian.DotNetDownloadDesc=Скачиваем официальный установщик с сайта Microsoft...
russian.DotNetInstallDesc=Устанавливаем .NET Desktop Runtime 10. Подтвердите запрос Windows (UAC), если он появится...
russian.DotNetDownloadFailed=Не удалось скачать .NET Desktop Runtime 10.%n%n%1%n%nОткрыть страницу загрузки в браузере? Установите .NET вручную и запустите установщик снова.
russian.DotNetBadSignature=Скачанный файл не прошёл проверку подписи Microsoft, установка .NET отменена.
russian.DotNetInstallFailed=Не удалось установить .NET Desktop Runtime 10 (код %1).%n%nУстановите его вручную со страницы загрузки и запустите установщик снова.
russian.DotNetSkipWarning=.NET Desktop Runtime 10 не найден, а установка его отключена. Без него CoBu не запустится.%n%nПродолжить установку без .NET?
russian.DirNotWritable=В эту папку нельзя записывать без прав администратора:%n%n%1%n%nВыберите папку в вашем профиле или на другом диске. Чтобы установить в «Program Files», закройте установщик и запустите его снова, выбрав режим «для всех пользователей».
russian.KeepHistory=Удалить сохранённую историю буфера обмена (тексты и картинки)?%n%nНажмите «Нет», чтобы сохранить её.
english.TaskStartup=Start CoBu with Windows (silently, straight to the tray)
english.TaskRuntimeGroup=Components:
english.TaskDotNet=Download and install .NET Desktop Runtime 10 (required to run CoBu, not found on this PC)
english.DotNetDownloadTitle=Downloading .NET Desktop Runtime 10
english.DotNetDownloadDesc=Downloading the official installer from Microsoft...
english.DotNetInstallDesc=Installing .NET Desktop Runtime 10. Confirm the Windows (UAC) prompt if it appears...
english.DotNetDownloadFailed=Could not download .NET Desktop Runtime 10.%n%n%1%n%nOpen the download page in your browser? Install .NET manually and run this setup again.
english.DotNetBadSignature=The downloaded file failed the Microsoft signature check, .NET installation was cancelled.
english.DotNetInstallFailed=Could not install .NET Desktop Runtime 10 (code %1).%n%nInstall it manually from the download page and run this setup again.
english.DotNetSkipWarning=.NET Desktop Runtime 10 was not found and its installation is turned off. CoBu will not start without it.%n%nContinue without .NET?
english.DirNotWritable=This folder cannot be written to without administrator rights:%n%n%1%n%nPick a folder in your profile or on another drive. To install into "Program Files", close this setup and run it again, choosing "for all users".
english.KeepHistory=Delete the saved clipboard history (texts and images)?%n%nChoose "No" to keep it.

[Tasks]
Name: "autostart"; Description: "{cm:TaskStartup}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "installdotnet"; Description: "{cm:TaskDotNet}"; GroupDescription: "{cm:TaskRuntimeGroup}"; Check: NeedsDotNet

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Excludes: "*.pdb,CoBu_Storage,crash.log"; Flags: ignoreversion recursesubdirs createallsubdirs

[Dirs]
; All-users install lives in a protected folder: let ordinary users write the history / settings there
Name: "{app}\CoBu_Storage"; Permissions: users-modify; Check: IsAdminInstallMode

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
; Same value the app writes itself (Settings -> autostart), so both stay in sync
Root: HKCU; Subkey: "{#RunKey}"; ValueType: string; ValueName: "{#AppName}"; ValueData: """{app}\{#AppExe}"" --tray"; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\crash.log"
Type: files; Name: "{app}\CoBu_Storage\crash.log"

[Code]
const
  DotNetUrl     = 'https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe';
  DotNetPageUrl = 'https://dotnet.microsoft.com/download/dotnet/10.0';
  DotNetRegKey  = 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';

  // Palette of the app (BGR for Delphi colors): midnight indigo glass + neon green accent
  clBg      = $1B0F0D;  // #0D0F1B
  clPanel   = $2A1A16;  // #161A2A
  clText    = $FFFFFF;
  clMuted   = $C9BFB8;  // soft grey-blue
  clAccent  = $76E600;  // #00E676

var
  DownloadPage: TDownloadWizardPage;

// ---------------------------------------------------------------- .NET detection

function HasDesktopRuntimeIn(const DotNetRoot: String): Boolean;
var
  FindRec: TFindRec;
begin
  Result := False;
  if DotNetRoot = '' then
    Exit;
  if FindFirst(AddBackslash(DotNetRoot) + 'shared\Microsoft.WindowsDesktop.App\10.*', FindRec) then
  begin
    try
      repeat
        if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
        begin
          Result := True;
          Exit;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

// The MSI registers installed runtimes in the registry, but zip / Visual Studio / SDK installs do not,
// so the registry is only one of the places we look: the default folder and the registered install location count too.
function IsDotNet10DesktopInstalled: Boolean;
var
  Versions: TArrayOfString;
  InstallLocation: String;
  I: Integer;
begin
  Result := False;

  if RegGetValueNames(HKLM64, DotNetRegKey, Versions) then
    for I := 0 to GetArrayLength(Versions) - 1 do
      if Copy(Versions[I], 1, 3) = '10.' then
      begin
        Result := True;
        Exit;
      end;

  if RegQueryStringValue(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64', 'InstallLocation', InstallLocation) and
     HasDesktopRuntimeIn(InstallLocation) then
  begin
    Result := True;
    Exit;
  end;

  Result := HasDesktopRuntimeIn(ExpandConstant('{commonpf64}\dotnet')) or
            HasDesktopRuntimeIn(GetEnv('DOTNET_ROOT')) or
            HasDesktopRuntimeIn(ExpandConstant('{localappdata}\Microsoft\dotnet'));
end;

function NeedsDotNet: Boolean;
begin
  Result := not IsDotNet10DesktopInstalled;
end;

// ---------------------------------------------------------------- .NET download + install

function OnDownloadProgress(const Url, FileName: String; const Progress, ProgressMax: Int64): Boolean;
begin
  Result := True;
end;

function IsMicrosoftSigned(const FileName: String): Boolean;
var
  ResultCode: Integer;
  Params: String;
begin
  Params := '-NoProfile -ExecutionPolicy Bypass -Command "$s = Get-AuthenticodeSignature -LiteralPath ''' + FileName +
            '''; if ($s.Status -eq ''Valid'' -and $s.SignerCertificate.Subject -like ''*Microsoft Corporation*'') { exit 0 } else { exit 1 }"';
  Result := Exec('powershell.exe', Params, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

// Returns True when the runtime is installed (or the user chose to continue anyway)
function DownloadAndInstallDotNet: Boolean;
var
  Installer: String;
  ResultCode: Integer;
  ErrorCode: Integer;
begin
  Result := False;
  Installer := ExpandConstant('{tmp}\windowsdesktop-runtime-win-x64.exe');

  DownloadPage.Clear;
  DownloadPage.Add(DotNetUrl, 'windowsdesktop-runtime-win-x64.exe', '');
  DownloadPage.SetText(CustomMessage('DotNetDownloadTitle'), CustomMessage('DotNetDownloadDesc'));
  DownloadPage.Show;
  try
    try
      DownloadPage.Download;
    except
      if not DownloadPage.AbortedByUser then
        if SuppressibleMsgBox(FmtMessage(CustomMessage('DotNetDownloadFailed'), [GetExceptionMessage]),
                              mbError, MB_YESNO, IDNO) = IDYES then
          ShellExec('open', DotNetPageUrl, '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
      Exit;
    end;

    if not IsMicrosoftSigned(Installer) then
    begin
      SuppressibleMsgBox(CustomMessage('DotNetBadSignature'), mbError, MB_OK, IDOK);
      Exit;
    end;

    // Installing the runtime needs admin rights: ask for them only for this one step
    DownloadPage.SetText(CustomMessage('DotNetDownloadTitle'), CustomMessage('DotNetInstallDesc'));
    DownloadPage.ProgressBar.Style := npbstMarquee;
    if not ShellExec('runas', Installer, '/install /quiet /norestart', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode) then
      ResultCode := -1;

    // 0 = ok, 3010 = ok but a restart is recommended, 1638 = a newer build is already installed
    if (ResultCode <> 0) and (ResultCode <> 3010) and (ResultCode <> 1638) then
    begin
      SuppressibleMsgBox(FmtMessage(CustomMessage('DotNetInstallFailed'), [IntToStr(ResultCode)]), mbError, MB_OK, IDOK);
      Exit;
    end;

    Result := True;
  finally
    DownloadPage.ProgressBar.Style := npbstNormal;
    DownloadPage.Hide;
  end;
end;

// ---------------------------------------------------------------- install folder check

// The chosen folder may not exist yet: test the nearest existing parent folder
function IsDirWritable(Dir: String): Boolean;
var
  Parent, Probe: String;
begin
  Dir := RemoveBackslash(Dir);
  while (Dir <> '') and (not DirExists(Dir)) do
  begin
    Parent := ExtractFileDir(Dir);
    if Parent = Dir then
      Break;
    Dir := Parent;
  end;

  Probe := AddBackslash(Dir) + '.cobu_write_test';
  Result := (Dir <> '') and SaveStringToFile(Probe, 'x', False);
  if Result then
    DeleteFile(Probe);
end;

// ---------------------------------------------------------------- running instance

procedure CloseRunningCoBu;
var
  ResultCode: Integer;
begin
  // CoBu lives in the tray without a taskbar button, so end it quietly before files are replaced / removed
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#AppExe}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(400);
end;

// ---------------------------------------------------------------- dark "frosted glass" look

procedure PaintPage(Page: TNewNotebookPage);
begin
  Page.Color := clBg;
end;

procedure StyleLabel(L: TNewStaticText; C: TColor);
begin
  L.Font.Color := C;
end;

procedure ApplyTheme;
begin
  WizardForm.Color := clBg;
  WizardForm.Font.Color := clText;
  WizardForm.MainPanel.Color := clPanel;
  WizardForm.Bevel.Visible := False;
  WizardForm.Bevel1.Visible := False;

  PaintPage(WizardForm.WelcomePage);
  PaintPage(WizardForm.InnerPage);
  PaintPage(WizardForm.FinishedPage);
  PaintPage(WizardForm.LicensePage);
  PaintPage(WizardForm.SelectDirPage);
  StyleLabel(WizardForm.SelectDirLabel, clText);
  StyleLabel(WizardForm.SelectDirBrowseLabel, clMuted);
  StyleLabel(WizardForm.DiskSpaceLabel, clMuted);
  WizardForm.DirEdit.Color := clPanel;
  WizardForm.DirEdit.Font.Color := clText;
  PaintPage(WizardForm.SelectComponentsPage);
  PaintPage(WizardForm.SelectTasksPage);
  PaintPage(WizardForm.ReadyPage);
  PaintPage(WizardForm.PreparingPage);
  PaintPage(WizardForm.InstallingPage);
  PaintPage(WizardForm.InfoBeforePage);
  PaintPage(WizardForm.InfoAfterPage);

  StyleLabel(WizardForm.PageNameLabel, clText);
  StyleLabel(WizardForm.PageDescriptionLabel, clMuted);
  StyleLabel(WizardForm.WelcomeLabel1, clAccent);
  StyleLabel(WizardForm.WelcomeLabel2, clText);
  StyleLabel(WizardForm.FinishedHeadingLabel, clAccent);
  StyleLabel(WizardForm.FinishedLabel, clText);
  StyleLabel(WizardForm.SelectTasksLabel, clText);
  StyleLabel(WizardForm.ReadyLabel, clText);
  StyleLabel(WizardForm.PreparingLabel, clText);
  StyleLabel(WizardForm.StatusLabel, clText);
  StyleLabel(WizardForm.FilenameLabel, clMuted);

  WizardForm.TasksList.Color := clBg;
  WizardForm.TasksList.Font.Color := clText;
  WizardForm.TasksList.BorderStyle := bsNone;
  WizardForm.RunList.Color := clBg;
  WizardForm.RunList.Font.Color := clText;
  WizardForm.RunList.BorderStyle := bsNone;
  WizardForm.ReadyMemo.Color := clPanel;
  WizardForm.ReadyMemo.Font.Color := clText;
end;

// ---------------------------------------------------------------- wizard events

procedure InitializeWizard;
begin
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), @OnDownloadProgress);
  ApplyTheme;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;

  if CurPageID = wpSelectDir then
  begin
    Result := IsDirWritable(WizardDirValue);
    if not Result then
      SuppressibleMsgBox(FmtMessage(CustomMessage('DirNotWritable'), [WizardDirValue]), mbError, MB_OK, IDOK);
  end;

  if CurPageID = wpReady then
  begin
    if WizardIsTaskSelected('installdotnet') then
      Result := DownloadAndInstallDotNet
    else if NeedsDotNet then
      // the check box was shown (runtime missing) but the user turned it off
      Result := SuppressibleMsgBox(CustomMessage('DotNetSkipWarning'), mbConfirmation, MB_YESNO, IDNO) = IDYES;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  CloseRunningCoBu;
  Result := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  // Re-installing with "autostart" unchecked must remove a previously created entry
  if (CurStep = ssPostInstall) and (not WizardIsTaskSelected('autostart')) then
    RegDeleteValue(HKCU, '{#RunKey}', '{#AppName}');
end;

// ---------------------------------------------------------------- uninstall

function InitializeUninstall: Boolean;
begin
  CloseRunningCoBu;
  Result := True;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  StorageDir: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // The app can also switch autostart on by itself (Settings), so always clean the entry
    RegDeleteValue(HKCU, '{#RunKey}', '{#AppName}');

    StorageDir := ExpandConstant('{app}\CoBu_Storage');
    if DirExists(StorageDir) and (not UninstallSilent) then
      if MsgBox(CustomMessage('KeepHistory'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
        DelTree(StorageDir, True, True, True);
  end;
end;
