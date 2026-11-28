using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Periods.Models;
using AumoDekstop.Services.Api;

namespace AumoDekstop.Features.Periods.Services;

public interface IPeriodsService
{
    event EventHandler? PeriodContextChanged;

    Task<GetPeriodsResponse> GetPeriodsAsync(CancellationToken cancellationToken = default);
    Task<PeriodsOpenInfo> GetPeriodsOpenInfoAsync(CancellationToken cancellationToken = default);
    Task<CreatePeriodResult> CreatePeriodAsync(CreatePeriodRequest createPeriodRequest, CancellationToken cancellationToken = default);
    Task SelectPeriodAsync(int id, CancellationToken cancellationToken = default);
    Task ClearPeriodSelectionAsync(CancellationToken cancellationToken = default);
    Task ClosePeriodAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class PeriodsService : IPeriodsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiService _apiService;

    public event EventHandler? PeriodContextChanged;

    public PeriodsService(ApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    }

    private void NotifyPeriodChanged() => PeriodContextChanged?.Invoke(this, EventArgs.Empty);

    // Mendukung 3 bentuk respons API: PeriodItem[], { items: [...] }, atau GetPeriodsResponse
    public async Task<GetPeriodsResponse> GetPeriodsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiService.GetAsync<JsonElement>("api/v1/periods", cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "Gagal memuat daftar periode.");
        }

        var root = response.Data;

        if (root.ValueKind == JsonValueKind.Array)
        {
            var list = root.Deserialize<List<PeriodItem>>(JsonOptions) ?? new List<PeriodItem>();
            return new GetPeriodsResponse { Periods = list };
        }

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("items", out var itemsProp) &&
            itemsProp.ValueKind == JsonValueKind.Array)
        {
            var list = itemsProp.Deserialize<List<PeriodItem>>(JsonOptions) ?? new List<PeriodItem>();
            return new GetPeriodsResponse { Periods = list };
        }

        return root.Deserialize<GetPeriodsResponse>(JsonOptions) ?? new GetPeriodsResponse();
    }

    public async Task<PeriodsOpenInfo> GetPeriodsOpenInfoAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiService.GetAsync<PeriodsOpenInfo>("api/v1/periods/open-info", cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "Gagal memuat informasi pembukaan periode.");
        }

        return response.Data ?? new PeriodsOpenInfo();
    }

    public async Task<CreatePeriodResult> CreatePeriodAsync(CreatePeriodRequest createPeriodRequest, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(createPeriodRequest);

        var response = await _apiService.PostAsync<CreatePeriodRequest, CreatePeriodResult>(
            "api/v1/periods",
            createPeriodRequest,
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(
                response.ErrorMessage ?? response.Data?.Message ?? "Gagal membuat periode baru.");
        }

        NotifyPeriodChanged();
        return response.Data ?? new CreatePeriodResult { Success = true };
    }

    public async Task SelectPeriodAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostEmptyAsync<JsonElement>(
            $"api/v1/periods/select/{id}",
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "Gagal memilih periode.");
        }

        NotifyPeriodChanged();
    }

    public async Task ClearPeriodSelectionAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostEmptyAsync<JsonElement>(
            "api/v1/periods/clear-selection",
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "Gagal menghapus pilihan periode.");
        }

        NotifyPeriodChanged();
    }

    public async Task ClosePeriodAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostEmptyAsync<JsonElement>(
            $"api/v1/periods/close/{id}",
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "Gagal menutup periode.");
        }

        NotifyPeriodChanged();
    }
}