// Define stuff we need to download/update/install

//#define use_iis
//#define use_kb835732
//#define use_kb886903
//#define use_kb928366

//#define use_msi20
//#define use_msi31
#define use_msi45
//#define use_ie6

//#define use_dotnetfx11
// German languagepack?
//#define use_dotnetfx11lp

//#define use_dotnetfx20
// German languagepack?
//#define use_dotnetfx20lp

//#define use_dotnetfx35
// German languagepack?
//#define use_dotnetfx35lp

// dotnet_Passive enabled shows the .NET/VC2010 installation progress, as it can take quite some time
#define dotnet_Passive
#define use_dotnetfx40full
#define use_vc2010

//#define use_mdac28
//#define use_jet4sp8
// SQL 3.5 Compact Edition
//#define use_scceruntime
// SQL Express
//#define use_sql2005express
//#define use_sql2008express

// Enable the required define(s) below if a local event function (prepended with Local) is used
//#define haveLocalPrepareToInstall
//#define haveLocalNeedRestart
//#define haveLocalNextButtonClick

// End

#define MyAppSetupName 'Roster Workshop'
#define MyAppVersion ''
#define MyAppVerInfo ''
#define CompiledPath 'E:\Development\Visual Studio 2010\Projects\RosterWorkshop\RosterWorkshop\bin\Release'
[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion} {#MyAppVerInfo}
AppCopyright=Copyright © Lefteris Aslanoglou 2011-2013
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany=Lefteris Aslanoglou
AppPublisher=Lefteris Aslanoglou
;AppPublisherURL=http://...
;AppSupportURL=http://...
;AppUpdatesURL=http://...
OutputBaseFilename={#MyAppSetupName} {#MyAppVersion} {#MyAppVerInfo}
DefaultGroupName={#MyAppSetupName}
DefaultDirName={code:GetDefaultDir}
UninstallDisplayIcon={app}\Roster Workshop.exe
UninstallDisplayName={#MyAppSetupName}
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
Source: "{#CompiledPath}\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes: "*.vshost.*,*.xml,*.pdb"
Source: "{#CompiledPath}\RosterWorkshop.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#CompiledPath}\LeftosCommonLibrary.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#CompiledPath}\TreeViewWithCheckBoxesLib.pdb"; DestDir: "{app}"; Flags: ignoreversion

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

#include "scripts\products.iss"

#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"

#ifdef use_iis
#include "scripts\products\iis.iss"
#endif

#ifdef use_kb835732
#include "scripts\products\kb835732.iss"
#endif
#ifdef use_kb886903
#include "scripts\products\kb886903.iss"
#endif
#ifdef use_kb928366
#include "scripts\products\kb928366.iss"
#endif

#ifdef use_msi20
#include "scripts\products\msi20.iss"
#endif
#ifdef use_msi31
#include "scripts\products\msi31.iss"
#endif
#ifdef use_msi45
#include "scripts\products\msi45.iss"
#endif
#ifdef use_ie6
#include "scripts\products\ie6.iss"
#endif

#ifdef use_dotnetfx11
#include "scripts\products\dotnetfx11.iss"
#include "scripts\products\dotnetfx11lp.iss"
#include "scripts\products\dotnetfx11sp1.iss"
#endif

#ifdef use_dotnetfx20
#include "scripts\products\dotnetfx20.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20lp.iss"
#endif
#include "scripts\products\dotnetfx20sp1.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20sp1lp.iss"
#endif
#include "scripts\products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20sp2lp.iss"
#endif
#endif

#ifdef use_dotnetfx35
#include "scripts\products\dotnetfx35.iss"
#ifdef use_dotnetfx35lp
#include "scripts\products\dotnetfx35lp.iss"
#endif
#include "scripts\products\dotnetfx35sp1.iss"
#ifdef use_dotnetfx35lp
#include "scripts\products\dotnetfx35sp1lp.iss"
#endif
#endif

#ifdef use_dotnetfx40client
#include "scripts\products\dotnetfx40client.iss"
#endif
#ifdef use_dotnetfx40full
#include "scripts\products\dotnetfx40full.iss"
#endif
#ifdef use_vc2010
#include "scripts\products\vc2010.iss"
#endif

#ifdef use_mdac28
#include "scripts\products\mdac28.iss"
#endif
#ifdef use_jet4sp8
#include "scripts\products\jet4sp8.iss"
#endif
// SQL 3.5 Compact Edition
#ifdef use_scceruntime
#include "scripts\products\scceruntime.iss"
#endif
// SQL Express
#ifdef use_sql2005express
#include "scripts\products\sql2005express.iss"
#endif
#ifdef use_sql2008express
#include "scripts\products\sql2008express.iss"
#endif

[CustomMessages]
win2000sp3_title=Windows 2000 Service Pack 3
winxpsp2_title=Windows XP Service Pack 2
winxpsp3_title=Windows XP Service Pack 3

#expr SaveToFile(AddBackslash(SourcePath) + "Preprocessed"+MyAppSetupname+".iss")

[PostCompile]

[Code]
function InitializeSetup(): Boolean;
begin
	//init windows version
	initwinversion();
	
	//check if dotnetfx20 can be installed on this OS
	//if not minwinspversion(5, 0, 3) then begin
	//	MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp3_title')]), mbError, MB_OK);
	//	exit;
	//end;
	if not minwinspversion(5, 1, 3) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp3_title')]), mbError, MB_OK);
		exit;
	end;
	
#ifdef use_iis
	if (not iis()) then exit;
#endif	
	
#ifdef use_msi20
	msi20('2.0');
#endif
#ifdef use_msi31
	msi31('3.1');
#endif
#ifdef use_msi45
	msi45('4.5');
#endif
#ifdef use_ie6
	ie6('5.0.2919');
#endif
	
#ifdef use_dotnetfx11
	dotnetfx11();
#ifdef use_dotnetfx11lp
	dotnetfx11lp();
#endif
	dotnetfx11sp1();
#endif
#ifdef use_kb886903
	kb886903(); //better use windows update
#endif
#ifdef use_kb928366
	kb928366(); //better use windows update
#endif
	
	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
#ifdef use_dotnetfx20lp
		dotnetfx20sp2lp();
#endif
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
#ifdef use_kb835732
			kb835732();
#endif
			dotnetfx20sp1();
#ifdef use_dotnetfx20lp
			dotnetfx20sp1lp();
#endif
		end else begin
			dotnetfx20();
#ifdef use_dotnetfx20lp
			dotnetfx20lp();
#endif
		end;
	end;
#endif
	
#ifdef use_dotnetfx35
	dotnetfx35();
#ifdef use_dotnetfx35lp
	dotnetfx35lp();
#endif
	dotnetfx35sp1();
#ifdef use_dotnetfx35lp
	dotnetfx35sp1lp();
#endif
#endif
	
	// If no .NET 4.0 framework found, install the smallest
#ifdef use_dotnetfx40
	if not dotnetfx40client(true) then
	    if not dotnetfx40full(true) then
	        dotnetfx40client(false);
	// Alternatively:
	// dotnetfx40full();
#endif

	// Visual C++ 2010 Redistributable
#ifdef use_vc2010
	vc2010();
#endif
	
#ifdef use_mdac28
	mdac28('2.7');
#endif
#ifdef use_jet4sp8
	jet4sp8('4.0.8015');
#endif
	// SQL 3.5 CE
#ifdef use_ssceruntime
	ssceruntime();
#endif
	// SQL Express
#ifdef use_sql2005express
	sql2005express();
#endif
#ifdef use_sql2008express
	sql2008express();
#endif
	
	Result := true;
end;

function GetDefaultDir(def: string): string;
var
sTemp : string;
begin
    //Set a defualt value so that the install doesn't fail.  
    sTemp := ExpandConstant('{pf32}') + '\Roster Workshop';

    //We need to get the current install directory.  
    if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\Lefteris Aslanoglou\Roster Workshop',
     'InstallDir', sTemp) then
     begin
    //We found the value in the registry so we'll use that.  Otherwise we use the default 
     end;
    Result := sTemp;
end;
