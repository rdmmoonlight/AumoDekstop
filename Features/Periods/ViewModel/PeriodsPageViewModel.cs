using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AumoDekstop.Features.Periods.Models;
using AumoDekstop.Features.Periods.Services;

namespace AumoDekstop.Features.Periods.ViewModels;

public partial class PeriodsPageViewModel : ObservableObject
{
    private readonly IPeriodsApiService _periodsApiService;

    [ObservableProperty]
    private ObservableCollection<PeriodItem> _periods = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActivePeriod))]
    private PeriodItem? _activePeriod;

    public bool HasActivePeriod => ActivePeriod is not null;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isCreating;

    [ObservableProperty]
    private bool _isClosing;

    [ObservableProperty]
    private bool _isSelecting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _actionError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _queryError;

    // Gabungkan error dari aksi mutasi terakhir maupun error dari query daftar periode
    public string? ErrorMessage => !string.IsNullOrWhiteSpace(ActionError) ? ActionError : QueryError;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public PeriodsPageViewModel(IPeriodsApiService periodsApiService)
    {
        _periodsApiService = periodsApiService;
    }

    private void UpdateActivePeriod()
    {
        // Periode aktif: periode terbuka yang sedang dipilih, atau periode terbuka pertama
        ActivePeriod =
            Periods.FirstOrDefault(p => p.IsSelected && !p.IsClosed) ??
            Periods.FirstOrDefault(p => !p.IsClosed);
    }

    [RelayCommand]
    public async Task FetchPeriodsAsync(CancellationToken cancellationToken = default)
    {
        ActionError = null;
        QueryError = null;
        IsLoading = true;

        try
        {
            var response = await _periodsApiService.GetPeriodsAsync(cancellationToken);
            Periods = new ObservableCollection<PeriodItem>(response.Periods);
            UpdateActivePeriod();
        }
        catch (OperationCanceledException)
        {
            // Abaikan pembatalan
        }
        catch (Exception ex)
        {
            QueryError = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Gagal memuat daftar periode";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CreatePeriodAsync(CreatePeriodPayload payload, CancellationToken cancellationToken = default)
    {
        ActionError = null;
        IsCreating = true;

        try
        {
            await _periodsApiService.CreatePeriodAsync(payload, cancellationToken);
            await FetchPeriodsAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Abaikan pembatalan
        }
        catch (Exception ex)
        {
            ActionError = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Gagal membuat periode baru";
        }
        finally
        {
            IsCreating = false;
        }
    }

    [RelayCommand]
    public async Task ClosePeriodAsync(int id, CancellationToken cancellationToken = default)
    {
        ActionError = null;
        IsClosing = true;

        try
        {
            await _periodsApiService.ClosePeriodAsync(id, cancellationToken);
            await FetchPeriodsAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Abaikan pembatalan
        }
        catch (Exception ex)
        {
            ActionError = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Gagal menutup periode";
        }
        finally
        {
            IsClosing = false;
        }
    }

    [RelayCommand]
    public async Task SelectPeriodAsync(int id, CancellationToken cancellationToken = default)
    {
        ActionError = null;
        IsSelecting = true;

        try
        {
            await _periodsApiService.SelectPeriodAsync(id, cancellationToken);
            await FetchPeriodsAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Abaikan pembatalan
        }
        catch (Exception ex)
        {
            ActionError = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Gagal memilih periode";
        }
        finally
        {
            IsSelecting = false;
        }
    }
}
