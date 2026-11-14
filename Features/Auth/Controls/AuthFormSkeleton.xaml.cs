using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace AumoDekstop.Features.Auth.Controls;

public sealed partial class AuthFormSkeleton : UserControl
{
    public AuthFormSkeleton()
    {
        Application.LoadComponent(this, new System.Uri("ms-appx:///Features/Auth/Controls/AuthFormSkeleton.xaml"));
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