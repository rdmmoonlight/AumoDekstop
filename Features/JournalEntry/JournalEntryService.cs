namespace AumoDekstop.Features.JournalEntry.Services;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.JournalEntry.Messages;
using AumoDekstop.Features.JournalEntry.Models;
using CommunityToolkit.Mvvm.Messaging;

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
    private readonly HttpClient _httpClient;
    private readonly IMessenger _messenger;

    public JournalEntryService(HttpClient httpClient, IMessenger? messenger = null)
    {
        _httpClient = httpClient;
        _messenger = messenger ?? WeakReferenceMessenger.Default;
    }

    public async Task<JournalEntryDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<JournalEntryDetailDto>(
            $"api/v1/journal-entry/{id}",
            cancellationToken);
    }

    public async Task<JournalEntryDetailDto?> CreateAsync(CreateJournalEntryRequest body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/journal-entry/create",
            body,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JournalEntryDetailDto>(cancellationToken: cancellationToken);

        // Invalidate cache / beritahu halaman Reports & Dashboard
        _messenger.Send(new JournalEntryChangedMessage(result?.Id));
        return result;
    }

    public async Task<JournalEntryDetailDto?> EditAsync(int id, UpdateJournalEntryRequest body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/journal-entry/edit/{id}",
            body,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JournalEntryDetailDto>(cancellationToken: cancellationToken);

        // Invalidate cache / beritahu halaman Reports & Dashboard
        _messenger.Send(new JournalEntryChangedMessage(id));
        return result;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/v1/journal-entry/delete/{id}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        // Invalidate cache / beritahu halaman Reports & Dashboard
        _messenger.Send(new JournalEntryChangedMessage(id));
    }

    public async Task<List<string>> SearchDescriptionsAsync(SearchDescriptionsParams? parameters = null, CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(parameters?.Q)
            ? string.Empty
            : $"?Q={Uri.EscapeDataString(parameters.Q)}";

        var result = await _httpClient.GetFromJsonAsync<List<string>>(
            $"api/v1/journal-entry/search-descriptions{query}",
            cancellationToken);

        return result ?? [];
    }

    public async Task<string> GetNextTransactionNumberAsync(NextTransactionNumberParams? parameters = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(parameters?.JournalType))
            queryParams.Add($"JournalType={Uri.EscapeDataString(parameters.JournalType)}");
        if (!string.IsNullOrWhiteSpace(parameters?.EntryDate))
            queryParams.Add($"EntryDate={Uri.EscapeDataString(parameters.EntryDate)}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        var result = await _httpClient.GetFromJsonAsync<string>(
            $"api/v1/journal-entry/next-transaction-number{queryString}",
            cancellationToken);

        return result ?? string.Empty;
    }
}