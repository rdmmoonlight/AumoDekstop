# Fase 3 — Lengkapi Halaman, Sinkron, Uji Akhir, dan Rilis

Tujuan: setiap menu ribbon membuka halaman nyata, data selalu sesuai periode terpilih, dan aplikasi siap dirilis.
Prasyarat: Fase 1 dan Fase 2 selesai.
Rujukan: `docs/PETA-APLIKASI-UTUH.md` (T4, T8, K6–K8, B-1, B-2).

## Halaman yang Belum Tersambung

| Halaman | Kondisi di kode | Yang kurang |
| ------- | --------------- | ----------- |
| Buku Besar Permanen | `GeneralLedgersPermanentPage` ada | DI, ViewModel, ribbon |
| Buku Besar Sementara | `GeneralLedgersTemporaryPage` ada | DI, ViewModel, ribbon |
| Jurnal Umum | `GeneralJournalTable` ada | Halaman, DI, ViewModel, ribbon |
| Jurnal Penyesuaian | Layanan ada di `JournalsReportService` | Halaman, DI, ViewModel, ribbon |
| Jurnal Penutup | `ClosingGroupTableControl` ada | Halaman, DI, ViewModel, ribbon |
| Input Jurnal | `JournalEntryPage` ada | Masuk navigasi (tab Data) |
| Beranda | `MainPage` berisi teks saja | Ringkasan dari backend |
| Pengaturan | Tab hanya menampilkan Beranda | Halaman pengaturan |

## Langkah

### 3.1 Daftarkan di DI

Ikuti pola Trial Balances di `App.xaml.cs`. Satu halaman = satu commit.

- [ ] `IGeneralLedgersService` → `GeneralLedgersService` (singleton, memakai `ApiService`).
- [ ] `GeneralLedgersViewModel` (transient) dengan `LoadState`, daftar akun, dan perintah muat ulang. Muat ulang memanggil `POST api/v1/reports/general-ledgers/refresh`.
- [ ] `GeneralLedgersPermanentPage`, `GeneralLedgersTemporaryPage` (transient).
- [ ] `IJournalsReportService` → `JournalsReportService` (singleton).
- [ ] `JournalsReportViewModel` (transient) untuk jurnal umum, penyesuaian, dan penutup. Jurnal penutup hanya dibaca. Backend menghitung jurnal penutup dari neraca saldo.
- [ ] Halaman `GeneralJournalPage`, `AdjustingJournalPage`, `ClosingJournalPage` memakai tabel yang sudah ada serta kontrol dari Fase 2.
- [ ] `JournalEntryPage` (transient). Pastikan `JournalEntryFormViewModel` memakai `ApiService` (butir 1.11).

### 3.2 Ribbon dan perintah

- [ ] Periksa `RibbonReportsPanel.xaml` dan `RibbonDataPanel.xaml`. Tambahkan tombol yang belum ada.
- [ ] Tambahkan perintah di `MainWindow.Commands.cs` dengan pola `ShowReportPageAsync<TPage>`:

| Command ID | Halaman | Tombol |
| ---------- | ------- | ------ |
| `GeneralLedgerPermanent` | `GeneralLedgersPermanentPage` | Reports |
| `GeneralLedgerTemporary` | `GeneralLedgersTemporaryPage` | Reports |
| `GeneralJournal` | `GeneralJournalPage` | Reports |
| `AdjustingJournal` | `AdjustingJournalPage` | Reports |
| `ClosingJournal` | `ClosingJournalPage` | Reports |

- [ ] Urutan menu Reports mengikuti siklus akuntansi: Jurnal Umum → Jurnal Penyesuaian → Buku Besar → Neraca Saldo → Kertas Kerja → Jurnal Penutup → Laporan Keuangan.
- [ ] Sisipkan perintah baru di `RibbonCommand` bila diperlukan, jangan memakai string bebas.

### 3.3 Navigasi tab

Ubah `OnRibbonTabChanged` di `MainWindow.Navigation.cs`:

- [ ] `"data"` → `JournalEntryPage` (dibuat baru setiap dibuka, agar tidak membawa data akun lain).
- [ ] `"report"` → tampilkan halaman laporan terakhir yang dibuka. Bila belum ada, tampilkan Jurnal Umum.
- [ ] `"settings"` → `SettingsPage` (butir 3.8).
- [ ] `"home"` → `HomePage` (butir 3.4).
- [ ] Hapus cabang `default` yang menampilkan Beranda untuk tab lain.

