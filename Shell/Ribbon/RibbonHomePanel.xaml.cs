
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Shell.Ribbon;
public sealed partial class RibbonHomePanel : UserControl
{
    public event EventHandler<string>? CommandInvoked;

    public RibbonHomePanel()
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
