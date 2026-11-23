namespace AumoDekstop.Features.Reports.TrialBalances.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Reports.TrialBalances.Models;
using AumoDekstop.Features.Reports.TrialBalances.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public abstract partial class TrialBalanceReportViewModelBase : ObservableObject
{
    private readonly ITrialBalancesApiService _apiService;
    private readonly TrialBalanceType _reportType;
    private readonly string _defaultErrorMessage;
    private CancellationTokenSource? _loadCts;

    protected TrialBalanceReportViewModelBase(
        ITrialBalancesApiService apiService,
        TrialBalanceType reportType,
        string defaultErrorMessage)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _reportType = reportType;
        _defaultErrorMessage = defaultErrorMessage;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContentVisible))]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowErrorAlert))]
    public partial bool IsError { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowErrorAlert))]
    [NotifyPropertyChangedFor(nameof(ShowReportContent))]
    public partial bool NoPeriod { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<TrialRow> Rows { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBalanced))]
    public partial decimal TotalDebit { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBalanced))]
    public partial decimal TotalCredit { get; set; }

    public bool IsContentVisible => !IsLoading;

    public bool ShowErrorAlert => IsError && !NoPeriod && !NotClosed;

    public bool ShowReportContent => !NoPeriod && !NotClosed;

    public bool IsBalanced => Math.Abs(TotalDebit - TotalCredit) < 0.01m;

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
            var result = await _apiService.GetTrialBalanceAsync(
                new GetTrialBalanceParams { Type = _reportType },
                token);

            SelectedPeriodName = result.SelectedPeriodName;
            NoPeriod = !result.HasPeriodSelected;
            NotClosed = result.HasPeriodSelected && !result.IsPeriodClosed;

            if (NoPeriod || NotClosed)
            {
                Rows = [];
                TotalDebit = 0m;
                TotalCredit = 0m;
                return;
            }

            Rows = new ObservableCollection<TrialRow>(result.Rows);
            TotalDebit = result.Rows.Sum(r => r.Debit);
            TotalCredit = result.Rows.Sum(r => r.Credit);
        }
        catch (OperationCanceledException)
        {
            // Request dibatalkan saat navigasi / reload
        }
        catch (Exception ex)
        {
            NoPeriod = false;
            IsError = true;
            ErrorMessage = string.IsNullOrWhiteSpace(ex.Message) ? _defaultErrorMessage : ex.Message;
            Rows = [];
            TotalDebit = 0m;
            TotalCredit = 0m;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Tambahkan properti & logika ini di dalam TrialBalanceReportViewModelBase:

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowReportContent))]
    [NotifyPropertyChangedFor(nameof(ShowErrorAlert))]
    public partial bool NotClosed { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotClosedMessage))]
    public partial string? SelectedPeriodName { get; set; }

    public string NotClosedMessage
    {
        get
        {
            var periodLabel = !string.IsNullOrWhiteSpace(SelectedPeriodName)
                ? $"\"{SelectedPeriodName}\" "
                : "ini ";
            return $"Periode {periodLabel}belum ditutup (closed), sehingga Post-Closing Trial Balance belum dibuat. Laporan ini baru tersedia setelah periode ditutup melalui halaman Periods.";
        }
    }
}

public sealed partial class UnadjustedTrialBalanceViewModel : TrialBalanceReportViewModelBase
{
    public UnadjustedTrialBalanceViewModel(ITrialBalancesApiService apiService)
        : base(
            apiService,
            TrialBalanceType.Unadjusted,
            "Gagal memuat laporan trial balance.")
    {
    }
}

public sealed partial class AdjustedTrialBalanceViewModel : TrialBalanceReportViewModelBase
{
    public AdjustedTrialBalanceViewModel(ITrialBalancesApiService apiService)
        : base(
            apiService,
            TrialBalanceType.Adjusted,
            "Gagal memuat data adjusted trial balance.")
    {
    }
}

public sealed partial class PostClosingTrialBalanceViewModel : TrialBalanceReportViewModelBase
{
    public PostClosingTrialBalanceViewModel(ITrialBalancesApiService apiService)
        : base(
            apiService,
            TrialBalanceType.PostClosing,
            "Failed to load post-closing trial balance.")
    {
    }
}