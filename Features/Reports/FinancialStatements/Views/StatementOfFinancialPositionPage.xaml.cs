
using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Features.Reports.FinancialStatements.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AumoDekstop.Features.Reports.FinancialStatements.Views;

public sealed partial class StatementOfFinancialPositionPage : Page
{
    public StatementOfFinancialPositionViewModel ViewModel { get; }

    public StatementOfFinancialPositionPage(StatementOfFinancialPositionViewModel viewModel)
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