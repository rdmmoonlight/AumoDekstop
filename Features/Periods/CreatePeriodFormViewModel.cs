using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AumoDekstop.Features.Periods.Models;
using AumoDekstop.Features.Periods.Services;

namespace AumoDekstop.Features.Periods.ViewModels;

public partial class CreatePeriodFormViewModel : ObservableObject
{
    private readonly IPeriodsApiService _periodsApiService;
    private readonly Func<Task>? _onSuccessCallback;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _error;

    public bool HasError => !string.IsNullOrWhiteSpace(Error);

    public CreatePeriodFormViewModel(
        IPeriodsApiService periodsApiService,
        Func<Task>? onSuccessCallback = null)
    {
        _periodsApiService = periodsApiService;
        _onSuccessCallback = onSuccessCallback;
    }

    // [RelayCommand] otomatis menghasilkan properti 'SubmitFormCommand'
    // beserta state 'SubmitFormCommand.IsRunning' (setara dengan isLoading / isPending di Vue)
    [RelayCommand]
    public async Task SubmitFormAsync(CreatePeriodPayload payload, CancellationToken cancellationToken = default)
    {
        Error = null;

        try
        {
            await _periodsApiService.CreatePeriodAsync(payload, cancellationToken);

            if (_onSuccessCallback is not null)
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
