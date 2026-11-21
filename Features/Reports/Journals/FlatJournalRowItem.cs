namespace AumoDekstop.Features.Reports.Journals;

using System;
using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Text;

public class FlatJournalRowItem
{
    public Guid EntryId { get; set; }
    public bool ShowHeader { get; set; }
    public string FormattedDate { get; set; } = string.Empty;
    public int GroupIdx { get; set; }
    public bool IsFirstLine { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? AccountName { get; set; }
    public string? LineDescription { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public object? OriginalEntry { get; set; }

    // --- Helper Properti untuk x:Bind di XAML ---

    public string CreatedAtText => $"Dibuat: {CreatedAt:dd MMM yyyy HH:mm}";
    public string UpdatedAtText => UpdatedAt.HasValue ? $"Diubah: {UpdatedAt.Value:dd MMM yyyy HH:mm}" : string.Empty;
    public Visibility UpdatedAtVisibility => UpdatedAt.HasValue ? Visibility.Visible : Visibility.Collapsed;

    public Visibility FirstLineVisibility => IsFirstLine ? Visibility.Visible : Visibility.Collapsed;

    public string AccountDisplay => AccountName ?? string.Empty;
    public Thickness AccountMargin => Debit > 0 ? new Thickness(0) : new Thickness(24, 0, 0, 0);
    public FontWeight AccountFontWeight => Debit > 0 ? FontWeights.SemiBold : FontWeights.Normal;
    public Brush AccountForeground => Debit > 0
        ? (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
        : (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];

    public string DescriptionDisplay => string.IsNullOrWhiteSpace(LineDescription) ? "-" : LineDescription;
    public string ReferenceDisplay => string.IsNullOrWhiteSpace(ReferenceNumber) ? "-" : ReferenceNumber;

    public string DebitDisplay => Debit > 0 ? Debit.ToString("N2", new CultureInfo("id-ID")) : "-";
    public string CreditDisplay => Credit > 0 ? Credit.ToString("N2", new CultureInfo("id-ID")) : "-";

    // Alternating row background (GroupIdx % 2 == 0)
    public Brush RowBackground => GroupIdx % 2 == 0
        ? new SolidColorBrush(Color.FromArgb(12, 255, 255, 255))
        : new SolidColorBrush(Colors.Transparent);
}