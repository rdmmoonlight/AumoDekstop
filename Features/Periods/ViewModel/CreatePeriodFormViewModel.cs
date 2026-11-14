using System;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Periods.Models;
using AumoDekstop.Features.Periods.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AumoDekstop.Features.Periods.ViewModels;

public sealed partial class CreatePeriodFormViewModel : ObservableObject
{
    private readonly IPeriodsApiService _periodsApiService;
    private readonly Func<Task>? _onSuccessCallback;

    private string? _error;

    public string? Error
    {
        get => _error;
        set
        {
            if (SetProperty(ref _error, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(Error);

    public CreatePeriodFormViewModel(
        IPeriodsApiService periodsApiService,
        Func<Task>? onSuccessCallback = null)
    {
        _periodsApiService = periodsApiService;
        _onSuccessCallback = onSuccessCallback;
    }

    [RelayCommand]
    public async Task SubmitFormAsync(CreatePeriodPayload payload, CancellationToken cancellationToken = default)
    {
        Error = null;

        try
        {
            await _periodsApiService.CreatePeriodAsync(payload, cancellationToken);

            if (_onSuccessCallback != null)
            {
                await _onSuccessCallback.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // Abaikan jika dibatalkan
        }
        catch (Exception ex)
        {
            Error = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Terjadi kesalahan saat membuat periode";
        }
    }
}
