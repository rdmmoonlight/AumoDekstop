using System;
using AumoDekstop.Features.Auth.Services;
using AumoDekstop.Features.Auth.Views;
using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Features.Shared.Http;
using AumoDekstop.Services;
using AumoDekstop.Services.Api;
using AumoDekstop.Shell.Updates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace AumoDekstop;
// Bagian lain dari kelas ini ada di folder Shell/Window:
//   MainWindow.Auth.cs        -> login, masuk aplikasi, keluar
//   MainWindow.Navigation.cs  -> perpindahan halaman & tab ribbon
//   MainWindow.Commands.cs    -> aksi tombol ribbon
public sealed partial class MainWindow : Window
{
    private bool _hasCheckedUpdate;
    private bool _isHandlingSessionExpired;
    private readonly UpdateCoordinator _updates;
    private readonly ApiService _api = App.Current.Services.GetRequiredService<ApiService>();
    private readonly ITokenStore _tokenStore = App.Current.Services.GetRequiredService<ITokenStore>();

    // Halaman Periods dibuat lewat DI (konstruktor membutuhkan ViewModel), lalu disimpan agar tidak dimuat ulang.
    private PeriodsPage? _periodsPage;
    private LoginPage? _loginPage;

    private readonly IAuthService _auth = App.Current.Services.GetRequiredService<IAuthService>();

    public MainWindow()
    {
        InitializeComponent();
        RibbonHost.Visibility = Visibility.Collapsed;
        _updates = new UpdateCoordinator(SetStatus);
        GitHubUpdateService.CleanupOldExecutable();

        _tokenStore.SessionExpired += TokenStore_SessionExpired;
        Closed += MainWindow_Closed;

        // Event Loaded pada Content menjamin XamlRoot sudah siap
        if (Content is FrameworkElement rootElement)
        {
            rootElement.Loaded += RootElement_Loaded;
        }
    }

    private void OnMenuRequested(object? sender, EventArgs e)
    {
        MainNavigation.IsPaneOpen = !MainNavigation.IsPaneOpen;
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        _tokenStore.SessionExpired -= TokenStore_SessionExpired;
    }

    private void TokenStore_SessionExpired(object? sender, EventArgs e)
    {
        // Pemanggilan HTTP sering berjalan di background thread; pastikan manipulasi UI masuk ke UI thread
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_isHandlingSessionExpired)
            {
                return;
            }

            _isHandlingSessionExpired = true;
            try
            {
                const string message = "Sesi berakhir, silakan masuk kembali.";
                _auth.ClearSession();
                SessionStorageService.ClearSession();
                ShowLogin(message);
                SetStatus($"● {message}");
            }
            finally
            {
                _isHandlingSessionExpired = false;
            }
        });
    }

    private async void RootElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (_hasCheckedUpdate)
        {
            return;
        }

        _hasCheckedUpdate = true;

        SetStatus("● Memulihkan sesi...");
        var (isRestored, errorMessage) = await _auth.RestoreSessionAsync();

        if (isRestored)
        {
            EnterApp();
        }
        else
        {
            ShowLogin(errorMessage);
        }

        await _updates.CheckAndPromptAsync(Content?.XamlRoot, isManualCheck: false);
    }

    private void SetStatus(string text)
    {
        if (AppStatusBar is not null)
        {
            AppStatusBar.SetStatus(text);
        }
    }
}
