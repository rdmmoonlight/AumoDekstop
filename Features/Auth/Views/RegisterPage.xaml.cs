using AumoDekstop.Features.Auth.Helpers;
using AumoDekstop.Features.Auth.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AumoDekstop.Features.Auth.Views.RegisterPage;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel { get; }
    private string? _redirectPath;

    public event EventHandler? RegisterSucceeded;

    public RegisterPage()
    {
        Microsoft.UI.Xaml.Application.LoadComponent(
            this,
            new Uri("ms-appx:///Features/Auth/Views/RegisterPage.xaml"));
        ViewModel = App.Current.Services.GetRequiredService<RegisterViewModel>();
        ViewModel.RegisterSucceeded += OnAuthSuccess;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string redirectPath)
        {
            _redirectPath = redirectPath;
        }
    }

    private void OnGoogleRegisterSucceeded(object? sender, EventArgs e)
    {
        ViewModel.NotifyRegisterSuccess();
    }

    private void OnAuthSuccess(object? sender, EventArgs e)
    {
        RegisterSucceeded?.Invoke(this, EventArgs.Empty);

        if (Frame is not null)
        {
            var targetPage = AuthHelpers.ResolvePostLoginPage(_redirectPath);
            Frame.Navigate(targetPage);
        }
    }
}