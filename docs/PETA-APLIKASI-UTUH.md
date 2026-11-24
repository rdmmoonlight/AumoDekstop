# Peta Menuju Aplikasi Utuh — AumoDekstop

Dasar: repo `rdmmoonlight/AumoDekstop` (commit `683af43`) dibandingkan dengan backend `rdmmoonlight/aumo-finance` → `backend/AumoBackend`.
Cakupan: sambungan ke backend, komponen bersama (`Features/Shared`), dan kelengkapan halaman.

## Berkas Panduan

| Berkas | Isi | Urutan |
| ------ | --- | ------ |
| `docs/PETA-APLIKASI-UTUH.md` | Temuan, keputusan, urutan kerja (berkas ini) | 0 |
| `docs/FASE-1-SAMBUNGAN-BACKEND.md` | Lapisan HTTP, login JWT, sesi, kecocokan endpoint | 1 |
| `docs/FASE-2-SHARED-KOMPONEN.md` | Isi folder `Features/Shared`, hapus duplikasi | 2 |
| `docs/FASE-3-LENGKAPI-HALAMAN-DAN-RILIS.md` | Halaman belum tersambung, sinkron antar halaman, uji akhir, rilis | 3 |

Kerjakan berurutan. Satu langkah = satu commit. Setiap commit wajib lolos:

```powershell
dotnet build AumoDekstop.csproj -c Debug -p:Platform=x64 -p:WindowsPackageType=None --no-self-contained
```

## Aturan Wajib (dari `docs/ATURAN-MIGRASI.md`)

- Model UI memakai `class` dengan `{ get; set; }`. Tidak memakai `record` dan `init`.
- Tidak memanggil `Bindings.Update()` secara sembarangan.
- Setiap `<DataTemplate>` yang memakai `{x:Bind}` wajib punya `x:DataType`.
- Namespace diawali `AumoDekstop.Features.` dan ditulis paling atas, sebelum `using`.
- Logika akuntansi tetap di backend. Desktop hanya menampilkan dan mengirim data.

## Posisi Saat Ini (ringkas)

| Area | Kondisi | Status |
| ---- | ------- | ------ |
| Shell (ribbon, status bar, update) | Berjalan | Selesai |
| Login email + sandi | Berjalan, tetapi token tidak diterima | Perlu perbaikan |
| Sesi (ingat saya, kedaluwarsa) | Kode ada, tidak dipakai | Belum tersambung |
| Periods | Berjalan, 2 path salah | Perlu perbaikan |
| Chart of Accounts | Tersambung via cookie | Perlu seragam |
| Neraca saldo, kertas kerja, laporan keuangan | Terdaftar di DI dan ribbon | Perlu seragam |
| Buku besar (permanen, sementara) | Halaman ada, tidak terdaftar di DI dan ribbon | Belum tersambung |
| Jurnal umum, penyesuaian, penutup | Halaman ada, tidak terdaftar di DI dan ribbon | Belum tersambung |
| Input jurnal (`JournalEntryPage`) | Halaman ada, tidak ada di navigasi | Belum tersambung |
| Beranda, Pengaturan | Placeholder | Belum dibuat |
| `Features/Shared` | Tidak ada di repo | Belum dibuat |

## Temuan yang Menghalangi Sambungan

| Kode | Temuan | Bukti | Dampak |
| ---- | ------ | ----- | ------ |
| T1 | Login tidak meminta token. Backend hanya mengirim `token` bila `IsMobileClient = true`. | `AuthController.AuthSuccess(result, isMobileClient)`; `LoginRequest` desktop hanya berisi email dan sandi | `Token` kosong, `IsAuthenticated` selalu `false` |
| T2 | Bearer hanya dipasang pada `ApiService`. Service lain memakai `factory.CreateClient("AumoApi")` tanpa Bearer. | `AuthService.ApplyToken` → `ApiService.SetAuthToken`; `App.xaml.cs` | Service lain bergantung pada cookie. `CookieContainer` dibuat di dalam handler yang diputar ulang oleh `IHttpClientFactory`, sehingga sesi tidak terjamin |
| T3 | Path tidak sama dengan backend | Tabel T3 di bawah | Pilih/tutup periode, ubah/hapus jurnal gagal |
| T4 | Pendaftaran akun tidak punya endpoint | `AuthController` hanya: `login`, `google-login`, `me`, `logout` | `RegisterPage` tidak bisa berfungsi |
| T5 | Ingat saya tidak berjalan | `SessionStorageService` tidak dipanggil; `LoginViewModel.RememberMe` tidak dipakai; `LoginRequest` tanpa `RememberMe` | Pengguna login ulang setiap aplikasi dibuka |
| T6 | Tidak ada penanganan 401 terpusat | Tidak ada kode yang menangani 401 selain pesan login | Token habis → halaman gagal tanpa arah |
| T7 | Tiga gaya akses HTTP | `ApiService` (`ApiResponse<T>`), `HttpClient` mentah (`EnsureSuccessStatusCode`), `ApiService.Instance` sebagai cadangan | Pesan galat dan retry tidak seragam |
| T8 | Retry cold start Render hanya ada di `ApiService` | `SendRequestAsync` | Service dengan `HttpClient` mentah langsung gagal saat server tidur |
| T9 | Alamat server tertulis tetap | `ApiService.BaseServerUrl = "https://aumo.onrender.com/"` | Sulit ganti lingkungan uji |
| T10 | `Features/Shared` tidak ada di repo | Git tidak mencatat folder kosong | Komponen kembar di 4–5 tempat |
| T11 | `timeout` klien 60 detik sama dengan total retry 12 × 5 detik | `ApiService` ctor dan `App.xaml.cs` | Retry terpotong sebelum selesai |

