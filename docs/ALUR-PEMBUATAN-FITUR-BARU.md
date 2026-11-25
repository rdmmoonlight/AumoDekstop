Untuk fitur-fitur baru ke depannya, seluruh alur HTTP dan autentikasi sudah terpusat sehingga kamu **tidak perlu lagi mengurus token, header `Authorization`, atau `CookieContainer` secara manual** di tiap service baru.

Alur kerja saat menambah fitur baru cukup **4 langkah**:

### 1. Struktur Folder Fitur

Ikuti pola _vertical slice_ yang sudah berjalan di proyekmu:

```text
Features/
└── NamaFiturBaru/
    ├── Models/        # DTO Request & Response
    ├── Services/      # INamaFiturService.cs & NamaFiturService.cs
    ├── ViewModels/    # NamaFiturViewModel.cs
    └── Views/         # NamaFiturPage.xaml & NamaFiturPage.xaml.cs

```

### 2. Buat Service Fitur dengan Menerima `HttpClient`

Cukup minta `HttpClient` melalui constructor. Tidak perlu membuat `new HttpClient()` sendiri dan tidak perlu memasang token manual:

```csharp
namespace AumoDekstop.Features.NamaFiturBaru.Services;

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

public interface INamaFiturService
{
    Task<ItemDto?> GetDataAsync(CancellationToken ct = default);
}

public sealed class NamaFiturService : INamaFiturService
{
    private readonly HttpClient _http;

    public NamaFiturService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ItemDto?> GetDataAsync(CancellationToken ct = default)
    {
        // Otomatis mengarah ke ApiConfig.BaseUrl & membawa Bearer token dari BearerAuthHandler
        return await _http.GetFromJsonAsync<ItemDto>("api/v1/nama-endpoint", ct);
    }
}

```

_(Alternatif: jika ingin memakai pembungkus `ApiResponse<T>` dan fitur retry cold-start Render, service baru cukup menerima `ApiService` di constructor seperti `PeriodsApiService`.)_

### 3. Daftarkan di `Extensions/DependencyInjection.cs`

Tambahkan registrasinya di `AddAumoServices()` menggunakan named client `"AumoApi"`:

```csharp
// Di dalam DependencyInjection.cs
services.AddTransient<INamaFiturService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new NamaFiturService(factory.CreateClient("AumoApi"));
});
services.AddTransient<NamaFiturViewModel>();
services.AddTransient<NamaFiturPage>();

```

_Gunakan `AddTransient` sebagai default untuk Service, ViewModel, dan Page agar tidak menyimpan state basi antar buka-tutup halaman (kecuali memang butuh cache lintas halaman seperti `AuthService`)._

### 4. Alur Eksekusi Otomatis Saat Runtime

Begitu langkah 1–3 selesai, setiap request dari fitur barumu otomatis melewati pipa yang sama:

1. `factory.CreateClient("AumoApi")` memberi `HttpClient` dengan `BaseAddress = ApiConfig.BaseUrl`.
2. `BearerAuthHandler` mencegat request keluar → membaca `ITokenStore.Token` → menempelkan `Authorization: Bearer <token>`.
3. Jika server membalas `401 Unauthorized` (token kedaluwarsa) → `BearerAuthHandler` otomatis memanggil `ITokenStore.RaiseSessionExpired()` sehingga aplikasi bisa langsung mengarahkan user kembali ke halaman login secara seragam di semua fitur.
