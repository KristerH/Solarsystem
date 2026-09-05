; Inno Setup script for The Solar System in 3D.
;
; Builds a conventional Windows installer around an unpackaged, self-contained
; publish. Self-contained is the point: the .NET runtime travels inside the
; folder, so the target machine needs nothing installed - which is exactly what
; the README's build instructions cannot offer.
;
; Build it in two steps, from the repository root:
;
;   dotnet publish Solarsystem.csproj -f net10.0-windows10.0.19041.0 -c Release ^
;       -p:RuntimeIdentifier=win-x64 --self-contained true
;   iscc Installer\Solarsystem.iss
;
; The result is Installer\Output\Solarsystem-<version>-setup.exe, which is
; gitignored - it belongs on a GitHub Release, not in the tree.
;
; The installer is not signed. Windows SmartScreen will therefore warn the
; first time it is run, and the user has to click "More info" and then "Run
; anyway". That is a deliberate decision, not an oversight; see the Releases
; section in README.md for why and what it looks like.

#define AppName "The Solar System in 3D"
#define AppExeName "Solarsystem.exe"
#define AppPublisher "Krister Hellsing"
#define AppUrl "https://github.com/KristerH/Solarsystem"
#define PublishDir "..\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"

; Read the version straight out of the built executable rather than repeating
; it here. There is then only one place a release version is written down -
; ApplicationDisplayVersion in Solarsystem.csproj - and the installer cannot
; disagree with the app it installs.
#define FullVersion GetVersionNumbersString(PublishDir + "\" + AppExeName)
; Windows file versions always have four parts, so the executable reports
; "1.0.0.0". Drop the last one so the installer's name matches the git tag
; (v1.0.0) instead of carrying a fourth digit that never means anything.
#define AppVersion Copy(FullVersion, 1, RPos(".", FullVersion) - 1)

[Setup]
; Never change AppId. It is how Windows recognises an existing installation,
; and a new one would leave the old version behind as a second entry in
; Add/Remove Programs instead of upgrading it.
AppId={{E6D243A2-6941-4924-B5BE-DB7C2BDE5F60}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}/issues
AppUpdatesURL={#AppUrl}/releases

DefaultDirName={autopf}\Solarsystem
DefaultGroupName={#AppName}
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppExeName}

; Install for the current user only, and so without a UAC prompt. A school or
; work laptop is the case this app is written for, and there the user is often
; not an administrator. An administrator who wants it for everyone can still
; choose that in the first dialog.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; x64 only, matching -p:RuntimeIdentifier=win-x64 above. Without this the
; installer would run on an ARM or 32-bit machine and produce an app that
; cannot start.
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763

LicenseFile=..\LICENSE
OutputDir=Output
OutputBaseFilename=Solarsystem-{#AppVersion}-setup
; LZMA2/max matters more than usual here: a self-contained publish is a couple
; of hundred megabytes, and this is a file people download.
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "swedish"; MessagesFile: "compiler:Languages\Swedish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; The whole publish folder, recursively. Listing files by hand would need
; editing every time a dependency changes, and a self-contained publish has
; several hundred of them.
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

; No [UninstallDelete] section, and that is checked rather than assumed. The app
; creates exactly one file that Inno does not know about - the error log from
; Simulation/Diagnostics - and it is written to Path.GetTempPath(), not to the
; install folder. Nothing else is written outside {app}: there are no settings
; files and no registry writes of our own, the language selector reading the
; operating system rather than storing a choice. An uninstall therefore leaves
; nothing behind on its own.
