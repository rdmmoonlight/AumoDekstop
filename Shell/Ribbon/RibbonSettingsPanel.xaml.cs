namespace AumoDekstop.Shell.Ribbon;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class RibbonSettingsPanel : UserControl
{
    public event EventHandler<string>? CommandInvoked;

    public RibbonSettingsPanel()
    {
        InitializeComponent();
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is RibbonButton button)
        {
            CommandInvoked?.Invoke(this, button.CommandId);
        }
    }
}
