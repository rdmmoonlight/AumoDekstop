namespace AumoDekstop.Shell.Updates;

using AumoDekstop.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>Pengecekan, dialog, dan pemasangan pembaruan aplikasi.</summary>
public sealed class UpdateCoordinator
{
    private readonly GitHubUpdateService _updateService = new();
    private readonly Action<string> _setStatus;

    public UpdateCoordinator(Action<string> setStatus)
    {
        _setStatus = setStatus;
    }

    public async Task CheckAndPromptAsync(XamlRoot? xamlRoot, bool isManualCheck)
    {
        if (xamlRoot == null)
            return;

        string currentVer = GitHubUpdateService.GetCurrentVersionString();

        if (isManualCheck)
        {
            _setStatus($"● Mengecek pembaruan di GitHub (Versi saat ini: v{currentVer})...");
        }

        var update = await GitHubUpdateService.CheckForUpdateAsync();

        if (update == null)
        {
            if (isManualCheck)
            {
                _setStatus($"● Aplikasi sudah versi terbaru (v{currentVer}).");

                var infoDialog = new ContentDialog
                {
                    Title = "Tidak Ada Pembaruan",
                    Content = $"Anda sudah menggunakan versi terbaru (v{currentVer}).",
                    CloseButtonText = "OK",
                    XamlRoot = xamlRoot
                };
                await infoDialog.ShowAsync();
            }
            return;
        }

        _setStatus($"● Pembaruan ditemukan: {update.VersionTag}!");

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
            XamlRoot = xamlRoot
        };

        var result = await confirmDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await ShowDownloadProgressAndInstallAsync(update, xamlRoot);
        }
    }

    private async Task ShowDownloadProgressAndInstallAsync(UpdateInfo update, XamlRoot xamlRoot)
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
            XamlRoot = xamlRoot
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
            _setStatus($"● Mengunduh pembaruan {update.VersionTag}...");
            await GitHubUpdateService.DownloadAndInstallAsync(update, progressReporter);
        }
        catch (Exception ex)
        {
            downloadDialog.Hide();
            _setStatus("● Gagal mengunduh pembaruan.");

            var errorDialog = new ContentDialog
            {
                Title = "Gagal Memperbarui",
                Content = $"Terjadi kesalahan saat mengunduh pembaruan:\n{ex.Message}",
                CloseButtonText = "Tutup",
                XamlRoot = xamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}
