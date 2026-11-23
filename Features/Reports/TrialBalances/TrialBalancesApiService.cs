namespace AumoDekstop.Features.Reports.TrialBalances.Services;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Reports.TrialBalances.Helpers;
using AumoDekstop.Features.Reports.TrialBalances.Models;

public sealed class TrialBalanceFetchResult
{
    public bool HasPeriodSelected { get; set; } = true;

    public bool IsPeriodClosed { get; set; } = true;

    public string? SelectedPeriodName { get; set; }

    public List<TrialRow> Rows { get; set; } = [];
}

public interface ITrialBalancesApiService
{
    Task<TrialBalanceFetchResult> GetTrialBalanceAsync(
        GetTrialBalanceParams? parameters = null,
        CancellationToken cancellationToken = default);
}

public sealed class TrialBalancesApiService : ITrialBalancesApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TrialBalancesApiService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<TrialBalanceFetchResult> GetTrialBalanceAsync(
        GetTrialBalanceParams? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var typeValue = (parameters?.Type ?? TrialBalanceType.Unadjusted).ToApiQueryValue();
        var requestUri = $"api/v1/reports/trial-balances?type={Uri.EscapeDataString(typeValue)}";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        // Status 404 dianggap belum ada periode yang dipilih (noPeriod)
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new TrialBalanceFetchResult { HasPeriodSelected = false };
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var serverMessage = ExtractErrorMessage(content);
            throw new HttpRequestException(serverMessage ?? $"Request failed with status {(int)response.StatusCode}.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return new TrialBalanceFetchResult();
        }

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Object)
        {
            var hasPeriodSelected = !(root.TryGetProperty("hasPeriodSelected", out var hasPeriodProp) &&
                                      hasPeriodProp.ValueKind == JsonValueKind.False);

            var isPeriodClosed = !(root.TryGetProperty("isPeriodClosed", out var isClosedProp) &&
                                   isClosedProp.ValueKind == JsonValueKind.False);

            string? selectedPeriodName = null;
            if (root.TryGetProperty("selectedPeriodName", out var periodNameProp) &&
                periodNameProp.ValueKind == JsonValueKind.String)
            {
                selectedPeriodName = periodNameProp.GetString();
            }

            if (!hasPeriodSelected || !isPeriodClosed)
            {
                return new TrialBalanceFetchResult
                {
                    HasPeriodSelected = hasPeriodSelected,
                    IsPeriodClosed = isPeriodClosed,
                    SelectedPeriodName = selectedPeriodName
                };
            }

            List<RawTrialRow>? rawRows = null;
            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
            {
                rawRows = JsonSerializer.Deserialize<List<RawTrialRow>>(dataProp.GetRawText(), JsonOptions);
            }
            else if (root.TryGetProperty("rows", out var rowsProp) && rowsProp.ValueKind == JsonValueKind.Array)
            {
                rawRows = JsonSerializer.Deserialize<List<RawTrialRow>>(rowsProp.GetRawText(), JsonOptions);
            }

            return new TrialBalanceFetchResult
            {
                HasPeriodSelected = true,
                IsPeriodClosed = true,
                SelectedPeriodName = selectedPeriodName,
                Rows = TrialBalanceNormalizer.NormalizeTrialRows(rawRows)
            };
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("hasPeriodSelected", out var hasPeriodProp) &&
                hasPeriodProp.ValueKind == JsonValueKind.False)
            {
                return new TrialBalanceFetchResult { HasPeriodSelected = false };
            }

            List<RawTrialRow>? rawRows = null;
            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
            {
                rawRows = JsonSerializer.Deserialize<List<RawTrialRow>>(dataProp.GetRawText(), JsonOptions);
            }
            else if (root.TryGetProperty("rows", out var rowsProp) && rowsProp.ValueKind == JsonValueKind.Array)
            {
                rawRows = JsonSerializer.Deserialize<List<RawTrialRow>>(rowsProp.GetRawText(), JsonOptions);
            }

            return new TrialBalanceFetchResult
            {
                HasPeriodSelected = true,
                Rows = TrialBalanceNormalizer.NormalizeTrialRows(rawRows)
            };
        }

        return new TrialBalanceFetchResult();
    }

    private static string? ExtractErrorMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                    return msg.GetString();

                if (root.TryGetProperty("data", out var data) &&
                    data.ValueKind == JsonValueKind.Object &&
                    data.TryGetProperty("message", out var nestedMsg) &&
                    nestedMsg.ValueKind == JsonValueKind.String)
                    return nestedMsg.GetString();
            }
        }
        catch
        {
            // Abaikan jika bukan JSON valid
        }

        return null;
    }
}