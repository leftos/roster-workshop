; BEGIN ISPPBUILTINS.ISS


; END ISPPBUILTINS.ISS











[Setup]
AppName=Roster Workshop
AppVersion=
AppVerName=Roster Workshop  
AppCopyright=Copyright © Lefteris Aslanoglou 2011-2013
VersionInfoVersion=
VersionInfoCompany=Lefteris Aslanoglou
AppPublisher=Lefteris Aslanoglou
;AppPublisherURL=http://...
;AppSupportURL=http://...
;AppUpdatesURL=http://...
OutputBaseFilename=Roster Workshop  
DefaultGroupName=Roster Workshop
DefaultDirName={code:GetDefaultDir}
UninstallDisplayIcon={app}\Roster Workshop.exe
UninstallDisplayName=Roster Workshop
Uninstallable=true
DirExistsWarning=no
CreateAppDir=true
OutputDir=..
SourceDir=.
AllowNoIcons=yes
UsePreviousGroup=yes
UsePreviousAppDir=yes
LanguageDetectionMethod=uilanguage
InternalCompressLevel=ultra
SolidCompression=yes
Compression=lzma/ultra
MinVersion=0,5.01
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64 ia64
ArchitecturesInstallIn64BitMode=x64 ia64
AppSupportURL=http://forums.nba-live.com/viewtopic.php?f=143&t=84110
AppUpdatesURL=http://forums.nba-live.com/viewtopic.php?f=143&t=84110

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "E:\Development\Visual Studio 2010\Projects\RosterWorkshop\RosterWorkshop\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes: "*.vshost.*,*.xml,*.pdb"
Source: "E:\Development\Visual Studio 2010\Projects\RosterWorkshop\RosterWorkshop\bin\Release\RosterWorkshop.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Development\Visual Studio 2010\Projects\RosterWorkshop\RosterWorkshop\bin\Release\LeftosCommonLibrary.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Development\Visual Studio 2010\Projects\RosterWorkshop\RosterWorkshop\bin\Release\TreeViewWithCheckBoxesLib.pdb"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Roster Workshop"; Filename: "{app}\RosterWorkshop.exe"; WorkingDir: "{app}"; IconFilename: "{app}\RosterWorkshop.exe"
Name: "{group}\{cm:UninstallProgram,Roster Workshop}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Roster Workshop"; Filename: "{app}\RosterWorkshop.exe"; WorkingDir: "{app}"; IconFilename: "{app}\RosterWorkshop.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Roster Workshop"; Filename: "{app}\RosterWorkshop.exe"; WorkingDir: "{app}"; IconFilename: "{app}\RosterWorkshop.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\Roster Workshop.exe"; WorkingDir: "{app}"; Flags: nowait postinstall runascurrentuser skipifsilent; Description: "{cm:LaunchProgram,Roster Workshop}"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Registry]
Root: "HKCU"; Subkey: "Software\Lefteris Aslanoglou\Roster Workshop"; Flags: uninsdeletekey
Root: "HKCU"; Subkey: "Software\Lefteris Aslanoglou\Roster Workshop"; ValueType: string; ValueName: "InstallDir"; ValueData: "{app}"

[Dirs]
Name: "{app}\Images"

[Files]
Source: "scripts\isxdl\isxdl.dll"; Flags: dontcopy

[Code]
procedure isxdl_AddFile(URL, Filename: PAnsiChar);
external 'isxdl_AddFile@files:isxdl.dll stdcall';

function isxdl_DownloadFiles(hWnd: Integer): Integer;
external 'isxdl_DownloadFiles@files:isxdl.dll stdcall';

function isxdl_SetOption(Option, Value: PAnsiChar): Integer;
external 'isxdl_SetOption@files:isxdl.dll stdcall';

[CustomMessages]
DependenciesDir=MyProgramDependencies

en.depdownload_msg=The following applications are required before setup can continue:%n%n%1%nDownload and install now?
;de.depdownload_msg=Die folgenden Programme werden benötigt bevor das Setup fortfahren kann:%n%n%1%nJetzt downloaden und installieren?

