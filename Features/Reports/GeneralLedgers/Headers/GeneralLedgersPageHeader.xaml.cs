
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AumoDekstop.Features.Reports.GeneralLedgers.Headers;
[ContentProperty(Name = nameof(ActionsContent))]
public sealed partial class GeneralLedgersPageHeader : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(GeneralLedgersPageHeader),
            new PropertyMetadata(string.Empty, OnHeaderPropertyChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(GeneralLedgersPageHeader),
            new PropertyMetadata(null, OnHeaderPropertyChanged));

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty PeriodNameProperty =
        DependencyProperty.Register(
            nameof(PeriodName),
            typeof(string),
            typeof(GeneralLedgersPageHeader),
            new PropertyMetadata(null, OnHeaderPropertyChanged));

    public string? PeriodName
    {
        get => (string?)GetValue(PeriodNameProperty);
        set => SetValue(PeriodNameProperty, value);
    }

    public static readonly DependencyProperty StartDateProperty =
        DependencyProperty.Register(
            nameof(StartDate),
            typeof(string),
            typeof(GeneralLedgersPageHeader),
            new PropertyMetadata(null, OnHeaderPropertyChanged));

    public string? StartDate
    {
        get => (string?)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    public static readonly DependencyProperty EndDateProperty =
        DependencyProperty.Register(
            nameof(EndDate),
            typeof(string),
            typeof(GeneralLedgersPageHeader),
            new PropertyMetadata(null, OnHeaderPropertyChanged));

    public string? EndDate
    {
        get => (string?)GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    public static readonly DependencyProperty IsClosedProperty =
        DependencyProperty.Register(
            nameof(IsClosed),
            typeof(bool),
            typeof(GeneralLedgersPageHeader),
            new PropertyMetadata(false, OnHeaderPropertyChanged));

    public bool IsClosed
    {
        get => (bool)GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }

    public static readonly DependencyProperty ActionsContentProperty =
        DependencyProperty.Register(
            nameof(ActionsContent),
            typeof(object),
            typeof(GeneralLedgersPageHeader),
            new PropertyMetadata(null, OnActionsContentChanged));

    /// <summary>
    /// Pengganti &lt;slot name="actions" /&gt; pada Vue untuk menampung tombol aksi di sisi kanan header.
    /// </summary>
    public object? ActionsContent
    {
        get => GetValue(ActionsContentProperty);
        set => SetValue(ActionsContentProperty, value);
    }

    public GeneralLedgersPageHeader()
    {
        this.InitializeComponent();
        UpdateDisplay();
    }

    private static void OnHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersPageHeader header)
        {
            header.UpdateDisplay();
        }
    }

    private static void OnActionsContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersPageHeader header)
        {
            header.ActionsPresenter.Content = e.NewValue;
            header.ActionsPresenter.Visibility = e.NewValue is not null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private void UpdateDisplay()
    {
        // 1. Judul
        TitleText.Text = Title ?? string.Empty;

        // 2. Badge Periode (v-if="periodName")
        if (!string.IsNullOrWhiteSpace(PeriodName))
        {
            PeriodBadgeBorder.Visibility = Visibility.Visible;
            var statusLabel = IsClosed ? "(Closed)" : "(Aktif)";
            PeriodBadgeText.Text = $"{PeriodName} {statusLabel}";

            if (IsClosed)
            {
                PeriodBadgeBorder.Background = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"];
                PeriodBadgeBorder.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
                PeriodBadgeText.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
            }
            else
            {
                PeriodBadgeBorder.Background = new SolidColorBrush(Color.FromArgb(24, 16, 185, 129));
                PeriodBadgeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(120, 16, 185, 129));
                PeriodBadgeText.Foreground = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129));
            }
        }
        else
        {
            PeriodBadgeBorder.Visibility = Visibility.Collapsed;
        }

        // 3. Deskripsi (v-if="description")
        if (!string.IsNullOrWhiteSpace(Description))
        {
            DescriptionText.Text = Description;
            DescriptionText.Visibility = Visibility.Visible;
        }
        else
        {
            DescriptionText.Visibility = Visibility.Collapsed;
        }

        // 4. Rentang Periode (v-if="startDate && endDate")
        if (!string.IsNullOrWhiteSpace(StartDate) && !string.IsNullOrWhiteSpace(EndDate))
        {
            var startFormatted = GeneralLedgerFormatter.FormatDate(StartDate);
            var endFormatted = GeneralLedgerFormatter.FormatDate(EndDate);
            DateRangeText.Text = $"Rentang Periode: {startFormatted} – {endFormatted}";
            DateRangeText.Visibility = Visibility.Visible;
        }
        else
        {
            DateRangeText.Visibility = Visibility.Collapsed;
        }
    }
}