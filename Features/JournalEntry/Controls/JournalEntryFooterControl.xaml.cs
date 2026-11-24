
using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.JournalEntry.Controls;
public sealed partial class JournalEntryFooterControl : UserControl, INotifyPropertyChanged
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    public JournalEntryFooterControl()
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    #region Dependency Properties

    public static readonly DependencyProperty TotalDebitProperty =
        DependencyProperty.Register(
            nameof(TotalDebit),
            typeof(decimal),
            typeof(JournalEntryFooterControl),
            new PropertyMetadata(0m, OnFooterPropertyChanged));

    public decimal TotalDebit
    {
        get => (decimal)GetValue(TotalDebitProperty);
        set => SetValue(TotalDebitProperty, value);
    }

    public static readonly DependencyProperty TotalCreditProperty =
        DependencyProperty.Register(
            nameof(TotalCredit),
            typeof(decimal),
            typeof(JournalEntryFooterControl),
            new PropertyMetadata(0m, OnFooterPropertyChanged));

    public decimal TotalCredit
    {
        get => (decimal)GetValue(TotalCreditProperty);
        set => SetValue(TotalCreditProperty, value);
    }

    public static readonly DependencyProperty IsBalancedProperty =
        DependencyProperty.Register(
            nameof(IsBalanced),
            typeof(bool),
            typeof(JournalEntryFooterControl),
            new PropertyMetadata(true, OnFooterPropertyChanged));

    public bool IsBalanced
    {
        get => (bool)GetValue(IsBalancedProperty);
        set => SetValue(IsBalancedProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public bool IsUnbalanced => !IsBalanced;

    public string FormattedTotalDebit => FormatCurrency(TotalDebit);

    public string FormattedTotalCredit => FormatCurrency(TotalCredit);

    public string DifferenceText =>
        $"Selisih: {FormatCurrency(Math.Abs(TotalDebit - TotalCredit))}";

    #endregion

    private static void OnFooterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is JournalEntryFooterControl control)
        {
            control.PropertyChanged?.Invoke(control, new PropertyChangedEventArgs(nameof(IsUnbalanced)));
            control.PropertyChanged?.Invoke(control, new PropertyChangedEventArgs(nameof(FormattedTotalDebit)));
            control.PropertyChanged?.Invoke(control, new PropertyChangedEventArgs(nameof(FormattedTotalCredit)));
            control.PropertyChanged?.Invoke(control, new PropertyChangedEventArgs(nameof(DifferenceText)));
        }
    }

    // Nanti bisa diganti memanggil JournalEntryHelpers.FormatCurrency() setelah helpers.ts dikonversi
    private static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C0", IdCulture);
    }
}