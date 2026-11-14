using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace AumoDekstop.Controls.Auth;

public sealed partial class AuthFormSkeleton : UserControl
{
    public AuthFormSkeleton()
    {
        this.InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Resources["PulseAnimation"] is Storyboard storyboard)
        {
            storyboard.Begin();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (Resources["PulseAnimation"] is Storyboard storyboard)
        {
            storyboard.Stop();
        }
    }
}