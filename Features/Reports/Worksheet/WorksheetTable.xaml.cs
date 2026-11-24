namespace AumoDekstop.Features.Reports.Worksheet;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class WorksheetTable : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty TotalsProperty =
        DependencyProperty.Register(
            nameof(Totals),
            typeof(WorksheetTotals),
            typeof(WorksheetTable),
            new PropertyMetadata(new WorksheetTotals(), OnTotalsChanged));

    public static readonly DependencyProperty NetIncomeProperty =
        DependencyProperty.Register(
            nameof(NetIncome),
            typeof(decimal),
            typeof(WorksheetTable),
            new PropertyMetadata(0m, OnNetIncomeChanged));

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<WorksheetRow> Rows { get; } = new();

    public WorksheetTotals Totals
    {
        get => (WorksheetTotals)GetValue(TotalsProperty);
        set => SetValue(TotalsProperty, value);
    }

    public decimal NetIncome
    {
        get => (decimal)GetValue(NetIncomeProperty);
        set => SetValue(NetIncomeProperty, value);
    }

    public Visibility EmptyStateVisibility => Rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public WorksheetTable()
    {
        this.InitializeComponent();
        Rows.CollectionChanged += (_, _) => OnPropertyChanged(nameof(EmptyStateVisibility));
    }

    public void SetRows(IEnumerable<WorksheetRow>? rows)
    {
        Rows.Clear();
        if (rows != null)
        {
            foreach (var row in rows)
            {
                Rows.Add(row);
            }
        }
    }

    private static void OnTotalsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WorksheetTable control)
        {
            if (e.NewValue is WorksheetTotals totals)
            {
                totals.NetIncome = control.NetIncome;
            }
            control.OnPropertyChanged(nameof(Totals));
        }
    }

    private static void OnNetIncomeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WorksheetTable control && control.Totals != null)
        {
            control.Totals.NetIncome = (decimal)e.NewValue;
            control.OnPropertyChanged(nameof(Totals));
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}