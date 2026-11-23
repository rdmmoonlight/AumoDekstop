namespace AumoDekstop.Features.Reports.FinancialStatements.Views;

using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Features.Reports.FinancialStatements.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class RetainedEarningsPage : Page
{
    public RetainedEarningsViewModel ViewModel { get; }

    public RetainedEarningsPage(RetainedEarningsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadAsync();
    }

    private void NoPeriodCard_ActionClicked(object sender, RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(PeriodsPage));
    }
}