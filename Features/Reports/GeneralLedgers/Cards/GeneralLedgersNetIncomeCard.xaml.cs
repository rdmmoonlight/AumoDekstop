
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AumoDekstop.Features.Reports.GeneralLedgers;

public sealed partial class GeneralLedgersNetIncomeCard : UserControl
{
    public static readonly DependencyProperty TotalRevenueProperty =
        DependencyProperty.Register(
            nameof(TotalRevenue),
            typeof(decimal),
            typeof(GeneralLedgersNetIncomeCard),
            new PropertyMetadata(0m, OnValuesChanged));

    public decimal TotalRevenue
    {
        get => (decimal)GetValue(TotalRevenueProperty);
        set => SetValue(TotalRevenueProperty, value);
    }

    public static readonly DependencyProperty TotalExpenseProperty =
        DependencyProperty.Register(
            nameof(TotalExpense),
            typeof(decimal),
            typeof(GeneralLedgersNetIncomeCard),
            new PropertyMetadata(0m, OnValuesChanged));

    public decimal TotalExpense
    {
        get => (decimal)GetValue(TotalExpenseProperty);
        set => SetValue(TotalExpenseProperty, value);
    }

    public static readonly DependencyProperty NetIncomeProperty =
        DependencyProperty.Register(
            nameof(NetIncome),
            typeof(decimal),
            typeof(GeneralLedgersNetIncomeCard),
            new PropertyMetadata(0m, OnValuesChanged));

    public decimal NetIncome
    {
        get => (decimal)GetValue(NetIncomeProperty);
        set => SetValue(NetIncomeProperty, value);
    }

    public GeneralLedgersNetIncomeCard()
    {
        this.InitializeComponent();
        UpdateDisplay();
    }

    private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersNetIncomeCard card)
        {
            card.UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        TotalRevenueText.Text = GeneralLedgerFormatter.FormatCurrency(TotalRevenue);
        TotalExpenseText.Text = GeneralLedgerFormatter.FormatCurrency(TotalExpense);
        NetIncomeValueText.Text = GeneralLedgerFormatter.FormatCurrency(NetIncome);

        var isProfit = NetIncome >= 0m;

        if (isProfit)
        {
            NetIncomeLabelText.Text = "LABA BERSIH (NET INCOME)";
            NetIncomeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(120, 16, 185, 129));   // Emerald border
            NetIncomeBorder.Background = new SolidColorBrush(Color.FromArgb(24, 16, 185, 129));     // Emerald tint
            NetIncomeLabelText.Foreground = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)); // Emerald text
            NetIncomeValueText.Foreground = new SolidColorBrush(Color.FromArgb(255, 52, 211, 153));
        }
        else
        {
            NetIncomeLabelText.Text = "RUGI BERSIH (NET LOSS)";
            NetIncomeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(120, 244, 63, 94));    // Rose border
            NetIncomeBorder.Background = new SolidColorBrush(Color.FromArgb(24, 244, 63, 94));      // Rose tint
            NetIncomeLabelText.Foreground = new SolidColorBrush(Color.FromArgb(255, 244, 63, 94));  // Rose text
            NetIncomeValueText.Foreground = new SolidColorBrush(Color.FromArgb(255, 251, 113, 133));
        }
    }
}