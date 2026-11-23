namespace AumoDekstop.Features.Reports.Journals.Controls;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;

public sealed class ClosingJournalLineDisplayItem
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    public string Reference { get; set; } = string.Empty;

    public string AccountName { get; set; } = string.Empty;

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public string FormattedReference => string.IsNullOrWhiteSpace(Reference) ? "-" : Reference;

    public string FormattedDebit => Debit > 0 ? Debit.ToString("N0", IdCulture) : "-";

    public string FormattedCredit => Credit > 0 ? Credit.ToString("N0", IdCulture) : "-";

    // Indent & muted jika Credit > 0 (sesuai standar penyajian jurnal akuntansi)
    public Thickness AccountMargin => Credit > 0 && Debit == 0
        ? new Thickness(24, 0, 0, 0)
        : new Thickness(0);

    public FontWeight AccountFontWeight => Credit > 0 && Debit == 0
        ? FontWeights.Normal
        : FontWeights.Medium;

    public Brush AccountForeground => Credit > 0 && Debit == 0
        ? (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
        : (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
}

public sealed partial class ClosingGroupTableControl : UserControl, INotifyPropertyChanged
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ClosingGroupTableControl()
    {
        InitializeComponent();
        Lines = new ObservableCollection<ClosingJournalLineDisplayItem>();
    }

    #region Dependency Properties

    public static readonly DependencyProperty GroupDescriptionProperty =
        DependencyProperty.Register(
            nameof(GroupDescription),
            typeof(string),
            typeof(ClosingGroupTableControl),
            new PropertyMetadata(string.Empty, OnGroupDescriptionChanged));

    public string GroupDescription
    {
        get => (string)GetValue(GroupDescriptionProperty);
        set => SetValue(GroupDescriptionProperty, value);
    }

    public static readonly DependencyProperty LinesProperty =
        DependencyProperty.Register(
            nameof(Lines),
            typeof(IList<ClosingJournalLineDisplayItem>),
            typeof(ClosingGroupTableControl),
            new PropertyMetadata(null, OnLinesPropertyChanged));

    public IList<ClosingJournalLineDisplayItem> Lines
    {
        get => (IList<ClosingJournalLineDisplayItem>)GetValue(LinesProperty);
        set => SetValue(LinesProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public IList<ClosingJournalLineDisplayItem> DisplayLines => Lines ?? [];

    public decimal TotalDebit => Lines?.Sum(x => x.Debit) ?? 0m;

    public decimal TotalCredit => Lines?.Sum(x => x.Credit) ?? 0m;

    public string FormattedTotalDebit => TotalDebit.ToString("N0", IdCulture);

    public string FormattedTotalCredit => TotalCredit.ToString("N0", IdCulture);

    #endregion

    private static void OnGroupDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ClosingGroupTableControl control)
        {
            control.OnPropertyChanged(nameof(GroupDescription));
        }
    }

    private static void OnLinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ClosingGroupTableControl control)
        {
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= control.Lines_CollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += control.Lines_CollectionChanged;
            }

            control.NotifySummaryChanged();
        }
    }

    private void Lines_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifySummaryChanged();
    }

    private void NotifySummaryChanged()
    {
        OnPropertyChanged(nameof(DisplayLines));
        OnPropertyChanged(nameof(TotalDebit));
        OnPropertyChanged(nameof(TotalCredit));
        OnPropertyChanged(nameof(FormattedTotalDebit));
        OnPropertyChanged(nameof(FormattedTotalCredit));
    }
}