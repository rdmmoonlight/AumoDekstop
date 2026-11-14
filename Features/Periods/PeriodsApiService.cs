using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Periods.Models;
using AumoDekstop.Services;

namespace AumoDekstop.Features.Periods.Services;

public interface IPeriodsApiService
{
    event EventHandler? PeriodContextChanged;

    Task<GetPeriodsResponse> GetPeriodsAsync(CancellationToken cancellationToken = default);
    Task<PeriodsOpenInfo> GetPeriodsOpenInfoAsync(CancellationToken cancellationToken = default);
    Task CreatePeriodAsync(object createPeriodRequest, CancellationToken cancellationToken = default);
    Task SelectPeriodAsync(int id, CancellationToken cancellationToken = default);
    Task ClearPeriodSelectionAsync(CancellationToken cancellationToken = default);
    Task ClosePeriodAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class PeriodsApiService : IPeriodsApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiService _apiService;

    public event EventHandler? PeriodContextChanged;

    public PeriodsApiService(ApiService? apiService = null)
    {
        _apiService = apiService ?? ApiService.Instance;
    }

    private void NotifyPeriodChanged() => PeriodContextChanged?.Invoke(this, EventArgs.Empty);

    // Mendukung 3 bentuk respons API: PeriodItem[], { items: [...] }, atau GetPeriodsResponse
    public async Task<GetPeriodsResponse> GetPeriodsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiService.GetAsync<JsonElement>("api/v1/periods", cancellationToken);

        if (!response.IsSuccess)
        {
            throw new HttpRequestException(
                response.ErrorMessage ?? "Gagal memuat daftar periode.",
                null,
                (HttpStatusCode)response.StatusCode);
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
            throw new HttpRequestException(
                response.ErrorMessage ?? "Gagal memuat informasi pembukaan periode.",
                null,
                (HttpStatusCode)response.StatusCode);
        }

        return response.Data ?? new PeriodsOpenInfo();
    }

    public async Task CreatePeriodAsync(object createPeriodRequest, CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostAsync<object, JsonElement>(
            "api/v1/periods",
            createPeriodRequest,
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new HttpRequestException(
                response.ErrorMessage ?? "Gagal membuat periode baru.",
                null,
                (HttpStatusCode)response.StatusCode);
        }

        NotifyPeriodChanged();
    }

    public async Task SelectPeriodAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostAsync<object?, JsonElement>(
            $"api/v1/periods/{id}/select",
            null,
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new HttpRequestException(
                response.ErrorMessage ?? "Gagal memilih periode.",
                null,
                (HttpStatusCode)response.StatusCode);
        }

        NotifyPeriodChanged();
    }

    public async Task ClearPeriodSelectionAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostAsync<object?, JsonElement>(
            "api/v1/periods/clear-selection",
            null,
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new HttpRequestException(
                response.ErrorMessage ?? "Gagal menghapus pilihan periode.",
                null,
                (HttpStatusCode)response.StatusCode);
        }

        NotifyPeriodChanged();
    }

    public async Task ClosePeriodAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostAsync<object?, JsonElement>(
            $"api/v1/periods/{id}/close",
            null,
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new HttpRequestException(
                response.ErrorMessage ?? "Gagal menutup periode.",
                null,
                (HttpStatusCode)response.StatusCode);
        }

        NotifyPeriodChanged();
    }
}
