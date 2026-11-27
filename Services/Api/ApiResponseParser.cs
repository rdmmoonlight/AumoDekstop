using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Services;

namespace AumoDekstop.Services.Api;

internal sealed class ApiResponseParser
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiResponseParser(JsonSerializerOptions jsonOptions)
    {
        ArgumentNullException.ThrowIfNull(jsonOptions);
        _jsonOptions = jsonOptions;
    }

    public async Task<ApiResponse<T>> BuildAsync<T>(
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

        var friendlyMessage = ApiErrorMapper.ExtractFriendlyErrorMessage(statusCode, response.ReasonPhrase, trimmed);
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
}