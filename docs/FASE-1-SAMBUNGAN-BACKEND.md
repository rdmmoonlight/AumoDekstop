# Fase 1 — Sambungan Backend yang Sempurna

Tujuan: satu jalur HTTP, login JWT, sesi tahan lama, dan endpoint cocok dengan backend.
Rujukan: `docs/PETA-APLIKASI-UTUH.md` (temuan T1–T11, keputusan K1–K3, K6).

Aturan: satu langkah = satu commit. Build wajib hijau di setiap commit.

## Struktur Baru

```text
Features/Shared/Http/
├── ApiConfig.cs                # alamat server dari konfigurasi
├── ITokenStore.cs              # antarmuka penyimpan token
├── TokenStore.cs               # token di memori + event sesi berakhir
├── AuthHeaderHandler.cs        # pasang Bearer, deteksi 401
└── ColdStartRetryHandler.cs    # ulang otomatis saat Render tidur
```

Folder `Features/Shared` belum ada di repo. Buat sekarang. Isi komponen UI menyusul di Fase 2.

## Langkah

### 1.1 Alamat server dari konfigurasi (T9)

- [x] Buat `ApiConfig.BaseUrl` dengan urutan baca: variabel lingkungan `AUMO_API_URL` → `AppLocalSettings` kunci `ApiBaseUrl` → bawaan `https://aumo.onrender.com`.
- [x] Pastikan hasilnya selalu berawalan `/`.
- [x] Ganti semua pemakaian `ApiService.BaseServerUrl` (`App.xaml.cs`, `MainWindow.Auth.cs`, `MainWindow.Navigation.cs`) dengan `ApiConfig.BaseUrl`.
- [x] Hapus konstanta `BaseServerUrl` dari `ApiService`.

### 1.2 Penyimpan token (T2)

- [x] Buat `ITokenStore`:

```csharp
namespace AumoDekstop.Features.Shared.Http;

public interface ITokenStore
{
    string? Token { get; }
    bool HasToken { get; }
    void Set(string? token);
    void Clear();
    event EventHandler? SessionExpired;
    void RaiseSessionExpired();
}
```

- [x] Implementasikan `TokenStore` (singleton). `Clear()` mengosongkan token. `RaiseSessionExpired()` memicu event hanya bila `HasToken` bernilai `true`, lalu memanggil `Clear()`. Cegah pemicuan ganda.

### 1.3 Handler Bearer dan 401 (T2, T6)

- [x] Buat `AuthHeaderHandler : DelegatingHandler`.
  - Sebelum kirim: bila `HasToken`, isi `Authorization: Bearer {token}`.
  - Sesudah terima: bila status 401, permintaan bukan ke `auth/login` dan `auth/google-login`, panggil `RaiseSessionExpired()`.
  - Handler tidak mengulang permintaan.

### 1.4 Handler retry cold start (T8, T11)

- [x] Buat `ColdStartRetryHandler : DelegatingHandler`. Pindahkan aturan dari `ApiService.SendRequestAsync`:

| Kondisi                | Diulang untuk |
| ---------------------- | ------------- |
| 502, 503               | Semua metode  |
| 504                    | GET dan PUT   |
| `HttpRequestException` | GET dan PUT   |

- [x] Maksimal 12 kali, jeda 5 detik. Hormati `CancellationToken`.
- [x] `HttpRequestMessage` tidak boleh dikirim dua kali. Sebelum pengiriman pertama, baca isi permintaan ke `byte[]` (`ReadAsByteArrayAsync`), lalu buat klon permintaan (metode, URI, header, isi, header isi) untuk setiap percobaan.
- [x] Bebaskan `HttpResponseMessage` dari percobaan yang gagal sebelum mengulang.

### 1.5 Daftarkan satu klien di DI (K2)

Ganti blok `AddHttpClient("AumoApi")` di `App.xaml.cs`:

```csharp
services.AddSingleton<ITokenStore, TokenStore>();
services.AddTransient<AuthHeaderHandler>();
services.AddTransient<ColdStartRetryHandler>();

services.AddHttpClient("AumoApi", client =>
    {
        client.BaseAddress = new Uri(ApiConfig.BaseUrl);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        // Harus lebih besar dari total retry (12 x 5 detik)
        client.Timeout = TimeSpan.FromSeconds(120);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        UseCookies = false,
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    })
    .AddHttpMessageHandler<ColdStartRetryHandler>()   // terluar
    .AddHttpMessageHandler<AuthHeaderHandler>();      // terdalam
```

