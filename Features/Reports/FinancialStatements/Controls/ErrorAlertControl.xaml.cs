
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.Reports.FinancialStatements.Controls;

public sealed partial class ErrorAlertControl : UserControl, INotifyPropertyChanged
{
    private const string DefaultTitle = "Gagal Memuat Data";
    private const string DefaultMessage = "Terjadi kesalahan saat mengambil data laporan keuangan. Silakan coba lagi.";

    private event RoutedEventHandler? _retryClicked;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ErrorAlertControl()
    {
        InitializeComponent();
    }

    #region Events & Commands (Pengganti onRetry & emit('retry'))

    public event RoutedEventHandler? RetryClicked
    {
        add
        {
            _retryClicked += value;
            OnPropertyChanged(nameof(HasRetryAction));
        }
        remove
        {
            _retryClicked -= value;
            OnPropertyChanged(nameof(HasRetryAction));
        }
    }

    public static readonly DependencyProperty RetryCommandProperty =
        DependencyProperty.Register(
            nameof(RetryCommand),
            typeof(ICommand),
            typeof(ErrorAlertControl),
            new PropertyMetadata(null, OnRetryCommandChanged));

    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    #endregion

    #region Dependency Properties (Pengganti Vue withDefaults(defineProps))

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ErrorAlertControl),
            new PropertyMetadata(DefaultTitle, OnTextChanged));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(ErrorAlertControl),
            new PropertyMetadata(DefaultMessage, OnTextChanged));

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public string EffectiveTitle => string.IsNullOrWhiteSpace(Title) ? DefaultTitle : Title;

    public string EffectiveMessage => string.IsNullOrWhiteSpace(Message) ? DefaultMessage : Message;

    public bool HasRetryAction => RetryCommand is not null || _retryClicked is not null;

    #endregion

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ErrorAlertControl control)
        {
            control.OnPropertyChanged(nameof(EffectiveTitle));
            control.OnPropertyChanged(nameof(EffectiveMessage));
        }
    }

    private static void OnRetryCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ErrorAlertControl control)
        {
            control.OnPropertyChanged(nameof(HasRetryAction));
        }
    }

    private void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        _retryClicked?.Invoke(this, e);

        if (RetryCommand?.CanExecute(null) == true)
        {
            RetryCommand.Execute(null);
        }
    }
}