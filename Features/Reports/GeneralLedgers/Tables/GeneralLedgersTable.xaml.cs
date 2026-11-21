namespace AumoDekstop.Features.Reports.GeneralLedgers;

using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class GeneralLedgersTable : UserControl
{
    public static readonly DependencyProperty EntriesProperty =
        DependencyProperty.Register(
            nameof(Entries),
            typeof(IEnumerable<LedgerEntryItem>),
            typeof(GeneralLedgersTable),
            new PropertyMetadata(null, OnEntriesChanged));

    public IEnumerable<LedgerEntryItem>? Entries
    {
        get => (IEnumerable<LedgerEntryItem>?)GetValue(EntriesProperty);
        set => SetValue(EntriesProperty, value);
    }

    public static readonly DependencyProperty BeginningBalanceProperty =
        DependencyProperty.Register(
            nameof(BeginningBalance),
            typeof(decimal),
            typeof(GeneralLedgersTable),
            new PropertyMetadata(0m, OnSummaryValuesChanged));

    public decimal BeginningBalance
    {
        get => (decimal)GetValue(BeginningBalanceProperty);
        set => SetValue(BeginningBalanceProperty, value);
    }

    public static readonly DependencyProperty EndingBalanceProperty =
        DependencyProperty.Register(
            nameof(EndingBalance),
            typeof(decimal),
            typeof(GeneralLedgersTable),
            new PropertyMetadata(0m, OnSummaryValuesChanged));

    public decimal EndingBalance
    {
        get => (decimal)GetValue(EndingBalanceProperty);
        set => SetValue(EndingBalanceProperty, value);
    }

    public static readonly DependencyProperty TotalDebitProperty =
        DependencyProperty.Register(
            nameof(TotalDebit),
            typeof(decimal),
            typeof(GeneralLedgersTable),
            new PropertyMetadata(0m, OnSummaryValuesChanged));

    public decimal TotalDebit
    {
        get => (decimal)GetValue(TotalDebitProperty);
        set => SetValue(TotalDebitProperty, value);
    }

    public static readonly DependencyProperty TotalCreditProperty =
        DependencyProperty.Register(
            nameof(TotalCredit),
            typeof(decimal),
            typeof(GeneralLedgersTable),
            new PropertyMetadata(0m, OnSummaryValuesChanged));

    public decimal TotalCredit
    {
        get => (decimal)GetValue(TotalCreditProperty);
        set => SetValue(TotalCreditProperty, value);
    }

    public GeneralLedgersTable()
    {
        this.InitializeComponent();
    }

    private static void OnEntriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersTable table)
        {
            var items = (e.NewValue as IEnumerable<LedgerEntryItem>)?.ToList() ?? [];
            table.EntriesListView.ItemsSource = items;

            var isEmpty = items.Count == 0;
            table.EntriesListView.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
            table.EmptyStateBorder.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnSummaryValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersTable table)
        {
            table.BeginningBalanceText.Text = GeneralLedgerFormatter.FormatCurrency(table.BeginningBalance);
            table.TotalDebitText.Text = GeneralLedgerFormatter.FormatCurrency(table.TotalDebit);
            table.TotalCreditText.Text = GeneralLedgerFormatter.FormatCurrency(table.TotalCredit);
            table.EndingBalanceText.Text = GeneralLedgerFormatter.FormatCurrency(table.EndingBalance);
        }
    }
}