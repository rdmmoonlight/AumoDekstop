namespace AumoDekstop.Features.Reports.TrialBalances.Controls;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

public sealed partial class BalanceAlertControl : UserControl, INotifyPropertyChanged
{
    // Emerald-500 (#10B981) -> 10% alpha = 0x1A, 20% alpha = 0x33
    private static readonly SolidColorBrush BalancedBackgroundBrush = new(Color.FromArgb(0x1A, 0x10, 0xB9, 0x81));
    private static readonly SolidColorBrush BalancedBorderBrush = new(Color.FromArgb(0x33, 0x10, 0xB9, 0x81));
    private static readonly SolidColorBrush BalancedIconBrush = new(Color.FromArgb(0xFF, 0x10, 0xB9, 0x81));

    // Red-500 (#EF4444) -> 10% alpha = 0x1A, 20% alpha = 0x33
    private static readonly SolidColorBrush UnbalancedBackgroundBrush = new(Color.FromArgb(0x1A, 0xEF, 0x44, 0x44));
    private static readonly SolidColorBrush UnbalancedBorderBrush = new(Color.FromArgb(0x33, 0xEF, 0x44, 0x44));
    private static readonly SolidColorBrush UnbalancedIconBrush = new(Color.FromArgb(0xFF, 0xEF, 0x44, 0x44));

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public BalanceAlertControl()
    {
        InitializeComponent();
    }

    #region Dependency Properties (Pengganti Vue defineProps)

    public static readonly DependencyProperty IsBalancedProperty =
        DependencyProperty.Register(
            nameof(IsBalanced),
            typeof(bool),
            typeof(BalanceAlertControl),
            new PropertyMetadata(false, OnAlertStateChanged));

    public bool IsBalanced
    {
        get => (bool)GetValue(IsBalancedProperty);
        set => SetValue(IsBalancedProperty, value);
    }

    public static readonly DependencyProperty BalancedTextProperty =
        DependencyProperty.Register(
            nameof(BalancedText),
            typeof(string),
            typeof(BalanceAlertControl),
            new PropertyMetadata(string.Empty, OnAlertStateChanged));

    public string BalancedText
    {
        get => (string)GetValue(BalancedTextProperty);
        set => SetValue(BalancedTextProperty, value);
    }

    public static readonly DependencyProperty UnbalancedTextProperty =
        DependencyProperty.Register(
            nameof(UnbalancedText),
            typeof(string),
            typeof(BalanceAlertControl),
            new PropertyMetadata(string.Empty, OnAlertStateChanged));

    public string UnbalancedText
    {
        get => (string)GetValue(UnbalancedTextProperty);
        set => SetValue(UnbalancedTextProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public string DisplayText => IsBalanced ? BalancedText : UnbalancedText;

    public Brush AlertBackground => IsBalanced ? BalancedBackgroundBrush : UnbalancedBackgroundBrush;

    public Brush AlertBorderBrush => IsBalanced ? BalancedBorderBrush : UnbalancedBorderBrush;

    public Brush IconForeground => IsBalanced ? BalancedIconBrush : UnbalancedIconBrush;

    #endregion

    private static void OnAlertStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BalanceAlertControl control)
        {
            control.OnPropertyChanged(nameof(DisplayText));
            control.OnPropertyChanged(nameof(AlertBackground));
            control.OnPropertyChanged(nameof(AlertBorderBrush));
            control.OnPropertyChanged(nameof(IconForeground));
        }
    }
}