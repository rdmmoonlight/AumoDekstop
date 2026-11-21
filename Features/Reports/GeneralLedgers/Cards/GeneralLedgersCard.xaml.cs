namespace AumoDekstop.Features.Reports.GeneralLedgers.Cards;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class GeneralLedgersCard : UserControl
{
    public static readonly DependencyProperty AccountProperty =
        DependencyProperty.Register(
            nameof(Account),
            typeof(LedgerAccountGroup),
            typeof(GeneralLedgersCard),
            new PropertyMetadata(null, OnAccountChanged));

    public LedgerAccountGroup? Account
    {
        get => (LedgerAccountGroup?)GetValue(AccountProperty);
        set => SetValue(AccountProperty, value);
    }

    public static readonly DependencyProperty DefaultExpandedProperty =
        DependencyProperty.Register(
            nameof(DefaultExpanded),
            typeof(bool),
            typeof(GeneralLedgersCard),
            new PropertyMetadata(true, OnDefaultExpandedChanged));

    public bool DefaultExpanded
    {
        get => (bool)GetValue(DefaultExpandedProperty);
        set => SetValue(DefaultExpandedProperty, value);
    }

    private bool _isExpanded = true;

    public GeneralLedgersCard()
    {
        InitializeComponent();
        UpdateExpandedVisualState();
    }

    private static void OnAccountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersCard card)
        {
            card.BindAccountData(e.NewValue as LedgerAccountGroup);
        }
    }

    private static void OnDefaultExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersCard card && e.NewValue is bool defaultExpanded)
        {
            card._isExpanded = defaultExpanded;
            card.UpdateExpandedVisualState();
        }
    }

    private void BindAccountData(LedgerAccountGroup? account)
    {
        if (account is null)
        {
            AccountCodeRun.Text = string.Empty;
            AccountNameRun.Text = string.Empty;
            TransactionCountText.Text = "0 Transaksi";
            EndingBalanceText.Text = "Rp 0";
            TableHost.Content = null;
            return;
        }

        AccountCodeRun.Text = account.AccountCode;
        AccountNameRun.Text = account.AccountName;
        TransactionCountText.Text = $"{account.Entries.Count} Transaksi";
        EndingBalanceText.Text = account.FormattedEndingBalance;

        // Hubungkan ke GeneralLedgersTable setelah file LedgerTable.vue dikonversi:
        TableHost.Content = new GeneralLedgersTable
        {
            Entries = account.Entries,
            BeginningBalance = account.BeginningBalance,
            EndingBalance = account.EndingBalance,
            TotalDebit = account.TotalDebit,
            TotalCredit = account.TotalCredit
        };
    }

    private void OnHeaderClicked(object sender, RoutedEventArgs e)
    {
        _isExpanded = !_isExpanded;
        UpdateExpandedVisualState();
    }

    private void UpdateExpandedVisualState()
    {
        ChevronRotateTransform.Angle = _isExpanded ? 90 : 0;
        BodyContainer.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
    }
}