
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.Reports.FinancialStatements.Controls;

public sealed partial class NoPeriodCardControl : UserControl, INotifyPropertyChanged
{
    private const string DefaultTitle = "Periode Akuntansi Tidak Ditemukan";
    private const string DefaultMessage = "Silakan pilih atau buat periode akuntansi aktif terlebih dahulu untuk melihat laporan keuangan.";
    private const string DefaultActionLabel = "Kelola Periode";

    private event RoutedEventHandler? _actionClicked;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public NoPeriodCardControl()
    {
        InitializeComponent();
    }

    #region Events & Commands (Pengganti emit('action') & $attrs.onAction)

    public event RoutedEventHandler? ActionClicked
    {
        add
        {
            _actionClicked += value;
            OnPropertyChanged(nameof(HasActionHandler));
        }
        remove
        {
            _actionClicked -= value;
            OnPropertyChanged(nameof(HasActionHandler));
        }
    }

    public static readonly DependencyProperty ActionCommandProperty =
        DependencyProperty.Register(
            nameof(ActionCommand),
            typeof(ICommand),
            typeof(NoPeriodCardControl),
            new PropertyMetadata(null, OnActionCommandChanged));

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    #endregion

    #region Dependency Properties (Pengganti withDefaults(defineProps))

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(NoPeriodCardControl),
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
            typeof(NoPeriodCardControl),
            new PropertyMetadata(DefaultMessage, OnTextChanged));

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty ActionLabelProperty =
        DependencyProperty.Register(
            nameof(ActionLabel),
            typeof(string),
            typeof(NoPeriodCardControl),
            new PropertyMetadata(DefaultActionLabel, OnTextChanged));

    public string? ActionLabel
    {
        get => (string?)GetValue(ActionLabelProperty);
        set => SetValue(ActionLabelProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public string EffectiveTitle => string.IsNullOrWhiteSpace(Title) ? DefaultTitle : Title;

    public string EffectiveMessage => string.IsNullOrWhiteSpace(Message) ? DefaultMessage : Message;

    public string EffectiveActionLabel => string.IsNullOrWhiteSpace(ActionLabel) ? DefaultActionLabel : ActionLabel;

    public bool HasActionHandler => ActionCommand is not null || _actionClicked is not null;

    #endregion

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NoPeriodCardControl control)
        {
            control.OnPropertyChanged(nameof(EffectiveTitle));
            control.OnPropertyChanged(nameof(EffectiveMessage));
            control.OnPropertyChanged(nameof(EffectiveActionLabel));
        }
    }

    private static void OnActionCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NoPeriodCardControl control)
        {
            control.OnPropertyChanged(nameof(HasActionHandler));
        }
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        _actionClicked?.Invoke(this, e);

        if (ActionCommand?.CanExecute(null) == true)
        {
            ActionCommand.Execute(null);
        }
    }
}