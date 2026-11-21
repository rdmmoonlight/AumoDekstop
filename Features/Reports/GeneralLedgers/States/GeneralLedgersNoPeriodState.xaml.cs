namespace AumoDekstop.Features.Reports.GeneralLedgers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

[ContentProperty(Name = nameof(ActionContent))]
public sealed partial class GeneralLedgersNoPeriodState : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(GeneralLedgersNoPeriodState),
            new PropertyMetadata("Tidak Ada Periode Aktif", OnTitleChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(GeneralLedgersNoPeriodState),
            new PropertyMetadata(
                "Silakan pilih atau buat periode akuntansi terlebih dahulu untuk menampilkan data buku besar.",
                OnMessageChanged));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(
            nameof(ActionContent),
            typeof(object),
            typeof(GeneralLedgersNoPeriodState),
            new PropertyMetadata(null, OnActionContentChanged));

    /// <summary>
    /// Pengganti default &lt;slot /&gt; pada Vue untuk menyisipkan tombol aksi tambahan.
    /// </summary>
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public GeneralLedgersNoPeriodState()
    {
        this.InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersNoPeriodState control)
        {
            control.TitleText.Text = e.NewValue as string ?? "Tidak Ada Periode Aktif";
        }
    }

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersNoPeriodState control)
        {
            control.MessageText.Text = e.NewValue as string
                ?? "Silakan pilih atau buat periode akuntansi terlebih dahulu untuk menampilkan data buku besar.";
        }
    }

    private static void OnActionContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GeneralLedgersNoPeriodState control)
        {
            control.ActionSlotPresenter.Content = e.NewValue;
            control.ActionSlotPresenter.Visibility = e.NewValue is not null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}