en.depdownload_admin=An Administrator account is required installing these dependencies.%nPlease run this setup again using 'Run as Administrator' or install the following dependencies manually:%n%n%1%nClose this message and press Cancel to exit setup.
;;de.depdownload_admin=

en.previousinstall_admin=This setup was previously run as Administrator. A non-administrator is not allowed to update in the selected location.%n%nPlease run this setup again using 'Run as Administrator'.%nClose this message and press Cancel to exit setup.
;;de.previousinstall_admin=

en.depdownload_memo_title=Download dependencies
;de.depdownload_memo_title=Abhängigkeiten downloaden

en.depinstall_memo_title=Install dependencies
;de.depinstall_memo_title=Abhängigkeiten installieren

en.depinstall_title=Installing dependencies
;de.depinstall_title=Installiere Abhängigkeiten

en.depinstall_description=Please wait while Setup installs dependencies on your computer.
;de.depinstall_description=Warten Sie bitte während Abhängigkeiten auf Ihrem Computer installiert wird.

en.depinstall_status=Installing %1...
;de.depinstall_status=Installiere %1...

en.depinstall_missing=%1 must be installed before setup can continue. Please install %1 and run Setup again.
;de.depinstall_missing=%1 muss installiert werden bevor das Setup fortfahren kann. Bitte installieren Sie %1 und starten Sie das Setup erneut.

en.depinstall_error=An error occured while installing the dependencies. Please restart the computer and run the setup again or install the following dependencies manually:%n
;de.depinstall_error=Ein Fehler ist während der Installation der Abghängigkeiten aufgetreten. Bitte starten Sie den Computer neu und führen Sie das Setup erneut aus oder installieren Sie die folgenden Abhängigkeiten per Hand:%n

en.isxdl_langfile=
;de.isxdl_langfile=german2.ini


[Files]
;Source: "scripts\isxdl\german2.ini"; Flags: dontcopy

[Code]
type
	TProduct = record
		File: String;
		Title: String;
		Parameters: String;
		InstallClean : Boolean;
		MustRebootAfter : Boolean;
        RequestRestart : Boolean;
	end;
	
var
	installMemo, downloadMemo, downloadMessage: string;
	products: array of TProduct;
	DependencyPage: TOutputProgressWizardPage;

	rebootRequired : boolean;
	rebootMessage : string;
  
procedure AddProduct(FileName, Parameters, Title, Size, URL: string; InstallClean : Boolean; MustRebootAfter : Boolean);
var
	path: string;
	i: Integer;
begin
	installMemo := installMemo + '%1' + Title + #13;
	
	path := ExpandConstant('{src}{\}') + CustomMessage('DependenciesDir') + '\' + FileName;
	if not FileExists(path) then begin
		path := ExpandConstant('{tmp}{\}') + FileName;
		
		isxdl_AddFile(URL, path);
		
		downloadMemo := downloadMemo + '%1' + Title + #13;
		downloadMessage := downloadMessage + '    ' + Title + ' (' + Size + ')' + #13;
	end;
	
	i := GetArrayLength(products);
	SetArrayLength(products, i + 1);
	products[i].File := path;
	products[i].Title := Title;
	products[i].Parameters := Parameters;
	products[i].InstallClean := InstallClean;
	products[i].MustRebootAfter := MustRebootAfter;
	products[i].RequestRestart := false;
end;

function GetProductcount: integer;
begin
    Result := GetArrayLength(products);
end;

function SmartExec(prod : TProduct; var ResultCode : Integer) : Boolean;
begin
    if (UpperCase(Copy(prod.File,Length(prod.File)-2,3)) <> 'EXE') then begin
        Result := ShellExec('', prod.File, prod.Parameters, '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
    end else begin
        Result := Exec(prod.File, prod.Parameters, '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
    end;
    if (ResultCode = 3010) then begin
        prod.RequestRestart := true;
        ResultCode := 0;
    end;
end;

function PendingReboot : Boolean;
var	Names: String;
begin
  if (RegQueryMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'PendingFileRenameOperations', Names)) then begin
      Result := true;
  end else if ((RegQueryMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'SetupExecute', Names)) and (Names <> ''))  then begin
		Result := true;
	end
	else begin
	  Result := false;
  end;		
