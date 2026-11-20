namespace AumoDekstop.Features.JournalEntry.Controls;

using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class JournalEntryHeaderControl : UserControl, INotifyPropertyChanged
{
    private bool _isUpdatingDateFromProperty;

    private CalendarDatePicker? EntryDatePicker =>
        FindName("EntryDatePicker") as CalendarDatePicker;

    public JournalEntryHeaderControl()
    {
        Application.LoadComponent(
            this,
            new Uri("ms-appx:///Features/JournalEntry/Controls/JournalEntryHeaderControl.xaml"));
    }

    #region Dependency Properties

    public static readonly DependencyProperty JournalTypeProperty =
        DependencyProperty.Register(
            nameof(JournalType),
            typeof(string),
            typeof(JournalEntryHeaderControl),
            new PropertyMetadata("GENERAL"));

    public string JournalType
    {
        get => (string)GetValue(JournalTypeProperty);
        set => SetValue(JournalTypeProperty, value);
    }

    public static readonly DependencyProperty EntryDateProperty =
        DependencyProperty.Register(
            nameof(EntryDate),
            typeof(string),
            typeof(JournalEntryHeaderControl),
            new PropertyMetadata(string.Empty, OnEntryDateChanged));

    /// <summary>
    /// Format tanggal ISO (yyyy-MM-dd) agar sesuai dengan kontrak input type="date" di Vue.
    /// </summary>
    public string EntryDate
    {
        get => (string)GetValue(EntryDateProperty);
        set => SetValue(EntryDateProperty, value);
    }

    public static readonly DependencyProperty TransactionNumberProperty =
        DependencyProperty.Register(
            nameof(TransactionNumber),
            typeof(string),
            typeof(JournalEntryHeaderControl),
            new PropertyMetadata(string.Empty));

    public string TransactionNumber
    {
        get => (string)GetValue(TransactionNumberProperty);
        set => SetValue(TransactionNumberProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(JournalEntryHeaderControl),
            new PropertyMetadata(string.Empty));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(JournalEntryHeaderControl),
            new PropertyMetadata(false, OnStateChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty IsEditModeProperty =
        DependencyProperty.Register(
            nameof(IsEditMode),
            typeof(bool),
            typeof(JournalEntryHeaderControl),
            new PropertyMetadata(false, OnStateChanged));

    public bool IsEditMode
    {
        get => (bool)GetValue(IsEditModeProperty);
        set => SetValue(IsEditModeProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public bool IsEditable => !IsReadOnly;

    public bool CanEditJournalType => !IsReadOnly && !IsEditMode;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is JournalEntryHeaderControl control)
        {
            control.OnPropertyChanged(nameof(IsEditable));
            control.OnPropertyChanged(nameof(CanEditJournalType));
        }
    }

    private static void OnEntryDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is JournalEntryHeaderControl control)
        {
            control.SyncDatePickerFromString(e.NewValue as string);
        }
    }

    private void SyncDatePickerFromString(string? dateStr)
    {
        if (_isUpdatingDateFromProperty) return;

        var datePicker = EntryDatePicker;
        if (datePicker is null || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;

        _isUpdatingDateFromProperty = true;
        try
        {
            if (DateTimeOffset.TryParseExact(
                    dateStr,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate) ||
                DateTimeOffset.TryParse(dateStr, out parsedDate))
            {
                datePicker.Date = parsedDate;
            }
            else
            {
                datePicker.Date = null;
            }
        }
        finally
        {
            _isUpdatingDateFromProperty = false;
        }
    }

    private void EntryDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        if (_isUpdatingDateFromProperty || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;

        _isUpdatingDateFromProperty = true;
        try
        {
            EntryDate = args.NewDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        }
        finally
        {
            _isUpdatingDateFromProperty = false;
        }
    }
}