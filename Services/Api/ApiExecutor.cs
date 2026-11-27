using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Services;

namespace AumoDekstop.Services.Api;

internal sealed class ApiExecutor
{
    private readonly ApiResponseParser _parser;

    public ApiExecutor(ApiResponseParser parser)
    {
        ArgumentNullException.ThrowIfNull(parser);
        _parser = parser;
    }

    public async Task<ApiResponse<T>> SendAsync<T>(
        string method,
        string path,
        Func<Task<HttpResponseMessage>> httpCall,
        bool isIdempotent,
        CancellationToken ct)
    {
        int maxAttempts = ApiRetryPolicy.MaxAttempts(isIdempotent);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var response = await httpCall();

                if (attempt < maxAttempts && ApiRetryPolicy.IsTransientStatus(response.StatusCode))
                {
                    AppLogger.Warn(
                        "ApiService",
                        $"Transient HTTP {(int)response.StatusCode} pada {method} {path} (percobaan {attempt}/{maxAttempts}), mencoba ulang...");

                    await Task.Delay(ApiRetryPolicy.GetDelay(attempt), ct);
                    continue;
                }

                return await _parser.BuildAsync<T>(method, path, response, ct);
            }
            catch (Exception ex) when (attempt < maxAttempts && ApiRetryPolicy.IsTransientException(ex, ct))
            {
                AppLogger.Warn(
                    "ApiService",
                    $"Transient error pada {method} {path} (percobaan {attempt}/{maxAttempts}): {ex.Message}");

                await Task.Delay(ApiRetryPolicy.GetDelay(attempt), ct);
            }
            catch (Exception ex)
            {
                return ApiErrorMapper.FromException<T>(method, path, ex, ct);
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
}