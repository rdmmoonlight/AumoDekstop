using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.Periods.Views;

public sealed partial class PeriodsAlertsControl : UserControl
{
    public static readonly DependencyProperty HasActivePeriodProperty =
        DependencyProperty.Register(
            nameof(HasActivePeriod),
            typeof(bool),
            typeof(PeriodsAlertsControl),
            new PropertyMetadata(false, OnAlertStateChanged));

    public static readonly DependencyProperty ActivePeriodNameProperty =
        DependencyProperty.Register(
            nameof(ActivePeriodName),
            typeof(string),
            typeof(PeriodsAlertsControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty WarningMessageProperty =
        DependencyProperty.Register(
            nameof(WarningMessage),
            typeof(string),
            typeof(PeriodsAlertsControl),
            new PropertyMetadata(null, OnAlertStateChanged));

    public bool HasActivePeriod
    {
        get => (bool)GetValue(HasActivePeriodProperty);
        set => SetValue(HasActivePeriodProperty, value);
    }

    public string ActivePeriodName
    {
        get => (string)GetValue(ActivePeriodNameProperty);
        set => SetValue(ActivePeriodNameProperty, value);
    }

    public string? WarningMessage
    {
        get => (string?)GetValue(WarningMessageProperty);
        set => SetValue(WarningMessageProperty, value);
    }

    public bool HasWarningMessage => !string.IsNullOrWhiteSpace(WarningMessage);

    public bool ShowNoActivePeriodAlert => !HasActivePeriod && !HasWarningMessage;

    public PeriodsAlertsControl()
    {
        InitializeComponent();
    }

    private static void OnAlertStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PeriodsAlertsControl control)
        {
            control.Bindings.Update();
        }
    }
}
