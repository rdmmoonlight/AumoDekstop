namespace AumoDekstop.Features.Reports.Journals;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
public sealed partial class GeneralJournalTable : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Pengganti emit('promptDelete', entry)
    /// </summary>
    public event EventHandler<object?>? PromptDelete;

    /// <summary>
    /// Pengganti RouterLink ke /journal-entry?id=...
    /// </summary>
    public event EventHandler<Guid>? EditRequested;

    public static readonly DependencyProperty FlatDataProperty =
        DependencyProperty.Register(
            nameof(FlatData),
            typeof(IEnumerable<FlatJournalRowItem>),
            typeof(GeneralJournalTable),
            new PropertyMetadata(null));

    public IEnumerable<FlatJournalRowItem>? FlatData
    {
        get => (IEnumerable<FlatJournalRowItem>?)GetValue(FlatDataProperty);
        set => SetValue(FlatDataProperty, value);
    }

    public static readonly DependencyProperty EditModeProperty =
        DependencyProperty.Register(
            nameof(EditMode),
            typeof(bool),
            typeof(GeneralJournalTable),
            new PropertyMetadata(false, OnStateChanged));

    public bool EditMode
    {
        get => (bool)GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    public static readonly DependencyProperty IsDeletingProperty =
        DependencyProperty.Register(
            nameof(IsDeleting),
            typeof(bool),
            typeof(GeneralJournalTable),
            new PropertyMetadata(false, OnStateChanged));

    public bool IsDeleting
    {
        get => (bool)GetValue(IsDeletingProperty);
        set => SetValue(IsDeletingProperty, value);
    }

    public bool IsNotDeleting => !IsDeleting;

    public GeneralJournalTable()
    {
        Application.LoadComponent(
            this,
            new Uri("ms-appx:///Features/Reports/Journals/Tables/GeneralJournalTable.xaml"));
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralJournalTable control)
        {
            control.OnPropertyChanged(nameof(EditMode));
            control.OnPropertyChanged(nameof(IsDeleting));
            control.OnPropertyChanged(nameof(IsNotDeleting));
        }
    }

    private void OnEditClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: Guid entryId })
        {
            EditRequested?.Invoke(this, entryId);
        }
    }

    private void OnDeleteClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: FlatJournalRowItem row })
        {
            PromptDelete?.Invoke(this, row.OriginalEntry);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public partial class JournalRowTemplateSelector : DataTemplateSelector
{
    public DataTemplate? HeaderTemplate { get; set; }
    public DataTemplate? LineTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is FlatJournalRowItem row && row.ShowHeader)
        {
            return HeaderTemplate;
        }

        return LineTemplate;
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        => SelectTemplateCore(item);
}