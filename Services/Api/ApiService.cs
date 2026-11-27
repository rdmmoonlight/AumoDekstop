using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Shared.Http;
using AumoDekstop.Services;

namespace AumoDekstop.Services.Api;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ApiExecutor _executor;

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

        _executor = new ApiExecutor(new ApiResponseParser(_jsonOptions));
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
        var normalized = ApiEndpointHelper.Normalize(healthEndpoint);
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

    public Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<T>("GET", path, () => _httpClient.GetAsync(path, ct), isIdempotent: true, ct);
    }

    public Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<TResponse>("POST", path, () => _httpClient.PostAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<bool>> PostAsync<TRequest>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<bool>("POST", path, () => _httpClient.PostAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<TResponse>> PostEmptyAsync<TResponse>(string endpoint, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<TResponse>("POST", path, () => _httpClient.PostAsync(path, content: null, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<bool>> PostEmptyAsync(string endpoint, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<bool>("POST", path, () => _httpClient.PostAsync(path, content: null, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<TResponse>> PostMultipartAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<TResponse>("POST", path, () => _httpClient.PostAsync(path, content, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<TResponse>("PUT", path, () => _httpClient.PutAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: true, ct);
    }

    public Task<ApiResponse<bool>> PutAsync<TRequest>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<bool>("PUT", path, () => _httpClient.PutAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: true, ct);
    }

    public Task<ApiResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<TResponse>("PATCH", path, () => _httpClient.PatchAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<bool>> PatchAsync<TRequest>(string endpoint, TRequest payload, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<bool>("PATCH", path, () => _httpClient.PatchAsJsonAsync(path, payload, _jsonOptions, ct), isIdempotent: false, ct);
    }

    public Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<bool>("DELETE", path, () => _httpClient.DeleteAsync(path, ct), isIdempotent: true, ct);
    }

    public Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string endpoint, CancellationToken ct = default)
    {
        var path = ApiEndpointHelper.Normalize(endpoint);
        return _executor.SendAsync<TResponse>("DELETE", path, () => _httpClient.DeleteAsync(path, ct), isIdempotent: true, ct);
    }
}