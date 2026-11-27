using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.JournalEntry.Messages;
using AumoDekstop.Features.JournalEntry.Models;
using AumoDekstop.Services;
using AumoDekstop.Services.Api;
using CommunityToolkit.Mvvm.Messaging;

namespace AumoDekstop.Features.JournalEntry.Services;

public interface IJournalEntryService
{
    Task<JournalEntryDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JournalEntryDetailDto?> CreateAsync(CreateJournalEntryRequest body, CancellationToken cancellationToken = default);
    Task<JournalEntryDetailDto?> EditAsync(int id, UpdateJournalEntryRequest body, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<List<string>> SearchDescriptionsAsync(SearchDescriptionsParams? parameters = null, CancellationToken cancellationToken = default);
    Task<string> GetNextTransactionNumberAsync(NextTransactionNumberParams? parameters = null, CancellationToken cancellationToken = default);
}

public class JournalEntryService : IJournalEntryService
{
    private readonly IApiService _apiService;
    private readonly IMessenger _messenger;

    public JournalEntryService(IApiService apiService, IMessenger? messenger = null)
    {
        ArgumentNullException.ThrowIfNull(apiService);
        _apiService = apiService;
        _messenger = messenger ?? WeakReferenceMessenger.Default;
    }

    public async Task<JournalEntryDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _apiService.GetAsync<JournalEntryDetailDto>(
            $"api/v1/journal-entry/{id}",
            cancellationToken);

        EnsureSuccess(response);
        return response.Data;
    }

    public async Task<JournalEntryDetailDto?> CreateAsync(CreateJournalEntryRequest body, CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PostAsync<CreateJournalEntryRequest, JournalEntryDetailDto>(
            "api/v1/journal-entry/create",
            body,
            cancellationToken);

        EnsureSuccess(response);

        // Invalidate cache / beritahu halaman Reports & Dashboard
        _messenger.Send(new JournalEntryChangedMessage(response.Data?.Id));
        return response.Data;
    }

    public async Task<JournalEntryDetailDto?> EditAsync(int id, UpdateJournalEntryRequest body, CancellationToken cancellationToken = default)
    {
        var response = await _apiService.PutAsync<UpdateJournalEntryRequest, JournalEntryDetailDto>(
            $"api/v1/journal-entry/edit/{id}",
            body,
            cancellationToken);

        EnsureSuccess(response);

        // Invalidate cache / beritahu halaman Reports & Dashboard
        _messenger.Send(new JournalEntryChangedMessage(id));
        return response.Data;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _apiService.DeleteAsync(
            $"api/v1/journal-entry/delete/{id}",
            cancellationToken);

        EnsureSuccess(response);

        // Invalidate cache / beritahu halaman Reports & Dashboard
        _messenger.Send(new JournalEntryChangedMessage(id));
    }

    public async Task<List<string>> SearchDescriptionsAsync(SearchDescriptionsParams? parameters = null, CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(parameters?.Q)
            ? string.Empty
            : $"?Q={Uri.EscapeDataString(parameters.Q)}";

        var response = await _apiService.GetAsync<List<string>>(
            $"api/v1/journal-entry/search-descriptions{query}",
            cancellationToken);

        EnsureSuccess(response);
        return response.Data ?? [];
    }

    public async Task<string> GetNextTransactionNumberAsync(NextTransactionNumberParams? parameters = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(parameters?.JournalType))
        {
            queryParams.Add($"JournalType={Uri.EscapeDataString(parameters.JournalType)}");
        }

        if (!string.IsNullOrWhiteSpace(parameters?.EntryDate))
        {
            queryParams.Add($"EntryDate={Uri.EscapeDataString(parameters.EntryDate)}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        var response = await _apiService.GetAsync<string>(
            $"api/v1/journal-entry/next-transaction-number{queryString}",
            cancellationToken);

        EnsureSuccess(response);
        return response.Data ?? string.Empty;
    }

    private static void EnsureSuccess<T>(ApiResponse<T> response)
    {
        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(
                response.ErrorMessage ?? "Terjadi kesalahan saat memproses jurnal.");
        }
    }
}