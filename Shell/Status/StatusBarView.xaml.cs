namespace AumoDekstop.Shell.Status;

using Microsoft.UI.Xaml.Controls;

public sealed partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();
    }

    public void SetStatus(string text)
    {
        StatusText.Text = text;
    }
}
