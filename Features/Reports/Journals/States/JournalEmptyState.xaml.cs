namespace AumoDekstop.Features.Reports.Journals.States;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class JournalEmptyState : UserControl
{
    public static readonly DependencyProperty SelectedPeriodNameProperty =
        DependencyProperty.Register(
            nameof(SelectedPeriodName),
            typeof(string),
            typeof(JournalEmptyState),
            new PropertyMetadata(null, OnSelectedPeriodNameChanged));

    public string? SelectedPeriodName
    {
        get => (string?)GetValue(SelectedPeriodNameProperty);
        set => SetValue(SelectedPeriodNameProperty, value);
    }

    public JournalEmptyState()
    {
    }

    private static void OnSelectedPeriodNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is JournalEmptyState control
            && control.FindName("PeriodNameRun") is Microsoft.UI.Xaml.Documents.Run periodNameRun)
        {
            var periodName = e.NewValue as string;
            periodNameRun.Text = string.IsNullOrWhiteSpace(periodName)
                ? "this period"
                : periodName;
        }
    }
}