end;

function InstallProducts: Boolean;
var
	ResultCode, i, productCount, finishCount: Integer;
begin
	Result := true;
	productCount := GetArrayLength(products);
		
	if productCount > 0 then begin
		DependencyPage := CreateOutputProgressPage(CustomMessage('depinstall_title'), CustomMessage('depinstall_description'));
		DependencyPage.Show;
		
		for i := 0 to productCount - 1 do begin
		    if ((products[i].InstallClean) and PendingReboot)  then begin
		        rebootRequired := true;
		        rebootmessage := products[i].Title;
		        exit;
		    end;
		  
		    DependencyPage.SetText(FmtMessage(CustomMessage('depinstall_status'), [products[i].Title]), '');
		    DependencyPage.SetProgress(i, productCount);
			
            if SmartExec(products[i], ResultCode) then begin
				if ResultCode = 0 then
					finishCount := finishCount + 1;
				if (products[i].MustRebootAfter = true) then begin
				    rebootRequired := true;
				    rebootmessage := products[i].Title;
				    if not PendingReboot then begin
  				        RegWriteMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'PendingFileRenameOperations', '');
                    end;
                    exit;
                end;
            end
			else begin
			    Result := false;
				break;
			end;
	    end;
		
		for i := 0 to productCount - finishCount - 1 do begin
			products[i] := products[i+finishCount];
		end;
		SetArrayLength(products, productCount - finishCount);
		
		DependencyPage.Hide;
	end;
end;


function PrepareToInstall(var NeedsRestart: Boolean): String;
var
	i: Integer;
	s: string;
begin
	if not InstallProducts() then begin
		s := CustomMessage('depinstall_error');
		
		for i := 0 to GetArrayLength(products) - 1 do begin
			s := s + #13 + '    ' + products[i].Title;
		end;
		
		Result := s;
	end
  else if (rebootrequired) then
	begin
	   Result := RebootMessage;
	   NeedsRestart := true;
	    RegWriteStringValue(HKEY_CURRENT_USER, 'SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce',
                           'InstallBootstrap', ExpandConstant('{srcexe}'));
	end;
end;


function NeedRestart : Boolean;
var i: integer;
begin
    result := false;
	for i := 0 to GetArrayLength(products) - 1 do
        result := result or products[i].RequestRestart;
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
	s: string;
begin
	if downloadMemo <> '' then
		s := s + CustomMessage('depdownload_memo_title') + ':' + NewLine + FmtMessage(downloadMemo, [Space]) + NewLine;
	if installMemo <> '' then
		s := s + CustomMessage('depinstall_memo_title') + ':' + NewLine + FmtMessage(installMemo, [Space]) + NewLine;

	s := s + MemoDirInfo + NewLine + NewLine + MemoGroupInfo
	
	if MemoTasksInfo <> '' then
		s := s + NewLine + NewLine + MemoTasksInfo;

	Result := s
end;


function NextButtonClick(CurPageID: Integer): Boolean;
var pf: string;
begin
	Result := true;

    if (CurPageID = wpWelcome) and (not IsAdminLoggedOn()) and Result then begin
   
        if (Wizardform.PrevAppDir <> '') then begin
            pf := ExpandConstant('{pf}');
            if Copy(Wizardform.PrevAppDir,1,Length(pf)) = pf then begin
                SuppressibleMsgBox(CustomMessage('previousinstall_admin'), mbConfirmation, MB_OK, IDOK);
                Result := false;
            end;
        end;
    end;
    if (CurPageID = wpWelcome) and (GetArrayLength(products) > 0) and (not IsAdminLoggedOn()) and Result then begin
        SuppressibleMsgBox(FmtMessage(CustomMessage('depdownload_admin'), [downloadMessage]), mbConfirmation, MB_OK, IDOK);
        Result := false;
    end;
	if CurPageID = wpReady then begin

		if downloadMemo <> '' then begin
			if ActiveLanguage() <> 'en' then begin
				ExtractTemporaryFile(CustomMessage('isxdl_langfile'));
				isxdl_SetOption('language', ExpandConstant('{tmp}{\}') + CustomMessage('isxdl_langfile'));
			end;
			
			if SuppressibleMsgBox(FmtMessage(CustomMessage('depdownload_msg'), [downloadMessage]), mbConfirmation, MB_YESNO, IDYES) = IDNO then
				Result := false
			else if isxdl_DownloadFiles(StrToInt(ExpandConstant('{wizardhwnd}'))) = 0 then
				Result := false;
		end;
	end;
