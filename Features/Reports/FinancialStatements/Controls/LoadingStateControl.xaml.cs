namespace AumoDekstop.Features.Reports.FinancialStatements.Controls;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class LoadingStateControl : UserControl, INotifyPropertyChanged
{
    private const string DefaultMessage = "Memuat data laporan keuangan...";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public LoadingStateControl()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(LoadingStateControl),
            new PropertyMetadata(DefaultMessage, OnMessageChanged));

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(LoadingStateControl),
            new PropertyMetadata(true, OnIsLoadingChanged));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string EffectiveMessage => string.IsNullOrWhiteSpace(Message) ? DefaultMessage : Message;

    #endregion

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingStateControl control)
        {
            control.OnPropertyChanged(nameof(EffectiveMessage));
        }
    }

    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingStateControl control)
        {
            control.OnPropertyChanged(nameof(IsLoading));
        }
    }
}