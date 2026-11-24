namespace AumoDekstop.Features.Reports.Worksheet;

using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class WorksheetPage : Page
{
    public WorksheetPageViewModel ViewModel { get; }

    // Event opsional jika navigasi dikelola oleh Shell/Parent Frame
    public event EventHandler<string>? NavigationRequested;

    public WorksheetPage() : this(((App)Application.Current).Services.GetRequiredService<WorksheetPageViewModel>())
    {
    }

    public WorksheetPage(WorksheetPageViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadWorksheetAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.CancelOngoingRequest();
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnNavigatedFrom(e);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WorksheetPageViewModel.Rows))
        {
            WorksheetTableControl.SetRows(ViewModel.Rows);
        }
    }

    private void OnGoToPeriodsClick(object sender, RoutedEventArgs e)
    {
        NavigationRequested?.Invoke(this, "/periods");
    }

    private void OnGoToIncomeStatementClick(object sender, RoutedEventArgs e)
    {
        NavigationRequested?.Invoke(this, "/reports/financial-statements/income-statement");
    }
}