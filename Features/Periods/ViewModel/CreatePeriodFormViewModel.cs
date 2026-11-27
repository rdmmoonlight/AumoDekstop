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
        IPeriodsApiService periodsApiService,
        Func<Task>? onSuccessCallback = null)
    {
        _periodsApiService = periodsApiService ?? throw new ArgumentNullException(nameof(periodsApiService));
        _onSuccessCallback = onSuccessCallback;
    }

    [RelayCommand]
    public async Task SubmitFormAsync(CreatePeriodPayload payload, CancellationToken cancellationToken = default)
    {
        if (payload is null || IsSubmitting)
        {
            return;
        }

        Error = null;
        IsSubmitting = true;

        try
        {
            var request = MapToCreatePeriodRequest(payload);
            var result = await _periodsApiService.CreatePeriodAsync(request, cancellationToken);

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
                : "Terjadi kesalahan saat membuat periode";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private static CreatePeriodRequest MapToCreatePeriodRequest(CreatePeriodPayload payload)
    {
        var isLoadExisting = string.Equals(
            payload.SetupMode,
            CreatePeriodRequest.ModeLoadExisting,
            StringComparison.OrdinalIgnoreCase);

        if (isLoadExisting)
        {
            return new CreatePeriodRequest
            {
                Month = payload.Month,
                Year = payload.Year,
                SetupMode = CreatePeriodRequest.ModeLoadExisting,
                CashAccountId = payload.CashAccountId,
                BankAccountId = payload.BankAccountId,
                RetainedEarningsAccountId = payload.RetainedEarningsAccountId
            };
        }

        return new CreatePeriodRequest
        {
            Month = payload.Month,
            Year = payload.Year,
            SetupMode = CreatePeriodRequest.ModeCreateNew,
            CashAccountCode = payload.CashAccountCode,
            CashAccountName = payload.CashAccountName,
            CashBalance = payload.CashBalance,
            BankAccountCode = payload.BankAccountCode,
            BankAccountName = payload.BankAccountName,
            BankBalance = payload.BankBalance,
            RetainedEarningsAccountCode = payload.RetainedEarningsAccountCode,
            RetainedEarningsAccountName = payload.RetainedEarningsAccountName
        };
    }
}