namespace AumoDekstop.Services.Api;

internal static class ApiEndpointHelper
{
    public static string Normalize(string endpoint) => endpoint.TrimStart('/');
}