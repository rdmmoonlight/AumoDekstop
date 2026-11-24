
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AumoDekstop.Features.Reports.GeneralLedgers.Views;

public sealed partial class GeneralLedgersPermanentPage : Page
{
    private readonly IGeneralLedgersService _generalLedgersService;
    private CancellationTokenSource? _loadCts;

    private bool _isLoading;
    private bool _isRefreshing;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GeneralLedgersPermanentPage()
    {
        this.InitializeComponent();

        // Ambil service dari DI container (sesuaikan jika nama properti Services di App.xaml.cs berbeda)
        _generalLedgersService = App.Current.Services.GetRequiredService<IGeneralLedgersService>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadPermanentLedgerAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
    }

    private async Task LoadPermanentLedgerAsync()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        SetLoadingState(true);
        HideAllContentStates();

        try
        {
            var response = await _generalLedgersService.GetGeneralLedgerPermanentAsync(token);

            HeaderControl.PeriodName = response.SelectedPeriodName;

            var accounts = ParseAccounts(response.Accounts);

            if (!response.HasPeriodSelected || accounts.Count == 0)
            {
                EmptyOrNoPeriodControl.Message = !response.HasPeriodSelected
                    ? "Silakan pilih periode terlebih dahulu."
                    : "Tidak ada data akun permanen.";

                EmptyOrNoPeriodControl.Visibility = Visibility.Visible;
            }
            else
            {
                AccountsRepeater.ItemsSource = accounts;
                AccountsRepeater.Visibility = Visibility.Visible;
            }
        }
        catch (OperationCanceledException)
        {
            // Permintaan dibatalkan saat navigasi atau refresh ulang
        }
        catch (Exception ex)
        {
            ErrorInfoBar.Message = string.IsNullOrWhiteSpace(ex.Message)
                ? "Gagal memuat data buku besar permanen."
                : ex.Message;
            ErrorInfoBar.IsOpen = true;
            ErrorInfoBar.Visibility = Visibility.Visible;
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async void OnRefreshClicked(object sender, RoutedEventArgs e)
    {
        if (_isLoading || _isRefreshing)
        {
            return;
        }

        SetRefreshingState(true);

        try
        {
            await _generalLedgersService.RefreshGeneralLedgerAsync();
            await LoadPermanentLedgerAsync();
        }
        catch (Exception ex)
        {
            ErrorInfoBar.Message = string.IsNullOrWhiteSpace(ex.Message)
                ? "Gagal memperbarui data buku besar permanen."
                : ex.Message;
            ErrorInfoBar.IsOpen = true;
            ErrorInfoBar.Visibility = Visibility.Visible;
        }
        finally
        {
            SetRefreshingState(false);
        }
    }

    private static List<LedgerAccountGroup> ParseAccounts(List<JsonElement>? rawAccounts)
    {
        var result = new List<LedgerAccountGroup>();
        if (rawAccounts is null || rawAccounts.Count == 0)
        {
            return result;
        }

        foreach (var element in rawAccounts)
        {
            var account = element.Deserialize<LedgerAccountGroup>(JsonOptions);
            if (account is not null)
            {
                result.Add(account);
            }
        }

        return result;
    }

    private void SetLoadingState(bool isLoading)
    {
        _isLoading = isLoading;
        LoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        UpdateRefreshButtonState();
    }

    private void SetRefreshingState(bool isRefreshing)
    {
        _isRefreshing = isRefreshing;
        RefreshSpinner.IsActive = isRefreshing;
        RefreshSpinner.Visibility = isRefreshing ? Visibility.Visible : Visibility.Collapsed;
        RefreshButtonText.Text = isRefreshing ? "Refreshing..." : "Refresh Data";
        UpdateRefreshButtonState();
    }

    private void UpdateRefreshButtonState()
    {
        RefreshButton.IsEnabled = !_isLoading && !_isRefreshing;
    }

    private void HideAllContentStates()
    {
        ErrorInfoBar.IsOpen = false;
        ErrorInfoBar.Visibility = Visibility.Collapsed;
        EmptyOrNoPeriodControl.Visibility = Visibility.Collapsed;
        AccountsRepeater.Visibility = Visibility.Collapsed;
    }
}