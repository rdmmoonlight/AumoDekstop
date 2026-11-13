; --- KONFIGURASI DASAR (MERUJUK KE AumoDekstop.csproj) ---
#define MyAppName "AumoDekstop"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "B&RC"
#define MyAppExeName "AumoDekstop.exe"
#define CertThumbprint "0569E0943DADA39CE0CD06CE2F09A0CA1B6A4B4E"

; Sesuaikan arsitektur target publish (win-x64 / win-arm64 / win-x86)
#define TargetRID "win-x64"
#define PublishDir "bin\Release\net10.0-windows10.0.26100.0\" + TargetRID + "\publish"

[Setup]
; Gunakan GUID unik yang tetap untuk aplikasi ini
AppId={{8A9B2C1D-4E5F-6A7B-8C9D-0E1F2A3B4C5D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes

; Konfigurasi Output Installer
OutputDir=OutputInstaller
OutputBaseFilename=Setup_{#MyAppName}_v{#MyAppVersion}_{#TargetRID}
SetupIconFile=Assets\AppIcon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; Kompresi (Gunakan normal/fast jika ukuran single-file sudah terkompresi oleh .NET)
Compression=lzma2/normal
SolidCompression=yes
WizardStyle=modern

; Arsitektur 64-bit (Wajib agar tidak masuk ke Program Files (x86) dan mendukung WinUI 3)
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Minimum Windows 10 versi 1809 (Build 17763) sesuai TargetPlatformMinVersion di csproj
MinVersion=10.0.17763

; WAJIB ADMIN: Memaksa UAC prompt untuk instalasi sertifikat ke LocalMachine Root
PrivilegesRequired=admin

[Languages]
Name: "indonesian"; MessagesFile: "compiler:Languages\Indonesian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Masukkan seluruh isi folder publish (Single-File EXE + file pendukung WinAppSDK jika ada)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "*.pdb,*.xml"; Flags: ignoreversion recursesubdirs createallsubdirs

; Masukkan file sertifikat publik ke folder sementara
Source: "codesign.cer"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; 1. Instal sertifikat ke Trusted Root Certification Authorities (Local Machine)
Filename: "certutil.exe"; Parameters: "-addstore -f ""Root"" ""{tmp}\codesign.cer"""; StatusMsg: "Memasang sertifikat keamanan aplikasi..."; Flags: runhidden waituntilterminated

; 2. Opsi menjalankan aplikasi setelah selesai install
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Cabut sertifikat berdasarkan Thumbprint saat aplikasi di-uninstall
Filename: "certutil.exe"; Parameters: "-delstore ""Root"" ""{#CertThumbprint}"""; Flags: runhidden waituntilterminated; RunOnceId: "RemoveAppCert"

[UninstallDelete]
; Bersihkan sisa file log/cache di direktori instalasi jika ada
Type: filesandordirs; Name: "{app}"

[Code]
// Peringatan eksplisit sebelum wizard dimulai jika belum berjalan sebagai Admin
function InitializeSetup(): Boolean;
begin
  if not IsAdmin then
  begin
    MsgBox('Instalasi ini membutuhkan hak akses Administrator untuk memasang aplikasi dan sertifikat keamanan sistem.' + #13#10 + #13#10 + 'Silakan tutup, lalu klik kanan pada file Setup dan pilih "Run as administrator".', mbCriticalError, MB_OK);
    Result := False;
  end
  else
  begin
    Result := True;
  end;
end;
