using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AumoDekstop.Features.Periods.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.Periods.Controls;

public sealed partial class PeriodsTableControl : UserControl
{
    public static readonly DependencyProperty PeriodsProperty =
        DependencyProperty.Register(
            nameof(Periods),
            typeof(object),
            typeof(PeriodsTableControl),
            new PropertyMetadata(null, OnPeriodsChanged));

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(PeriodsTableControl),
            new PropertyMetadata(false, OnStatePropertyChanged));

    // Bertipe object (seperti ItemsControl.ItemsSource): DP bertipe IEnumerable<T> dengan T kelas sendiri
    // gagal di-marshal oleh WinUI pada build Native AOT ("No such interface supported").
    public object? Periods
    {
        get => GetValue(PeriodsProperty);
        set => SetValue(PeriodsProperty, value);
    }

    private IEnumerable<PeriodItem>? PeriodItems => Periods as IEnumerable<PeriodItem>;

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public event EventHandler<int>? ClosePeriodRequested;
    public event EventHandler<int>? SelectPeriodRequested;

    public bool ShowLoadingState => IsLoading;
    public bool ShowEmptyState => !IsLoading && (PeriodItems is null || !PeriodItems.Any());
    public bool ShowListState => !IsLoading && PeriodItems is not null && PeriodItems.Any();

    public PeriodsTableControl()
    {
        InitializeComponent();
    }

    private static void OnPeriodsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PeriodsTableControl control)
        {
            return;
        }

        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= control.OnCollectionChanged;
        }

        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += control.OnCollectionChanged;
        }

        control.Bindings.Update();
    }

    private static void OnStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PeriodsTableControl control)
        {
            control.Bindings.Update();
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Bindings.Update();
    }

    private void ClosePeriodMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: int periodId })
        {
            ClosePeriodRequested?.Invoke(this, periodId);
        }
    }

    private void SelectPeriodMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: int periodId })
        {
            SelectPeriodRequested?.Invoke(this, periodId);
        }
    }
}
