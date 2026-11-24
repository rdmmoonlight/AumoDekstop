
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Shell.Status;

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
