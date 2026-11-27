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
    private readonly IPeriodsService _periodsService;
    private readonly Func<Task>? _onSuccessCallback;

    private string? _error;
    private bool _isSubmitting;

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

    public bool IsSubmitting
    {
        get => _isSubmitting;
        private set => SetProperty(ref _isSubmitting, value);
    }

    public CreatePeriodFormViewModel(
        IPeriodsService periodsService,
        Func<Task>? onSuccessCallback = null)
    {
        _periodsService = periodsService ?? throw new ArgumentNullException(nameof(periodsService));
        _onSuccessCallback = onSuccessCallback;
    }

    [RelayCommand]
    public async Task SubmitFormAsync(CreatePeriodRequest? request, CancellationToken cancellationToken = default)
    {
        if (request is null || IsSubmitting)
        {
            return;
        }

        Error = null;
        IsSubmitting = true;

        try
        {
            var result = await _periodsService.CreatePeriodAsync(request, cancellationToken);

            if (!result.Success)
            {
                Error = !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Gagal membuat periode baru.";
                return;
            }

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
                : "Terjadi kesalahan saat membuat periode.";
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}