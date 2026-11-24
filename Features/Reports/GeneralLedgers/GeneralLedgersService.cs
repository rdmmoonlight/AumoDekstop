
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AumoDekstop.Features.Reports.GeneralLedgers;
#region DTO / Response Models

public class GetGeneralLedgerPermanentResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("hasPeriodSelected")]
    public bool HasPeriodSelected { get; set; }

    [JsonPropertyName("selectedPeriodName")]
    public string? SelectedPeriodName { get; set; }

    [JsonPropertyName("isTemporary")]
    public bool IsTemporary { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("accounts")]
    public List<JsonElement>? Accounts { get; set; }
}

public class GetGeneralLedgerTemporaryResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("hasPeriodSelected")]
    public bool HasPeriodSelected { get; set; }

    [JsonPropertyName("selectedPeriodName")]
    public string? SelectedPeriodName { get; set; }

    [JsonPropertyName("isTemporary")]
    public bool IsTemporary { get; set; }

    [JsonPropertyName("netIncomeBeforeClosing")]
    public decimal? NetIncomeBeforeClosing { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("accounts")]
    public List<JsonElement>? Accounts { get; set; }
}

public class RefreshGeneralLedgerResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Menampung properti tambahan dinamis ([key: string]: unknown)
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

#endregion

#region API Service

public interface IGeneralLedgersService
{
    Task<GetGeneralLedgerPermanentResponse> GetGeneralLedgerPermanentAsync(CancellationToken cancellationToken = default);
    Task<GetGeneralLedgerTemporaryResponse> GetGeneralLedgerTemporaryAsync(CancellationToken cancellationToken = default);
    Task<RefreshGeneralLedgerResponse> RefreshGeneralLedgerAsync(CancellationToken cancellationToken = default);
}

public class GeneralLedgersService : IGeneralLedgersService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GeneralLedgersService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetGeneralLedgerPermanentResponse> GetGeneralLedgerPermanentAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "/api/v1/reports/general-ledgers/permanent",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<GetGeneralLedgerPermanentResponse>(
            JsonOptions,
            cancellationToken);

        return data ?? throw new InvalidOperationException("Respons permanent general ledger kosong.");
    }

    public async Task<GetGeneralLedgerTemporaryResponse> GetGeneralLedgerTemporaryAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "/api/v1/reports/general-ledgers/temporary",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<GetGeneralLedgerTemporaryResponse>(
            JsonOptions,
            cancellationToken);

        return data ?? throw new InvalidOperationException("Respons temporary general ledger kosong.");
    }

    public async Task<RefreshGeneralLedgerResponse> RefreshGeneralLedgerAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            "/api/v1/reports/general-ledgers/refresh",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<RefreshGeneralLedgerResponse>(
            JsonOptions,
            cancellationToken);

        return data ?? throw new InvalidOperationException("Respons refresh general ledger kosong.");
    }
}

#endregion