using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Shared.Http;

namespace AumoDekstop.Services;

/// <summary>
/// Pembungkus standar untuk semua respons dari backend Aumo.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    private const int MaxRetries = 2;
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromMilliseconds(600);

    public ApiService(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = AppJsonContext.Default
        };
    }

    public void SetAuthToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            TokenStore.Instance.Clear();
        }
        else
        {
            TokenStore.Instance.Set(token);
        }
    }

    public async Task<bool> CheckServerHealthAsync(string healthEndpoint = "health", CancellationToken ct = default)
    {
        var normalized = NormalizeEndpoint(healthEndpoint);
        try
        {
            using var response = await _httpClient.GetAsync(normalized, ct);
            if (!response.IsSuccessStatusCode)
            {
                AppLogger.Warn("ApiService", $"Health check GET {normalized} gagal: HTTP {(int)response.StatusCode}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            AppLogger.Warn("ApiService", $"Health check GET {normalized} exception: {ex.Message}");
            return false;
        }
    }

    // =========================================================================
    // HTTP METHODS (Satu-satunya pintu untuk seluruh feature service)
    // =========================================================================

    public Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<T>("GET", path, () => _httpClient.GetAsync(path, ct), isIdempotent: true, ct);
    }

    public Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<TResponse>("POST", path, () => _httpClient.PostAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<bool>> PostAsync<TRequest>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<bool>("POST", path, () => _httpClient.PostAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<TResponse>> PostEmptyAsync<TResponse>(string endpoint, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<TResponse>("POST", path, () => _httpClient.PostAsync(path, content: null, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<bool>> PostEmptyAsync(string endpoint, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<bool>("POST", path, () => _httpClient.PostAsync(path, content: null, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<TResponse>> PostMultipartAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<TResponse>("POST", path, () => _httpClient.PostAsync(path, content, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<TResponse>("PUT", path, () => _httpClient.PutAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: true, ct);
    }

    public Task<ApiResponse<bool>> PutAsync<TRequest>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<bool>("PUT", path, () => _httpClient.PutAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: true, ct);
    }

    public Task<ApiResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<TResponse>("PATCH", path, () => _httpClient.PatchAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<bool>> PatchAsync<TRequest>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<bool>("PATCH", path, () => _httpClient.PatchAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<bool>("DELETE", path, () => _httpClient.DeleteAsync(path, ct), isIdempotent: true, ct);
    }

    public Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string endpoint, CancellationToken ct = default)
    {
        var path = NormalizeEndpoint(endpoint);
        return SendRequestAsync<TResponse>("DELETE", path, () => _httpClient.DeleteAsync(path, ct), isIdempotent: true, ct);
    }

    // =========================================================================
    // PIPELINE EKSEKUSI, RETRY & PARSING SERAGAM
    // =========================================================================

    private async Task<ApiResponse<T>> SendRequestAsync<T>(
        string method,
        string path,
        Func<Task<HttpResponseMessage>> httpCall,
        bool isIdempotent,
        CancellationToken ct)
    {
        int maxAttempts = isIdempotent ? MaxRetries + 1 : 1;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var response = await httpCall();

                // Retry otomatis hanya jika method idempotent dan error bersifat sementara (502, 503, 504, 408)
                if (attempt < maxAttempts && IsTransientStatus(response.StatusCode))
                {
                    AppLogger.Warn(
                        "ApiService",
                        $"Transient HTTP {(int)response.StatusCode} pada {method} {path} (percobaan {attempt}/{maxAttempts}), mencoba ulang...");

                    await Task.Delay(TimeSpan.FromMilliseconds(BaseRetryDelay.TotalMilliseconds * attempt), ct);
                    continue;
                }

                return await BuildResponseAsync<T>(method, path, response, ct);
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransientException(ex, ct))
            {
                AppLogger.Warn(
                    "ApiService",
                    $"Transient error pada {method} {path} (percobaan {attempt}/{maxAttempts}): {ex.Message}");

                await Task.Delay(TimeSpan.FromMilliseconds(BaseRetryDelay.TotalMilliseconds * attempt), ct);
            }
            catch (Exception ex)
            {
                return HandleException<T>(method, path, ex, ct);
            }
        }

        const string exhaustedMsg = "Gagal memproses permintaan setelah beberapa kali percobaan.";
        AppLogger.Error("ApiService", $"{method} {path}: {exhaustedMsg}");

        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = 0,
            ErrorMessage = exhaustedMsg
        };
    }

    private async Task<ApiResponse<T>> BuildResponseAsync<T>(
        string method,
        string path,
        HttpResponseMessage response,
        CancellationToken ct)
    {
        int statusCode = (int)response.StatusCode;

        if (response.IsSuccessStatusCode)
        {
            if (typeof(T) == typeof(bool))
            {
                return new ApiResponse<T>
                {
                    IsSuccess = true,
                    StatusCode = statusCode,
                    Data = (T)(object)true
                };
            }

            if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
            {
                return new ApiResponse<T> { IsSuccess = true, StatusCode = statusCode };
            }

            var rawSuccess = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(rawSuccess))
            {
                return new ApiResponse<T> { IsSuccess = true, StatusCode = statusCode };
            }

            var data = JsonSerializer.Deserialize<T>(rawSuccess, _jsonOptions);
            return new ApiResponse<T>
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Data = data
            };
        }

        string errorBody = await response.Content.ReadAsStringAsync(ct);
        string trimmed = errorBody.TrimStart();

        var friendlyMessage = ExtractFriendlyErrorMessage(statusCode, response.ReasonPhrase, trimmed);
        var bodySnippet = trimmed.Length > 500 ? $"{trimmed[..500]}..." : trimmed;

        AppLogger.Error(
            "ApiService",
            $"HTTP {statusCode} ({response.ReasonPhrase}) pada {method} {path} -> {friendlyMessage}" +
            (string.IsNullOrWhiteSpace(bodySnippet) ? string.Empty : $"\nBody: {bodySnippet}"));

        T? errorData = default;
        if (trimmed.StartsWith('{'))
        {
            try
            {
                errorData = JsonSerializer.Deserialize<T>(errorBody, _jsonOptions);
            }
            catch (JsonException) { }
        }

        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Data = errorData,
            ErrorMessage = friendlyMessage
        };
    }

    private static string ExtractFriendlyErrorMessage(int statusCode, string? reasonPhrase, string trimmedBody)
    {
        if (string.IsNullOrWhiteSpace(trimmedBody) || trimmedBody.StartsWith('<'))
        {
            return statusCode switch
            {
                401 => "Sesi berakhir atau tidak terautentikasi. Silakan login kembali.",
                403 => "Anda tidak memiliki izin untuk mengakses fitur ini.",
                404 => "Data atau endpoint tidak ditemukan di server.",
                502 or 503 or 504 => "Server sedang bangun (cold start) atau sibuk. Silakan coba beberapa saat lagi.",
                _ => $"HTTP {statusCode}: {reasonPhrase ?? "Terjadi kesalahan pada server"}"
            };
        }

        // Ekstrak field pesan umum dari backend JSON (message, error, detail, title)
        if (trimmedBody.StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(trimmedBody);
                var root = doc.RootElement;

                foreach (var prop in new[] { "message", "error", "detail", "title" })
                {
                    if (root.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
                    {
                        var msg = val.GetString();
                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            return msg;
                        }
                    }
                }
            }
            catch (JsonException) { }
        }

        return trimmedBody;
    }

    private static ApiResponse<T> HandleException<T>(string method, string path, Exception ex, CancellationToken ct)
    {
        if (ex is OperationCanceledException && ct.IsCancellationRequested)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = 0,
                ErrorMessage = "Permintaan dibatalkan."
            };
        }

        string message = ex switch
        {
            TaskCanceledException => "Koneksi timeout. Server Render sedang bangun (cold start), silakan coba lagi.",
            HttpRequestException httpEx => $"Gagal terhubung ke server: {httpEx.Message}",
            JsonException => $"Format respons JSON dari server tidak sesuai untuk tipe {typeof(T).Name}.",
            _ => $"Kesalahan tidak terduga: {ex.Message}"
        };

        AppLogger.Error("ApiService", $"Exception pada {method} {path} -> {message}", ex);

        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = 0,
            ErrorMessage = message
        };
    }

    private static string NormalizeEndpoint(string endpoint) => endpoint.TrimStart('/');

    private static bool IsTransientStatus(HttpStatusCode code) =>
        code is HttpStatusCode.RequestTimeout
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static bool IsTransientException(Exception ex, CancellationToken ct) =>
        ex is HttpRequestException || (ex is TaskCanceledException && !ct.IsCancellationRequested);
}
