using System.Collections.ObjectModel;
using AumoDekstop.Features.ChartOfAccounts.Dialogs;
using AumoDekstop.Features.ChartOfAccounts.Models;
using AumoDekstop.Features.ChartOfAccounts.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.ChartOfAccounts.Views;

public sealed partial class ChartOfAccountsPage : Page
{
    private readonly IChartOfAccountsService _chartOfAccountsService;
    private readonly ObservableCollection<Account> _accounts = new();
    private CancellationTokenSource? _loadCts;
    private bool _isSubmitting;

    // Constructor default untuk XAML Frame navigation (mengambil service dari DI container App)
    public ChartOfAccountsPage()
        : this(App.Current.Services.GetRequiredService<IChartOfAccountsService>())
    {
    }

    // Constructor dengan Dependency Injection eksplisit
    public ChartOfAccountsPage(IChartOfAccountsService chartOfAccountsService)
    {
        this.InitializeComponent();
        _chartOfAccountsService = chartOfAccountsService;
        AccountsTable.Accounts = _accounts;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAccountsAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        SetLoading(true);
        ClearError();

        try
        {
            var result = await _chartOfAccountsService.GetChartOfAccountsAsync(cancellationToken: token);

            if (token.IsCancellationRequested)
                return;

            _accounts.Clear();
            foreach (var account in result.Accounts)
            {
                _accounts.Add(account);
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
                SetLoading(false);
            }
        }
    }

    private void UpdateSubtitle(string? selectedPeriodName)
    {
        const string baseText = "Kelola daftar akun buku besar untuk pencatatan transaksi keuangan.";
        SubtitleText.Text = string.IsNullOrWhiteSpace(selectedPeriodName)
            ? baseText
            : $"{baseText} Periode saldo: {selectedPeriodName}.";
    }

    private async void CreateAccountButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isSubmitting) return;
        ClearError();

        var dialog = new AccountFormDialog(this.XamlRoot, editingAccount: null);
        var dialogResult = await dialog.ShowAsync();

        if (dialogResult != ContentDialogResult.Primary || dialog.Result == null)
            return;

        await SubmitCreateAsync(dialog.Result);
    }

    private async void AccountsTable_EditAccountRequested(object? sender, Account account)
    {
        if (_isSubmitting) return;
        ClearError();

        var dialog = new AccountFormDialog(this.XamlRoot, editingAccount: account);
        var dialogResult = await dialog.ShowAsync();

        if (dialogResult != ContentDialogResult.Primary || dialog.Result == null)
            return;

        await SubmitUpdateAsync(account.Id, dialog.Result);
    }

    private async void AccountsTable_DeleteAccountRequested(object? sender, Account account)
    {
        if (_isSubmitting) return;
        ClearError();

        bool confirmed = await DeleteAccountDialog.ShowAsync(this.XamlRoot, account);
        if (!confirmed)
            return;

        await SubmitDeleteAsync(account.Id);
    }

    private async Task SubmitCreateAsync(AccountFormValues values)
    {
        SetSubmitting(true);
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

            // Setara queryClient.invalidateQueries() di Vue Query
            await LoadAccountsAsync();
        }
        catch (Exception ex)
        {
            ShowError(ExtractMessage(ex, "Gagal menyimpan data akun"));
        }
        finally
        {
            SetSubmitting(false);
        }
    }

    private async Task SubmitUpdateAsync(int id, AccountFormValues values)
    {
        SetSubmitting(true);
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
            SetSubmitting(false);
        }
    }

    private async Task SubmitDeleteAsync(int id)
    {
        SetSubmitting(true);
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
            SetSubmitting(false);
        }
    }

    private void SetLoading(bool isLoading)
    {
        AccountsTable.IsLoading = isLoading;
        RefreshButton.IsEnabled = !isLoading;
    }

    private void SetSubmitting(bool isSubmitting)
    {
        _isSubmitting = isSubmitting;
        CreateAccountButton.IsEnabled = !isSubmitting;
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }

    private void ClearError()
    {
        ErrorInfoBar.IsOpen = false;
        ErrorInfoBar.Message = string.Empty;
    }

    private void ErrorInfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        sender.Message = string.Empty;
    }

    private static string ExtractMessage(Exception ex, string fallback)
    {
        return !string.IsNullOrWhiteSpace(ex.Message) ? ex.Message : fallback;
    }
}