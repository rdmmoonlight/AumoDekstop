using AumoDekstop.Helpers;
using AumoDekstop.ViewModels.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AumoDekstop.Views.Auth;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel { get; }
    private string? _redirectPath;

    public event EventHandler? RegisterSucceeded;

    public RegisterPage()
    {
        this.InitializeComponent();
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