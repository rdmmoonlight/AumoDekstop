namespace AumoDekstop.Features.JournalEntry;

using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class JournalAlertsControl : UserControl
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    public JournalAlertsControl()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(
            nameof(ErrorMessage),
            typeof(string),
            typeof(JournalAlertsControl),
            new PropertyMetadata(null, OnAlertStateChanged));

    public string? ErrorMessage
    {
        get => (string?)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public static readonly DependencyProperty IsBalancedProperty =
        DependencyProperty.Register(
            nameof(IsBalanced),
            typeof(bool),
            typeof(JournalAlertsControl),
            new PropertyMetadata(true, OnAlertStateChanged));

    public bool IsBalanced
    {
        get => (bool)GetValue(IsBalancedProperty);
        set => SetValue(IsBalancedProperty, value);
    }

    public static readonly DependencyProperty HasLinesProperty =
        DependencyProperty.Register(
            nameof(HasLines),
            typeof(bool),
            typeof(JournalAlertsControl),
            new PropertyMetadata(false, OnAlertStateChanged));

    public bool HasLines
    {
        get => (bool)GetValue(HasLinesProperty);
        set => SetValue(HasLinesProperty, value);
    }

    public static readonly DependencyProperty TotalDebitProperty =
        DependencyProperty.Register(
            nameof(TotalDebit),
            typeof(decimal),
            typeof(JournalAlertsControl),
            new PropertyMetadata(0m, OnAlertStateChanged));

    public decimal TotalDebit
    {
        get => (decimal)GetValue(TotalDebitProperty);
        set => SetValue(TotalDebitProperty, value);
    }

    public static readonly DependencyProperty TotalCreditProperty =
        DependencyProperty.Register(
            nameof(TotalCredit),
            typeof(decimal),
            typeof(JournalAlertsControl),
            new PropertyMetadata(0m, OnAlertStateChanged));

    public decimal TotalCredit
    {
        get => (decimal)GetValue(TotalCreditProperty);
        set => SetValue(TotalCreditProperty, value);
    }

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(string),
            typeof(JournalAlertsControl),
            new PropertyMetadata("DRAFT", OnAlertStateChanged));

    public string Status
    {
        get => (string)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool IsUnbalancedVisible => !IsBalanced && HasLines;

    public string UnbalancedMessage =>
        $"Total Debit ({FormatCurrency(TotalDebit)}) dan Total Kredit ({FormatCurrency(TotalCredit)}) harus sama/seimbang sebelum diposting.";

    public bool IsPosted => Status == "POSTED";

    public bool IsCancelled => Status == "CANCELLED";

    #endregion

    private static void OnAlertStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is JournalAlertsControl control)
        {
            control.Bindings.Update();
        }
    }

    // Nanti bisa diganti memanggil JournalEntryHelpers.FormatCurrency() setelah helpers.ts dikonversi
    private static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C0", IdCulture);
    }
}