namespace AumoDekstop.Features.Reports.TrialBalances.Controls;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class NoPeriodCardControl : UserControl, INotifyPropertyChanged
{
    private const string DefaultMessage = "Select a period to view trial balance.";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public NoPeriodCardControl()
    {
        InitializeComponent();
    }

    #region Events & Commands (Pengganti RouterLink to="/periods")

    public event RoutedEventHandler? NavigateToPeriodsClicked;

    public static readonly DependencyProperty NavigateCommandProperty =
        DependencyProperty.Register(
            nameof(NavigateCommand),
            typeof(ICommand),
            typeof(NoPeriodCardControl),
            new PropertyMetadata(null));

    public ICommand? NavigateCommand
    {
        get => (ICommand?)GetValue(NavigateCommandProperty);
        set => SetValue(NavigateCommandProperty, value);
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(NoPeriodCardControl),
            new PropertyMetadata(DefaultMessage, OnMessageChanged));

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string EffectiveMessage => string.IsNullOrWhiteSpace(Message) ? DefaultMessage : Message;

    #endregion

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NoPeriodCardControl control)
        {
            control.OnPropertyChanged(nameof(EffectiveMessage));
        }
    }

    private void GoToPeriodsButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPeriodsClicked?.Invoke(this, e);

        if (NavigateCommand?.CanExecute(null) == true)
        {
            NavigateCommand.Execute(null);
        }
    }
}