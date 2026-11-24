
using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Features.Reports.TrialBalances.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AumoDekstop.Features.Reports.TrialBalances.Views;
public sealed partial class UnadjustedTrialBalancePage : Page
{
    public UnadjustedTrialBalanceViewModel ViewModel { get; }

    public UnadjustedTrialBalancePage(UnadjustedTrialBalanceViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadAsync();
    }

    private void NoPeriodCard_NavigateToPeriodsClicked(object sender, RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(PeriodsPage));
    }
}