
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AumoDekstop.Features.Reports.Journals.Services;
public interface IJournalsReportService
{
    Task<T?> GetJournalsGeneralAsync<T>(CancellationToken cancellationToken = default);
    Task<JsonElement?> GetJournalsGeneralAsync(CancellationToken cancellationToken = default);

    Task<T?> GetJournalsAdjustingAsync<T>(CancellationToken cancellationToken = default);
    Task<JsonElement?> GetJournalsAdjustingAsync(CancellationToken cancellationToken = default);

    Task DeleteJournalsAdjustingByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<T?> GetJournalsClosingAsync<T>(CancellationToken cancellationToken = default);
    Task<JsonElement?> GetJournalsClosingAsync(CancellationToken cancellationToken = default);
}

public class JournalsReportService : IJournalsReportService
{
    private readonly HttpClient _httpClient;

    public JournalsReportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> GetJournalsGeneralAsync<T>(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<T>(
            "api/v1/reports/journals/general",
            cancellationToken);
    }

    public Task<JsonElement?> GetJournalsGeneralAsync(CancellationToken cancellationToken = default)
        => GetJournalsGeneralAsync<JsonElement?>(cancellationToken);

    public async Task<T?> GetJournalsAdjustingAsync<T>(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<T>(
            "api/v1/reports/journals/adjusting",
            cancellationToken);
    }

    public Task<JsonElement?> GetJournalsAdjustingAsync(CancellationToken cancellationToken = default)
        => GetJournalsAdjustingAsync<JsonElement?>(cancellationToken);

    public async Task DeleteJournalsAdjustingByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/v1/reports/journals/adjusting/{id}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<T?> GetJournalsClosingAsync<T>(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<T>(
            "api/v1/reports/journals/closing",
            cancellationToken);
    }

    public Task<JsonElement?> GetJournalsClosingAsync(CancellationToken cancellationToken = default)
        => GetJournalsClosingAsync<JsonElement?>(cancellationToken);
}