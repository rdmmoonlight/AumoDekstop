using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Services;

namespace AumoDekstop.Services.Api;

internal static class ApiErrorMapper
{
    public static string ExtractFriendlyErrorMessage(int statusCode, string? reasonPhrase, string trimmedBody)
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

    public static ApiResponse<T> FromException<T>(string method, string path, Exception ex, CancellationToken ct)
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
}