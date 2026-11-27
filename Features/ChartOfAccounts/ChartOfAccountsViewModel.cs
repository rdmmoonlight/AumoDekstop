using System.Collections.ObjectModel;
using AumoDekstop.Features.ChartOfAccounts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AumoDekstop.Features.ChartOfAccounts;

public sealed partial class ChartOfAccountsViewModel : ObservableObject
{
    private readonly IChartOfAccountsService _chartOfAccountsService;
    private CancellationTokenSource? _loadCts;

    private const string BaseSubtitle = "Kelola daftar akun buku besar untuk pencatatan transaksi keuangan.";

    public ObservableCollection<Account> Accounts { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRefresh))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private bool _isSubmitting;

    [ObservableProperty]
    private Account? _selectedAccount;

    [ObservableProperty]
    private string _subtitleText = BaseSubtitle;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isErrorOpen;

    public bool CanRefresh => !IsLoading;
    public bool CanSubmit => !IsSubmitting;

    public ChartOfAccountsViewModel(IChartOfAccountsService chartOfAccountsService)
    {
        _chartOfAccountsService = chartOfAccountsService;
    }

    [RelayCommand]
    public async Task LoadAccountsAsync()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        IsLoading = true;
        ClearError();

        try
        {
            var result = await _chartOfAccountsService.GetChartOfAccountsAsync(cancellationToken: token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            Accounts.Clear();
            foreach (var account in result.Accounts)
            {
                Accounts.Add(account);
            }

            UpdateSubtitle(result.SelectedPeriodName);
        }
        catch (OperationCanceledException)
        {
            // Permintaan dibatalkan oleh pemuatan ulang berikutnya
        }
        catch (Exception ex)
        {
            ShowError(ExtractMessage(ex, "Gagal memuat data bagan akun"));
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsLoading = false;
            }
        }
    }

    public async Task CreateAccountAsync(AccountFormValues values)
    {
        if (IsSubmitting)
        {
            return;
        }

        IsSubmitting = true;
        ClearError();

        try
        {
            await _chartOfAccountsService.CreateChartOfAccountAsync(new CreateAccountRequest
            {
                ReferenceNumber = values.ReferenceNumber,
                AccountName = values.AccountName,
                Type = values.Type,
                Role = values.Role,
                IsActive = true
            });

            await LoadAccountsAsync();
        }
        catch (Exception ex)
        {
            ShowError(ExtractMessage(ex, "Gagal menyimpan data akun"));
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    public async Task UpdateAccountAsync(int id, AccountFormValues values)
    {
        if (IsSubmitting)
        {
            return;
        }

        IsSubmitting = true;
        ClearError();

        try
        {
            await _chartOfAccountsService.UpdateChartOfAccountAsync(id, new UpdateAccountRequest
            {
                ReferenceNumber = values.ReferenceNumber,
                AccountName = values.AccountName,
                Type = values.Type,
                Role = values.Role,
                IsActive = values.IsActive
            });

            await LoadAccountsAsync();
        }
        catch (Exception ex)
        {
            ShowError(ExtractMessage(ex, "Gagal menyimpan data akun"));
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    public async Task DeleteAccountAsync(int id)
    {
        if (IsSubmitting)
        {
            return;
        }

        IsSubmitting = true;
        ClearError();

        try
        {
            await _chartOfAccountsService.DeleteChartOfAccountAsync(id);
            await LoadAccountsAsync();
        }
        catch (Exception ex)
        {
            ShowError(ExtractMessage(ex, "Gagal menghapus akun"));
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    public void ClearError()
    {
        IsErrorOpen = false;
        ErrorMessage = string.Empty;
    }

    private void UpdateSubtitle(string? selectedPeriodName)
    {
        SubtitleText = string.IsNullOrWhiteSpace(selectedPeriodName)
            ? BaseSubtitle
            : $"{BaseSubtitle} Periode saldo: {selectedPeriodName}.";
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        IsErrorOpen = true;
    }

    private static string ExtractMessage(Exception ex, string fallback)
    {
        return !string.IsNullOrWhiteSpace(ex.Message) ? ex.Message : fallback;
    }
}