end;

function IsX64: Boolean;
begin
	Result := Is64BitInstallMode and (ProcessorArchitecture = paX64);
end;

function IsIA64: Boolean;
begin
	Result := Is64BitInstallMode and (ProcessorArchitecture = paIA64);
end;

function GetURL(x86, x64, ia64: String): String;
begin
	if IsX64() and (x64 <> '') then
		Result := x64;
	if IsIA64() and (ia64 <> '') then
		Result := ia64;
	
	if Result = '' then
		Result := x86;
end;

[Code]
var
	WindowsVersion: TWindowsVersion;
	
procedure initwinversion();
begin
	GetWindowsVersionEx(WindowsVersion);
end;

function exactwinversion(MajorVersion, MinorVersion: integer): boolean;
begin
	Result := (WindowsVersion.Major = MajorVersion) and (WindowsVersion.Minor = MinorVersion);
end;

function minwinversion(MajorVersion, MinorVersion: integer): boolean;
begin
	Result := (WindowsVersion.Major > MajorVersion) or ((WindowsVersion.Major = MajorVersion) and (WindowsVersion.Minor >= MinorVersion));
end;

function maxwinversion(MajorVersion, MinorVersion: integer): boolean;
begin
	Result := (WindowsVersion.Major < MajorVersion) or ((WindowsVersion.Major = MajorVersion) and (WindowsVersion.Minor <= MinorVersion));
end;

function exactwinspversion(MajorVersion, MinorVersion, SpVersion: integer): boolean;
begin
	if exactwinversion(MajorVersion, MinorVersion) then
		Result := WindowsVersion.ServicePackMajor = SpVersion
	else
		Result := true;
end;

function minwinspversion(MajorVersion, MinorVersion, SpVersion: integer): boolean;
begin
	if exactwinversion(MajorVersion, MinorVersion) then
		Result := WindowsVersion.ServicePackMajor >= SpVersion
	else
		Result := true;
end;

function maxwinspversion(MajorVersion, MinorVersion, SpVersion: integer): boolean;
begin
	if exactwinversion(MajorVersion, MinorVersion) then
		Result := WindowsVersion.ServicePackMajor <= SpVersion
	else
		Result := true;
end;
[Code]
function GetFullVersion(VersionMS, VersionLS: cardinal): string;
var
	version: string;
begin
	version := IntToStr(word(VersionMS shr 16));
	version := version + '.' + IntToStr(word(VersionMS and not $ffff0000));
	
	version := version + '.' + IntToStr(word(VersionLS shr 16));
	version := version + '.' + IntToStr(word(VersionLS and not $ffff0000));
	
	Result := version;
end;

function fileversion(file: string): string;
var
	versionMS, versionLS: cardinal;
begin
	if GetVersionNumbers(file, versionMS, versionLS) then
		Result := GetFullVersion(versionMS, versionLS)
	else
		Result := '0';
end;



[CustomMessages]
msi45win60_title=Windows Installer 4.5 for Windows Vista & Windows 7
msi45win52_title=Windows Installer 4.5 for Server 2003
msi45win51_title=Windows Installer 4.5 for Windows XP

msi45win60_size=1.7 MB
msi45win52_size=3.0 MB
msi45win51_size=3.2 MB



[Code]
const
	msi45win60_url = 'http://download.microsoft.com/download/2/6/1/261fca42-22c0-4f91-9451-0e0f2e08356d/Windows6.0-KB942288-v2-x86.msu';
  msi45win52_url = 'http://download.microsoft.com/download/2/6/1/261fca42-22c0-4f91-9451-0e0f2e08356d/WindowsServer2003-KB942288-v4-x86.exe';
  msi45win51_url = 'http://download.microsoft.com/download/2/6/1/261fca42-22c0-4f91-9451-0e0f2e08356d/WindowsXP-KB942288-v3-x86.exe';
