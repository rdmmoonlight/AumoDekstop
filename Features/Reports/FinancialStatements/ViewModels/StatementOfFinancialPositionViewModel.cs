
using System;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Reports.FinancialStatements.Models;
using AumoDekstop.Features.Reports.FinancialStatements.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AumoDekstop.Features.Reports.FinancialStatements.ViewModels;

public sealed partial class StatementOfFinancialPositionViewModel : ObservableObject
{
    private readonly IFinancialStatementsApiService _apiService;
    private CancellationTokenSource? _loadCts;

    public StatementOfFinancialPositionViewModel(IFinancialStatementsApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    }

    [ObservableProperty]
    public partial bool IsPostClosing { get; set; }

    partial void OnIsPostClosingChanged(bool value)
    {
        _ = LoadAsync();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowErrorState))]
    [NotifyPropertyChangedFor(nameof(ShowNoPeriodState))]
    [NotifyPropertyChangedFor(nameof(ShowDataState))]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowErrorState))]
    [NotifyPropertyChangedFor(nameof(ShowNoPeriodState))]
    [NotifyPropertyChangedFor(nameof(ShowDataState))]
    public partial bool IsError { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPeriod))]
    [NotifyPropertyChangedFor(nameof(PeriodSubtitle))]
    [NotifyPropertyChangedFor(nameof(ShowNoPeriodState))]
    [NotifyPropertyChangedFor(nameof(ShowDataState))]
    public partial FinancialPositionData? Data { get; set; }

    public bool HasPeriod => Data?.Period is not null && !string.IsNullOrWhiteSpace(Data.Period.StartDate);

    public string PeriodSubtitle => HasPeriod
        ? $"Periode: {Data!.Period.StartDate} s/d {Data.Period.EndDate}"
        : string.Empty;

    public bool ShowErrorState => !IsLoading && IsError;

    public bool ShowNoPeriodState => !IsLoading && !IsError && Data is null;

    public bool ShowDataState => !IsLoading && !IsError && Data is not null;

    [RelayCommand]
    public async Task LoadAsync()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        IsLoading = true;
        IsError = false;
        ErrorMessage = string.Empty;

        try
        {
            Data = await _apiService.GetStatementOfFinancialPositionAsync(
                new StatementOfFinancialPositionParams { IsPostClosing = IsPostClosing },
                token);
        }
        catch (OperationCanceledException)
        {
            // Request dibatalkan saat toggle diubah / navigasi ulang
        }
        catch (Exception ex)
        {
            IsError = true;
            ErrorMessage = string.IsNullOrWhiteSpace(ex.Message)
                ? "Terjadi kesalahan saat mengambil data Laporan Posisi Keuangan."
                : ex.Message;
            Data = null;
        }
        finally
        {
            IsLoading = false;
        }
    }
}