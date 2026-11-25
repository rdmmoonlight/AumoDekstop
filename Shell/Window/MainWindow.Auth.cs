
using AumoDekstop.Features.Auth.Views;
using AumoDekstop.Services;
using Microsoft.UI.Xaml;

namespace AumoDekstop;
public sealed partial class MainWindow
{
    private void ShowLogin()
    {
        RibbonHost.Visibility = Visibility.Collapsed;
        _periodsPage = null;

        if (_loginPage is not null)
        {
            _loginPage.LoginSucceeded -= OnLoginSucceeded;
        }

        _loginPage = new LoginPage();
        _loginPage.LoginSucceeded += OnLoginSucceeded;
        RootFrame.Content = _loginPage;

        SetStatus($"● Silakan masuk | Server: {ApiConfig.BaseUrl}");
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        EnterApp();
    }

    private void EnterApp()
    {
        RibbonHost.Visibility = Visibility.Visible;
        RibbonHost.SelectHome();
        ShowHome();
        SetStatus($"● Siap | Versi: v{GitHubUpdateService.GetCurrentVersionString()} | Server: {ApiConfig.BaseUrl}");
    }

    private async Task LogoutAsync()
    {
        SetStatus("● Keluar dari akun...");

        try
        {
            await _auth.LogoutAsync();
        }
        catch
        {
            // Sesi lokal tetap dibersihkan walau server tidak merespons
            _auth.ClearSession();
        }

        ShowLogin();
    }
}