procedure msi45(MinVersion: string);
begin
	if minwinversion(6, 0) and (fileversion(ExpandConstant('{sys}{\}msi.dll')) < MinVersion) then
		AddProduct('msi45win60.msu',
			'/quiet /norestart',
			CustomMessage('msi45win60_title'),
			CustomMessage('msi45win60_size'),
			msi45win60_url,false,false)
	else if minwinversion(5, 2) and (fileversion(ExpandConstant('{sys}{\}msi.dll')) < MinVersion) then
		AddProduct('msi45win52.exe',
			'/quiet /norestart',
			CustomMessage('msi45win52_title'),
			CustomMessage('msi45win52_size'),
			msi45win52_url,false,false)
	else if minwinversion(5, 1) and (fileversion(ExpandConstant('{sys}{\}msi.dll')) < MinVersion) then
		AddProduct('msi45win51.exe',
			'/quiet /norestart',
			CustomMessage('msi45win51_title'),
			CustomMessage('msi45win51_size'),
			msi45win51_url,false,false);
end;





[CustomMessages]
dotnetfx40full_title=.NET 4.0 Full Framework

dotnetfx40full_size=3 MB - 197 MB
en.dotnetfx40full_lcid=''
;de.dotnetfx40full_lcid='/lcid 1031 '


[Code]
const
	dotnetfx40full_url = 'http://download.microsoft.com/download/1/B/E/1BE39E79-7E39-46A3-96FF-047F95396215/dotNetFx40_Full_setup.exe';

function dotnetfx40full(checkOnly : boolean) : boolean;
var
	version: cardinal;
begin
    result := true;
	RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\full', 'Install', version);
	if version <> 1 then begin
        result := false;
        if not checkOnly then
    		AddProduct('dotNetFx40_Full_setup.exe',
    			CustomMessage('dotnetfx40full_lcid') + '/q ' + '/passive ' + '/norestart',
    			CustomMessage('dotnetfx40full_title'),
    			CustomMessage('dotnetfx40full_size'),
    			dotnetfx40full_url,false,false);
    end;
end;

[CustomMessages]
vc2010x86_title=MS Visual C++ 2010 Redistributable package (x86)
vc2010x64_title=MS Visual C++ 2010 Redistributable package (x64)

en.vc2010x86_size=4.8 MB
en.vc2010x64_size=5.5 MB
en.vc2010_lcid=''


[Code]
const
    vc2010x86_url = 'http://download.microsoft.com/download/5/B/C/5BC5DBB3-652D-4DCE-B14A-475AB85EEF6E/vcredist_x86.exe';
    vc2010x64_url = 'http://download.microsoft.com/download/3/2/2/3224B87F-CFA0-4E70-BDA3-3DE650EFEBA5/vcredist_x64.exe';

procedure vc2010();
var
	version: cardinal;
begin
    if not RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist\x86', 'Installed', version) then
        RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\10.0\VC\Runtimes\x86', 'Installed', version);
        if version <> 1 then
    		AddProduct('vcredist_x86.exe',
    			CustomMessage('vc2010_lcid') + '/q ' + '/passive ' + '/norestart',
    			CustomMessage('vc2010x86_title'),
    			CustomMessage('vc2010x86_size'),
    			vc2010x86_url,false,false);
    if isX64 then begin
        version := 0;
	    if not RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist\x64', 'Installed', version) then
            RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\10.0\VC\Runtimes\x64', 'Installed', version);
        if version <> 1 then
    		AddProduct('vcredist_x64.exe',
    			CustomMessage('vc2010_lcid') + '/q ' + '/passive ' + '/norestart',
    			CustomMessage('vc2010x64_title'),
    			CustomMessage('vc2010x64_size'),
    			vc2010x64_url,false,false);
    end;
end;


[CustomMessages]
win2000sp3_title=Windows 2000 Service Pack 3
winxpsp2_title=Windows XP Service Pack 2
winxpsp3_title=Windows XP Service Pack 3

