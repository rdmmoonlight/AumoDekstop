namespace AumoDekstop.Features.JournalEntry.Controls;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public record AccountOption(string Id, string Code, string Name)
{
    public string DisplayLabel => $"{Code} - {Name}";
}

public partial class JournalLineRowItem : ObservableObject
{
    [ObservableProperty]
    private int _rowNumber;

    [ObservableProperty]
    private string _accountId = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private decimal _debit;

    [ObservableProperty]
    private decimal _credit;

    public double DebitValue
    {
        get => (double)Debit;
        set
        {
            var sanitized = double.IsNaN(value) || value < 0 ? 0m : Convert.ToDecimal(value);
            if (Debit != sanitized)
            {
                Debit = sanitized;
                OnPropertyChanged();
            }
        }
    }

    public double CreditValue
    {
        get => (double)Credit;
        set
        {
            var sanitized = double.IsNaN(value) || value < 0 ? 0m : Convert.ToDecimal(value);
            if (Credit != sanitized)
            {
                Credit = sanitized;
                OnPropertyChanged();
            }
        }
    }

    partial void OnDebitChanged(decimal value) => OnPropertyChanged(nameof(DebitValue));

    partial void OnCreditChanged(decimal value) => OnPropertyChanged(nameof(CreditValue));
}

public sealed record JournalLineUpdatedEventArgs(int Index, string Field, object? Value);

public sealed partial class JournalLinesTableControl : UserControl
{
    public JournalLinesTableControl()
    {
        InitializeComponent();
        Lines = new ObservableCollection<JournalLineRowItem>();
        Accounts = Array.Empty<AccountOption>();
    }

    #region Events (Pengganti Vue emit)

    public event RoutedEventHandler? AddLineClicked;
    public event EventHandler<int>? RemoveLineClicked;
    public event EventHandler<JournalLineUpdatedEventArgs>? LineUpdated;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty LinesProperty =
        DependencyProperty.Register(
            nameof(Lines),
            typeof(ObservableCollection<JournalLineRowItem>),
            typeof(JournalLinesTableControl),
            new PropertyMetadata(null, OnLinesPropertyChanged));

    public ObservableCollection<JournalLineRowItem> Lines
    {
        get => (ObservableCollection<JournalLineRowItem>)GetValue(LinesProperty);
        set => SetValue(LinesProperty, value);
    }

    public static readonly DependencyProperty AccountsProperty =
        DependencyProperty.Register(
            nameof(Accounts),
            typeof(IList<AccountOption>),
            typeof(JournalLinesTableControl),
            new PropertyMetadata(null));

    public IList<AccountOption> Accounts
    {
        get => (IList<AccountOption>)GetValue(AccountsProperty);
        set => SetValue(AccountsProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(JournalLinesTableControl),
            new PropertyMetadata(false, OnStateChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty AddLineCommandProperty =
        DependencyProperty.Register(
            nameof(AddLineCommand),
            typeof(ICommand),
            typeof(JournalLinesTableControl),
            new PropertyMetadata(null));

    public ICommand? AddLineCommand
    {
        get => (ICommand?)GetValue(AddLineCommandProperty);
        set => SetValue(AddLineCommandProperty, value);
    }

    public static readonly DependencyProperty RemoveLineCommandProperty =
        DependencyProperty.Register(
            nameof(RemoveLineCommand),
            typeof(ICommand),
            typeof(JournalLinesTableControl),
            new PropertyMetadata(null));

    public ICommand? RemoveLineCommand
    {
        get => (ICommand?)GetValue(RemoveLineCommandProperty);
        set => SetValue(RemoveLineCommandProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public bool IsEditable => !IsReadOnly;

    public bool HasLines => Lines is { Count: > 0 };

    public bool IsEmpty => !HasLines;

    #endregion

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is JournalLinesTableControl control)
        {
            control.Bindings.Update();
        }
    }

    private static void OnLinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is JournalLinesTableControl control)
        {
            if (e.OldValue is ObservableCollection<JournalLineRowItem> oldCollection)
            {
                oldCollection.CollectionChanged -= control.Lines_CollectionChanged;
            }

            if (e.NewValue is ObservableCollection<JournalLineRowItem> newCollection)
            {
                newCollection.CollectionChanged += control.Lines_CollectionChanged;
                control.RefreshRowNumbers();
            }

            control.Bindings.Update();
        }
    }

    private void Lines_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshRowNumbers();
        Bindings.Update();
    }

    private void RefreshRowNumbers()
    {
        if (Lines is null) return;

        for (int i = 0; i < Lines.Count; i++)
        {
            Lines[i].RowNumber = i + 1;
        }
    }

    private void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        AddLineClicked?.Invoke(this, e);
        if (AddLineCommand?.CanExecute(null) == true)
        {
            AddLineCommand.Execute(null);
        }
    }

    private void RemoveLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: JournalLineRowItem item } && Lines is not null)
        {
            var index = Lines.IndexOf(item);
            if (index >= 0)
            {
                RemoveLineClicked?.Invoke(this, index);
                if (RemoveLineCommand?.CanExecute(index) == true)
                {
                    RemoveLineCommand.Execute(index);
                }
            }
        }
    }

    private void AccountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { DataContext: JournalLineRowItem item } && Lines is not null)
        {
            var index = Lines.IndexOf(item);
            if (index >= 0)
            {
                LineUpdated?.Invoke(this, new JournalLineUpdatedEventArgs(index, "accountId", item.AccountId));
            }
        }
    }

    private void DebitNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (sender.DataContext is JournalLineRowItem item && Lines is not null)
        {
            var index = Lines.IndexOf(item);
            if (index >= 0)
            {
                LineUpdated?.Invoke(this, new JournalLineUpdatedEventArgs(index, "debit", item.Debit));
            }
        }
    }

    private void CreditNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (sender.DataContext is JournalLineRowItem item && Lines is not null)
        {
            var index = Lines.IndexOf(item);
            if (index >= 0)
            {
                LineUpdated?.Invoke(this, new JournalLineUpdatedEventArgs(index, "credit", item.Credit));
            }
        }
    }
}