- [x] Hapus `CookieContainer` dan `HttpClientHandler` lama.
- [x] Pasang `X-Client-Platform` dan `User-Agent` pada `DefaultRequestHeaders` klien ini (pindah dari konstruktor `ApiService`).

### 1.6 `ApiService` menjadi satu-satunya pintu (T7, K3)

- [x] Hapus `Instance`, konstruktor opsional `HttpClient? = null`, `SetAuthToken`, dan seluruh loop retry.
- [x] Konstruktor menjadi `ApiService(HttpClient httpClient)`. Tetap daftarkan sebagai singleton dari `factory.CreateClient("AumoApi")`.
- [x] Pertahankan `ApiResponse<T>`, `GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync`, dan pemetaan galat.
- [x] Tambahkan `DeleteAsync<T>` dan `PostAsync<T>` tanpa isi (untuk `select`, `close`, `refresh`) bila belum ada.
- [x] Ubah `AuthService` dan `PeriodsApiService`: hilangkan `?? ApiService.Instance`. Konstruktor wajib menerima `ApiService`.
- [x] Perbaiki `CheckServerHealthAsync` agar memanggil `api/v1/health`. Ganti panggilan `CheckServerHealthAsync("")` di `App.OnLaunched`.

### 1.7 Login meminta token (T1)

- [x] Ubah `LoginRequest` menjadi `class` (aturan migrasi):

```csharp
namespace AumoDekstop.Features.Auth.Models;

using System.Text.Json.Serialization;

public class LoginRequest
{
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;
    [JsonPropertyName("rememberMe")] public bool RememberMe { get; set; }
    [JsonPropertyName("isMobileClient")] public bool IsMobileClient { get; set; } = true;
    [JsonPropertyName("operatingSystem")] public string OperatingSystem { get; set; } = "Windows";
}
```

- [x] Lakukan hal yang sama untuk `GoogleLoginRequest`: tambahkan `IsMobileClient = true`.
- [x] Perbarui pemanggil `new LoginRequest(Email.Trim(), Password)` di `LoginViewModel` menjadi inisialisasi objek, termasuk `RememberMe`.
- [x] `AuthService.ApplyToken` memanggil `ITokenStore.Set(token)`, bukan `ApiService.SetAuthToken`.
- [x] `AuthService.IsAuthenticated` membaca `ITokenStore.HasToken`.
- [x] Bila login berhasil tetapi `Token` kosong, kembalikan galat "Server tidak mengirim token". Jangan masuk ke aplikasi.

### 1.8 Ingat saya dan pemulihan sesi (T5)

- [x] Setelah login sukses, panggil `SessionStorageService.SaveSession(email, token, rememberMe)`.
- [x] Tambahkan `IAuthService.RestoreSessionAsync()`:
  1. `GetSavedSession()`. Bila kosong → kembalikan `false`.
  2. `ITokenStore.Set(token)`.
  3. `GET api/v1/auth/me`. Bila 200 → isi `CachedProfile`, kembalikan `true`.
  4. Bila 401 → `ClearSession()` dan `SessionStorageService.ClearSession()`, kembalikan `false`.
  5. Bila galat jaringan (status 0) → pertahankan token, kembalikan `false`, tampilkan login dengan pesan "Server belum dapat dihubungi".
- [x] Ubah `MainWindow.RootElement_Loaded`: tampilkan status "Memulihkan sesi...", panggil `RestoreSessionAsync()`, lalu `EnterApp()` atau `ShowLogin()`. Jangan lagi memakai `_auth.IsAuthenticated` sebagai gerbang awal.
- [x] `LogoutAsync`: panggil endpoint, lalu `ITokenStore.Clear()`, `SessionStorageService.ClearSession()`, dan `ShowLogin()`.

### 1.9 Sesi berakhir terpusat (T6)

- [x] Di `MainWindow`, berlangganan `ITokenStore.SessionExpired`. Event datang dari utas non-UI, jadi pindahkan ke `DispatcherQueue.TryEnqueue`.
- [x] Handler: bersihkan sesi tersimpan, `ShowLogin()`, tampilkan pesan "Sesi berakhir, silakan masuk kembali."
- [x] Lepas langganan saat jendela ditutup.

### 1.10 Perbaiki path (Tabel T3)

