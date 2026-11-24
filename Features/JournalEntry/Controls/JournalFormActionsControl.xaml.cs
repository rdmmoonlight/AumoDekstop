
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.JournalEntry.Controls;

public sealed partial class JournalFormActionsControl : UserControl
{
    public JournalFormActionsControl()
    {
    }

    #region Events (Pengganti Vue emit)

    public event RoutedEventHandler? CancelClicked;
    public event RoutedEventHandler? SubmitClicked;
    public event RoutedEventHandler? DeleteClicked;

    #endregion

    #region Dependency Properties (State)

    public static readonly DependencyProperty IsSubmittingProperty =
        DependencyProperty.Register(
            nameof(IsSubmitting),
            typeof(bool),
            typeof(JournalFormActionsControl),
            new PropertyMetadata(false, OnActionStateChanged));

    public bool IsSubmitting
    {
        get => (bool)GetValue(IsSubmittingProperty);
        set => SetValue(IsSubmittingProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(JournalFormActionsControl),
            new PropertyMetadata(false, OnActionStateChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty IsEditModeProperty =
        DependencyProperty.Register(
            nameof(IsEditMode),
            typeof(bool),
            typeof(JournalFormActionsControl),
            new PropertyMetadata(false, OnActionStateChanged));

    public bool IsEditMode
    {
        get => (bool)GetValue(IsEditModeProperty);
        set => SetValue(IsEditModeProperty, value);
    }

    public static readonly DependencyProperty IsBalancedProperty =
        DependencyProperty.Register(
            nameof(IsBalanced),
            typeof(bool),
            typeof(JournalFormActionsControl),
            new PropertyMetadata(true, OnActionStateChanged));

    public bool IsBalanced
    {
        get => (bool)GetValue(IsBalancedProperty);
        set => SetValue(IsBalancedProperty, value);
    }

    #endregion

    #region Dependency Properties (Optional MVVM Commands)

    public static readonly DependencyProperty CancelCommandProperty =
        DependencyProperty.Register(
            nameof(CancelCommand),
            typeof(ICommand),
            typeof(JournalFormActionsControl),
            new PropertyMetadata(null));

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public static readonly DependencyProperty SubmitCommandProperty =
        DependencyProperty.Register(
            nameof(SubmitCommand),
            typeof(ICommand),
            typeof(JournalFormActionsControl),
            new PropertyMetadata(null));

    public ICommand? SubmitCommand
    {
        get => (ICommand?)GetValue(SubmitCommandProperty);
        set => SetValue(SubmitCommandProperty, value);
    }

    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(
            nameof(DeleteCommand),
            typeof(ICommand),
            typeof(JournalFormActionsControl),
            new PropertyMetadata(null));

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    #endregion

    #region Computed Properties untuk x:Bind

    public bool IsNotSubmitting => !IsSubmitting;

    public bool IsEditable => !IsReadOnly;

    public bool CanDelete => IsEditMode && !IsReadOnly;

    public bool CanSubmit => !IsSubmitting && IsBalanced;

    public string CancelButtonText => IsReadOnly ? "Kembali" : "Batal";

    public string SubmitButtonText => IsEditMode ? "Perbarui Jurnal" : "Simpan Jurnal";

    #endregion

    private static void OnActionStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is JournalFormActionsControl control)
        {
            UpdateActionState();
        }
    }

    private static void UpdateActionState()
    {
        // Keep this hook available for state changes without depending on generated x:Bind bindings.
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        DeleteClicked?.Invoke(this, e);
        if (DeleteCommand?.CanExecute(null) == true)
        {
            DeleteCommand.Execute(null);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        CancelClicked?.Invoke(this, e);
        if (CancelCommand?.CanExecute(null) == true)
        {
            CancelCommand.Execute(null);
        }
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        SubmitClicked?.Invoke(this, e);
        if (SubmitCommand?.CanExecute(null) == true)
        {
            SubmitCommand.Execute(null);
        }
    }
}