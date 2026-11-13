using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AumoDekstop.Features.Periods.ViewModels;

namespace AumoDekstop.Features.Periods.Views;

public sealed partial class PeriodsPage : Page
{
    public PeriodsPageViewModel ViewModel { get; }

    public string ActivePeriodName => ViewModel.ActivePeriod?.PeriodName ?? string.Empty;

    public bool IsNotCreating => !ViewModel.IsCreating;

    // Constructor untuk Dependency Injection (misal via Microsoft.Extensions.DependencyInjection)
    public PeriodsPage(PeriodsPageViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PeriodsPageViewModel.ActivePeriod)
            or nameof(PeriodsPageViewModel.IsCreating))
        {
            Bindings.Update();
        }
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.FetchPeriodsCommand.ExecuteAsync(null);
    }

    private async void OpenNewPeriodButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenNewPeriodDialog
        {
            XamlRoot = this.XamlRoot,
            OnSubmitAsync = async payload =>
            {
                await ViewModel.CreatePeriodCommand.ExecuteAsync(payload);
            }
        };

        await dialog.ShowAsync();
    }

    private async void PeriodsTable_ClosePeriodRequested(object? sender, int periodId)
    {
        await ViewModel.ClosePeriodCommand.ExecuteAsync(periodId);
    }

    private async void PeriodsTable_SelectPeriodRequested(object? sender, int periodId)
    {
        await ViewModel.SelectPeriodCommand.ExecuteAsync(periodId);
    }
}
