
using AumoDekstop.Features.ChartOfAccounts.Views;
using AumoDekstop.Features.Periods.ViewModels;
using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Services;
using AumoDekstop.Shell.Ribbon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop;

public sealed partial class MainWindow
{
    private void ShowHome()
    {
        if (RootFrame.Content is not MainPage)
        {
            RootFrame.Content = new MainPage();
        }
    }

    private void ShowPeriods()
    {
        _periodsPage ??= new PeriodsPage(App.Current.Services.GetRequiredService<PeriodsPageViewModel>());

        if (!ReferenceEquals(RootFrame.Content, _periodsPage))
        {
            RootFrame.Content = _periodsPage;
        }
    }

    private void ShowCoa()
    {
        // Halaman dibuat baru setiap tab dibuka agar data selalu dimuat ulang (dan tidak tersisa antar akun)
        RootFrame.Content = App.Current.Services.GetRequiredService<ChartOfAccountsPage>();
    }

    private async Task ShowReportPageAsync<TPage>(string statusTitle, Func<TPage, Task> loadAction)
        where TPage : Page
    {
        var page = App.Current.Services.GetRequiredService<TPage>();
        RootFrame.Content = page;
        SetStatus($"● Membuka {statusTitle}...");
        await loadAction(page);
        SetStatus($"● {statusTitle} dimuat.");
    }

    private void OnRibbonTabChanged(object? sender, RibbonTabChangedEventArgs e)
    {
        // Event ini dapat terpicu saat InitializeComponent, sebelum RootFrame/AppStatusBar terisi
        if (RootFrame is null || AppStatusBar is null)
        {
            return;
        }

        SetStatus($"● Tab aktif: {e.Text} | Versi: v{GitHubUpdateService.GetCurrentVersionString()} | Server: {ApiConfig.BaseUrl}");

        switch (e.Tag)
        {
            case "periods":
                ShowPeriods();
                break;
            case "coa":
                ShowCoa();
                break;
            default:
                // "home", "data", "report", "settings": sementara menampilkan Beranda
                ShowHome();
                break;
        }
    }
}