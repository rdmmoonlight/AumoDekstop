using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Services.Api;

namespace AumoDekstop.Features.Reports.GeneralLedgers;

public interface IGeneralLedgersService
{
    Task<GetGeneralLedgerPermanentResponse> GetGeneralLedgerPermanentAsync(CancellationToken cancellationToken = default);
    Task<GetGeneralLedgerTemporaryResponse> GetGeneralLedgerTemporaryAsync(CancellationToken cancellationToken = default);
    Task<RefreshGeneralLedgerResponse> RefreshGeneralLedgerAsync(CancellationToken cancellationToken = default);
}

public class GeneralLedgersService : IGeneralLedgersService
{
    private readonly ApiService _apiService;

    public GeneralLedgersService(ApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    }

    public async Task<GetGeneralLedgerPermanentResponse> GetGeneralLedgerPermanentAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _apiService.GetAsync<GetGeneralLedgerPermanentResponse>(
            "api/v1/reports/general-ledgers/permanent",
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? $"Request failed with status {response.StatusCode}.");
        }

        return response.Data ?? throw new InvalidOperationException("Respons permanent general ledger kosong.");
    }

    public async Task<GetGeneralLedgerTemporaryResponse> GetGeneralLedgerTemporaryAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _apiService.GetAsync<GetGeneralLedgerTemporaryResponse>(
            "api/v1/reports/general-ledgers/temporary",
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? $"Request failed with status {response.StatusCode}.");
        }

        return response.Data ?? throw new InvalidOperationException("Respons temporary general ledger kosong.");
    }

    public async Task<RefreshGeneralLedgerResponse> RefreshGeneralLedgerAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostEmptyAsync<RefreshGeneralLedgerResponse>(
            "api/v1/reports/general-ledgers/refresh",
            cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? $"Request failed with status {response.StatusCode}.");
        }

        return response.Data ?? throw new InvalidOperationException("Respons refresh general ledger kosong.");
    }
}
