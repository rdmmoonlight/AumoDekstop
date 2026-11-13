; --- KONFIGURASI DASAR ---
#define MyAppName "Nama Aplikasi WinUI Anda"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Nama Tim / Perusahaan Anda"
#define MyAppExeName "AplikasiWinUIAnda.exe"
#define CertThumbprint "0569E0943DADA39CE0CD06CE2F09A0CA1B6A4B4E"

[Setup]
AppId={{8A9B2C1D-4E5F-6A7B-8C9D-0E1F2A3B4C5D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=OutputInstaller
OutputBaseFilename=Setup_{#MyAppName}_v{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

; WAJIB ADMIN: Memaksa UAC prompt jika user lupa klik kanan "Run as administrator"
PrivilegesRequired=admin

[Files]
; Masukkan seluruh hasil publish WinUI (sesuaikan path Source)
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Masukkan file sertifikat publik ke folder sementara
Source: "codesign.cer"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
; 1. Instal sertifikat ke Trusted Root saat proses instalasi berjalan
Filename: "certutil.exe"; Parameters: "-addstore -f ""Root"" ""{tmp}\codesign.cer"""; StatusMsg: "Memasang sertifikat keamanan internal..."; Flags: runhidden waituntilterminated

; 2. Opsi menjalankan aplikasi setelah selesai install
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Cabut sertifikat berdasarkan Thumbprint saat aplikasi di-uninstall
Filename: "certutil.exe"; Parameters: "-delstore ""Root"" ""{#CertThumbprint}"""; Flags: runhidden waituntilterminated

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
