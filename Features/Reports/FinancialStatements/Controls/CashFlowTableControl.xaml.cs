
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AumoDekstop.Features.Reports.FinancialStatements.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.Reports.FinancialStatements.Controls;

public sealed partial class CashFlowTableControl : UserControl, INotifyPropertyChanged
{
    private static readonly CashFlowData EmptyData = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public CashFlowTableControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(CashFlowData),
            typeof(CashFlowTableControl),
            new PropertyMetadata(null, OnDataChanged));

    public CashFlowData? Data
    {
        get => (CashFlowData?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public CashFlowData SafeData => Data ?? EmptyData;

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CashFlowTableControl control)
        {
            control.OnPropertyChanged(nameof(SafeData));
        }
    }
}