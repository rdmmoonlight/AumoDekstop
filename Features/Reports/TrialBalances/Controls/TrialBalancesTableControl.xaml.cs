namespace AumoDekstop.Features.Reports.TrialBalances.Controls;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AumoDekstop.Features.Reports.TrialBalances.Helpers;
using AumoDekstop.Features.Reports.TrialBalances.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class TrialBalancesTableControl : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public TrialBalancesTableControl()
    {
        InitializeComponent();
        Rows = new ObservableCollection<TrialRow>();
    }

    #region Dependency Properties (Pengganti Vue defineProps)

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register(
            nameof(Rows),
            typeof(IList<TrialRow>),
            typeof(TrialBalancesTableControl),
            new PropertyMetadata(null, OnRowsPropertyChanged));

    public IList<TrialRow> Rows
    {
        get => (IList<TrialRow>)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public static readonly DependencyProperty TotalDebitProperty =
        DependencyProperty.Register(
            nameof(TotalDebit),
            typeof(decimal),
            typeof(TrialBalancesTableControl),
            new PropertyMetadata(0m, OnTotalsPropertyChanged));

    public decimal TotalDebit
    {
        get => (decimal)GetValue(TotalDebitProperty);
        set => SetValue(TotalDebitProperty, value);
    }

    public static readonly DependencyProperty TotalCreditProperty =
        DependencyProperty.Register(
            nameof(TotalCredit),
            typeof(decimal),
            typeof(TrialBalancesTableControl),
            new PropertyMetadata(0m, OnTotalsPropertyChanged));

    public decimal TotalCredit
    {
        get => (decimal)GetValue(TotalCreditProperty);
        set => SetValue(TotalCreditProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public IList<TrialRow> DisplayRows => Rows ?? [];

    public bool HasRows => Rows is { Count: > 0 };

    public bool IsEmpty => !HasRows;

    public string FormattedTotalDebit => TrialBalanceFormatter.FormatNumber(TotalDebit);

    public string FormattedTotalCredit => TrialBalanceFormatter.FormatNumber(TotalCredit);

    #endregion

    private static void OnRowsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TrialBalancesTableControl control)
        {
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= control.Rows_CollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += control.Rows_CollectionChanged;
            }

            control.NotifyRowsChanged();
        }
    }

    private static void OnTotalsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TrialBalancesTableControl control)
        {
            control.OnPropertyChanged(nameof(FormattedTotalDebit));
            control.OnPropertyChanged(nameof(FormattedTotalCredit));
        }
    }

    private void Rows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyRowsChanged();
    }

    private void NotifyRowsChanged()
    {
        OnPropertyChanged(nameof(DisplayRows));
        OnPropertyChanged(nameof(HasRows));
        OnPropertyChanged(nameof(IsEmpty));
    }
}