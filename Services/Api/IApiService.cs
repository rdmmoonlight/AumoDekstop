using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AumoDekstop.Services.Api;

public interface IApiService
{
    void SetAuthToken(string? token);

    Task<bool> CheckServerHealthAsync(string healthEndpoint = "health", CancellationToken ct = default);

    Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken ct = default);

    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default);

    Task<ApiResponse<bool>> PostAsync<TRequest>(string endpoint, TRequest payload, CancellationToken ct = default);

    Task<ApiResponse<TResponse>> PostEmptyAsync<TResponse>(string endpoint, CancellationToken ct = default);

    Task<ApiResponse<bool>> PostEmptyAsync(string endpoint, CancellationToken ct = default);

    Task<ApiResponse<TResponse>> PostMultipartAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken ct = default);

    Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default);

    Task<ApiResponse<bool>> PutAsync<TRequest>(string endpoint, TRequest payload, CancellationToken ct = default);

    Task<ApiResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken ct = default);

    Task<ApiResponse<bool>> PatchAsync<TRequest>(string endpoint, TRequest payload, CancellationToken ct = default);

    Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken ct = default);

    Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string endpoint, CancellationToken ct = default);
}