using System;
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

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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

    private async Task<ApiResponse<T>> SendRequestAsync<T>(
        Func<Task<HttpResponseMessage>> httpCall,
        CancellationToken ct)
    {
        try
        {
            using var response = await httpCall();
            return await BuildResponseAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return HandleException<T>(ex);
        }
    }

    private async Task<ApiResponse<T>> BuildResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        int statusCode = (int)response.StatusCode;

        if (response.IsSuccessStatusCode)
        {
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
        string trimmed = errorBody.TrimStart();

        T? errorData = default;
        if (trimmed.StartsWith('{'))
        {
            try
            {
                errorData = JsonSerializer.Deserialize<T>(errorBody, _jsonOptions);
            }
            catch (JsonException)
            {
            }
        }

        bool isHtml = trimmed.StartsWith('<');

        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Data = errorData,
            ErrorMessage = string.IsNullOrWhiteSpace(errorBody) || isHtml
                ? $"HTTP {statusCode}: {response.ReasonPhrase}"
                : errorBody
        };
    }

    private static ApiResponse<T> HandleException<T>(Exception ex)
    {
        string message = ex switch
        {
            TaskCanceledException => "Koneksi timeout. Server Render sedang bangun (cold start), silakan coba lagi.",
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