Perintah `New` dan `Save` pada ribbon bekerja menurut halaman aktif:

| Halaman aktif | `New` | `Save` |
| ------------- | ----- | ------ |
| `ChartOfAccountsPage` | Tambah akun | Tidak ada |
| `PeriodsPage` | Buka periode baru | Tidak ada |
| `JournalEntryPage` | Form jurnal baru | Simpan jurnal |
| Lainnya | Nonaktif | Nonaktif |

- [ ] Nonaktifkan tombol yang tidak berlaku pada halaman aktif. Tombol tidak boleh diam tanpa respons.

### 3.4 Beranda

- [ ] Buat `HomePage` dan `HomeViewModel`.
- [ ] Sumber data: `GET api/v1/summary` dan `GET api/v1/dashboard`. Sebelum menulis model, buka Scalar (`{server}/reference`) dan salin bentuk respons yang sebenarnya.
- [ ] Tampilkan: periode terpilih, total aset, kewajiban, ekuitas, laba bersih, dan jurnal terbaru, sebatas yang disediakan backend.
- [ ] Pakai `StateCard` untuk keadaan tanpa periode dan `AlertBanner` untuk galat.

### 3.5 Sinkron antar halaman

Server menyimpan periode terpilih per pengguna. Halaman yang terbuka harus ikut berubah saat periode berganti atau jurnal berubah.

- [ ] Buat pesan di `Features/Shared/Messages`: `PeriodChangedMessage`, `OpenPeriodsRequestedMessage`. Pakai `WeakReferenceMessenger` (sudah dipakai oleh `JournalEntryChangedMessage`).
- [ ] `PeriodsPageViewModel` mengirim `PeriodChangedMessage` setelah pilih, buka, atau tutup periode berhasil.
- [ ] `JournalEntryService` sudah mengirim `JournalEntryChangedMessage`. Kirim pesan yang sama setelah hapus jurnal.
- [ ] Semua ViewModel laporan berlangganan kedua pesan dan memuat ulang bila halaman sedang terlihat. Bila tidak terlihat, tandai "perlu muat ulang" dan muat saat halaman dibuka.
- [ ] `StateCard` aksi "Buka Periods" mengirim `OpenPeriodsRequestedMessage`. `MainWindow` menerima dan memanggil `RibbonHost` ke tab Periods.
- [ ] Lepas langganan saat halaman ditinggalkan agar tidak bocor memori.

### 3.6 Ubah dan hapus jurnal (bergantung pada B-1)

Backend belum memiliki endpoint ubah jurnal, hapus jurnal, dan hapus jurnal penyesuaian (Tabel T3).

- [ ] Sekarang: nonaktifkan tombol Ubah dan Hapus jurnal di UI. Jangan memanggil path yang tidak ada.
- [ ] Setelah B-1 selesai di backend: salin path dan bentuk isi dari Scalar, perbarui `JournalEntryService.EditAsync`, `DeleteAsync`, dan `JournalsReportService.DeleteAdjustingAsync`, lalu aktifkan tombol.
- [ ] Hapus memakai `ConfirmDialog` (`IsDestructive = true`).
- [ ] Setelah berhasil, kirim `JournalEntryChangedMessage`.

### 3.7 Pendaftaran akun (bergantung pada B-2)

- [ ] Sekarang: sembunyikan tautan "Daftar" di `LoginPage`. `RegisterPage` dan `RegisterViewModel` tetap di repo.
- [ ] Setelah B-2 selesai: tulis `RegisterRequest` sebagai `class`, tambah `AuthService.RegisterAsync`, hubungkan `RegisterViewModel`, dan tampilkan kembali tautannya.

### 3.8 Pengaturan

- [ ] `SettingsPage` dengan bagian:

| Bagian | Isi | Penyimpanan |
| ------ | --- | ----------- |
| Tampilan | Tema Dark, Light, atau Sistem | `App.SetTheme` (sudah ada) |
| Server | Alamat API, tombol "Uji koneksi" memanggil `api/v1/health` | `AppLocalSettings` kunci `ApiBaseUrl` |
| Akun | Nama dan email dari `GET api/v1/auth/me`, tombol Keluar | Memori (`CachedProfile`) |
| Pembaruan | Versi saat ini, tombol "Cek pembaruan" | `UpdateCoordinator` (sudah ada) |

- [ ] Perubahan alamat server berlaku setelah aplikasi dibuka ulang. Tampilkan catatan itu di halaman.

### 3.9 Keadaan halaman yang konsisten

Untuk setiap halaman di ribbon:

