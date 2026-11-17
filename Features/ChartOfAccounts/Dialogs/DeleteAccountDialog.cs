using AumoDekstop.Features.ChartOfAccounts.Models;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AumoDekstop.Features.ChartOfAccounts.Dialogs;

public static class DeleteAccountDialog
{
    public static async Task<bool> ShowAsync(XamlRoot xamlRoot, Account account)
    {
        var descriptionText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Brush
        };

        descriptionText.Inlines.Add(new Run { Text = "Tindakan ini tidak dapat dibatalkan. Akun " });
        descriptionText.Inlines.Add(new Run
        {
            Text = account.AccountName,
            FontWeight = FontWeights.SemiBold,
            Foreground = Application.Current.Resources["TextFillColorPrimaryBrush"] as Brush
        });
        descriptionText.Inlines.Add(new Run { Text = " akan dihapus secara permanen dari sistem." });

        // Gaya tombol merah (Destructive) untuk PrimaryButton
        var deleteButtonStyle = new Style(typeof(Button));
        deleteButtonStyle.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromArgb(255, 196, 43, 28))));
        deleteButtonStyle.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Colors.White)));

        var dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = "Apakah Anda yakin?",
            Content = descriptionText,
            PrimaryButtonText = "Hapus",
            CloseButtonText = "Batal",
            DefaultButton = ContentDialogButton.Close,
            PrimaryButtonStyle = deleteButtonStyle
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}