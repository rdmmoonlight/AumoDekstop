namespace AumoDekstop;

using AumoDekstop.Features.Auth.Views;
using AumoDekstop.Features.Periods.ViewModels;
using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Services;
using AumoDekstop.Shell.Ribbon;
using Microsoft.Extensions.DependencyInjection;

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

    private void OnRibbonTabChanged(object? sender, RibbonTabChangedEventArgs e)
    {
        // Event ini dapat terpicu saat InitializeComponent, sebelum RootFrame/AppStatusBar terisi
        if (RootFrame is null || AppStatusBar is null) return;

        SetStatus($"● Tab aktif: {e.Text} | Versi: v{GitHubUpdateService.GetCurrentVersionString()} | Server: {ApiService.BaseServerUrl}");

        switch (e.Tag)
        {
            case "periods":
                ShowPeriods();
                break;
            default:
                // "home", "data", "report", "settings": sementara menampilkan Beranda
                ShowHome();
                break;
        }
    }
}
