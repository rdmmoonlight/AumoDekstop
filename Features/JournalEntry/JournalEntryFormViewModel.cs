namespace AumoDekstop.Features.JournalEntry.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.JournalEntry.Models;
using AumoDekstop.Features.JournalEntry.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class JournalEntryFormViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly IJournalEntryService _journalEntryService;

    public JournalEntryFormViewModel(
        HttpClient httpClient,
        IJournalEntryService journalEntryService,
        IEnumerable<JournalLineRowItem>? initialLines = null)
    {
        _httpClient = httpClient;
        _journalEntryService = journalEntryService;

        Lines.CollectionChanged += Lines_CollectionChanged;
        InitializeLines(initialLines);
    }

    #region 1. Available Accounts (dari useAvailableAccounts.ts)

    public ObservableCollection<AccountOption> Accounts { get; } = [];

    [ObservableProperty]
    private bool _isLoadingAccounts;

    [ObservableProperty]
    private string? _accountsError;

    [RelayCommand]
    public async Task FetchAccountsAsync(CancellationToken cancellationToken = default)
    {
        IsLoadingAccounts = true;
        AccountsError = null;
        try
        {
            var data = await _httpClient.GetFromJsonAsync<List<AccountOption>>(
                "api/chart-of-accounts",
                cancellationToken);

            Accounts.Clear();
            if (data is not null)
            {
                foreach (var acc in data)
                {
                    Accounts.Add(new AccountOption(acc.Id, acc.Code, acc.Name));
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Dibatalkan
        }
        catch (Exception ex)
        {
            AccountsError = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Gagal memuat akun";
        }
        finally
        {
            IsLoadingAccounts = false;
        }
    }

    #endregion

    #region 2. Journal Entry Detail (dari useJournalEntryDetail.ts)

    [ObservableProperty]
    private JournalEntryDetailDto? _entry;

    [ObservableProperty]
    private bool _isLoadingDetail;

    [ObservableProperty]
    private string? _detailError;

    [RelayCommand]
    public async Task FetchDetailAsync(string? entryId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entryId) || !int.TryParse(entryId, out var numericId))
        {
            return;
        }

        IsLoadingDetail = true;
        DetailError = null;
        try
        {
            var result = await _journalEntryService.GetByIdAsync(numericId, cancellationToken);
            Entry = result;

            if (result is not null)
            {
                TransactionDate = result.EntryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                ReferenceNumber = result.TransactionNumber;
                Description = result.Description ?? string.Empty;

                var loadedLines = result.Lines.Select(l => new JournalLineRowItem
                {
                    AccountId = l.AccountId.ToString(CultureInfo.InvariantCulture),
                    Debit = l.Debit,
                    Credit = l.Credit,
                    Description = l.Description
                });

                InitializeLines(loadedLines);
            }
        }
        catch (OperationCanceledException)
        {
            // Dibatalkan
        }
        catch (Exception ex)
        {
            DetailError = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Gagal mengambil detail entri jurnal";
        }
        finally
        {
            IsLoadingDetail = false;
        }
    }

    #endregion

    #region 3. Form State & Calculations (dari useJournalForm.ts)

    [ObservableProperty]
    private string _transactionDate = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    [ObservableProperty]
    private string _referenceNumber = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    public ObservableCollection<JournalLineRowItem> Lines { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBalanced))]
    private decimal _totalDebit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBalanced))]
    private decimal _totalCredit;

    public bool IsBalanced => TotalDebit > 0m && TotalDebit == TotalCredit;

    [RelayCommand]
    public void AddLine()
    {
        Lines.Add(new JournalLineRowItem
        {
            AccountId = string.Empty,
            Debit = 0m,
            Credit = 0m,
            Description = string.Empty
        });
    }

    [RelayCommand]
    public void RemoveLine(int index)
    {
        // Minimal 2 baris sesuai logika useJournalForm.ts
        if (Lines.Count > 2 && index >= 0 && index < Lines.Count)
        {
            Lines.RemoveAt(index);
        }
    }

    public void UpdateLine(int index, string field, object? value)
    {
        if (index < 0 || index >= Lines.Count) return;

        var line = Lines[index];
        switch (field.ToLowerInvariant())
        {
            case "accountid":
                line.AccountId = value?.ToString() ?? string.Empty;
                break;
            case "debit":
                line.Debit = value is null ? 0m : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                break;
            case "credit":
                line.Credit = value is null ? 0m : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                break;
            case "memo":
            case "description":
                line.Description = value?.ToString();
                break;
        }

        RecalculateTotals();
    }

    [RelayCommand]
    public void ResetForm()
    {
        TransactionDate = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        ReferenceNumber = string.Empty;
        Description = string.Empty;
        Entry = null;
        DetailError = null;

        InitializeLines(null);
    }

    /// <summary>
    /// Dipanggil saat halaman/kontrol dimuat (pengganti onMounted di Vue).
    /// </summary>
    public async Task InitializeAsync(string? entryId = null, CancellationToken cancellationToken = default)
    {
        await FetchAccountsAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(entryId))
        {
            await FetchDetailAsync(entryId, cancellationToken);
        }
    }

    #endregion

    #region Private Helpers

    private void InitializeLines(IEnumerable<JournalLineRowItem>? initialLines)
    {
        Lines.Clear();

        var items = initialLines?.ToList();
        if (items is { Count: > 0 })
        {
            foreach (var item in items)
            {
                Lines.Add(item);
            }
        }
        else
        {
            Lines.Add(new JournalLineRowItem { AccountId = string.Empty, Debit = 0m, Credit = 0m, Description = string.Empty });
            Lines.Add(new JournalLineRowItem { AccountId = string.Empty, Debit = 0m, Credit = 0m, Description = string.Empty });
        }

        RecalculateTotals();
    }

    private void Lines_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (JournalLineRowItem item in e.OldItems)
            {
                item.PropertyChanged -= LineItem_PropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (JournalLineRowItem item in e.NewItems)
            {
                item.PropertyChanged += LineItem_PropertyChanged;
            }
        }

        RecalculateTotals();
    }

    private void LineItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(JournalLineRowItem.Debit) or nameof(JournalLineRowItem.Credit))
        {
            RecalculateTotals();
        }
    }

    private void RecalculateTotals()
    {
        TotalDebit = Lines.Sum(l => l.Debit);
        TotalCredit = Lines.Sum(l => l.Credit);
    }

    #endregion
}