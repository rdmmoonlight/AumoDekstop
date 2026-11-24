namespace AumoDekstop.Features.Worksheet;

using System;
using System.ComponentModel;
using System.Net.Http;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class WorksheetPage : Page
{
    public WorksheetPageViewModel ViewModel { get; }

    // Event opsional jika navigasi dikelola oleh Shell/Parent Frame
    public event EventHandler<string>? NavigationRequested;

    public WorksheetPage() : this(CreateDefaultViewModel())
    {
    }

    public WorksheetPage(WorksheetPageViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private static WorksheetPageViewModel CreateDefaultViewModel()
    {
        // Sesuaikan jika project menggunakan DI container (misal App.Services.GetRequiredService<WorksheetPageViewModel>())
        var httpClient = new HttpClient();
        var apiService = new WorksheetApiService(httpClient);
        return new WorksheetPageViewModel(apiService);
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