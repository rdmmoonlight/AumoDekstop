using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AumoDekstop.Features.Periods.Models;
using AumoDekstop.Features.Periods.ViewModels;

namespace AumoDekstop.Features.Periods.Views;

public sealed partial class OpenNewPeriodDialog : ContentDialog
{
    public PeriodFormState FormState { get; } = new();

    /// <summary>
    /// Opsional: Handler async saat tombol "Simpan & Buka Periode" ditekan.
    /// Jika diisi, dialog akan menunggu task selesai sebelum menutup.
    /// </summary>
    public Func<CreatePeriodPayload, Task>? OnSubmitAsync { get; set; }

    public CreatePeriodPayload? SubmittedPayload { get; private set; }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            IsPrimaryButtonEnabled = !value;
            IsSecondaryButtonEnabled = !value;
            StartDatePicker.IsEnabled = !value;
            EndDatePicker.IsEnabled = !value;
            LoadingPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public OpenNewPeriodDialog()
    {
        InitializeComponent();
    }

    private void OnDateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        if (sender == StartDatePicker && args.NewDate.HasValue)
        {
            StartDateErrorText.Visibility = Visibility.Collapsed;
        }
        else if (sender == EndDatePicker && args.NewDate.HasValue)
        {
            EndDateErrorText.Visibility = Visibility.Collapsed;
        }
    }

    private bool ValidateForm()
    {
        bool isStartValid = FormState.StartDate.HasValue;
        bool isEndValid = FormState.EndDate.HasValue;

        StartDateErrorText.Visibility = isStartValid ? Visibility.Collapsed : Visibility.Visible;
        EndDateErrorText.Visibility = isEndValid ? Visibility.Collapsed : Visibility.Visible;

        return isStartValid && isEndValid;
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validasi setara Zod schema (min 1)
        if (!ValidateForm())
        {
            args.Cancel = true;
            return;
        }

        var payload = FormState.ToPayload();
        SubmittedPayload = payload;

        if (OnSubmitAsync is not null)
        {
            // Gunakan deferral agar ContentDialog menunggu operasi async selesai sebelum tertutup
            var deferral = args.GetDeferral();
            try
            {
                IsLoading = true;
                await OnSubmitAsync(payload);
            }
            catch
            {
                // Biarkan dialog tetap terbuka jika terjadi error saat submit
                args.Cancel = true;
            }
            finally
            {
                IsLoading = false;
                deferral.Complete();
            }
        }
    }
}
