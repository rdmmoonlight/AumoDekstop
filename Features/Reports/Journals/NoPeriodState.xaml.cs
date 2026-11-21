namespace AumoDekstop.Features.Reports.Journals;

using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

public sealed partial class NoPeriodState : UserControl
{
    /// <summary>
    /// Event yang dipicu saat tautan "Periods" diklik (pengganti RouterLink to="/periods").
    /// </summary>
    public event EventHandler? NavigateToPeriodsRequested;

    /// <summary>
    /// Opsional: Command MVVM jika ingin di-bind langsung dari ViewModel.
    /// </summary>
    public static readonly DependencyProperty NavigateToPeriodsCommandProperty =
        DependencyProperty.Register(
            nameof(NavigateToPeriodsCommand),
            typeof(ICommand),
            typeof(NoPeriodState),
            new PropertyMetadata(null));

    public ICommand? NavigateToPeriodsCommand
    {
        get => (ICommand?)GetValue(NavigateToPeriodsCommandProperty);
        set => SetValue(NavigateToPeriodsCommandProperty, value);
    }

    public NoPeriodState()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        Microsoft.UI.Xaml.Application.LoadComponent(
            this,
            new Uri("ms-appx:///Features/Reports/Journals/NoPeriodState.xaml"));
    }

    private void OnNavigateToPeriodsClick(Hyperlink sender, HyperlinkClickEventArgs args)
    {
        NavigateToPeriodsRequested?.Invoke(this, EventArgs.Empty);

        if (NavigateToPeriodsCommand?.CanExecute(null) == true)
        {
            NavigateToPeriodsCommand.Execute(null);
        }
    }
}