### Tabel T3 — Path desktop dibanding backend

| Fitur | Desktop sekarang | Backend (sumber kebenaran) | Tindakan |
| ----- | ---------------- | -------------------------- | -------- |
| Pilih periode | `POST api/v1/periods/{id}/select` | `POST api/v1/periods/select/{id}` | Ubah desktop |
| Tutup periode | `POST api/v1/periods/{id}/close` | `POST api/v1/periods/close/{id}` | Ubah desktop |
| Ubah jurnal | `PUT api/v1/journal-entry/edit/{id}` | Tidak ada | Tunggu backend (B-1) |
| Hapus jurnal | `DELETE api/v1/journal-entry/delete/{id}` | Tidak ada | Tunggu backend (B-1) |
| Hapus jurnal penyesuaian | `DELETE api/v1/reports/journals/adjusting/{id}` | Tidak ada di `JournalsController` | Tunggu backend (B-1) |
| Sehat (health) | `GET ""` (root) | `GET api/v1/health` | Ubah desktop |

Endpoint lain sudah sama: `auth/login`, `auth/me`, `auth/logout`, `periods`, `periods/open-info`, `periods/clear-selection`, `chart-of-accounts`, `journal-entry/{id}`, `journal-entry/create`, `journal-entry/search-descriptions`, `journal-entry/next-transaction-number`, `reports/general-ledgers/{permanent,temporary,refresh}`, `reports/journals/{general,adjusting,closing}`, `reports/trial-balances`, `reports/worksheet`, `reports/{income-statement,retained-earnings,statement-of-cash-flow,statement-of-financial-position}`.

Backend menambahkan awalan `api/v1` lewat `ApiVersionPrefixConvention`. Rute pada atribut controller (`/auth`, `/periods`) tanpa awalan itu. Gunakan Scalar di `{server}/reference` untuk memeriksa bentuk data sebelum mengubah model.

## Keputusan

| Kode | Keputusan |
| ---- | --------- |
| K1 | Desktop memakai JWT Bearer (`IsMobileClient = true`), bukan cookie. Sesuai README backend: web memakai cookie, klien non-browser memakai JWT. |
| K2 | Satu `HttpClient` bernama `AumoApi` dengan dua handler: `ColdStartRetryHandler` dan `AuthHeaderHandler`. Cookie dimatikan (`UseCookies = false`). |
| K3 | Semua service memakai `ApiService` sebagai satu-satunya pintu. Tidak ada `HttpClient` mentah di service fitur. |
| K4 | Folder komponen bersama berada di `Features/Shared`. Alasan: namespace wajib `AumoDekstop.Features.*`. |
| K5 | Dependensi satu arah: `Features/*` boleh memakai `Features/Shared`. `Features/Shared` tidak boleh memakai fitur lain. |
| K6 | Desktop mengikuti backend. Endpoint yang belum ada di backend tidak ditebak. Tombolnya dinonaktifkan sampai backend siap. |
| K7 | Pendaftaran akun disembunyikan sampai backend menyediakan endpoint. |
| K8 | Impor COA tetap "belum tersedia". Backend belum punya endpoint impor. |

## Pekerjaan di Backend (di luar repo ini)

| Kode | Tugas | Diperlukan oleh |
| ---- | ----- | --------------- |
| B-1 | Tambah ubah dan hapus jurnal umum, hapus jurnal penyesuaian | Fase 3, butir 3.6 |
| B-2 | Tambah endpoint pendaftaran (opsional) | Fase 3, butir 3.7 |

Bila B-1 dan B-2 belum selesai, Fase 1–3 tetap dapat dikerjakan. Hanya butir terkait yang ditunda.

## Urutan Kerja

| No | Fase | Hasil yang dapat diuji |
| -- | ---- | ---------------------- |
| 1 | Fase 1 | Login → token → semua halaman lama memakai satu klien. Ingat saya dan sesi kedaluwarsa berjalan. |
| 2 | Fase 2 | `Features/Shared` terisi. Komponen kembar hilang. Tampilan seragam. |
| 3 | Fase 3 | Semua laporan dan input jurnal ada di ribbon. Uji siklus akuntansi penuh. Rilis. |

## Definisi Selesai (aplikasi utuh)

- [ ] Login, ingat saya, keluar, dan sesi kedaluwarsa berjalan tanpa cookie.
- [ ] Tidak ada `HttpClient` mentah dan tidak ada `ApiService.Instance` di kode.
- [ ] Semua endpoint pada Tabel T3 cocok dengan backend.
- [ ] `Features/Shared` dipakai oleh seluruh fitur. Tidak ada kelas kembar.
- [ ] Setiap menu ribbon membuka halaman nyata, tidak ada yang menampilkan Beranda sebagai pengganti.
- [ ] Siklus akuntansi pada Fase 3 butir 3.10 lolos dari awal sampai akhir.
- [ ] Build `dotnet build` hijau. Workflow rilis menghasilkan installer.

## Catatan Verifikasi

Panduan ini disusun dari pembacaan kode dan pencocokan dengan controller backend. Aplikasi belum dikompilasi dan dijalankan, karena WinUI 3 membutuhkan Windows. Jalankan build dan uji di mesin Windows pada setiap langkah.