- [ ] `PeriodsApiService`: `api/v1/periods/select/{id}` dan `api/v1/periods/close/{id}`.
- [ ] Cocokkan isi `CreatePeriodAsync` dengan `CreatePeriodRequest` backend: `Month`, `Year`, `SetupMode` (`LoadExisting` atau `CreateNew`), `CashAccountId`, `BankAccountId`, dan akun laba ditahan. Buka `backend/AumoBackend/DTOs/PeriodsDto.cs` untuk daftar lengkap, lalu cek bentuk respons `open-info` di Scalar.
- [ ] Jurnal: biarkan path ubah/hapus apa adanya sampai B-1. Lihat Fase 3 butir 3.6.

### 1.11 Seragamkan service ke `ApiService` (T7, K3)

Satu service per commit. Urutan dari yang paling sederhana:

- [ ] `TrialBalancesApiService`
- [ ] `FinancialStatementsApiService`
- [ ] `WorksheetApiService`
- [ ] `ChartOfAccountsService`
- [ ] `JournalEntryService`
- [ ] `GeneralLedgersService`
- [ ] `JournalsReportService`

Untuk setiap service:

1. Konstruktor menerima `ApiService`, bukan `HttpClient`.
2. Ganti `GetFromJsonAsync`, `PostAsJsonAsync`, `EnsureSuccessStatusCode` dengan `ApiResponse<T>`.
3. Pertahankan antarmuka publik service selama tidak menghambat penyeragaman. Bila galat dulu dilempar sebagai exception, ViewModel sekarang membaca `IsSuccess` dan `ErrorMessage`.
4. Ubah registrasi DI-nya di `App.xaml.cs` menjadi `AddSingleton<IXxx, Xxx>()` tanpa `IHttpClientFactory`.
5. Hapus `using System.Net.Http` yang tidak terpakai.
6. Bangun dan jalankan halaman terkait.

Setelah semua selesai:

- [ ] `grep -rn "CreateClient\|new HttpClient\|ApiService.Instance" --include=*.cs .` hanya boleh menemukan satu `CreateClient("AumoApi")` di `App.xaml.cs`.

### 1.12 Pesan galat seragam

- [ ] Tambahkan pemetaan status di `ApiService`:

| Status | Pesan                                                                                          |
| ------ | ---------------------------------------------------------------------------------------------- |
| 0      | Server tidak dapat dihubungi                                                                   |
| 401    | Sesi berakhir (ditangani handler)                                                              |
| 403    | Akses ditolak                                                                                  |
| 404    | Data tidak ditemukan                                                                           |
| 429    | Terlalu banyak permintaan, tunggu sebentar. Bila ada `lockoutEnd` pada isi, tampilkan waktunya |
| 5xx    | Server bermasalah                                                                              |

Backend memberi batas laju global 100 permintaan per jendela waktu, dan kebijakan lebih ketat pada otentikasi. Jangan memuat banyak laporan sekaligus saat startup.

## Uji Sambungan

Jalankan setelah langkah 1.11. Server Render bisa butuh sampai 60 detik pada permintaan pertama.

| No  | Uji                                                                 | Harapan                                                               |
| --- | ------------------------------------------------------------------- | --------------------------------------------------------------------- |
| 1   | Login sandi benar                                                   | 200, `token` terisi, ribbon tampil                                    |
| 2   | Login sandi salah                                                   | Pesan galat dari backend, tetap di halaman login                      |
| 3   | Tutup aplikasi, buka lagi (ingat saya aktif)                        | Langsung masuk, tanpa login                                           |
| 4   | Tutup aplikasi, buka lagi (ingat saya mati)                         | Halaman login                                                         |
| 5   | Hapus token tersimpan atau tunggu kedaluwarsa, buka halaman Periods | Kembali ke login dengan pesan "Sesi berakhir"                         |
| 6   | Pilih periode lalu buka Neraca Saldo                                | Data sesuai periode terpilih                                          |
| 7   | Tutup periode                                                       | Sukses atau pesan validasi dari backend (neraca saldo wajib seimbang) |
| 8   | Putuskan internet saat membuka COA                                  | Pesan "Server tidak dapat dihubungi", aplikasi tidak macet            |
| 9   | Server tidur, login pertama                                         | Aplikasi menunggu dan mengulang otomatis, berhasil dalam 60 detik     |
| 10  | Keluar                                                              | Kembali ke login, token tersimpan terhapus                            |

## Selesai Fase 1 Bila

- [ ] Tabel uji 1–10 lolos.
- [ ] Hanya ada satu pembuat `HttpClient` (`App.xaml.cs`).
- [ ] Tidak ada cookie yang dipakai untuk sesi.
- [ ] Tidak ada URL server tertulis tetap selain nilai bawaan di `ApiConfig`.
