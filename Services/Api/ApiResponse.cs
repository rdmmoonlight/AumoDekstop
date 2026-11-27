namespace AumoDekstop.Services.Api;

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