- [ ] Memuat → `LoadingState`.
- [ ] Tanpa periode → `StateCard` dengan aksi "Buka Periods".
- [ ] Data kosong → `StateCard`.
- [ ] Galat → `AlertBanner` + "Coba lagi".
- [ ] Tidak ada pemuatan ganda saat halaman dibuka (cek lewat log atau status bar).
- [ ] Tidak ada pengecualian yang tidak tertangkap di `async void`. Bungkus dengan `try/catch` dan tampilkan di `AlertBanner`.

### 3.10 Uji akhir menurut siklus akuntansi

Jalankan sekali dari awal, memakai akun uji. Catat hasil pada kolom paling kanan.

| No | Langkah | Harapan | Lolos |
| -- | ------- | ------- | ----- |
| 1 | Login | Masuk, ribbon tampil | [ ] |
| 2 | Periods → buka periode baru (`CreateNew`) | Periode muncul, status terbuka | [ ] |
| 3 | Periods → pilih periode | Beranda dan laporan mengikuti periode | [ ] |
| 4 | COA → tambah, ubah, hapus akun | Daftar akun diperbarui | [ ] |
| 5 | Input Jurnal → simpan jurnal seimbang | Nomor transaksi terisi otomatis, jurnal tersimpan | [ ] |
| 6 | Input Jurnal → simpan jurnal tidak seimbang | Ditolak dengan pesan dari backend | [ ] |
| 7 | Jurnal Umum | Jurnal langkah 5 muncul | [ ] |
| 8 | Jurnal Penyesuaian (input lalu lihat) | Muncul di daftar penyesuaian | [ ] |
| 9 | Buku Besar Permanen dan Sementara | Saldo sesuai jurnal | [ ] |
| 10 | Neraca Saldo: belum disesuaikan, disesuaikan | Total debit = total kredit | [ ] |
| 11 | Kertas Kerja | Kolom dan total sesuai neraca saldo | [ ] |
| 12 | Laporan Keuangan: laba rugi, perubahan modal, posisi keuangan, arus kas | Angka konsisten satu sama lain | [ ] |
| 13 | Jurnal Penutup | Tampil dan hanya dapat dibaca | [ ] |
| 14 | Periods → tutup periode | Sukses. Bila neraca saldo tidak seimbang, ditolak dengan pesan | [ ] |
| 15 | Neraca Saldo setelah penutupan | Akun nominal bersaldo nol | [ ] |
| 16 | Keluar, buka lagi (ingat saya aktif) | Langsung masuk | [ ] |
| 17 | Server tidur, lalu buka halaman apa saja | Menunggu dan berhasil, tanpa error mentah | [ ] |
| 18 | Ganti tema Dark dan Light | Semua halaman terbaca | [ ] |

### 3.11 Rilis

Workflow yang sudah ada: `.github/workflows/dotnet-desktop.yml` (manual, `workflow_dispatch`, Windows, installer Inno Setup `setup.iss`) dan `code-quality.yml`.

- [ ] Jalankan `code-quality.yml`. Perbaiki semua temuan.
- [ ] Pastikan versi yang dihasilkan workflow (CalVer `yy.M.N.PC`) terbaca oleh `GitHubUpdateService.GetCurrentVersionString()`. `AumoDekstop.csproj` masih berisi `Version` `1.0.0`. Cek apakah workflow menimpanya saat publish. Bila tidak, aplikasi akan selalu menganggap dirinya versi lama.
- [ ] Jalankan `dotnet-desktop.yml` dan unduh installer. Pasang di mesin bersih.
- [ ] Ulangi uji 1, 3, 16, dan 17 pada hasil instalasi.
- [ ] Pasang versi lama, rilis versi baru, lalu cek pembaruan otomatis dari ribbon sampai aplikasi diperbarui.
- [ ] Jangan memasukkan token, kata sandi, atau alamat rahasia ke repo. `GOOGLE_CLIENT_ID` dibaca dari variabel lingkungan (`AuthHelpers.GoogleClientId`). Pastikan tersedia di mesin pengguna atau tombol Google disembunyikan bila kosong.

## Selesai Fase 3 Bila

- [ ] Semua butir 3.1–3.9 tercentang. Butir 3.6 dan 3.7 tercentang sebagian bila B-1 dan B-2 belum selesai, dengan catatan.
- [ ] Tabel uji 3.10: seluruh baris lolos kecuali baris yang bergantung pada B-1.
- [ ] Installer hasil workflow lolos uji 3.11.
