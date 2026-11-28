using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Services;
using AumoDekstop.Services.Api;

namespace AumoDekstop.Features.ChartOfAccounts.Services;

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
    private readonly ApiService _apiService;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string BaseEndpoint = "api/v1/chart-of-accounts";

    public ChartOfAccountsService(ApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
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

        var response = await _apiService.GetAsync<JsonElement>(url, cancellationToken);
        EnsureSuccess(response);

        return NormalizeList(response.Data);
    }

    public async Task CreateChartOfAccountAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostAsync(BaseEndpoint, request, cancellationToken);
        EnsureSuccess(response);
    }

    public async Task UpdateChartOfAccountAsync(
        int id,
        UpdateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PutAsync($"{BaseEndpoint}/{id}", request, cancellationToken);
        EnsureSuccess(response);
    }

    public async Task DeleteChartOfAccountAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiService.DeleteAsync($"{BaseEndpoint}/{id}", cancellationToken);
        EnsureSuccess(response);
    }

    /// <summary>
    /// Menormalkan respons JSON agar mendukung bentuk array langsung maupun objek pembungkus
    /// { success, selectedPeriodName, accounts }.
    /// </summary>
    private ChartOfAccountsList NormalizeList(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            var accounts = root.Deserialize<List<Account>>(_jsonOptions) ?? new List<Account>();
            return new ChartOfAccountsList
            {
                Success = true,
                SelectedPeriodName = null,
                Accounts = accounts
            };
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            var list = root.Deserialize<ChartOfAccountsList>(_jsonOptions);
            return list ?? new ChartOfAccountsList { Success = true };
        }

        return new ChartOfAccountsList { Success = true };
    }

    private static void EnsureSuccess<T>(ApiResponse<T> response)
    {
        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "Permintaan ke server gagal.");
        }
    }
}