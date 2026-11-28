using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Reports.TrialBalances.Helpers;
using AumoDekstop.Features.Reports.TrialBalances.Models;
using AumoDekstop.Services.Api;

namespace AumoDekstop.Features.Reports.TrialBalances.Services;

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
    private readonly ApiService _apiService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TrialBalancesApiService(ApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    }

    public async Task<TrialBalanceFetchResult> GetTrialBalanceAsync(
        GetTrialBalanceParams? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var typeValue = (parameters?.Type ?? TrialBalanceType.Unadjusted).ToApiQueryValue();
        var requestUri = $"api/v1/reports/trial-balances?type={Uri.EscapeDataString(typeValue)}";

        var response = await _apiService.GetAsync<JsonElement>(requestUri, cancellationToken);

        // Status 404 dianggap belum ada periode yang dipilih (noPeriod)
        if (response.StatusCode == 404)
        {
            return new TrialBalanceFetchResult { HasPeriodSelected = false };
        }

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? $"Request failed with status {response.StatusCode}.");
        }

        var root = response.Data;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return new TrialBalanceFetchResult();
        }

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
}