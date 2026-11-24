
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AumoDekstop.Features.Reports.FinancialStatements.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.Reports.FinancialStatements.Controls;

public sealed partial class RetainedEarningsTableControl : UserControl, INotifyPropertyChanged
{
    private static readonly RetainedEarningsData EmptyData = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public RetainedEarningsTableControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(RetainedEarningsData),
            typeof(RetainedEarningsTableControl),
            new PropertyMetadata(null, OnDataChanged));

    public RetainedEarningsData? Data
    {
        get => (RetainedEarningsData?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public RetainedEarningsData SafeData => Data ?? EmptyData;

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RetainedEarningsTableControl control)
        {
            control.OnPropertyChanged(nameof(SafeData));
        }
    }
}