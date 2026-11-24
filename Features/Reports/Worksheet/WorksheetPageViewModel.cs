
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace AumoDekstop.Features.Reports.Worksheet;

public partial class WorksheetPageViewModel : INotifyPropertyChanged
{
    private readonly IWorksheetService _worksheetService;
    private CancellationTokenSource? _fetchCts;

    private bool _isLoading = true;
    private string? _errorMessage;
    private bool _hasPeriodSelected = true;
    private decimal _netIncome;
    private WorksheetTotals _totals = new();
    private List<WorksheetRow> _rows = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public WorksheetPageViewModel(IWorksheetService worksheetService)
    {
        _worksheetService = worksheetService;
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoadingVisibility));
                OnPropertyChanged(nameof(ContentVisibility));
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasPeriodSelected
    {
        get => _hasPeriodSelected;
        set
        {
            if (_hasPeriodSelected != value)
            {
                _hasPeriodSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoPeriodVisibility));
                OnPropertyChanged(nameof(WorksheetContentVisibility));
            }
        }
    }

    public decimal NetIncome
    {
        get => _netIncome;
        set
        {
            if (_netIncome != value)
            {
                _netIncome = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NetIncomeDisplay));
            }
        }
    }

    public string NetIncomeDisplay => WorksheetFormatter.FormatNumber(NetIncome);

    public WorksheetTotals Totals
    {
        get => _totals;
        set
        {
            if (_totals != value)
            {
                _totals = value;
                OnPropertyChanged();
            }
        }
    }

    public List<WorksheetRow> Rows
    {
        get => _rows;
        set
        {
            _rows = value;
            OnPropertyChanged();
        }
    }

    public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ContentVisibility => IsLoading ? Visibility.Collapsed : Visibility.Visible;
    public Visibility NoPeriodVisibility => !HasPeriodSelected ? Visibility.Visible : Visibility.Collapsed;
    public Visibility WorksheetContentVisibility => HasPeriodSelected ? Visibility.Visible : Visibility.Collapsed;

    public async Task LoadWorksheetAsync()
    {
        CancelOngoingRequest();
        _fetchCts = new CancellationTokenSource();
        var token = _fetchCts.Token;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var rawData = await _worksheetService.GetWorksheetAsync(token);

            var mappedRows = new List<WorksheetRow>();
            var calculatedTotals = new WorksheetTotals();

            if (rawData.Rows != null)
            {
                foreach (var r in rawData.Rows)
                {
                    var row = new WorksheetRow
                    {
                        AccountId = r.AccountId,
                        ReferenceNumber = r.ReferenceNumber,
                        AccountName = r.AccountName ?? string.Empty,
                        Type = r.Type ?? string.Empty,
                        NormalBalanceIsDebit = r.NormalBalanceIsDebit ?? true,
                        UnadjustedDebit = r.TbDebit ?? 0m,
                        UnadjustedCredit = r.TbCredit ?? 0m,
                        AdjustmentDebit = r.AdjDebit ?? 0m,
                        AdjustmentCredit = r.AdjCredit ?? 0m,
                        AdjustedDebit = r.AdjTbDebit ?? 0m,
                        AdjustedCredit = r.AdjTbCredit ?? 0m,
                        IncomeStatementDebit = r.IsDebit ?? 0m,
                        IncomeStatementCredit = r.IsCredit ?? 0m,
                        FinancialPositionDebit = r.BsDebit ?? 0m,
                        FinancialPositionCredit = r.BsCredit ?? 0m
                    };

                    mappedRows.Add(row);

                    calculatedTotals.UnadjustedDebit += row.UnadjustedDebit;
                    calculatedTotals.UnadjustedCredit += row.UnadjustedCredit;
                    calculatedTotals.AdjustmentDebit += row.AdjustmentDebit;
                    calculatedTotals.AdjustmentCredit += row.AdjustmentCredit;
                    calculatedTotals.AdjustedDebit += row.AdjustedDebit;
                    calculatedTotals.AdjustedCredit += row.AdjustedCredit;
                    calculatedTotals.IsDebit += row.IncomeStatementDebit;
                    calculatedTotals.IsCredit += row.IncomeStatementCredit;
                    calculatedTotals.BsDebit += row.FinancialPositionDebit;
                    calculatedTotals.BsCredit += row.FinancialPositionCredit;
                }
            }

            var netIncomeValue = rawData.Totals?.NetIncome ?? 0m;
            calculatedTotals.NetIncome = netIncomeValue;

            HasPeriodSelected = rawData.HasPeriodSelected != false;
            NetIncome = netIncomeValue;
            Totals = calculatedTotals;
            Rows = mappedRows;
        }
        catch (OperationCanceledException)
        {
            // Request dibatalkan saat navigasi keluar atau reload, abaikan
        }
        catch (Exception ex)
        {
            ErrorMessage = string.IsNullOrWhiteSpace(ex.Message)
                ? "Gagal memuat data worksheet."
                : ex.Message;
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsLoading = false;
            }
        }
    }

    public Task LoadAsync() => LoadWorksheetAsync();

    public void CancelOngoingRequest()
    {
        if (_fetchCts != null)
        {
            _fetchCts.Cancel();
            _fetchCts.Dispose();
            _fetchCts = null;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}