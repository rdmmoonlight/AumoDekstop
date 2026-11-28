using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Services.Api;

namespace AumoDekstop.Features.Reports.Journals.Services;

public interface IJournalsReportService
{
    Task<ApiResponse<T>> GetJournalsGeneralAsync<T>(CancellationToken cancellationToken = default);
    Task<ApiResponse<JsonElement>> GetJournalsGeneralAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<T>> GetJournalsAdjustingAsync<T>(CancellationToken cancellationToken = default);
    Task<ApiResponse<JsonElement>> GetJournalsAdjustingAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteJournalsAdjustingByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResponse<T>> GetJournalsClosingAsync<T>(CancellationToken cancellationToken = default);
    Task<ApiResponse<JsonElement>> GetJournalsClosingAsync(CancellationToken cancellationToken = default);
}

public class JournalsReportService : IJournalsReportService
{
    private readonly ApiService _apiService;

    public JournalsReportService(ApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    }

    public Task<ApiResponse<T>> GetJournalsGeneralAsync<T>(CancellationToken cancellationToken = default)
        => _apiService.GetAsync<T>("api/v1/reports/journals/general", cancellationToken);

    public Task<ApiResponse<JsonElement>> GetJournalsGeneralAsync(CancellationToken cancellationToken = default)
        => GetJournalsGeneralAsync<JsonElement>(cancellationToken);

    public Task<ApiResponse<T>> GetJournalsAdjustingAsync<T>(CancellationToken cancellationToken = default)
        => _apiService.GetAsync<T>("api/v1/reports/journals/adjusting", cancellationToken);

    public Task<ApiResponse<JsonElement>> GetJournalsAdjustingAsync(CancellationToken cancellationToken = default)
        => GetJournalsAdjustingAsync<JsonElement>(cancellationToken);

    public Task<ApiResponse<bool>> DeleteJournalsAdjustingByIdAsync(int id, CancellationToken cancellationToken = default)
        => _apiService.DeleteAsync($"api/v1/reports/journals/adjusting/{id}", cancellationToken);

    public Task<ApiResponse<T>> GetJournalsClosingAsync<T>(CancellationToken cancellationToken = default)
        => _apiService.GetAsync<T>("api/v1/reports/journals/closing", cancellationToken);

    public Task<ApiResponse<JsonElement>> GetJournalsClosingAsync(CancellationToken cancellationToken = default)
        => GetJournalsClosingAsync<JsonElement>(cancellationToken);
}