using System.Net.Http.Json;
using System.Text.Json;
using AumoDekstop.Features.Periods.Models;

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

public sealed class PeriodsApiService(HttpClient httpClient) : IPeriodsApiService
{
    public event EventHandler? PeriodContextChanged;

    private void NotifyPeriodChanged() => PeriodContextChanged?.Invoke(this, EventArgs.Empty);

    // Mendukung 3 bentuk respons API: PeriodItem[], { items: [...] }, atau GetPeriodsResponse
    public async Task<GetPeriodsResponse> GetPeriodsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("/api/v1/periods", cancellationToken);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            var list = root.Deserialize<List<PeriodItem>>() ?? [];
            return new GetPeriodsResponse { Periods = list };
        }

        if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
        {
            var list = itemsProp.Deserialize<List<PeriodItem>>() ?? [];
            return new GetPeriodsResponse { Periods = list };
        }

        return root.Deserialize<GetPeriodsResponse>() ?? new GetPeriodsResponse();
    }

    public async Task<PeriodsOpenInfo> GetPeriodsOpenInfoAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<PeriodsOpenInfo>(
            "/api/v1/periods/open-info",
            cancellationToken);

        return result ?? new PeriodsOpenInfo();
    }

    // Catatan: Tipe parameter 'createPeriodRequest' akan kita spesifikkan sesuai struktur di use-create-period-form.ts
    public async Task CreatePeriodAsync(object createPeriodRequest, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/v1/periods",
            createPeriodRequest,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        NotifyPeriodChanged();
    }

    public async Task SelectPeriodAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync(
            $"/api/v1/periods/{id}/select",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        NotifyPeriodChanged();
    }

    public async Task ClearPeriodSelectionAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync(
            "/api/v1/periods/clear-selection",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        NotifyPeriodChanged();
    }

    public async Task ClosePeriodAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync(
            $"/api/v1/periods/{id}/close",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        NotifyPeriodChanged();
    }
}
