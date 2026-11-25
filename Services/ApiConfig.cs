namespace AumoDekstop.Services;

using System;

/// <summary>
/// Konfigurasi endpoint API Aumo dengan urutan prioritas:
/// 1. Environment Variable: AUMO_API_URL
/// 2. AppLocalSettings: ApiBaseUrl
/// 3. Default: https://aumo.onrender.com
/// </summary>
public static class ApiConfig
{
    public const string EnvVarName = "AUMO_API_URL";
    public const string LocalSettingsKey = "ApiBaseUrl";
    public const string DefaultBaseUrl = "https://aumo.onrender.com";

    public static string BaseUrl
    {
        get
        {
            var envUrl = Environment.GetEnvironmentVariable(EnvVarName);
            if (IsValidHttpUrl(envUrl, out var normalizedEnv))
                return normalizedEnv;

            var localUrl = AppLocalSettings.GetString(LocalSettingsKey);
            if (IsValidHttpUrl(localUrl, out var normalizedLocal))
                return normalizedLocal;

            return DefaultBaseUrl;
        }
    }

    private static bool IsValidHttpUrl(string? rawUrl, out string normalizedUrl)
    {
        normalizedUrl = string.Empty;
        if (string.IsNullOrWhiteSpace(rawUrl))
            return false;

        var trimmed = rawUrl.Trim().TrimEnd('/');
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            normalizedUrl = trimmed;
            return true;
        }

        return false;
    }
}