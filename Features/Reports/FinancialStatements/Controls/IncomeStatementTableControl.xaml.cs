
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AumoDekstop.Features.Reports.FinancialStatements.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.Reports.FinancialStatements.Controls;

public sealed partial class IncomeStatementTableControl : UserControl, INotifyPropertyChanged
{
    private static readonly IncomeStatementData EmptyData = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public IncomeStatementTableControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(IncomeStatementData),
            typeof(IncomeStatementTableControl),
            new PropertyMetadata(null, OnDataChanged));

    public IncomeStatementData? Data
    {
        get => (IncomeStatementData?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public IncomeStatementData SafeData => Data ?? EmptyData;

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IncomeStatementTableControl control)
        {
            control.OnPropertyChanged(nameof(SafeData));
        }
    }
}