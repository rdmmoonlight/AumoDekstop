using System.Text.Json;
using AumoDekstop.Features.Auth.Models;
using AumoDekstop.Services;

namespace AumoDekstop.Features.Auth.Helpers;

public static class AuthHelpers
{
    public static string GoogleClientId =>
        Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? string.Empty;

    public static string GetAuthErrorMessage(ApiResponse<AuthResponse> response, string fallback = "Terjadi kesalahan. Silakan coba lagi.")
    {
        // 1. Jika HTTP 200 OK tetapi payload Success == false
        if (response.Data is not null)
        {
            if (response.Data.Errors is { Count: > 0 })
                return response.Data.Errors[0];

            if (!string.IsNullOrWhiteSpace(response.Data.Message))
                return response.Data.Message;
        }

        // 2. Jika HTTP error (400/401/429/500), ErrorMessage dari ApiService bisa berupa JSON string dari backend
        if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<AuthResponse>(response.ErrorMessage, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (parsed?.Errors is { Count: > 0 })
                    return parsed.Errors[0];

                if (!string.IsNullOrWhiteSpace(parsed?.Message))
                    return parsed.Message;
            }
            catch
            {
                // Bukan JSON (misal pesan timeout Render dari HandleException), kembalikan langsung
            }

            return response.ErrorMessage;
        }

        return fallback;
    }

    private static readonly Dictionary<string, Type> RegisteredRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/"] = typeof(MainPage),
    };

    public static Type ResolvePostLoginPage(string? redirectPath = null)
    {
        if (!string.IsNullOrWhiteSpace(redirectPath) &&
            redirectPath.StartsWith('/') &&
            !redirectPath.StartsWith("//") &&
            RegisteredRoutes.TryGetValue(redirectPath, out var targetPage))
        {
            return targetPage;
        }

        return RegisteredRoutes.TryGetValue("/home", out var homePage)
            ? homePage
            : typeof(MainPage);
    }

    public static bool HasHomeRoute() => RegisteredRoutes.ContainsKey("/home");
}