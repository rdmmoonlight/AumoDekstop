using AumoDekstop.Features.Periods.ViewModels;
using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop;

public sealed partial class MainWindow : Window
{
    private bool _hasCheckedUpdate;
    private readonly GitHubUpdateService _updateService = new();
    private readonly ApiService _api = App.Current.Services.GetRequiredService<ApiService>();

    // Halaman Periods dibuat lewat DI (konstruktor membutuhkan ViewModel), lalu disimpan agar tidak dimuat ulang.
    private PeriodsPage? _periodsPage;

    public MainWindow()
    {
        InitializeComponent();
        GitHubUpdateService.CleanupOldExecutable();

        // Event Loaded pada Content menjamin XamlRoot sudah siap
        if (Content is FrameworkElement rootElement)
        {
            rootElement.Loaded += RootElement_Loaded;
        }
    }

    private async void RootElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (_hasCheckedUpdate) return;
        _hasCheckedUpdate = true;

        ShowHome();
        SetStatus($"● Siap | Versi: v{GitHubUpdateService.GetCurrentVersionString()} | Server: {ApiService.BaseServerUrl}");

        await CheckAndPromptUpdateAsync(isManualCheck: false);
    }

    private void SetStatus(string text)
    {
        if (StatusText is not null)
        {
            StatusText.Text = text;
        }
    }

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

    private async void OnCheckUpdateClick(object sender, RoutedEventArgs e)
    {
        await CheckAndPromptUpdateAsync(isManualCheck: true);
    }

    private async Task CheckAndPromptUpdateAsync(bool isManualCheck)
    {
        if (Content?.XamlRoot == null)
            return;

        string currentVer = GitHubUpdateService.GetCurrentVersionString();

        if (isManualCheck)
        {
            SetStatus($"● Mengecek pembaruan di GitHub (Versi saat ini: v{currentVer})...");
        }

        var update = await _updateService.CheckForUpdateAsync();

        if (update == null)
        {
            if (isManualCheck)
            {
                SetStatus($"● Aplikasi sudah versi terbaru (v{currentVer}).");
                var infoDialog = new ContentDialog
                {
                    Title = "Tidak Ada Pembaruan",
                    Content = $"Anda sudah menggunakan versi terbaru (v{currentVer}).",
                    CloseButtonText = "OK",
                    XamlRoot = Content.XamlRoot
                };
                await infoDialog.ShowAsync();
            }
            return;
        }

        SetStatus($"● Pembaruan ditemukan: {update.VersionTag}!");

        var confirmDialog = new ContentDialog
        {
            Title = $"Pembaruan Tersedia ({update.VersionTag})",
            Content = new ScrollViewer
            {
                MaxHeight = 250,
                Content = new TextBlock
                {
                    Text = $"Versi saat ini: v{currentVer}\nVersi terbaru: {update.VersionTag}\n\nCatatan Rilis:\n{update.ReleaseNotes}",
                    TextWrapping = TextWrapping.Wrap
                }
            },
            PrimaryButtonText = "Update Sekarang",
            CloseButtonText = "Nanti Saja",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = Content.XamlRoot
        };

        var result = await confirmDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ShowDownloadProgressAndInstallAsync(update);
        }
    }

    private async Task ShowDownloadProgressAndInstallAsync(UpdateInfo update)
    {
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Height = 8,
            Margin = new Thickness(0, 12, 0, 8)
        };

        var progressLabel = new TextBlock
        {
            Text = "Menghubungkan ke server GitHub... 0%",
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var panel = new StackPanel { Width = 360 };
        panel.Children.Add(new TextBlock
        {
            Text = $"Mengunduh {update.FileName}...\nAplikasi akan otomatis menutup dan memasang pembaruan.",
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(progressBar);
        panel.Children.Add(progressLabel);

        var downloadDialog = new ContentDialog
        {
            Title = "Mengunduh Pembaruan",
            Content = panel,
            XamlRoot = Content.XamlRoot
        };

        // Dialog tampil non-blocking agar unduhan berjalan bersamaan
        _ = downloadDialog.ShowAsync();

        var progressReporter = new Progress<double>(percent =>
        {
            progressBar.Value = percent;
            progressLabel.Text = $"Mengunduh... {percent:F0}%";
        });

        try
        {
            SetStatus($"● Mengunduh pembaruan {update.VersionTag}...");
            await _updateService.DownloadAndInstallAsync(update, progressReporter);
        }
        catch (Exception ex)
        {
            downloadDialog.Hide();
            SetStatus("● Gagal mengunduh pembaruan.");

            var errorDialog = new ContentDialog
            {
                Title = "Gagal Memperbarui",
                Content = $"Terjadi kesalahan saat mengunduh pembaruan:\n{ex.Message}",
                CloseButtonText = "Tutup",
                XamlRoot = Content.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }

    private void RibbonTabs_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        // Event ini dapat terpicu saat InitializeComponent, sebelum RootFrame/StatusText terisi
        if (RootFrame is null || StatusText is null) return;

        if (sender.SelectedItem is not SelectorBarItem selected) return;

        SetStatus($"● Tab aktif: {selected.Text} | Versi: v{GitHubUpdateService.GetCurrentVersionString()} | Server: {ApiService.BaseServerUrl}");

        switch (selected.Tag?.ToString())
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

    private async void OnNewClick(object sender, RoutedEventArgs e)
    {
        // Di halaman Periods, tombol "Baru" membuka dialog Buka Periode Baru
        if (RootFrame.Content is PeriodsPage periodsPage && Content?.XamlRoot != null)
        {
            SetStatus("● Membuka form periode baru...");
            var dialog = new OpenNewPeriodDialog
            {
                XamlRoot = Content.XamlRoot,
                OnSubmitAsync = async payload =>
                {
                    await periodsPage.ViewModel.CreatePeriodCommand.ExecuteAsync(payload);
                }
            };
            await dialog.ShowAsync();
            return;
        }

        SetStatus("● Membuat entri baru...");
    }

    private async void OnSyncClick(object sender, RoutedEventArgs e)
    {
        SetStatus($"● Menghubungi server {ApiService.BaseServerUrl} (mohon tunggu)...");

        bool isOnline = await _api.CheckServerHealthAsync("");

        if (isOnline)
        {
            if (RootFrame.Content is PeriodsPage periodsPage)
            {
                await periodsPage.ViewModel.FetchPeriodsCommand.ExecuteAsync(null);
            }

            SetStatus($"● Terhubung ke Server ({ApiService.BaseServerUrl}) | Versi: v{GitHubUpdateService.GetCurrentVersionString()}");
        }
        else
        {
            SetStatus("● Server tidak merespons atau masih dalam proses bangun (cold start).");
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        SetStatus("● Mengirim data ke server...");

        var payload = new
        {
            Client = "AumoDekstop",
            Timestamp = DateTime.UtcNow
        };

        var result = await _api.PostAsync<object, object>("api/data", payload);

        SetStatus(result.IsSuccess
            ? "● Data berhasil disimpan ke server."
            : $"● Gagal menyimpan: {result.ErrorMessage}");
    }
}
