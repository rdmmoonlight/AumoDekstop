using System;
using System.Threading.Tasks;
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
    
        this.Activated += MainWindow_Activated;
    }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        // Pastikan pengecekan hanya berjalan sekali saat aplikasi pertama kali dibuka
        if (_hasCheckedUpdate || args.WindowActivationState == WindowActivationState.Deactivated)
            return;

        _hasCheckedUpdate = true;
        await CheckAndPromptUpdateAsync();
    }

    private async Task CheckAndPromptUpdateAsync()
    {
        // Pastikan Content dan XamlRoot sudah siap sebelum menampilkan dialog
        if (this.Content?.XamlRoot == null)
            return;

        var update = await _updateService.CheckForUpdateAsync();
        if (update == null)
            return;

        var confirmDialog = new ContentDialog
        {
            Title = $"Pembaruan Tersedia ({update.VersionTag})",
            Content = new ScrollViewer
            {
                MaxHeight = 250,
                Content = new TextBlock
                {
                    Text = $"Versi baru AumoDekstop siap diunduh.\n\nCatatan Rilis:\n{update.ReleaseNotes}",
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
            StatusText.Text = $"● Tab aktif: {selected.Text} | Server: https://aumo.onrender.com";
            // Nantinya di sini kita arahkan RootFrame.Navigate(typeof(...)) sesuai tab
        }
    }

    private void OnNewClick(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "● Membuat entri baru...";
    }

// Di dalam MainWindow.xaml.cs

private async void OnSyncClick(object sender, RoutedEventArgs e)
{
    StatusText.Text = "● Menghubungi server https://aumo.onrender.com (mohon tunggu)...";

    // Ganti "api/health" atau "" sesuai endpoint root/health backend Anda
    bool isOnline = await ApiService.Instance.CheckServerHealthAsync("");

    if (isOnline)
    {
        StatusText.Text = $"● Terhubung ke Server ({ApiService.BaseServerUrl}) | Versi: {GitHubUpdateService.GetCurrentVersionString()}";
    }
    else
    {
        StatusText.Text = "● Server tidak merespons atau masih dalam proses bangun (cold start).";
    }
}

private async void OnSaveClick(object sender, RoutedEventArgs e)
{
    StatusText.Text = "● Mengirim data ke server...";

    // Contoh payload yang dikirim ke endpoint POST /api/data
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
