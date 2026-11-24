
using AumoDekstop.Features.Auth.Services;
using AumoDekstop.Features.Auth.Views;
using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Services;
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
    private readonly UpdateCoordinator _updates;
    private readonly ApiService _api = App.Current.Services.GetRequiredService<ApiService>();

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
    MainNavigation.IsPaneOpen = !MainNavigation.IsPaneOpen;
    }

    // Halaman Periods dibuat lewat DI (konstruktor membutuhkan ViewModel), lalu disimpan agar tidak dimuat ulang.
    private PeriodsPage? _periodsPage;
    private LoginPage? _loginPage;

    private readonly IAuthService _auth = App.Current.Services.GetRequiredService<IAuthService>();

    public MainWindow()
    {
        InitializeComponent();
        _updates = new UpdateCoordinator(SetStatus);
        GitHubUpdateService.CleanupOldExecutable();

        // Event Loaded pada Content menjamin XamlRoot sudah siap
        if (Content is FrameworkElement rootElement)
        {
            rootElement.Loaded += RootElement_Loaded;
        }
    }

    private async void RootElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (_hasCheckedUpdate)
        {
            return;
        }

        _hasCheckedUpdate = true;

        // Gerbang autentikasi: tanpa sesi login, tampilkan halaman login dan sembunyikan ribbon
        if (_auth.IsAuthenticated)
        {
            EnterApp();
        }
        else
        {
            ShowLogin();
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
