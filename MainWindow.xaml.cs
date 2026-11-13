using System;
using System.Threading.Tasks;
using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop;

public sealed partial class MainWindow : Window
{
    private bool _hasCheckedUpdate = false;
    private readonly GitHubUpdateService _updateService = new();

    public MainWindow()
    {
        this.InitializeComponent();
        GitHubUpdateService.CleanupOldExecutable();

        // Gunakan event Loaded pada Content agar XamlRoot dijamin sudah siap 100%
        if (this.Content is FrameworkElement rootElement)
        {
            rootElement.Loaded += RootElement_Loaded;
        }
    }

    private async void RootElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (_hasCheckedUpdate) return;
        _hasCheckedUpdate = true;

        // Tampilkan versi aktif di StatusBar saat pertama kali terbuka
        string currentVer = GitHubUpdateService.GetCurrentVersionString();
        StatusText.Text = $"● Siap | Versi: v{currentVer} | Server: {ApiService.BaseServerUrl}";

        await CheckAndPromptUpdateAsync(isManualCheck: false);
    }

    /// <summary>
    /// Handler untuk tombol manual "Cek Update" di toolbar/menu Anda.
    /// </summary>
    private async void OnCheckUpdateClick(object sender, RoutedEventArgs e)
    {
        await CheckAndPromptUpdateAsync(isManualCheck: true);
    }

    private async Task CheckAndPromptUpdateAsync(bool isManualCheck)
    {
        if (this.Content?.XamlRoot == null)
            return;

        string currentVer = GitHubUpdateService.GetCurrentVersionString();

        if (isManualCheck)
        {
            StatusText.Text = $"● Mengecek pembaruan di GitHub (Versi saat ini: v{currentVer})...";
        }

        var update = await _updateService.CheckForUpdateAsync();

        if (update == null)
        {
            if (isManualCheck)
            {
                StatusText.Text = $"● Aplikasi sudah versi terbaru (v{currentVer}).";
                var infoDialog = new ContentDialog
                {
                    Title = "Tidak Ada Pembaruan",
                    Content = $"Anda sudah menggunakan versi terbaru (v{currentVer}).",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await infoDialog.ShowAsync();
            }
            return;
        }

        StatusText.Text = $"● Pembaruan ditemukan: {update.VersionTag}!";

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
            XamlRoot = this.Content.XamlRoot
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
            XamlRoot = this.Content.XamlRoot
        };

        // Tampilkan dialog secara non-blocking agar proses download bisa berjalan bersamaan
        var dialogTask = downloadDialog.ShowAsync();

        var progressReporter = new Progress<double>(percent =>
        {
            progressBar.Value = percent;
            progressLabel.Text = $"Mengunduh... {percent:F0}%";
        });

        try
        {
            StatusText.Text = $"● Mengunduh pembaruan {update.VersionTag}...";
            await _updateService.DownloadAndInstallAsync(update, progressReporter);
        }
        catch (Exception ex)
        {
            downloadDialog.Hide();
            StatusText.Text = "● Gagal mengunduh pembaruan.";

            var errorDialog = new ContentDialog
            {
                Title = "Gagal Memperbarui",
                Content = $"Terjadi kesalahan saat mengunduh pembaruan:\n{ex.Message}",
                CloseButtonText = "Tutup",
                XamlRoot = this.Content.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }

    private void RibbonTabs_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is SelectorBarItem selected)
        {
            string currentVer = GitHubUpdateService.GetCurrentVersionString();
            StatusText.Text = $"● Tab aktif: {selected.Text} | Versi: v{currentVer} | Server: {ApiService.BaseServerUrl}";

            switch (selected.Tag?.ToString())
            {
                case "periods":
                    if (RootFrame.CurrentSourcePageType != typeof(PeriodsPage))
                    {
                        RootFrame.Navigate(typeof(PeriodsPage));
                    }
                    break;

                // Tambahkan case untuk tab lainnya ("home", "data", "report", "settings") di sini
            }
        }
    }

    private async void OnNewClick(object sender, RoutedEventArgs e)
    {
        // Jika sedang berada di halaman Periods, tombol "Baru" di Ribbon langsung membuka dialog Buka Periode Baru
        if (RootFrame.Content is PeriodsPage periodsPage && this.Content?.XamlRoot != null)
        {
            StatusText.Text = "● Membuka form periode baru...";
            var dialog = new OpenNewPeriodDialog
            {
                XamlRoot = this.Content.XamlRoot,
                OnSubmitAsync = async payload =>
                {
                    await periodsPage.ViewModel.CreatePeriodCommand.ExecuteAsync(payload);
                }
            };
            await dialog.ShowAsync();
            return;
        }

        StatusText.Text = "● Membuat entri baru...";
    }

    private async void OnSyncClick(object sender, RoutedEventArgs e)
    {
        StatusText.Text = $"● Menghubungi server {ApiService.BaseServerUrl} (mohon tunggu)...";

        bool isOnline = await ApiService.Instance.CheckServerHealthAsync("");

        if (isOnline)
        {
            // Jika halaman Periods sedang terbuka, ikut segarkan daftar periodenya
            if (RootFrame.Content is PeriodsPage periodsPage)
            {
                await periodsPage.ViewModel.FetchPeriodsCommand.ExecuteAsync(null);
            }

            StatusText.Text = $"● Terhubung ke Server ({ApiService.BaseServerUrl}) | Versi: v{GitHubUpdateService.GetCurrentVersionString()}";
        }
        else
        {
            StatusText.Text = "● Server tidak merespons atau masih dalam proses bangun (cold start).";
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "● Mengirim data ke server...";

        var payload = new
        {
            Client = "AumoDekstop",
            Timestamp = DateTime.UtcNow
        };

        var result = await ApiService.Instance.PostAsync<object, object>("api/data", payload);

        if (result.IsSuccess)
        {
            StatusText.Text = "● Data berhasil disimpan ke server.";
        }
        else
        {
            StatusText.Text = $"● Gagal menyimpan: {result.ErrorMessage}";
        }
    }
}
