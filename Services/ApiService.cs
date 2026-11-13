using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AumoDekstop.Services;

/// <summary>
/// Pembungkus standar untuk semua respons dari backend Aumo.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
}

public class ApiService
{
    public const string BaseServerUrl = "https://aumo.onrender.com/";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    // Singleton ringan agar bisa diakses dari seluruh halaman/window dengan 1 sesi koneksi
    private static ApiService? _instance;
    public static ApiService Instance => _instance ??= new ApiService();

    public ApiService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(BaseServerUrl);
        }

        // Timeout 60 detik untuk mengantisipasi cold-start server Render
        _httpClient.Timeout = TimeSpan.FromSeconds(60);

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"AumoDekstop/{GitHubUpdateService.GetCurrentVersionString()}");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Platform", "Windows-Desktop");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Menyimpan JWT / API Token setelah login agar otomatis terbawa di setiap request berikutnya.
    /// </summary>
    public void SetAuthToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Mengecek koneksi & membangunkan server Render (ping/health check).
    /// </summary>
    public async Task<bool> CheckServerHealthAsync(string healthEndpoint = "health", CancellationToken ct = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(healthEndpoint.TrimStart('/'), ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        return await SendRequestAsync<T>(() => _httpClient.GetAsync(endpoint.TrimStart('/'), ct), ct);
    }

    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        return await SendRequestAsync<TResponse>(() => _httpClient.PostAsJsonAsync(endpoint.TrimStart('/'), payload, _jsonOptions, ct), ct);
    }

    public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        return await SendRequestAsync<TResponse>(() => _httpClient.PutAsJsonAsync(endpoint.TrimStart('/'), payload, _jsonOptions, ct), ct);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync(endpoint.TrimStart('/'), ct);
            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { IsSuccess = true, StatusCode = (int)response.StatusCode, Data = true };
            }

            var err = await response.Content.ReadAsStringAsync(ct);
            return new ApiResponse<bool>
            {
                IsSuccess = false,
                StatusCode = (int)response.StatusCode,
                ErrorMessage = string.IsNullOrWhiteSpace(err) ? response.ReasonPhrase : err
            };
        }
        catch (Exception ex)
        {
            return HandleException<bool>(ex);
        }
    }

    private async Task<ApiResponse<T>> SendRequestAsync<T>(Func<Task<HttpResponseMessage>> httpCall, CancellationToken ct)
    {
        try
        {
            using var response = await httpCall();
            int statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                // Jika server mengembalikan 204 No Content
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return new ApiResponse<T> { IsSuccess = true, StatusCode = statusCode };
                }

                var data = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, ct);
                return new ApiResponse<T>
                {
                    IsSuccess = true,
                    StatusCode = statusCode,
                    Data = data
                };
            }

            string errorBody = await response.Content.ReadAsStringAsync(ct);
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                ErrorMessage = string.IsNullOrWhiteSpace(errorBody) ? $"HTTP {statusCode}: {response.ReasonPhrase}" : errorBody
            };
        }
        catch (Exception ex)
        {
            return HandleException<T>(ex);
        }
    }

    private static ApiResponse<T> HandleException<T>(Exception ex)
    {
        string message = ex switch
        {
            TaskCanceledException => "Koneksi timeout. Server Render mungkin sedang bangun (cold start), silakan coba lagi.",
            HttpRequestException httpEx => $"Gagal terhubung ke server: {httpEx.Message}",
            JsonException => "Format respons JSON dari server tidak sesuai.",
            _ => $"Kesalahan tidak terduga: {ex.Message}"
        };

        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = 0,
            ErrorMessage = message
        };
    }
}
