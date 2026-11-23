namespace AumoDekstop.Features.Reports.FinancialStatements.Controls;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using AumoDekstop.Features.Reports.FinancialStatements.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class FinancialPositionTableControl : UserControl, INotifyPropertyChanged
{
    private static readonly FinancialPositionData EmptyData = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public FinancialPositionTableControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(FinancialPositionData),
            typeof(FinancialPositionTableControl),
            new PropertyMetadata(null, OnDataChanged));

    public FinancialPositionData? Data
    {
        get => (FinancialPositionData?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public FinancialPositionData SafeData => Data ?? EmptyData;

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FinancialPositionTableControl control)
        {
            control.OnPropertyChanged(nameof(SafeData));
        }
    }
}