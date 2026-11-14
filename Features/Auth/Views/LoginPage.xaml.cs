using AumoDekstop.Features.Auth.Helpers;
using AumoDekstop.Features.Auth.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace AumoDekstop.Features.Auth.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }
    private string? _redirectPath;

    public event EventHandler? LoginSucceeded;

    public LoginPage()
    {
        // 1. Resolve ViewModel TERLEBIH DAHULU sebelum InitializeComponent()
        ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
        ViewModel.LoginSucceeded += OnAuthSuccess;

        // 2. Gunakan InitializeComponent() agar {x:Bind} terhubung dengan benar
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string redirectPath)
        {
            _redirectPath = redirectPath;
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.LoginSucceeded -= OnAuthSuccess;
    }

    private void OnInputKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.SubmitCommand.CanExecute(null))
        {
            e.Handled = true;
            ViewModel.SubmitCommand.Execute(null);
        }
    }

    private void OnGoogleLoginSucceeded(object? sender, EventArgs e)
    {
        ViewModel.NotifyLoginSuccess();
    }

    private void OnAuthSuccess(object? sender, EventArgs e)
    {
        LoginSucceeded?.Invoke(this, EventArgs.Empty);

        if (Frame is not null)
        {
            var targetPage = AuthHelpers.ResolvePostLoginPage(_redirectPath);
            Frame.Navigate(targetPage);
        }
    }
}
