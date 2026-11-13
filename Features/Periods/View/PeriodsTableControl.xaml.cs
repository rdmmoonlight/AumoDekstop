using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AumoDekstop.Features.Periods.Models;

namespace AumoDekstop.Features.Periods.Views;

public sealed partial class PeriodsTableControl : UserControl
{
    public static readonly DependencyProperty PeriodsProperty =
        DependencyProperty.Register(
            nameof(Periods),
            typeof(IEnumerable<PeriodItem>),
            typeof(PeriodsTableControl),
            new PropertyMetadata(null, OnPeriodsChanged));

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(PeriodsTableControl),
            new PropertyMetadata(false, OnStatePropertyChanged));

    public IEnumerable<PeriodItem>? Periods
    {
        get => (IEnumerable<PeriodItem>?)GetValue(PeriodsProperty);
        set => SetValue(PeriodsProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public event EventHandler<int>? ClosePeriodRequested;
    public event EventHandler<int>? SelectPeriodRequested;

    public bool ShowLoadingState => IsLoading;
    public bool ShowEmptyState => !IsLoading && (Periods is null || !Periods.Any());
    public bool ShowListState => !IsLoading && Periods is not null && Periods.Any();

    public PeriodsTableControl()
    {
        InitializeComponent();
    }

    private static void OnPeriodsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PeriodsTableControl control) return;

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
