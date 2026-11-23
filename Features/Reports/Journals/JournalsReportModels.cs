namespace AumoDekstop.Features.Reports.Journals;

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Text;

#region Formatter Helpers (Konversi dari format.ts)

public static class JournalFormatter
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    /// <summary>
    /// formatNumber: new Intl.NumberFormat("id-ID").format(Math.abs(n))
    /// </summary>
    public static string FormatNumber(decimal n)
    {
        return Math.Abs(n).ToString("N0", IdCulture);
    }

    /// <summary>
    /// formatNumberWithParen: format angka tanpa desimal, negatif dibungkus tanda kurung (1.000)
    /// </summary>
    public static string FormatNumberWithParen(decimal amount)
    {
        var formatted = Math.Abs(amount).ToString("N0", IdCulture);
        return amount < 0 ? $"({formatted})" : formatted;
    }

    /// <summary>
    /// formatDateDisplay: "dd MMM yyyy" (id-ID) atau "-" jika kosong
    /// </summary>
    public static string FormatDateDisplay(string? s)
    {
        if (string.IsNullOrWhiteSpace(s) || !DateTime.TryParse(s, out var dt))
            return "-";

        return dt.ToString("dd MMM yyyy", IdCulture);
    }

    public static string FormatDateDisplay(DateTime? dt)
    {
        return dt.HasValue ? dt.Value.ToString("dd MMM yyyy", IdCulture) : "-";
    }

    /// <summary>
    /// formatDateTimeDisplay: "dd MMM yyyy, HH:mm" (id-ID) atau null jika kosong
    /// </summary>
    public static string? FormatDateTimeDisplay(string? s)
    {
        if (string.IsNullOrWhiteSpace(s) || !DateTime.TryParse(s, out var dt))
            return null;

        return $"{dt.ToString("dd MMM yyyy", IdCulture)}, {dt:HH:mm}";
    }

    public static string? FormatDateTimeDisplay(DateTime? dt)
    {
        if (!dt.HasValue)
            return null;

        return $"{dt.Value.ToString("dd MMM yyyy", IdCulture)}, {dt.Value:HH:mm}";
    }
}

#endregion

#region Closing Journal Models

public class ClosingJournalLine
{
    public string? ReferenceNumber { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class ClosingJournalEntryGroup
{
    public string Description { get; set; } = string.Empty;
    public List<ClosingJournalLine> Lines { get; set; } = [];
}

public class ClosingJournalViewModel
{
    public decimal NetIncome { get; set; }
    public string RetainedEarningsAccountName { get; set; } = string.Empty;
    public List<ClosingJournalEntryGroup> Groups { get; set; } = [];
}

#endregion

#region General & Adjusting Journal Models

public class Account
{
    public int Id { get; set; }
    public int ReferenceNumber { get; set; }
    public string AccountName { get; set; } = string.Empty;
}

public class JournalLine
{
    public int Id { get; set; }
    public int LineOrder { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? LineDescription { get; set; }
    public string? AccountName { get; set; }
    public int? ReferenceNumber { get; set; }
    public Account? Account { get; set; }
}

public class JournalEntry
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string EntryDate { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
    public List<JournalLine> Lines { get; set; } = [];
    public bool? IsAdjusting { get; set; }
}

public class FlatJournalRow
{
    public int EntryId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string EntryDate { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
    public int LineId { get; set; }
    public int LineOrder { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? LineDescription { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public bool IsFirstLine { get; set; }
    public bool ShowHeader { get; set; }
    public string FormattedDate { get; set; } = string.Empty;
    public int GroupIdx { get; set; }
    public JournalEntry OriginalEntry { get; set; } = new();

    // --- Properti Terformat untuk x:Bind di GeneralJournalTable.xaml ---

    public string CreatedAtText =>
        $"Dibuat: {JournalFormatter.FormatDateTimeDisplay(CreatedAt) ?? "-"}";

    public string UpdatedAtText
    {
        get
        {
            var formatted = JournalFormatter.FormatDateTimeDisplay(UpdatedAt);
            return formatted is not null ? $"Diubah: {formatted}" : string.Empty;
        }
    }

    public Visibility UpdatedAtVisibility =>
        !string.IsNullOrWhiteSpace(UpdatedAt) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility FirstLineVisibility =>
        IsFirstLine ? Visibility.Visible : Visibility.Collapsed;

    public Thickness AccountMargin =>
        Debit > 0 ? new Thickness(0) : new Thickness(24, 0, 0, 0);

    public FontWeight AccountFontWeight =>
        Debit > 0 ? FontWeights.SemiBold : FontWeights.Normal;

    public Brush AccountForeground => Debit > 0
        ? (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
        : (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];

    public string DescriptionDisplay =>
        string.IsNullOrWhiteSpace(LineDescription) ? "-" : LineDescription;

    public string ReferenceDisplay =>
        string.IsNullOrWhiteSpace(ReferenceNumber) ? "-" : ReferenceNumber;

    public string DebitDisplay =>
        Debit > 0 ? JournalFormatter.FormatNumber(Debit) : "-";

    public string CreditDisplay =>
        Credit > 0 ? JournalFormatter.FormatNumber(Credit) : "-";

    public Brush RowBackground => GroupIdx % 2 == 0
        ? new SolidColorBrush(Color.FromArgb(12, 255, 255, 255))
        : new SolidColorBrush(Colors.Transparent);
}

#endregion