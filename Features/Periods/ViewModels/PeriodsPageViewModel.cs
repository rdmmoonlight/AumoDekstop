using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Periods.Models;
using AumoDekstop.Features.Periods.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AumoDekstop.Features.Periods.ViewModels;

public partial class PeriodsPageViewModel : ObservableObject
{
    private readonly IPeriodsService _PeriodsService;

    [ObservableProperty]
    public partial ObservableCollection<PeriodItem> Periods { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActivePeriod))]
    public partial PeriodItem? ActivePeriod { get; set; } = null;

    public bool HasActivePeriod => ActivePeriod is not null;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    [ObservableProperty]
    public partial bool IsCreating { get; set; } = false;

    [ObservableProperty]
    public partial bool IsClosing { get; set; } = false;

    [ObservableProperty]
    public partial bool IsSelecting { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ActionError { get; set; } = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? QueryError { get; set; } = null;

    // Gabungkan error dari aksi mutasi terakhir maupun error dari query daftar periode
    public string? ErrorMessage => !string.IsNullOrWhiteSpace(ActionError) ? ActionError : QueryError;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public PeriodsPageViewModel(IPeriodsService PeriodsService)
    {
        _PeriodsService = PeriodsService;
    }

    private void UpdateActivePeriod(int? selectedPeriodId = null)
    {
        // Periode aktif: cocokkan dengan SelectedPeriodId dari server, lalu flag IsSelected, atau periode terbuka pertama
        ActivePeriod =
            (selectedPeriodId.HasValue
                ? Periods.FirstOrDefault(p => p.Id == selectedPeriodId.Value && !p.IsClosed)
                : null) ??
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
            var response = await _PeriodsService.GetPeriodsAsync(cancellationToken);

            Periods.Clear();
            foreach (var item in response.Periods)
            {
                Periods.Add(item);
            }

            UpdateActivePeriod(response.SelectedPeriodId);
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
    public async Task CreatePeriodAsync(CreatePeriodRequest? request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return;
        }

        ActionError = null;
        IsCreating = true;

        try
        {
            var result = await _PeriodsService.CreatePeriodAsync(request, cancellationToken);
            if (!result.Success)
            {
                ActionError = !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Gagal membuat periode baru";
                return;
            }

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
            await _PeriodsService.ClosePeriodAsync(id, cancellationToken);
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
            await _PeriodsService.SelectPeriodAsync(id, cancellationToken);
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