namespace AumoDekstop.Features.JournalEntry.Controls;

using System;
using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

[SupportedOSPlatform("windows10.0.17763")]
public sealed partial class TransactionInfoCardControl : UserControl
{
    private bool _isUpdatingDateFromProperty;

    public TransactionInfoCardControl()
    {
    }

    #region Dependency Properties

    public static readonly DependencyProperty TransactionDateProperty =
        DependencyProperty.Register(
            nameof(TransactionDate),
            typeof(string),
            typeof(TransactionInfoCardControl),
            new PropertyMetadata(string.Empty, OnTransactionDateChanged));

    /// <summary>
    /// Tanggal transaksi dalam format ISO (yyyy-MM-dd).
    /// </summary>
    public string TransactionDate
    {
        get => (string)GetValue(TransactionDateProperty);
        set => SetValue(TransactionDateProperty, value);
    }

    public static readonly DependencyProperty ReferenceNumberProperty =
        DependencyProperty.Register(
            nameof(ReferenceNumber),
            typeof(string),
            typeof(TransactionInfoCardControl),
            new PropertyMetadata(string.Empty));

    public string ReferenceNumber
    {
        get => (string)GetValue(ReferenceNumberProperty);
        set => SetValue(ReferenceNumberProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(TransactionInfoCardControl),
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
            typeof(TransactionInfoCardControl),
            new PropertyMetadata(false));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public bool IsEditable => !IsReadOnly;

    #endregion

    private static void OnTransactionDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransactionInfoCardControl control)
        {
            control.SyncDatePickerFromString(e.NewValue as string);
        }
    }

    private void SyncDatePickerFromString(string? dateStr)
    {
        if (_isUpdatingDateFromProperty) return;

        _isUpdatingDateFromProperty = true;
        try
        {
            if (FindName("TransactionDatePicker") is not CalendarDatePicker datePicker)
            {
                return;
            }

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

    private void TransactionDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        if (_isUpdatingDateFromProperty) return;

        _isUpdatingDateFromProperty = true;
        try
        {
            TransactionDate = args.NewDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        }
        finally
        {
            _isUpdatingDateFromProperty = false;
        }
    }
}