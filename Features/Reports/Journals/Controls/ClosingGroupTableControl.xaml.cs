namespace AumoDekstop.Features.Reports.Journals.Controls;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;

public record ClosingJournalLine(
    string? ReferenceNumber,
    string AccountName,
    decimal Debit,
    decimal Credit
);

public record ClosingJournalEntryGroup(
    string Description,
    IReadOnlyList<ClosingJournalLine>? Lines
);

public record JournalTotals(
    decimal TotalDebit,
    decimal TotalCredit
);

public sealed class ClosingJournalLineDisplayItem
{
    public ClosingJournalLineDisplayItem(ClosingJournalLine source, Func<decimal, string> numberFormatter)
    {
        FormattedReference = decimal.TryParse(source.ReferenceNumber, NumberStyles.Any, CultureInfo.InvariantCulture, out var refNum) && refNum != 0m
            ? source.ReferenceNumber!
            : "-";

        AccountName = source.AccountName;
        IsCreditLine = source.Credit > 0m;
        AccountMargin = IsCreditLine ? new Thickness(24, 0, 0, 0) : new Thickness(0);
        AccountFontWeight = IsCreditLine ? FontWeights.Normal : FontWeights.Medium;

        FormattedDebit = source.Debit > 0m ? numberFormatter(source.Debit) : "-";
        FormattedCredit = source.Credit > 0m ? numberFormatter(source.Credit) : "-";
    }

    public string FormattedReference { get; }
    public string AccountName { get; }
    public bool IsCreditLine { get; }
    public Thickness AccountMargin { get; }
    public FontWeight AccountFontWeight { get; }
    public string FormattedDebit { get; }
    public string FormattedCredit { get; }

    public Brush AccountForeground =>
        (Brush)Application.Current.Resources[
            IsCreditLine ? "TextFillColorSecondaryBrush" : "TextFillColorPrimaryBrush"
        ];
}

public sealed partial class ClosingGroupTableControl : UserControl
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    public ClosingGroupTableControl()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.Register(
            nameof(Group),
            typeof(ClosingJournalEntryGroup),
            typeof(ClosingGroupTableControl),
            new PropertyMetadata(null, OnDataChanged));

    public ClosingJournalEntryGroup? Group
    {
        get => (ClosingJournalEntryGroup?)GetValue(GroupProperty);
        set => SetValue(GroupProperty, value);
    }

    public static readonly DependencyProperty TotalsProperty =
        DependencyProperty.Register(
            nameof(Totals),
            typeof(JournalTotals),
            typeof(ClosingGroupTableControl),
            new PropertyMetadata(new JournalTotals(0m, 0m), OnDataChanged));

    public JournalTotals Totals
    {
        get => (JournalTotals)GetValue(TotalsProperty);
        set => SetValue(TotalsProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public string GroupDescription => Group?.Description ?? string.Empty;

    public IReadOnlyList<ClosingJournalLineDisplayItem> DisplayLines =>
        Group?.Lines?
            .Select(line => new ClosingJournalLineDisplayItem(line, FormatNumberWithParen))
            .ToList()
        ?? [];

    public string FormattedTotalDebit => FormatNumberWithParen(Totals?.TotalDebit ?? 0m);

    public string FormattedTotalCredit => FormatNumberWithParen(Totals?.TotalCredit ?? 0m);

    #endregion

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ClosingGroupTableControl control)
        {
            control.Bindings.Update();
        }
    }

    // Nanti bisa diganti memanggil helper dari lib/format setelah dikonversi
    private static string FormatNumberWithParen(decimal value)
    {
        if (value < 0m)
        {
            return $"({Math.Abs(value).ToString("N0", IdCulture)})";
        }

        return value.ToString("N0", IdCulture);
    }
}