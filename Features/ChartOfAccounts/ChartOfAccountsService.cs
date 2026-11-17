namespace AumoDekstop.Features.ChartOfAccounts.Services;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AumoDekstop.Features.ChartOfAccounts.Models;

public record GetChartOfAccountsParams(
    string? Search = null,
    string? Category = null
);

public class CreateAccountRequest
{
    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; set; }

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public AccountType Type { get; set; }

    [JsonPropertyName("role")]
    public AccountRole Role { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
}

public class UpdateAccountRequest
{
    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; set; }

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public AccountType Type { get; set; }

    [JsonPropertyName("role")]
    public AccountRole Role { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

public interface IChartOfAccountsService
{
    Task<ChartOfAccountsList> GetChartOfAccountsAsync(GetChartOfAccountsParams? parameters = null, CancellationToken cancellationToken = default);
    Task CreateChartOfAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);
    Task UpdateChartOfAccountAsync(int id, UpdateAccountRequest request, CancellationToken cancellationToken = default);
    Task DeleteChartOfAccountAsync(int id, CancellationToken cancellationToken = default);
}

public class ChartOfAccountsService : IChartOfAccountsService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string BaseEndpoint = "api/v1/chart-of-accounts";

    public ChartOfAccountsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ChartOfAccountsList> GetChartOfAccountsAsync(
        GetChartOfAccountsParams? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(parameters?.Search))
        {
            queryParts.Add($"Search={Uri.EscapeDataString(parameters.Search)}");
        }

        if (!string.IsNullOrWhiteSpace(parameters?.Category))
        {
            queryParts.Add($"Category={Uri.EscapeDataString(parameters.Category)}");
        }

        var url = queryParts.Count > 0
            ? $"{BaseEndpoint}?{string.Join("&", queryParts)}"
            : BaseEndpoint;

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowBackendMessageAsync(response, cancellationToken);

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        return NormalizeList(rawJson);
    }

    public async Task CreateChartOfAccountAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BaseEndpoint, request, _jsonOptions, cancellationToken);
        await EnsureSuccessOrThrowBackendMessageAsync(response, cancellationToken);
    }

    public async Task UpdateChartOfAccountAsync(
        int id,
        UpdateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseEndpoint}/{id}", request, _jsonOptions, cancellationToken);
        await EnsureSuccessOrThrowBackendMessageAsync(response, cancellationToken);
    }

    public async Task DeleteChartOfAccountAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BaseEndpoint}/{id}", cancellationToken);
        await EnsureSuccessOrThrowBackendMessageAsync(response, cancellationToken);
    }

    /// <summary>
    /// Menormalkan respons JSON agar mendukung bentuk array langsung maupun objek pembungkus
    /// { success, selectedPeriodName, accounts }.
    /// </summary>
    private ChartOfAccountsList NormalizeList(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return new ChartOfAccountsList { Success = true };
        }

        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            var accounts = JsonSerializer.Deserialize<List<Account>>(rawJson, _jsonOptions) ?? new List<Account>();
            return new ChartOfAccountsList
            {
                Success = true,
                SelectedPeriodName = null,
                Accounts = accounts
            };
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            var list = JsonSerializer.Deserialize<ChartOfAccountsList>(rawJson, _jsonOptions);
            return list ?? new ChartOfAccountsList { Success = true };
        }

        return new ChartOfAccountsList { Success = true };
    }

    /// <summary>
    /// Mengekstrak pesan error dari backend (misalnya ProblemDetails / validation message) saat status HTTP gagal.
    /// </summary>
    private static async Task EnsureSuccessOrThrowBackendMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(errorContent))
        {
            try
            {
                using var doc = JsonDocument.Parse(errorContent);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    throw new HttpRequestException(msgProp.GetString());
                }
                if (doc.RootElement.TryGetProperty("title", out var titleProp))
                {
                    throw new HttpRequestException(titleProp.GetString());
                }
            }
            catch (JsonException)
            {
                // Jika bukan JSON, lempar teks mentahnya
            }

            throw new HttpRequestException(errorContent);
        }

        response.EnsureSuccessStatusCode();
    }
}